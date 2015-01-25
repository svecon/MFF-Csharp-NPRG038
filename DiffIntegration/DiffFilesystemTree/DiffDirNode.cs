using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Interfaces;

namespace DiffIntegration.DiffFilesystemTree
{
    public class DiffDirNode : DirNode
    {

        public DiffDirNode(IFilesystemTreeDirNode rootNode, string relativePath, DirectoryInfo info, LocationEnum location, DiffModeEnum mode)
            : base(rootNode, relativePath, info, location, mode)
        {
        }

        protected override IFilesystemTreeDirNode createDirNode(DirectoryInfo info, LocationEnum location)
        {
            return new DiffDirNode(RootNode, RelativePath, info, location, Mode);
        }

        protected override IFilesystemTreeFileNode createFileNode(FileInfo info, LocationEnum location)
        {
            return new DiffFileNode(this, info, location, Mode);
        }
    }
}