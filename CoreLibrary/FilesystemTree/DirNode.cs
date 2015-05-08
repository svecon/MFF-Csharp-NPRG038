using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree.Visitors;

namespace CoreLibrary.FilesystemTree
{
    /// <summary>
    /// Directory node representing a directory in multiple locations.
    /// </summary>
    public class DirNode : AbstractNode, INodeDirNode
    {
        public List<INodeDirNode> Directories { get; protected set; }

        public List<INodeFileNode> Files { get; protected set; }

        public INodeDirNode RootNode { get; set; }

        public string RelativePath { get; set; }

        /// <summary>
        /// Initializes new instance of the <see cref="DirNode"/>
        /// </summary>
        /// <param name="rootNode">Root directory for this node.</param>
        /// <param name="relativePath">Relative path from top root directory.</param>
        /// <param name="info">Directory info for this node.</param>
        /// <param name="location">Location where this node has been found from.</param>
        /// <param name="mode">Default diff mode.</param>
        public DirNode(INodeDirNode rootNode, string relativePath, FileSystemInfo info, LocationEnum location, DiffModeEnum mode)
            : base(info, location, mode)
        {
            Directories = new List<INodeDirNode>();
            Files = new List<INodeFileNode>();

            RootNode = rootNode ?? this;

            if (relativePath == string.Empty)
                RelativePath = info.Name;
            else if (relativePath == null)
                RelativePath = string.Empty;
            else
                RelativePath = Path.Combine(relativePath, info.Name);
        }

        /// <summary>
        /// Helper method for constructing new DirNode, which allows more flexibility (for children classes).
        /// </summary>
        /// <param name="info">Directory info for the new directory node.</param>
        /// <param name="location">Location from where the dir has been found from.</param>
        /// <returns>Instance of <see cref="INodeDirNode" /></returns>
        protected virtual INodeDirNode CreateDirNode(DirectoryInfo info, LocationEnum location)
        {
            return new DirNode(RootNode, RelativePath, info, location, Mode);
        }

        /// <summary>
        /// Helper method for constructing new FileNode, which allows more flexibility (for children classes).
        /// </summary>
        /// <param name="info">Directory info for the new directory node.</param>
        /// <param name="location">Location from where the dir has been found from.</param>
        /// <returns>Instance of <see cref="INodeFileNode"/></returns>
        protected virtual INodeFileNode CreateFileNode(FileInfo info, LocationEnum location)
        {
            return new FileNode(this, info, location, Mode);
        }

        public override void Accept(IFilesystemTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public INodeDirNode SearchForDir(DirectoryInfo info)
        {
            return Directories.FirstOrDefault(dir => dir.Info.Name == info.Name);
        }

        public INodeFileNode SearchForFile(FileInfo info)
        {
            return Files.FirstOrDefault(file => file.Info.Name == info.Name);
        }

        public INodeDirNode AddDir(DirectoryInfo info, LocationEnum location)
        {
            INodeDirNode dirDiffNode = CreateDirNode(info, location);
            Directories.Add(dirDiffNode);
            return dirDiffNode;
        }

        public INodeFileNode AddFile(FileInfo info, LocationEnum location)
        {
            INodeFileNode node = CreateFileNode(info, location);
            Files.Add(node);
            return node;
        }

        public double GetSize()
        {
            return (double)Files
                .Where(f => f.Status != NodeStatusEnum.IsIgnored)
                .Sum(f => ((FileInfo)f.Info).Length)
                / 1024
                + Directories.Select(f => f.GetSize()).Sum();
        }

        public override string GetAbsolutePath(LocationEnum location)
        {
            FileSystemInfo info = null;

            switch (location)
            {
                case LocationEnum.OnBase:
                    info = RootNode.InfoBase;
                    break;
                case LocationEnum.OnLocal:
                    info = RootNode.InfoLocal;
                    break;
                case LocationEnum.OnRemote:
                    info = RootNode.InfoRemote;
                    break;
                default:
                    throw new InvalidOperationException("This path does not exist.");
            }

            return RelativePath == "" ? info.FullName : info.FullName + @"\" + RelativePath;
        }

        public IEnumerable<INodeAbstractNode> FilesAndDirectories
        {
            get
            {
                foreach (INodeDirNode filesystemTreeDirNode in Directories)
                {
                    yield return filesystemTreeDirNode;
                }

                foreach (INodeFileNode filesystemTreeFileNode in Files.Where(f => f.Status != NodeStatusEnum.IsIgnored))
                {
                    yield return filesystemTreeFileNode;
                }
            }
        }

    }
}
