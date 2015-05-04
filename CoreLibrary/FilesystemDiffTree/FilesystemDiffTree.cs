﻿using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.FilesystemDiffTree
{
    /// <summary>
    /// FilesystemDiffTree is enhanced for some features needed in diffing between files.
    /// </summary>
    public class FilesystemDiffTree : FilesystemTree.FilesystemTree
    {
        /// <summary>
        /// Constructor for FilesystemDiffTree
        /// </summary>
        /// <param name="mode">Default Diff mode for the tree.</param>
        public FilesystemDiffTree(DiffModeEnum mode)
            : base(mode)
        {
        }

        protected override IFilesystemTreeDirNode CreateDirNode(DirectoryInfo root, LocationEnum location)
        {
            return new DirDiffNode(null, null, root, location, DiffMode);
        }
    }
}