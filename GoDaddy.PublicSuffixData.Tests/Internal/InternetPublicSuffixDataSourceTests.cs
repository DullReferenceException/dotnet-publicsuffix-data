using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GoDaddy.PublicSuffixData.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoDaddy.PublicSuffixData.Tests.Internal
{
    [TestClass]
    public class InternetPublicSuffixDataSourceTests : TestFixture
    {
        private const string SourceUrl = "https://publicsuffix.org/list/effective_tld_names.dat";

        private readonly MockHttpMessageHandler _httpHandler = new MockHttpMessageHandler();

        [TestInitialize]
        public void Initialize()
        {
            Mocked<IHttpClientFactory>()
                .Setup(f => f.GetHttpClient())
                .Returns(new HttpClient(_httpHandler));
            Mocked<IPublicSuffixConfig>()
                .Setup(c => c.DataSourceUrl)
                .Returns(SourceUrl);
            Mocked<IPublicSuffixConfig>()
                .Setup(c => c.TimeToStale)
                .Returns(TimeSpan.FromDays(10));
            Mocked<IPublicSuffixConfig>()
                .Setup(c => c.TimeToExpired)
                .Returns(TimeSpan.FromDays(30));
        }

        [TestMethod]
        public async Task InternetPublicSuffixDataSource_GetDataAsync_FetchesDataViaHttp()
        {
            _httpHandler.ExecutingRequest += (s, e) => 
            {
                e.Request.RequestUri.AbsoluteUri.Should().Be(SourceUrl);
                e.Response = new HttpResponseMessage { Content = new StringContent("co\nuk\nco.uk") };
            };

            var data = await Subject.GetDataAsync();

            data.Contains("co").Should().BeTrue();
            data.Contains("uk").Should().BeTrue();
            data["uk"].Contains("co").Should().BeTrue();
        }

        [TestMethod]
        public async Task InternetPublicSuffixDataSource_GetDataAsync_IgnoresCommentAndBlankLines()
        {
            _httpHandler.ExecutingRequest += (s, e) =>
            {
                e.Request.RequestUri.AbsoluteUri.Should().Be(SourceUrl);
                e.Response = new HttpResponseMessage { Content = new StringContent("co\n\n// United Kingdom Domains\nuk\nco.uk") };
            };

            var data = await Subject.GetDataAsync();

            data.Contains("co").Should().BeTrue();
            data.Contains("uk").Should().BeTrue();
            data["uk"].Contains("co").Should().BeTrue();
        }

        [TestMethod, ExpectedException(typeof(HttpRequestException))]
        public async Task InternetPublicSuffixDataSource_GetDataAsync_ThrowsIfFetchingDataFails()
        {
            _httpHandler.ExecutingRequest += (s, e) =>
            {
                e.Request.RequestUri.AbsoluteUri.Should().Be(SourceUrl);
                e.Response = new HttpResponseMessage { 
                    StatusCode = HttpStatusCode.InternalServerError, 
                    Content = new StringContent("<html><body>Error!</body></html>", Encoding.UTF8, "text/html") 
                };
            };

            await Subject.GetDataAsync();
        }

        private InternetPublicSuffixDataSource _subject;
        private InternetPublicSuffixDataSource Subject
        {
            get
            {
                return _subject ?? (_subject = new InternetPublicSuffixDataSource(
                    Mocked<IPublicSuffixConfig>().Object,
                    Mocked<IHttpClientFactory>().Object));
            }
        }

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            public event EventHandler<MockHttpEventArgs> ExecutingRequest = delegate { };

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, 
                CancellationToken cancellationToken)
            {
                var eventArgs = new MockHttpEventArgs {Request = request};
                ExecutingRequest(this, eventArgs);
                return Task.FromResult(eventArgs.Response);
            }
        }

        private class MockHttpEventArgs : EventArgs
        {
            public HttpRequestMessage Request { get; set; }
            public HttpResponseMessage Response { get; set; }
        }
    }
}
