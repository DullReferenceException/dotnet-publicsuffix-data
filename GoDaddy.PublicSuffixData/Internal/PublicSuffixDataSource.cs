using System;
using System.Threading.Tasks;

namespace GoDaddy.PublicSuffixData.Internal
{
    internal abstract class PublicSuffixDataSource : IPublicSuffixDataSource
    {
        private readonly IPublicSuffixConfig _config;
        private IPublicSuffixDataSource _upstreamDataSource;

        protected PublicSuffixDataSource(IPublicSuffixConfig config)
        {
            _config = config;
        }

        public IPublicSuffixDataSource Upstream 
        {
            get { return _upstreamDataSource; }
            set
            {
                if (_upstreamDataSource != null)
                {
                    _upstreamDataSource.CacheError -= HandleUpstreamCacheError;
                    _upstreamDataSource.DataRefreshError -= HandleDataRefreshError;
                }

                _upstreamDataSource = value;

                if (_upstreamDataSource != null)
                {
                    _upstreamDataSource.CacheError += HandleUpstreamCacheError;
                    _upstreamDataSource.DataRefreshError += HandleDataRefreshError;
                }
            }
        }

        public event EventHandler<PublicSuffixErrorEventArgs> DataRefreshError = delegate { };
        public event EventHandler<PublicSuffixErrorEventArgs> CacheError = delegate { };

        public async Task<DomainSegmentTree> GetDataAsync()
        {
            var timestamp = GetDataTimestamp();
            if (IsNewerDataNeeded(timestamp))
            {
                var getLatestData = Task.Run(async () => await FetchAndCacheUpstream());
                
                if (IsNewerDataRequired(timestamp))
                {
                    return await getLatestData;
                }

                Task.Run(async () =>
                {
                    try
                    {
                        await getLatestData;
                    }
                    catch (Exception ex)
                    {
                        DataRefreshError(this, new PublicSuffixErrorEventArgs(ex));
                    }
                }).ConfigureAwait(false);
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

                    Task.Run(async () => 
                    {
                        try
                        {
                            await CacheUpstreamDataAsync(newData);
                        }
                        catch (Exception ex)
                        {
                            CacheError(this, new PublicSuffixErrorEventArgs(ex));
                        }
                    }).ConfigureAwait(false);

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

        private void HandleUpstreamCacheError(object sender, PublicSuffixErrorEventArgs e)
        {
            CacheError(this, e);
        }

        private void HandleDataRefreshError(object sender, PublicSuffixErrorEventArgs e)
        {
            DataRefreshError(this, e);
        }

        protected abstract DateTime? GetDataTimestamp();
        protected abstract Task<DomainSegmentTree> GetCurrentDataAsync();
        protected abstract Task CacheUpstreamDataAsync(DomainSegmentTree data);
    }
}
