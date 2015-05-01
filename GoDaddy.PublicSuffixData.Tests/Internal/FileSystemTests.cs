using System;
using System.IO;
using FluentAssertions;
using GoDaddy.PublicSuffixData.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GoDaddy.PublicSuffixData.Tests.Internal
{
    [TestClass]
    public class FileSystemTests
    {
        [TestMethod]
        public async Task FileSystem_WrappersFunctionAsExpected()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var startTime = DateTime.Now;

            try
            {
                var fs = new FileSystem();

                fs.Exists(path).Should().BeFalse();

                using (var writeStream = fs.OpenWrite(path))
                {
                    writeStream.Should().NotBeNull();
                    await writeStream.WriteAsync(new byte[] { 1, 2, 3 }, 0, 3);
                }

                fs.GetLastWriteTime(path).Should().BeAfter(startTime);

                using (var readStream = fs.OpenRead(path))
                {
                    readStream.Should().NotBeNull();

                    var buffer = new byte[3];
                    await readStream.ReadAsync(buffer, 0, 3);
                    buffer.Should().ContainInOrder((byte)1, (byte)2, (byte)3);
                }
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
