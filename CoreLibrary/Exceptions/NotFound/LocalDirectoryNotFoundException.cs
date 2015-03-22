using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Exception is thrown when the Local directory is not found or is not accessible.
    /// </summary>
    public class LocalDirectoryNotFoundException : LocationDirectoryNotFoundException
    {
        public LocalDirectoryNotFoundException(FileSystemInfo info)
            : base(info)
        {
        }
        public override string ToString()
        {
            return "Local " + base.ToString();
        }
    }
}
