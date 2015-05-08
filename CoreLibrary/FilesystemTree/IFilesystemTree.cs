using System.IO;
using CoreLibrary.FilesystemTree.Enums;

namespace CoreLibrary.FilesystemTree
{
    /// <summary>
    /// Interface for a Node structure.
    /// 
    /// Hold information about filesystem trees from all locations.
    /// </summary>
    public interface INode : INodeVisitable
    {
        /// <summary>
        /// Root of the filesystem tree.
        /// </summary>
        INodeDirNode Root { get; }

        /// <summary>
        /// Diff mode for constructed tree.
        /// </summary>
        DiffModeEnum DiffMode { get; }

        /// <summary>
        /// Adds new Directory path to Root.
        /// </summary>
        /// <param name="root">DirectoryInfo for given directory.</param>
        /// <param name="location">Behave like folder from this location.</param>
        void AddDirToRoot(DirectoryInfo root, LocationEnum location);
    }
}
