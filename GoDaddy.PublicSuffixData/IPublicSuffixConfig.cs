using System;

namespace GoDaddy.PublicSuffixData
{
    public interface IPublicSuffixConfig
    {
        string DataSourceUrl { get; }
        string CacheFilePath { get; }
        TimeSpan TimeToStale { get; }
        TimeSpan TimeToExpired { get; }
    }
}
