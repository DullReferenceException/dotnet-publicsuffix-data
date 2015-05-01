using System;
using System.IO;

namespace GoDaddy.PublicSuffixData.Internal
{
    internal class DefaultConfig : IPublicSuffixConfig
    {
        public const string DataSourceUrl = "https://publicsuffix.org/list/effective_tld_names.dat";

        string IPublicSuffixConfig.DataSourceUrl 
        {
            get { return DataSourceUrl; }
        }

        public string CacheFilePath 
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    ".publicsuffix.org");
            }
        }

        public TimeSpan TimeToStale { get { return TimeSpan.FromDays(10); } }
        public TimeSpan TimeToExpired { get { return TimeSpan.FromDays(30); } }
    }
}
