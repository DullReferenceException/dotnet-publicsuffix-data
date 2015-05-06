using System;

namespace GoDaddy.PublicSuffixData
{
    public class PublicSuffixErrorEventArgs : EventArgs
    {
        public PublicSuffixErrorEventArgs(Exception ex)
        {
            Exception = ex;
        }

        public Exception Exception { get; private set; }
    }
}
