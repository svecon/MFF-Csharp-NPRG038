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
        /// <summary>
        /// Location for the file.
        /// </summary>
        public FileSystemInfo Info;

        /// <summary>
        /// Initializes new instance of the <see cref="LocationFileNotFoundException"/>
        /// </summary>
        /// <param name="info">Info for the file.</param>
        public LocationFileNotFoundException(FileSystemInfo info)
        {
            Info = info;
        }

        public override string ToString()
        {
            return "file '" + Info.FullName + "' not found or not readable.";
        }
    }
}
