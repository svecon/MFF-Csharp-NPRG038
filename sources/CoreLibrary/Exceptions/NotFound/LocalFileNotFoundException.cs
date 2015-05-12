using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Exception is thrown when the Local file is not found or is not accessible.
    /// </summary>
    public class LocalFileNotFoundException : LocationFileNotFoundException
    {
        /// <summary>
        /// Initializes new instance of the <see cref="LocalFileNotFoundException"/>
        /// </summary>
        /// <param name="info">Info for the file.</param>
        public LocalFileNotFoundException(FileSystemInfo info)
            : base(info)
        {
        }

        public override string ToString()
        {
            return "Local " + base.ToString();
        }
    }
}
