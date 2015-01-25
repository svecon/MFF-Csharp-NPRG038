using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;

namespace CoreLibrary.FilesystemTree
{
    public class DirNode : AbstractNode, IFilesystemTreeDirNode
    {

        public List<IFilesystemTreeDirNode> Directories { get; protected set; }

        public List<IFilesystemTreeFileNode> Files { get; protected set; }

        public IFilesystemTreeDirNode RootNode { get; set; }

        public string RelativePath { get; set; }

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

        public override void Accept(IFilesystemTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IFilesystemTreeDirNode SearchForDir(DirectoryInfo info)
        {
            //TODO maybe this could be done faster? (now it is N^2)

            //foreach (var dir in Directories)
            //{
            //    if (dir.Info.Name == info.Name)
            //        return dir;
            //}
            //return null;

            return Directories.FirstOrDefault(dir => dir.Info.Name == info.Name);
        }

        public IFilesystemTreeFileNode SearchForFile(FileInfo info)
        {
            //TODO maybe this could be done faster? (now it is N^2)

            //foreach (var file in Files)
            //{
            //    if (file.Info.Name == info.Name)
            //        return file;
            //}
            //return null;

            return Files.FirstOrDefault(file => file.Info.Name == info.Name);
        }

        public IFilesystemTreeDirNode AddDir(DirectoryInfo info, LocationEnum location)
        {
            var dirDiffNode = createDirNode(info, location);
            Directories.Add(dirDiffNode);
            return dirDiffNode;
        }

        protected virtual IFilesystemTreeDirNode createDirNode(DirectoryInfo info, LocationEnum location)
        {
            return new DirNode(RootNode, RelativePath, info, location, Mode);
        }

        public IFilesystemTreeFileNode AddFile(FileInfo info, LocationEnum location)
        {
            var node = createFileNode(info, location);
            Files.Add(node);
            return node;
        }

        protected virtual IFilesystemTreeFileNode createFileNode(FileInfo info, LocationEnum location)
        {
            return new FileNode(this, info, location, Mode);
        }

        public double GetSize()
        {
            return (double)Files.Sum(f => ((FileInfo)f.Info).Length) / 1024 + Directories.Select(f => f.GetSize()).Sum();
        }

        public override string GetAbsolutePath(LocationEnum location)
        {
            FileSystemInfo info = null;

            switch (location)
            {
                case LocationEnum.OnBase:
                    info = RootNode.InfoBase;
                    break;
                case LocationEnum.OnLeft:
                    info = RootNode.InfoLeft;
                    break;
                case LocationEnum.OnRight:
                    info = RootNode.InfoRight;
                    break;
            }

            if (info == null)
                throw new InvalidOperationException("This path does not exist.");

            return RelativePath == "" ? info.FullName : info.FullName + @"\" + RelativePath;
        }

    }
}