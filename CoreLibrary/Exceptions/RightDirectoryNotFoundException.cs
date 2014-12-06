using System.IO;

namespace CoreLibrary.Exceptions
{
    /// <summary>
    /// Exception is thrown when the Right directory is not found or is not accessible.
    /// </summary>
    public class RightDirectoryNotFoundException : LocationNotFoundException
    {
        public RightDirectoryNotFoundException(FileSystemInfo info)
            : base(info)
        {
        }
    }
}
