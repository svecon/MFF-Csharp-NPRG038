using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Exception is thrown when the Local directory is not found or is not accessible.
    /// </summary>
    public class LocalDirectoryNotFoundException : LocationDirectoryNotFoundException
    {
        /// <summary>
        /// Initializes new instance of the <see cref="LocalDirectoryNotFoundException"/>
        /// </summary>
        /// <param name="info">Info for the directory.</param>
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
