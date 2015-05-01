using System;
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

            await Subject.GetDataAsync();
            await Subject.GetDataAsync();

            Mocked<IPublicSuffixDataSource>().Verify(ds => ds.GetDataAsync(), Times.Once());
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
