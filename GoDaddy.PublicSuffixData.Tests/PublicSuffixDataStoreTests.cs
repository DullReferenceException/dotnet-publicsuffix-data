using AutoMoq.Helpers;
using FluentAssertions;
using GoDaddy.PublicSuffixData.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GoDaddy.PublicSuffixData.Tests
{
    [TestClass]
    public class PublicSuffixDataStoreTests : AutoMoqTestFixture<PublicSuffixDataStore>
    {
        new private PublicSuffixDataStore Subject
        {
            get
            {
                // Can't override the constructor AutoMoqTestFixture uses :(
                return new PublicSuffixDataStore(Mocked<IPublicSuffixDataSource>().Object);
            }
        }

        [TestMethod]
        public async Task PublicSuffixDataStore_GetTld_ReturnsDotSeparatedSegments()
        {
            Mocked<IPublicSuffixDataSource>()
                .Setup(s => s.GetDataAsync())
                .Returns(Task.FromResult(new DomainSegmentTree {
                    Children = new DomainSegmentNodeCollection
                    {
                        new DomainSegmentNode {
                            Segment = "com",
                            Children = new DomainSegmentNodeCollection
                            {
                                new DomainSegmentNode 
                                {
                                    Segment = "uk"
                                }
                            }
                        }
                    }
                }));

            var tld = await Subject.GetTldAsync("foo.bar.uk.com");

            tld.Should().Be("uk.com");
        }

        [TestMethod]
        public async Task PublicSuffixDataStore_GetTld_HandlesWildcards()
        {
            Mocked<IPublicSuffixDataSource>()
                .Setup(s => s.GetDataAsync())
                .Returns(Task.FromResult(new DomainSegmentTree
                {
                    Children = new DomainSegmentNodeCollection
                    {
                        new DomainSegmentNode {
                            Segment = "bd",
                            Children = new DomainSegmentNodeCollection
                            {
                                new DomainSegmentNode 
                                {
                                    Segment = "*"
                                }
                            }
                        }
                    }
                }));

            var tld = await Subject.GetTldAsync("foo.bar.gov.bd");

            tld.Should().Be("gov.bd");
        }

        [TestMethod]
        public async Task PublicSuffixDataStore_GetTld_HandlesWildcardExclusions()
        {
            Mocked<IPublicSuffixDataSource>()
                .Setup(s => s.GetDataAsync())
                .Returns(Task.FromResult(new DomainSegmentTree
                {
                    Children = new DomainSegmentNodeCollection
                    {
                        new DomainSegmentNode {
                            Segment = "ck",
                            Children = new DomainSegmentNodeCollection
                            {
                                new DomainSegmentNode { Segment = "*" },
                                new DomainSegmentNode { Segment = "!www" }
                            }
                        }
                    }
                }));

            var tld = await Subject.GetTldAsync("foo.bar.gov.ck");
            tld.Should().Be("gov.ck");

            tld = await Subject.GetTldAsync("foo.bar.www.ck");
            tld.Should().Be("ck");
        }
    }
}
