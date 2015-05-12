using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Custom exception for handling not found directories.
    /// 
    /// Has more children - one for each Location type.
    /// </summary>
    public abstract class LocationDirectoryNotFoundException : DirectoryNotFoundException
    {
        /// <summary>
        /// Info for the directory.
        /// </summary>
        public FileSystemInfo Info;

        /// <summary>
        /// Initializes new instance of the <see cref="LocationDirectoryNotFoundException"/>
        /// </summary>
        /// <param name="info">Info for the directory.</param>
        protected LocationDirectoryNotFoundException(FileSystemInfo info)
        {
            Info = info;
        }

        public override string ToString()
        {
            return "directory '" + Info.FullName + "' not found or not readable.";
        }
    }
}
