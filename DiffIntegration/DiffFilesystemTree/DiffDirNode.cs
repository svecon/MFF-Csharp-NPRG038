using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Interfaces;

namespace DiffIntegration.DiffFilesystemTree
{
    /// <summary>
    /// DiffDirNode enhances DirNode for some features needed in diffing between files.
    /// </summary>
    public class DiffDirNode : DirNode
    {

        /// <summary>
        /// Constructor for creating DiffDirNode.
        /// </summary>
        /// <param name="rootNode">Root directory for this node.</param>
        /// <param name="relativePath">Relative path from top root directory.</param>
        /// <param name="info">Directory info for this node.</param>
        /// <param name="location">Location where this node has been found from.</param>
        /// <param name="mode">Default diff mode.</param>
        public DiffDirNode(IFilesystemTreeDirNode rootNode, string relativePath, DirectoryInfo info, LocationEnum location, DiffModeEnum mode)
            : base(rootNode, relativePath, info, location, mode)
        {
        }

        protected override IFilesystemTreeDirNode CreateDirNode(DirectoryInfo info, LocationEnum location)
        {
            return new DiffDirNode(RootNode, RelativePath, info, location, Mode);
        }

        protected override IFilesystemTreeFileNode CreateFileNode(FileInfo info, LocationEnum location)
        {
            return new DiffFileNode(this, info, location, Mode);
        }


        /// <summary>
        /// TODO: ADD TO INTERFACE? VYMYSLET
        /// </summary>
        public IEnumerable<IFilesystemTreeAbstractNode> FilesAndDirectories
        {
            get
            {
                foreach (IFilesystemTreeDirNode filesystemTreeDirNode in Directories)
                {
                    yield return filesystemTreeDirNode;
                }

                foreach (IFilesystemTreeFileNode filesystemTreeFileNode in !FilterIgnored ? Files : Files.Where(f => f.Status != NodeStatusEnum.IsIgnored))
                {
                    yield return filesystemTreeFileNode;
                }
            }
        }

        public static bool FilterIgnored = false;
    }
}