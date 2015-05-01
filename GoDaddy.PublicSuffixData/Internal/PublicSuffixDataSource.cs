using System;
using System.Threading.Tasks;

namespace GoDaddy.PublicSuffixData.Internal
{
    internal abstract class PublicSuffixDataSource : IPublicSuffixDataSource
    {
        private readonly IPublicSuffixConfig _config;

        protected PublicSuffixDataSource(IPublicSuffixConfig config)
        {
            _config = config;
        }

        public IPublicSuffixDataSource Upstream { get; set; }

        public async Task<DomainSegmentTree> GetDataAsync()
        {
            var timestamp = GetDataTimestamp();
            if (IsNewerDataNeeded(timestamp))
            {
                var getLatestData = FetchAndCacheUpstream();
                if (IsNewerDataRequired(timestamp))
                {
                    return await getLatestData;
                }
            }

            try
            {
                return await GetCurrentDataAsync();
            }
            catch
            {
                if (Upstream == null)
                {
                    throw;
                }
            }

            // Retry from upstream
            return await FetchAndCacheUpstream();
        }

        private Task<DomainSegmentTree> FetchAndCacheUpstream()
        {
            return Upstream
                .GetDataAsync()
                .ContinueWith(t =>
                {
                    var newData = t.Result;
                    CacheUpstreamDataAsync(newData);
                    return newData;
                });
        }

        private bool IsNewerDataNeeded(DateTime? timestamp)
        {
            return TimeHasElapsed(_config.TimeToStale, timestamp);
        }

        private bool IsNewerDataRequired(DateTime? timestamp)
        {
            return TimeHasElapsed(_config.TimeToExpired, timestamp);
        }

        private static bool TimeHasElapsed(TimeSpan span, DateTime? timestamp)
        {
            return !timestamp.HasValue || (DateTime.Now - timestamp.Value) > span;
        }

        protected abstract DateTime? GetDataTimestamp();
        protected abstract Task<DomainSegmentTree> GetCurrentDataAsync();
        protected abstract Task CacheUpstreamDataAsync(DomainSegmentTree data);
    }
}
