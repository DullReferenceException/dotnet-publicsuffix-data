using System;
using System.IO;

namespace GoDaddy.PublicSuffixData.Internal
{
    public class FileSystem : IFileSystem
    {
        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        public DateTime GetLastWriteTime(string filePath)
        {
            return File.GetLastWriteTime(filePath);
        }

        public Stream OpenRead(string filePath)
        {
            return File.OpenRead(filePath);
        }

        public Stream OpenWrite(string filePath)
        {
            return File.OpenWrite(filePath);
        }
    }
}
