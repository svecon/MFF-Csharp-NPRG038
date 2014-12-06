using System.IO;

namespace CoreLibrary.Exceptions
{
    /// <summary>
    /// Exception is thrown when the Left directory is not found or is not accessible.
    /// </summary>
    public class LeftDirectoryNotFoundException : LocationNotFoundException
    {
        public LeftDirectoryNotFoundException(FileSystemInfo info)
            : base(info)
        {
        }
    }
}
