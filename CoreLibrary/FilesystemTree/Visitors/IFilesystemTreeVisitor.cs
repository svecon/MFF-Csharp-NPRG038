
namespace CoreLibrary.FilesystemTree.Visitors
{
    /// <summary>
    /// Interface for a visitor that can traverse Node.
    /// </summary>
    public interface IFilesystemTreeVisitor
    {
        /// <summary>
        /// Visit and process a directory node.
        /// </summary>
        /// <param name="node">Node directory node</param>
        void Visit(INodeDirNode node);
        /// <summary>
        /// Visit and process a file node.
        /// </summary>
        /// <param name="node">Node file node</param>
        void Visit(INodeFileNode node);
    }
}
