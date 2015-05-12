
using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Exception is thrown when the Base file is not found or is not accessible.
    /// </summary>
    public class BaseFileNotFoundException : LocationFileNotFoundException
    {
        /// <summary>
        /// Initializes new instance of the <see cref="BaseFileNotFoundException"/>
        /// </summary>
        /// <param name="info">Info for the file.</param>
        public BaseFileNotFoundException(FileSystemInfo info)
            : base(info)
        {
        }

        public override string ToString()
        {
            return "Base " + base.ToString();
        }
    }
}
