using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Interfaces;

namespace DiffIntegration.DiffFilesystemTree
{
    class DiffFilesystemTree : FilesystemTree
    {
        public DiffFilesystemTree(DiffModeEnum mode)
            : base(mode)
        {
        }

        protected override IFilesystemTreeDirNode createDirNode(DirectoryInfo root, LocationEnum location)
        {
            return new DiffDirNode(null, null, root, location, DiffMode);
        }
    }
}
