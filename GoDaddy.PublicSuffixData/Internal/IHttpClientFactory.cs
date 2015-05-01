using System.Net.Http;

namespace GoDaddy.PublicSuffixData.Internal
{
    internal interface IHttpClientFactory
    {
        HttpClient GetHttpClient();
    }
}
