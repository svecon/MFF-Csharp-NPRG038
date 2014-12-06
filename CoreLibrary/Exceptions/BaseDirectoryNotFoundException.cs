using System.IO;

namespace CoreLibrary.Exceptions
{
    /// <summary>
    /// Exception is thrown when the Base directory is not found or is not accessible.
    /// </summary>
    public class BaseDirectoryNotFoundException : LocationNotFoundException
    {
        public BaseDirectoryNotFoundException(FileSystemInfo info)
            : base(info)
        {
        }
    }
}
