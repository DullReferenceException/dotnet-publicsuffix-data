using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoDaddy.PublicSuffixData.Tests
{
    [TestClass]
    public class CodeSampleTests
    {
        [TestMethod]
        public async Task SimpleCodeSampleWorks()
        {
            var dataStore = new PublicSuffixDataStore();
            var tld = await dataStore.GetTldAsync("sample.co.uk");
            Assert.AreEqual(tld, "co.uk");
        }
    }
}
