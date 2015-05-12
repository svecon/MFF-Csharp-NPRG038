using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Exception is thrown when the Base directory is not found or is not accessible.
    /// </summary>
    public class BaseDirectoryNotFoundException : LocationDirectoryNotFoundException
    {
        /// <summary>
        /// Initializes new instance of the <see cref="BaseDirectoryNotFoundException"/>
        /// </summary>
        /// <param name="info">Info for the directory.</param>
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
