using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoDaddy.PublicSuffixData.Internal
{
    internal class FileSystemPublicSuffixDataSource : PublicSuffixDataSource
    {
        private readonly IFileSystem _fileSystem;
        private readonly IPublicSuffixConfig _config;

        public FileSystemPublicSuffixDataSource(IPublicSuffixConfig config, IFileSystem fileSystem)
            : base(config)
        {
            _fileSystem = fileSystem;
            _config = config;
        }

        protected override DateTime? GetDataTimestamp()
        {
            return _fileSystem.Exists(_config.CacheFilePath) 
                ? _fileSystem.GetLastWriteTime(_config.CacheFilePath) 
                : (DateTime?)null;
        }

        protected override Task<DomainSegmentTree> GetCurrentDataAsync()
        {
            return Task.Run(() => {
                var serializer = new JsonSerializer();

                using (var stream = _fileSystem.OpenRead(_config.CacheFilePath))
                using (var streamReader = new StreamReader(stream))
                using (var reader = new JsonTextReader(streamReader))
                {
                    var rootObject = serializer.Deserialize<JObject>(reader);
                    if (rootObject == null)
                    {
                        throw new ApplicationException("Invalid data in cache");
                    }

                    return new DomainSegmentTree
                    {
                        Children = ConvertFromJsonRecursive(rootObject)
                    };
                }
            });
        }

        private static DomainSegmentNodeCollection ConvertFromJsonRecursive(JObject obj)
        {
            return new DomainSegmentNodeCollection(
                from prop in obj.Properties()
                select new DomainSegmentNode
                {
                    Segment = prop.Name,
                    Children = ConvertFromJsonRecursive((JObject)prop.Value)
                });
        }

        protected override Task CacheUpstreamDataAsync(DomainSegmentTree data)
        {
            return Task.Run(() => {
                var serializer = new JsonSerializer();

                using (var stream = _fileSystem.OpenWrite(_config.CacheFilePath))
                using (var writer = new StreamWriter(stream))
                {
                    var json = ConvertToJsonRecursive(data);
                    serializer.Serialize(writer, json);
                }
            });
        }

        private static JObject ConvertToJsonRecursive(DomainSegmentTree tree)
        {
            return new JObject(
                from node in tree.Children ?? Enumerable.Empty<DomainSegmentNode>()
                select new JProperty(node.Segment, ConvertToJsonRecursive(node)));
        }
    }
}
