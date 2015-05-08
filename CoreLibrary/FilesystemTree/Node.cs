using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree.Visitors;

namespace CoreLibrary.FilesystemTree
{
    /// <summary>
    /// Node that contains information about all processed directories.
    /// 
    /// The tree is represented with Nodes that are joined as a one way linked list.
    /// 
    /// The nodes contain all information about the files location, status and more.
    /// </summary>
    public class Node : INode
    {
        public DiffModeEnum DiffMode { get; protected set; }

        public INodeDirNode Root { get; protected set; }

        /// <summary>
        /// Initializes new instance of the <see cref="Node"/>
        /// </summary>
        /// <param name="mode">Mode for the comparison</param>
        public Node(DiffModeEnum mode)
        {
            DiffMode = mode;
        }

        public void AddDirToRoot(DirectoryInfo root, LocationEnum location)
        {
            if (Root == null)
            {
                Root = CreateDirNode(root, location);
            } else
            {
                Root.AddInfoFromLocation(root, location);
            }
        }

        public void Accept(IFilesystemTreeVisitor visitor)
        {
            visitor.Visit(Root);
        }

        /// <summary>
        /// A virtual method for creating <see cref="INodeDirNode"/>
        /// </summary>
        /// <param name="info">Info of the directory</param>
        /// <param name="location">Location of the directory</param>
        /// <returns>An instance of <see cref="INodeDirNode"/></returns>
        protected virtual INodeDirNode CreateDirNode(DirectoryInfo info, LocationEnum location)
        {
            return new DirNode(null, null, info, location, DiffMode);
        }
    }
}
