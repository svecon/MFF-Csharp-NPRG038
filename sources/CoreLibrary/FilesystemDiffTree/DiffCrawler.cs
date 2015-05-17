using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;

namespace CoreLibrary.FilesystemDiffTree
{
    /// <summary>
    /// DiffCrawler returns instance of FilesystemDiffTree
    /// </summary>
    public class DiffCrawler : Crawler
    {
        /// <inheritdoc />
        protected override IFilesystemTree CreateFilesystemTree(DiffModeEnum mode)
        {
            return new FilesystemDiffTree(mode);
        }
    }
}
