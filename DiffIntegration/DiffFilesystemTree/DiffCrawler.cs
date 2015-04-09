using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Interfaces;

namespace DiffIntegration.DiffFilesystemTree
{
    /// <summary>
    /// DiffCrawler returns instance of DiffFilesystemTree
    /// </summary>
    public class DiffCrawler : Crawler
    {
        public DiffCrawler() : base()
        {
        }

        protected override IFilesystemTree CreateFilesystemTree(DiffModeEnum mode)
        {
            return new DiffFilesystemTree(mode);
        }
    }
}
