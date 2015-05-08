
namespace CoreLibrary.FilesystemTree
{
    /// <summary>
    /// Interface for Crawlers who traverse directories and create Node
    /// </summary>
    public interface ICrawler
    {
        /// <summary>
        /// Initializes Crawler for 3-way Traversing.
        /// </summary>
        /// <param name="localDirPath">Path to the left directory</param>
        /// <param name="baseDirPath">Base directory (origin)</param>
        /// <param name="remoteDirPath">Path to the right directory</param>
        /// <returns>Returns itself (chaining pattern)</returns>
        ICrawler InitializeCrawler(string localDirPath, string baseDirPath, string remoteDirPath);

        /// <summary>
        /// Initializes Crawler for 3-way Traversing.
        /// </summary>
        /// <param name="localDirPath">Path to the left directory</param>
        /// <param name="remoteDirPath">Path to the right directory</param>
        /// <returns>Returns itself (chaining pattern)</returns>
        ICrawler InitializeCrawler(string localDirPath, string remoteDirPath);

        /// <summary>
        /// Traverses filesystem directories specified in constructor.
        /// Creates a filesystem tree with all files from all paths.
        /// </summary>
        /// <returns>Node with populated nodes.</returns>
        INode TraverseTree();
    }
}
