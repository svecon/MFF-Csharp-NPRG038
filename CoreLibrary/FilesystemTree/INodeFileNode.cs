
namespace CoreLibrary.FilesystemTree
{
    /// <summary>
    /// File node of Node
    /// </summary>
    public interface INodeFileNode : INodeAbstractNode
    {
        /// <summary>
        /// Reference to the parent directory node.
        /// </summary>
        INodeDirNode ParentNode { get; }
    }
}
