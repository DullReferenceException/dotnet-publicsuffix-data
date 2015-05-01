using System;
using System.Configuration;
using System.IO;
using GoDaddy.PublicSuffixData.Internal;

namespace GoDaddy.PublicSuffixData
{
    public class PublicSuffixDataConfigSection : ConfigurationSection, IPublicSuffixConfig
    {
        private readonly IPublicSuffixConfig _defaultConfig = new DefaultConfig();

        [ConfigurationProperty("timeToStale")]
        public TimeSpan TimeToStale 
        {
            get { return ReadTimeSpanWithDefault("timeToStale", _defaultConfig.TimeToStale); }
        }

        [ConfigurationProperty("timeToExpired")]
        public TimeSpan TimeToExpired 
        {
            get { return ReadTimeSpanWithDefault("timeToExpired", _defaultConfig.TimeToExpired); }
        }

        [ConfigurationProperty("url", DefaultValue = DefaultConfig.DataSourceUrl)]
        public string DataSourceUrl 
        {
            get { return (string) this["url"]; }
        }


        [ConfigurationProperty("cacheFile")]
        public string CacheFilePath 
        {
            get
            {
                var currentValue = (string) this["cacheFile"];
                return string.IsNullOrWhiteSpace(currentValue) 
                    ? _defaultConfig.CacheFilePath
                    : currentValue;
 
            }
        }

        private TimeSpan ReadTimeSpanWithDefault(string key, TimeSpan defaultValue)
        {
            var currentValue = (TimeSpan)this[key];
            return (currentValue == default(TimeSpan)) ? defaultValue : currentValue;
        }
    }
}
