using System.Threading.Tasks;

namespace GoDaddy.PublicSuffixData.Internal
{
    internal interface IPublicSuffixDataSource
    {
        Task<DomainSegmentTree> GetDataAsync();
    }
}
