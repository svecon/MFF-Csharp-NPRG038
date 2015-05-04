using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.FilesystemDiffTree
{
    /// <summary>
    /// DirDiffNode enhances DirNode for some features needed in diffing between files.
    /// </summary>
    public class DirDiffNode : DirNode
    {
        /// <summary>
        /// Constructor for creating DirDiffNode.
        /// </summary>
        /// <param name="rootNode">Root directory for this node.</param>
        /// <param name="relativePath">Relative path from top root directory.</param>
        /// <param name="info">Directory info for this node.</param>
        /// <param name="location">Location where this node has been found from.</param>
        /// <param name="mode">Default diff mode.</param>
        public DirDiffNode(IFilesystemTreeDirNode rootNode, string relativePath, DirectoryInfo info, LocationEnum location, DiffModeEnum mode)
            : base(rootNode, relativePath, info, location, mode)
        {
        }

        protected override IFilesystemTreeDirNode CreateDirNode(DirectoryInfo info, LocationEnum location)
        {
            return new DirDiffNode(RootNode, RelativePath, info, location, Mode);
        }

        protected override IFilesystemTreeFileNode CreateFileNode(FileInfo info, LocationEnum location)
        {
            return new FileDiffNode(this, info, location, Mode);
        }
    }
}