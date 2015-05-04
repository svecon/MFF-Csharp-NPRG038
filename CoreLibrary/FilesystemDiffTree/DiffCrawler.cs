using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.FilesystemDiffTree
{
    /// <summary>
    /// DiffCrawler returns instance of FilesystemDiffTree
    /// </summary>
    public class DiffCrawler : Crawler
    {
        public DiffCrawler() : base()
        {
        }

        protected override IFilesystemTree CreateFilesystemTree(DiffModeEnum mode)
        {
            return new FilesystemDiffTree(mode);
        }
    }
}
