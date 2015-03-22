using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Exception is thrown when the Base directory is not found or is not accessible.
    /// </summary>
    public class BaseDirectoryNotFoundException : LocationDirectoryNotFoundException
    {
        public BaseDirectoryNotFoundException(FileSystemInfo info)
            : base(info)
        {
        }

        public override string ToString()
        {
            return "Base " + base.ToString();
        }
    }
}
