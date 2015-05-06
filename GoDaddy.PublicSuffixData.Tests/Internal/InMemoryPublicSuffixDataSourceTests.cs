using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using GoDaddy.PublicSuffixData.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GoDaddy.PublicSuffixData.Tests.Internal
{
    [TestClass]
    public class InMemoryPublicSuffixDataSourceTests : TestFixture
    {
        [TestInitialize]
        public void Initialize()
        {
            Mocked<IPublicSuffixConfig>()
                .Setup(s => s.TimeToStale)
                .Returns(TimeSpan.FromDays(10));
            Mocked<IPublicSuffixConfig>()
                .Setup(s => s.TimeToExpired)
                .Returns(TimeSpan.FromDays(30));

            Subject.Upstream = Mocked<IPublicSuffixDataSource>().Object;
        }

        [TestMethod]
        public async Task GetDataAsync_AlwaysGetsDataFromUpstreamAtFirst()
        {
            var upstreamData = new DomainSegmentTree();
            Mocked<IPublicSuffixDataSource>()
                .Setup(s => s.GetDataAsync())
                .Returns(Task.FromResult(upstreamData));

            var data = await Subject.GetDataAsync();

            data.Should().Be(upstreamData);
        }

        [TestMethod]
        public async Task GetDataAsync_CachesUpstreamData()
        {
            var upstreamData = new DomainSegmentTree();
            Mocked<IPublicSuffixDataSource>()
                .Setup(s => s.GetDataAsync())
                .Returns(Task.FromResult(upstreamData));
            Subject.Upstream = Mocked<IPublicSuffixDataSource>().Object;

            await Subject.GetDataAsync();
            await Task.Delay(100);

            await Subject.GetDataAsync();

            Mocked<IPublicSuffixDataSource>().Verify(ds => ds.GetDataAsync(), Times.Once());
        }

        [TestMethod]
        public void CacheError_IsRaisedOnUpstreamCacheErrors()
        {
            Exception cacheException = null;
            Subject.Upstream = Mocked<IPublicSuffixDataSource>().Object;
            Subject.CacheError += (s, e) =>
            {
                cacheException = e.Exception;
            };

            var mockError = new IOException();
            Mocked<IPublicSuffixDataSource>()
                .Raise(s => s.CacheError += null, new PublicSuffixErrorEventArgs(mockError));

            cacheException.Should().Be(mockError);
        }

        [TestMethod]
        public void DataRefreshError_IsRaisedOnUpstreamDataRefreshErrors()
        {
            Exception upstreamException = null;
            Subject.Upstream = Mocked<IPublicSuffixDataSource>().Object;
            Subject.DataRefreshError += (s, e) =>
            {
                upstreamException = e.Exception;
            };

            var mockError = new IOException();
            Mocked<IPublicSuffixDataSource>()
                .Raise(s => s.DataRefreshError += null, new PublicSuffixErrorEventArgs(mockError));

            upstreamException.Should().Be(mockError);
        }

        private InMemoryPublicSuffixDataSource _subject;
        private InMemoryPublicSuffixDataSource Subject
        {
            get
            {
                return _subject ?? (_subject = new InMemoryPublicSuffixDataSource(Mocked<IPublicSuffixConfig>().Object));
            }
        }
    }
}
