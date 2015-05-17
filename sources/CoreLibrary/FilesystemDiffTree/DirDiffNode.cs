using System.IO;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;

namespace CoreLibrary.FilesystemDiffTree
{
    /// <summary>
    /// DirDiffNode enhances DirNode for some features needed in diffing between files.
    /// </summary>
    public class DirDiffNode : DirNode
    {
        /// <summary>
        /// Initializes new instance of the <see cref="DirDiffNode"/>
        /// </summary>
        /// <param name="rootNode">Root directory for this FilesystemTree.</param>
        /// <param name="relativePath">Relative path from top root directory.</param>
        /// <param name="info">Directory info for this FilesystemTree.</param>
        /// <param name="location">Location where this FilesystemTree has been found from.</param>
        /// <param name="mode">Default diff mode.</param>
        public DirDiffNode(INodeDirNode rootNode, string relativePath, DirectoryInfo info, LocationEnum location, DiffModeEnum mode)
            : base(rootNode, relativePath, info, location, mode)
        {
        }

        /// <inheritdoc />
        protected override INodeDirNode CreateDirNode(DirectoryInfo info, LocationEnum location)
        {
            return new DirDiffNode(RootNode, RelativePath, info, location, Mode);
        }

        /// <inheritdoc />
        protected override INodeFileNode CreateFileNode(FileInfo info, LocationEnum location)
        {
            return new FileDiffNode(this, info, location, Mode);
        }
    }
}