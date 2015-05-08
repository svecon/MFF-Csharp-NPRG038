
namespace CoreLibrary.FilesystemTree
{
    /// <summary>
    /// File FilesystemTree of FilesystemTree
    /// </summary>
    public interface INodeFileNode : INodeAbstractNode
    {
        /// <summary>
        /// Reference to the parent directory FilesystemTree.
        /// </summary>
        INodeDirNode ParentNode { get; }
    }
}
