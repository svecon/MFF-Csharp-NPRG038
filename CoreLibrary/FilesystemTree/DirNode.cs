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
    public class DirNode : AbstractNode, IFilesystemTreeDirNode
    {

        public List<IFilesystemTreeDirNode> Directories { get; protected set; }

        public List<IFilesystemTreeFileNode> Files { get; protected set; }

        public IFilesystemTreeDirNode RootNode { get; set; }

        public string RelativePath { get; set; }

        /// <summary>
        /// Constructor for creating DirNode.
        /// </summary>
        /// <param name="rootNode">Root directory for this node.</param>
        /// <param name="relativePath">Relative path from top root directory.</param>
        /// <param name="info">Directory info for this node.</param>
        /// <param name="location">Location where this node has been found from.</param>
        /// <param name="mode">Default diff mode.</param>
        public DirNode(IFilesystemTreeDirNode rootNode, string relativePath, DirectoryInfo info, LocationEnum location, DiffModeEnum mode)
            : base(info, location, mode)
        {
            Directories = new List<IFilesystemTreeDirNode>();
            Files = new List<IFilesystemTreeFileNode>();

            RootNode = rootNode ?? this;

            if (relativePath == "")
                RelativePath = info.Name;
            else if (relativePath == null)
                RelativePath = "";
            else
                RelativePath = relativePath + @"\" + info.Name;
        }

        /// <summary>
        /// Helper method for constructing new DirNode, which allows more flexibility (for children classes).
        /// </summary>
        /// <param name="info">Directory info for the new directory node.</param>
        /// <param name="location">Location from where the dir has been found from.</param>
        /// <returns>new IFilesystemTreeDirNode</returns>
        protected virtual IFilesystemTreeDirNode CreateDirNode(DirectoryInfo info, LocationEnum location)
        {
            return new DirNode(RootNode, RelativePath, info, location, Mode);
        }

        /// <summary>
        /// Helper method for constructing new FileNode, which allows more flexibility (for children classes).
        /// </summary>
        /// <param name="info">Directory info for the new directory node.</param>
        /// <param name="location">Location from where the dir has been found from.</param>
        /// <returns>new IFilesystemTreeFileNode</returns>
        protected virtual IFilesystemTreeFileNode CreateFileNode(FileInfo info, LocationEnum location)
        {
            return new FileNode(this, info, location, Mode);
        }

        public override void Accept(IFilesystemTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IFilesystemTreeDirNode SearchForDir(DirectoryInfo info)
        {
            //TODO maybe this could be done faster?

            return Directories.FirstOrDefault(dir => dir.Info.Name == info.Name);
        }

        public IFilesystemTreeFileNode SearchForFile(FileInfo info)
        {
            //TODO maybe this could be done faster?

            return Files.FirstOrDefault(file => file.Info.Name == info.Name);
        }

        public IFilesystemTreeDirNode AddDir(DirectoryInfo info, LocationEnum location)
        {
            IFilesystemTreeDirNode dirDiffNode = CreateDirNode(info, location);
            Directories.Add(dirDiffNode);
            return dirDiffNode;
        }

        public IFilesystemTreeFileNode AddFile(FileInfo info, LocationEnum location)
        {
            IFilesystemTreeFileNode node = CreateFileNode(info, location);
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

                foreach (IFilesystemTreeFileNode filesystemTreeFileNode in Files.Where(f => f.Status != NodeStatusEnum.IsIgnored))
                {
                    yield return filesystemTreeFileNode;
                }
            }
        }

    }
}