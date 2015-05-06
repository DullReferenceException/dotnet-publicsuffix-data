using System;
using System.Threading.Tasks;

namespace GoDaddy.PublicSuffixData.Internal
{
    internal interface IPublicSuffixDataSource
    {
        event EventHandler<PublicSuffixErrorEventArgs> CacheError;
        event EventHandler<PublicSuffixErrorEventArgs> DataRefreshError;

        Task<DomainSegmentTree> GetDataAsync();
    }
}
