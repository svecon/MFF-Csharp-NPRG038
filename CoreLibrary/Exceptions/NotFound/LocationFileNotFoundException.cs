using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Custom exception for handling not found directories.
    /// 
    /// Has more children - one for each Location type.
    /// </summary>
    public class LocationFileNotFoundException : FileNotFoundException
    {

        public FileSystemInfo Info;

        public LocationFileNotFoundException(FileSystemInfo info)
        {
            this.Info = info;
        }

        public override string ToString()
        {
            return "file '" + Info.FullName + "' not found or not readable.";
        }
    }
}
