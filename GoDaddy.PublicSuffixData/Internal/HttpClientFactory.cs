using System.Net.Http;

namespace GoDaddy.PublicSuffixData.Internal
{
    public class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient GetHttpClient()
        {
            return new HttpClient();
        }
    }
}
