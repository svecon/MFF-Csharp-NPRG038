using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Exception is thrown when the Remote directory is not found or is not accessible.
    /// </summary>
    public class RemoteDirectoryNotFoundException : LocationDirectoryNotFoundException
    {
        /// <summary>
        /// Initializes new instance of the <see cref="RemoteDirectoryNotFoundException"/>
        /// </summary>
        /// <param name="info">Info for the directory.</param>
        public RemoteDirectoryNotFoundException(FileSystemInfo info)
            : base(info)
        {
        }

        public override string ToString()
        {
            return "Remote " + base.ToString();
        }
    }
}
