using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Interfaces;

namespace DiffIntegration.DiffFilesystemTree
{
    /// <summary>
    /// DiffFilesystemTree is enhanced for some features needed in diffing between files.
    /// </summary>
    public class DiffFilesystemTree : FilesystemTree
    {
        /// <summary>
        /// Constructor for DiffFilesystemTree
        /// </summary>
        /// <param name="mode">Default Diff mode for the tree.</param>
        public DiffFilesystemTree(DiffModeEnum mode)
            : base(mode)
        {
        }

        protected override IFilesystemTreeDirNode CreateDirNode(DirectoryInfo root, LocationEnum location)
        {
            return new DiffDirNode(null, null, root, location, DiffMode);
        }
    }
}
