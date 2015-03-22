using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Custom exception for handling not found directories.
    /// 
    /// Has more children - one for each Location type.
    /// </summary>
    public class LocationDirectoryNotFoundException : DirectoryNotFoundException
    {

        public FileSystemInfo Info;

        public LocationDirectoryNotFoundException(FileSystemInfo info)
        {
            this.Info = info;
        }

        public override string ToString()
        {
            return "directory '" + Info.FullName + "' not found or not readable.";
        }
    }
}
