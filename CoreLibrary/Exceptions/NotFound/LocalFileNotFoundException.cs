using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Exception is thrown when the Local file is not found or is not accessible.
    /// </summary>
    public class LocalFileNotFoundException : LocationFileNotFoundException
    {
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
