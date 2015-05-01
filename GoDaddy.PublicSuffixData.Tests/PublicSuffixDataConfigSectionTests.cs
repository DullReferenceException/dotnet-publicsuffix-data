using System;
using System.Configuration;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoDaddy.PublicSuffixData.Tests
{
    [TestClass]
    public class PublicSuffixDataConfigSectionTests
    {
        [TestMethod]
        public void PublicSuffixDataConfigSection_TimeToStale_WithDefaults_Returns10Days()
        {
            var config = (PublicSuffixDataConfigSection) ConfigurationManager.GetSection("sectionWithDefaults");

            config.TimeToStale.Should().Be(TimeSpan.FromDays(10));
        }

        [TestMethod]
        public void PublicSuffixDataConfigSection_TimeToStale_WithOverrides_ReturnsValueInFile()
        {
            var config = (PublicSuffixDataConfigSection)ConfigurationManager.GetSection("sectionWithValues");

            config.TimeToStale.Should().Be(TimeSpan.FromDays(5));
        }

        [TestMethod]
        public void PublicSuffixDataConfigSection_TimeToExpired_WithDefaults_Returns30Days()
        {
            var config = (PublicSuffixDataConfigSection)ConfigurationManager.GetSection("sectionWithDefaults");

            config.TimeToExpired.Should().Be(TimeSpan.FromDays(30));
        }

        [TestMethod]
        public void PublicSuffixDataConfigSection_TimeToExpired_WithOverrides_ReturnsValueInFile()
        {
            var config = (PublicSuffixDataConfigSection)ConfigurationManager.GetSection("sectionWithValues");

            config.TimeToExpired.Should().Be(TimeSpan.FromDays(10));
        }

        [TestMethod]
        public void PublicSuffixDataConfigSection_DataSourceUrl_WithDefaults_ReturnsOfficalUrl()
        {
            var config = (PublicSuffixDataConfigSection)ConfigurationManager.GetSection("sectionWithDefaults");

            config.DataSourceUrl.Should().Be("https://publicsuffix.org/list/effective_tld_names.dat");
        }

        [TestMethod]
        public void PublicSuffixDataConfigSection_DataSourceUrl_WithOverrides_ReturnsCustomUrl()
        {
            var config = (PublicSuffixDataConfigSection)ConfigurationManager.GetSection("sectionWithValues");

            config.DataSourceUrl.Should().Be("http://other.url/");
        }

        [TestMethod]
        public void PublicSuffixDataConfigSection_CacheFilePath_WithDefaults_ReturnsAppDataPath()
        {
            var config = (PublicSuffixDataConfigSection)ConfigurationManager.GetSection("sectionWithDefaults");

            var expectedPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ".publicsuffix.org");
            config.CacheFilePath.Should().Be(expectedPath);
        }

        [TestMethod]
        public void PublicSuffixDataConfigSection_CacheFilePath_WithOverrides_ReturnsCustomPath()
        {
            var config = (PublicSuffixDataConfigSection)ConfigurationManager.GetSection("sectionWithValues");

            config.CacheFilePath.Should().Be(@"C:\Path\To\File.json");
        }
    }
}
