using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Exception is thrown when the Remote file is not found or is not accessible.
    /// </summary>
    public class RemoteFileNotFoundException : LocationFileNotFoundException
    {
        /// <summary>
        /// Initializes new instance of the <see cref="RemoteFileNotFoundException"/>
        /// </summary>
        /// <param name="info">Info for the file.</param>
        public RemoteFileNotFoundException(FileSystemInfo info)
            : base(info)
        {
        }

        public override string ToString()
        {
            return "Remote " + base.ToString();
        }
    }
}
