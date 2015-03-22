
using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Exception is thrown when the Base file is not found or is not accessible.
    /// </summary>
    public class BaseFileNotFoundException : LocationFileNotFoundException
    {
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
