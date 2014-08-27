using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using CoreLibrary.Interfaces;
using CoreLibrary.Enums;

namespace CoreLibrary.FilesystemTree
{

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
                Root = new DirNode(null, null, root, location, DiffMode);
            } else
            {
                Root.AddInfoFromLocation(root, location);
            }
        }

        public void Accept(IFilesystemTreeVisitor visitor)
        {
            visitor.Visit(Root);
        }

        public abstract class AbstractNode : IFilesystemTreeAbstractNode
        {
            public FileSystemInfo InfoBase { get; protected set; }

            public FileSystemInfo InfoLeft { get; protected set; }

            public FileSystemInfo InfoRight { get; protected set; }

            public NodeStatusEnum Status { get; set; }

            public DifferencesStatusEnum Differences { get; set; }

            /// <summary>
            /// Returns first (out of Base, Left or RightInfo) FileSystemInfo that is not null.
            /// </summary>
            public FileSystemInfo Info
            {
                get
                {
                    if (InfoBase != null)
                    {
                        return InfoBase;
                    } else if (InfoLeft != null)
                    {
                        return InfoLeft;
                    } else // if (InfoRight != null)
                    {
                        return InfoRight;
                    }
                }
            }

            public int Location { get; protected set; }

            public DiffModeEnum Mode { get; protected set; }

            public abstract string GetAbsolutePath(LocationEnum location);

            public AbstractNode(FileSystemInfo info, LocationEnum location, DiffModeEnum mode)
            {
                Mode = mode;
                AddInfoFromLocation(info, location);
            }

            public bool IsInLocation(LocationEnum location)
            {
                return ((int)location & Location) > 0;
            }

            protected void markFound(LocationEnum location)
            {
                Location = Location | (int)location;
            }

            public void AddInfoFromLocation(FileSystemInfo info, LocationEnum location, bool markIsFound = true)
            {
                if (markIsFound)
                    markFound(location);

                switch (location)
                {
                    case LocationEnum.OnBase:
                        InfoBase = info;
                        break;
                    case LocationEnum.OnLeft:
                        InfoLeft = info;
                        break;
                    case LocationEnum.OnRight:
                        InfoRight = info;
                        break;
                    default:
                        throw new ArgumentException("Cannot add Info from this location.");
                }
            }

            public abstract void Accept(IFilesystemTreeVisitor visitor);

        }

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

                RootNode = rootNode == null ? this : rootNode;

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

                foreach (var dir in Directories)
                {
                    if (dir.Info.Name == info.Name)
                        return dir;
                }

                return null;
            }

            public IFilesystemTreeFileNode SearchForFile(FileInfo info)
            {
                //TODO maybe this could be done faster? (now it is N^2)

                foreach (var file in Files)
                {
                    if (file.Info.Name == info.Name)
                        return file;
                }

                return null;
            }

            public IFilesystemTreeDirNode AddDir(DirectoryInfo info, LocationEnum location)
            {
                var dirDiffNode = new DirNode(RootNode, RelativePath, info, location, Mode);
                Directories.Add(dirDiffNode);
                return dirDiffNode;
            }

            public IFilesystemTreeFileNode AddFile(FileInfo info, LocationEnum location)
            {
                var node = new FileNode(this, info, location, Mode);
                Files.Add(node);
                return node;
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

        public class FileNode : AbstractNode, IFilesystemTreeFileNode
        {
            public IFilesystemTreeDirNode ParentNode { get; set; }

            public FileNode(IFilesystemTreeDirNode parentNode, FileInfo info, LocationEnum location, DiffModeEnum mode)
                : base(info, location, mode)
            {
                ParentNode = parentNode;
            }

            public override void Accept(IFilesystemTreeVisitor visitor)
            {
                visitor.Visit(this);
            }

            public override string GetAbsolutePath(LocationEnum location)
            {
                return ParentNode.GetAbsolutePath(location) + @"\" + Info.Name;
            }
        }
    }
}
