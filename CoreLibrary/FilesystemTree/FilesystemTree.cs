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
                Root = new DirNode(root, location, DiffMode);
            } else
            {
                Root.AddInfoFromLocation(root, location);
            }
        }

        public void FillMissingPaths()
        {
            Root.FillMissingPaths(Root.InfoBase == null ? null : Root.InfoBase.FullName, Root.InfoLeft.FullName, Root.InfoRight.FullName);
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

            public AbstractNode(FileSystemInfo info, LocationEnum location, DiffModeEnum mode)
            {
                Mode = mode;
                AddInfoFromLocation(info, location);
            }

            public bool IsInLocation(LocationEnum location)
            {
                return (((int)location) & ((int)Location)) > 0;
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

            public DirNode(DirectoryInfo info, LocationEnum location, DiffModeEnum mode)
                : base(info, location, mode)
            {
                Directories = new List<IFilesystemTreeDirNode>();
                Files = new List<IFilesystemTreeFileNode>();
            }

            public override void Accept(IFilesystemTreeVisitor visitor)
            {
                visitor.Visit(this);
            }

            public IFilesystemTreeDirNode SearchForDir(DirectoryInfo info)
            {
                foreach (var dir in Directories)
                {
                    if (dir.Info.Name == info.Name)
                        return dir;
                }

                return null;

                // THIS WAS FOR SORTED DIRECTORIES
                //int i = 0;
                //int comparison = -1;
                //while (i < Directories.Count && (comparison = Directories[i].Info.Name.CompareTo(info.Name)) == -1)
                //{
                //    i++;
                //}

                //if (comparison == 0)
                //{
                //    return Directories[i];
                //} else
                //{
                //    return null;
                //}

            }

            public IFilesystemTreeDirNode AddDir(DirectoryInfo info, LocationEnum location)
            {
                var dirDiffNode = new DirNode(info, location, Mode);
                Directories.Add(dirDiffNode);
                return dirDiffNode;
            }

            public IFilesystemTreeFileNode SearchForFile(FileInfo info)
            {
                foreach (var file in Files)
                {
                    if (file.Info.Name == info.Name)
                        return file;
                }

                return null;


                // THIS WAS FOR SORTED FILES
                //int i = 0;
                //int comparison = -1;
                //while (i < Files.Count && (comparison = Files[i].Info.Name.CompareTo(info.Name)) == -1)
                //{
                //    i++;
                //}

                //if (comparison == 0)
                //{
                //    return Files[i];
                //} else
                //{
                //    return null;
                //}

            }

            public IFilesystemTreeFileNode AddFile(FileInfo info, LocationEnum location)
            {
                var node = new FileNode(info, location, Mode);
                Files.Add(node);
                return node;
            }

            public double GetSize()
            {
                return (double)Files.Sum(f => ((FileInfo)f.Info).Length) / 1024 + Directories.Select(f => f.GetSize()).Sum();
            }

            public void FillMissingPaths(string basePath, string leftPath, string rightPath)
            {
                if (InfoBase == null && Mode == DiffModeEnum.ThreeWay)
                    InfoBase = new FileInfo(basePath + @"\" + Info.Name);

                if (InfoLeft == null)
                    InfoLeft = new FileInfo(leftPath + @"\" + Info.Name);

                if (InfoRight == null)
                    InfoRight = new FileInfo(rightPath + @"\" + Info.Name);

                foreach (var file in Files)
                {
                    if (file.InfoBase == null && Mode == DiffModeEnum.ThreeWay)
                        file.AddInfoFromLocation(new FileInfo(InfoBase.FullName + @"\" + file.Info.Name), LocationEnum.OnBase, false);

                    if (file.InfoLeft == null)
                        file.AddInfoFromLocation(new FileInfo(InfoLeft.FullName + @"\" + file.Info.Name), LocationEnum.OnLeft, false);

                    if (file.InfoRight == null)
                        file.AddInfoFromLocation(new FileInfo(InfoRight.FullName + @"\" + file.Info.Name), LocationEnum.OnRight, false);
                }

                foreach (var dir in Directories)
                {
                    dir.FillMissingPaths(InfoBase == null ? null : InfoBase.FullName, InfoLeft.FullName, InfoRight.FullName);
                }
            }

        }

        public class FileNode : AbstractNode, IFilesystemTreeFileNode
        {
            public FileNode(FileInfo info, LocationEnum location, DiffModeEnum mode)
                : base(info, location, mode)
            {

            }

            public override void Accept(IFilesystemTreeVisitor visitor)
            {
                visitor.Visit(this);
            }
        }
    }
}
