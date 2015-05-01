using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GoDaddy.PublicSuffixData.Internal
{
    internal class InternetPublicSuffixDataSource : PublicSuffixDataSource
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IPublicSuffixConfig _config;

        public InternetPublicSuffixDataSource(IPublicSuffixConfig config, IHttpClientFactory httpFactory) 
            : base(config)
        {
            _httpFactory = httpFactory;
            _config = config;
        }

        protected override DateTime? GetDataTimestamp()
        {
            // Data is always fresh
            return DateTime.Now;
        }

        protected override async Task<DomainSegmentTree> GetCurrentDataAsync()
        {
            var tree = new DomainSegmentTree();

            using (var client = _httpFactory.GetHttpClient())
            using (var stream = await client.GetStreamAsync(_config.DataSourceUrl))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (!IsOfSubstance(line)) continue;

                    line.Split('.')
                        .Reverse()
                        .Aggregate(tree, (current, segment) => current[segment] ?? current.Add(segment));
                }
            }

            return tree;
        }

        private static bool IsOfSubstance(string line)
        {
            return !string.IsNullOrWhiteSpace(line) && !line.StartsWith("//");
        }

        protected override Task CacheUpstreamDataAsync(DomainSegmentTree data)
        {
            throw new NotImplementedException();
        }
    }
}
