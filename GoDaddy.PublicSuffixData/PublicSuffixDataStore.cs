﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using GoDaddy.PublicSuffixData.Internal;

namespace GoDaddy.PublicSuffixData
{
    public class PublicSuffixDataStore
    {
        private IPublicSuffixDataSource _dataSource;

        public PublicSuffixDataStore() : this(
            ConfigurationManager.GetSection("goDaddy.publicSuffixData") as IPublicSuffixConfig 
            ?? new DefaultConfig())
        {
        }

        public PublicSuffixDataStore(IPublicSuffixConfig config)
        {
            var fileSystem = new FileSystem();
            var dataSource = new InMemoryPublicSuffixDataSource(config)
            {
                Upstream = new FileSystemPublicSuffixDataSource(config, fileSystem)
                {
                    Upstream = new InternetPublicSuffixDataSource(config, new HttpClientFactory())
                }
            };
            SetDataSource(dataSource);
        }

        internal PublicSuffixDataStore(IPublicSuffixDataSource dataSource)
        {
            SetDataSource(dataSource);
        }

        public event EventHandler<PublicSuffixErrorEventArgs> CacheError = delegate { };
        public event EventHandler<PublicSuffixErrorEventArgs> DataRefreshError = delegate { };

        public async Task<string> GetTldAsync(string domainName)
        {
            var data = await _dataSource.GetDataAsync();
            var segments = domainName.Split('.').Select(s => s.ToLowerInvariant()).Reverse();
            return string.Join(".", GetTldSegments(data, segments).Reverse());
        }

        private static IEnumerable<string> GetTldSegments(DomainSegmentTree tree, IEnumerable<string> segments)
        {
            var node = tree;
            foreach (var segment in segments)
            {
                if (node.Contains(segment))
                {
                    yield return segment;
                    node = node[segment];
                }
                else if (!node.Contains("!" + segment) && node.Contains("*"))
                {
                    yield return segment;
                    node = node["*"];
                }
                else
                {
                    break;
                }
            }
        }

        private void SetDataSource(IPublicSuffixDataSource dataSource)
        {
            _dataSource = dataSource;
            _dataSource.CacheError += (s, e) => CacheError(this, e);
            _dataSource.DataRefreshError += (s, e) => DataRefreshError(this, e);
        }
    }
}
