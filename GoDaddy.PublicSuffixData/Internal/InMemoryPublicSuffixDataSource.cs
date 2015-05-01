using System;
using System.Threading.Tasks;

namespace GoDaddy.PublicSuffixData.Internal
{
    internal class InMemoryPublicSuffixDataSource : PublicSuffixDataSource
    {
        private DateTime? _cacheTime;
        private DomainSegmentTree _cache;

        public InMemoryPublicSuffixDataSource(IPublicSuffixConfig config) : base(config)
        {
        }

        protected override DateTime? GetDataTimestamp()
        {
            return _cacheTime;
        }

        protected override Task<DomainSegmentTree> GetCurrentDataAsync()
        {
            return Task.FromResult(_cache);
        }

        protected override async Task CacheUpstreamDataAsync(DomainSegmentTree data)
        {
            _cache = data;
            _cacheTime = DateTime.Now;
        }
    }
}
