using System;
using System.IO;

namespace GoDaddy.PublicSuffixData.Internal
{
    internal interface IFileSystem
    {
        bool Exists(string filePath);
        DateTime GetLastWriteTime(string filePath);
        Stream OpenRead(string filePath);
        Stream OpenWrite(string filePath);
    }
}
