using System.IO;
using CoreLibrary.Interfaces;
using CoreLibrary.Enums;

namespace CoreLibrary.FilesystemTree
{
    /// <summary>
    /// FilesystemTree that contains information about all processed directories.
    /// 
    /// The tree is represented with Nodes that are joined as a one way linked list.
    /// 
    /// The nodes contain all information about the files location, status and more.
    /// </summary>
    public class FilesystemTree : IFilesystemTree
    {
        public DiffModeEnum DiffMode { get; protected set; }

        public IFilesystemTreeDirNode Root { get; protected set; }

        public FilesystemTree(DiffModeEnum mode)
        {
            DiffMode = mode;
        }

        public void AddDirToRoot(DirectoryInfo root, LocationEnum location)
        {
            if (Root == null)
            {
                Root = createDirNode(root, location);
            } else
            {
                Root.AddInfoFromLocation(root, location);
            }
        }

        public void Accept(IFilesystemTreeVisitor visitor)
        {
            visitor.Visit(Root);
        }

        protected virtual IFilesystemTreeDirNode createDirNode(DirectoryInfo root, LocationEnum location)
        {
            return new DirNode(null, null, root, location, DiffMode);
        }
    }
}
