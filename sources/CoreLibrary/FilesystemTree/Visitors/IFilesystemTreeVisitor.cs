
namespace CoreLibrary.FilesystemTree.Visitors
{
    /// <summary>
    /// Interface for a visitor that can traverse FilesystemTree.
    /// </summary>
    public interface IFilesystemTreeVisitor
    {
        /// <summary>
        /// Visit and process a directory FilesystemTree.
        /// </summary>
        /// <param name="node">FilesystemTree directory FilesystemTree</param>
        void Visit(INodeDirNode node);
        /// <summary>
        /// Visit and process a file FilesystemTree.
        /// </summary>
        /// <param name="node">FilesystemTree file node</param>
        void Visit(INodeFileNode node);
    }
}
