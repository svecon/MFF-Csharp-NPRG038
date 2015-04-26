
namespace CoreLibrary.FilesystemTree
{
    /// <summary>
    /// File node of FilesystemTree
    /// </summary>
    public interface IFilesystemTreeFileNode : IFilesystemTreeAbstractNode
    {
        /// <summary>
        /// Reference to the parent directory node.
        /// </summary>
        IFilesystemTreeDirNode ParentNode { get; }
    }
}
