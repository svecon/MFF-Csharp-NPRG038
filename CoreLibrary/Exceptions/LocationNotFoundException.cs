using System.IO;

namespace CoreLibrary.Exceptions
{
    /// <summary>
    /// Custom exception for handling not found directories.
    /// 
    /// Has more children - one for each Location type.
    /// </summary>
    public class LocationNotFoundException : DirectoryNotFoundException
    {

        protected FileSystemInfo Info;

        public LocationNotFoundException(FileSystemInfo info)
        {
            this.Info = info;
        }

        public override string ToString()
        {
            return "Directory: " + Info.FullName + "not found or not readable.";
        }
    }
}
