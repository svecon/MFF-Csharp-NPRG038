using System.IO;

namespace CoreLibrary.Exceptions.NotFound
{
    /// <summary>
    /// Exception is thrown when the Remote file is not found or is not accessible.
    /// </summary>
    public class RemoteFileNotFoundException : LocationFileNotFoundException
    {
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
