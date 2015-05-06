using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GoDaddy.PublicSuffixData.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoDaddy.PublicSuffixData.Tests.Internal
{
    [TestClass]
    public class FileSystemPublicSuffixDataSourceTests : TestFixture
    {
        private readonly string _cacheFilePath = Path.GetTempFileName();
        private FileSystemPublicSuffixDataSource _subject;

        private FileSystemPublicSuffixDataSource Subject
        {
            get 
            { 
                return _subject ?? (_subject = new FileSystemPublicSuffixDataSource(
                    Mocked<IPublicSuffixConfig>().Object,
                    Mocked<IFileSystem>().Object));
            }
        }
        
        [TestInitialize]
        public void Initialize()
        {
            Mocked<IPublicSuffixConfig>().Setup(c => c.TimeToStale).Returns(TimeSpan.FromDays(10));
            Mocked<IPublicSuffixConfig>().Setup(c => c.TimeToExpired).Returns(TimeSpan.FromDays(30));
            Mocked<IPublicSuffixConfig>().Setup(c => c.CacheFilePath).Returns(_cacheFilePath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(_cacheFilePath);
        }

        [TestMethod]
        public async Task FileSystemPublicSuffixDataSource_GetDataAsync_WhenDiskDataExistsAndIsNotExpired_ParsesData()
        {
            Mocked<IFileSystem>()
                .Setup(fs => fs.Exists(_cacheFilePath))
                .Returns(true);
            Mocked<IFileSystem>()
                .Setup(fs => fs.GetLastWriteTime(_cacheFilePath))
                .Returns(DateTime.Now);
            Mocked<IFileSystem>()
                .Setup(fs => fs.OpenRead(_cacheFilePath))
                .Returns(new MemoryStream(Encoding.UTF8.GetBytes("{ \"com\": { } }")));

            var data = await Subject.GetDataAsync();

            data.Children.Should().HaveCount(1);
            data.Contains("com").Should().BeTrue();
        }

        [TestMethod]
        public async Task FileSystemPublicSuffixDataSource_GetDataAsync_WhenDiskDataExistsAndIsStaleButNotExpired_ReturnsStaleDataAndFetchesNewFromUpstream()
        {
            Mocked<IFileSystem>()
                .Setup(fs => fs.Exists(_cacheFilePath))
                .Returns(true);
            Mocked<IFileSystem>()
                .Setup(fs => fs.GetLastWriteTime(_cacheFilePath))
                .Returns(DateTime.Now.AddDays(-11));
            Mocked<IFileSystem>()
                .Setup(fs => fs.OpenRead(_cacheFilePath))
                .Returns(new MemoryStream(Encoding.UTF8.GetBytes("{ \"com\": { } }")));

            var latestData = new DomainSegmentTree
            {
                Children = new DomainSegmentNodeCollection
                {
                    new DomainSegmentNode {Segment = "com"},
                    new DomainSegmentNode {Segment = "net"}
                }
            };
            var upstreamSource = new Mock<IPublicSuffixDataSource>();
            upstreamSource.Setup(s => s.GetDataAsync()).Returns(Task.FromResult(latestData));
            Subject.Upstream = upstreamSource.Object;

            var data = await Subject.GetDataAsync();

            data.Children.Should().HaveCount(1);
            data.Contains("com").Should().BeTrue();
            upstreamSource.Verify(s => s.GetDataAsync());

            // Wait for cache to be asynchronously written
            await Task.Delay(100);
            Mocked<IFileSystem>().Verify(fs => fs.OpenWrite(_cacheFilePath));
        }

        [TestMethod]
        public async Task FileSystemPublicSuffixDataSource_GetDataAsync_WhenDiskDataIsStaleAndUpstreamRaisesError_RaisesDataRefreshErrorEvent()
        {
            Mocked<IFileSystem>()
                .Setup(fs => fs.Exists(_cacheFilePath))
                .Returns(true);
            Mocked<IFileSystem>()
                .Setup(fs => fs.GetLastWriteTime(_cacheFilePath))
                .Returns(DateTime.Now.AddDays(-11));
            Mocked<IFileSystem>()
                .Setup(fs => fs.OpenRead(_cacheFilePath))
                .Returns(new MemoryStream(Encoding.UTF8.GetBytes("{ \"com\": { } }")));

            var upstreamSource = new Mock<IPublicSuffixDataSource>();
            var upstreamError = new WebException("Internal error!");
            upstreamSource.Setup(s => s.GetDataAsync()).Throws(upstreamError);
            Subject.Upstream = upstreamSource.Object;

            Exception refreshError = null;
            Subject.DataRefreshError += (s, e) =>
            {
                refreshError = e.Exception;
            };
            await Subject.GetDataAsync();

            // Wait for upstream to be asynchronously called
            await Task.Delay(100);
            refreshError.Should().Be(upstreamError);
        }

        [TestMethod]
        public async Task FileSystemPublicSuffixDataSource_GetDataAsync_WhenDiskDataExistsAndIsExpired_ReturnsNewDataFromUpstream()
        {
            Mocked<IFileSystem>()
                .Setup(fs => fs.GetLastWriteTime(_cacheFilePath))
                .Returns(DateTime.Now.AddDays(-11));
            Mocked<IFileSystem>()
                .Setup(fs => fs.OpenRead(_cacheFilePath))
                .Returns(new MemoryStream(Encoding.UTF8.GetBytes("{ \"com\": { } }")));

            var latestData = new DomainSegmentTree
            {
                Children = new DomainSegmentNodeCollection
                {
                    new DomainSegmentNode {Segment = "com"},
                    new DomainSegmentNode {Segment = "net"}
                }
            };
            var upstreamSource = new Mock<IPublicSuffixDataSource>();
            upstreamSource.Setup(s => s.GetDataAsync()).Returns(Task.FromResult(latestData));
            Subject.Upstream = upstreamSource.Object;

            var data = await Subject.GetDataAsync();

            data.Should().Be(latestData);
        }

        [TestMethod]
        public async Task FileSystemPublicSuffixDataSource_GetDataAsync_WhenDiskDataDoesNotExist_ReturnsNewDataFromUpstream()
        {
            Mocked<IFileSystem>()
                .Setup(fs => fs.Exists(_cacheFilePath))
                .Returns(false);

            var latestData = new DomainSegmentTree
            {
                Children = new DomainSegmentNodeCollection
                {
                    new DomainSegmentNode {Segment = "com"},
                    new DomainSegmentNode {Segment = "net"}
                }
            };
            var upstreamSource = new Mock<IPublicSuffixDataSource>();
            upstreamSource.Setup(s => s.GetDataAsync()).Returns(Task.FromResult(latestData));
            Subject.Upstream = upstreamSource.Object;

            var data = await Subject.GetDataAsync();

            data.Should().Be(latestData);
        }

        [TestMethod]
        public async Task FileSystemPublicSuffixDataSource_GetDataAsync_WhenReadingDiskDataFails_ReturnsDataFromUpstream()
        {
            Mocked<IFileSystem>()
                .Setup(fs => fs.Exists(_cacheFilePath))
                .Returns(true);
            Mocked<IFileSystem>()
                .Setup(fs => fs.GetLastWriteTime(_cacheFilePath))
                .Returns(DateTime.Now);
            Mocked<IFileSystem>()
                .Setup(fs => fs.OpenRead(_cacheFilePath))
                .Throws(new IOException());

            var latestData = new DomainSegmentTree
            {
                Children = new DomainSegmentNodeCollection
                {
                    new DomainSegmentNode {Segment = "com"},
                    new DomainSegmentNode {Segment = "net"}
                }
            };
            var upstreamSource = new Mock<IPublicSuffixDataSource>();
            upstreamSource.Setup(s => s.GetDataAsync()).Returns(Task.FromResult(latestData));
            Subject.Upstream = upstreamSource.Object;

            var data = await Subject.GetDataAsync();

            data.Should().Be(latestData);
        }

        [TestMethod]
        public async Task FileSystemPublicSuffixDataSource_GetDataAsync_CachesAfterGettingDataFromUpstream()
        {
            var mockStream = new MemoryStream();
            Mocked<IFileSystem>()
                .Setup(fs => fs.Exists(_cacheFilePath))
                .Returns(false);
            Mocked<IFileSystem>()
                .Setup(fs => fs.OpenWrite(_cacheFilePath))
                .Returns(mockStream);

            var latestData = new DomainSegmentTree
            {
                Children = new DomainSegmentNodeCollection
                {
                    new DomainSegmentNode {Segment = "com"},
                    new DomainSegmentNode {Segment = "net"}
                }
            };
            var upstreamSource = new Mock<IPublicSuffixDataSource>();
            upstreamSource.Setup(s => s.GetDataAsync()).Returns(Task.FromResult(latestData));
            Subject.Upstream = upstreamSource.Object;

            await Subject.GetDataAsync();

            // Hack... wait for the cache to be written
            await Task.Delay(100);

            var serializer = new JsonSerializer();
            var buffer = mockStream.GetBuffer();
            using (var readStream = new MemoryStream(buffer))
            using (var textReader = new StreamReader(readStream))
            using (var reader = new JsonTextReader(textReader))
            {
                var obj = serializer.Deserialize<JObject>(reader);
                obj.Properties().Should().HaveCount(2);
                obj["com"].Should().NotBeNull();
                obj["net"].Should().NotBeNull();
            }
        }

        [TestMethod]
        public async Task FileSystemPublicSuffixDataSource_GetDataAsync_RaisesAnEventWhenReceivingErrorCachingUpstreamData()
        {
            var ioException = new IOException();
            Mocked<IFileSystem>()
                .Setup(fs => fs.Exists(_cacheFilePath))
                .Returns(false);
            Mocked<IFileSystem>()
                .Setup(fs => fs.OpenWrite(_cacheFilePath))
                .Throws(ioException);

            var latestData = new DomainSegmentTree
            {
                Children = new DomainSegmentNodeCollection
                {
                    new DomainSegmentNode {Segment = "com"},
                    new DomainSegmentNode {Segment = "net"}
                }
            };

            var upstreamSource = new Mock<IPublicSuffixDataSource>();
            upstreamSource.Setup(s => s.GetDataAsync()).Returns(Task.FromResult(latestData));

            Exception cacheException = null;
            Subject.CacheError += (s, e) => cacheException = e.Exception;
            Subject.Upstream = upstreamSource.Object;

            await Subject.GetDataAsync();
            await Task.Delay(100);

            cacheException.Should().Be(ioException);
        }
    }
}
