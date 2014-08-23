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

        public DirNode Root { get; protected set; }

        public FilesystemTree(DiffModeEnum mode)
        {
            DiffMode = mode;
        }

        public FilesystemTree AddDirToRoot(DirectoryInfo root, LocationEnum location)
        {
            if (Root == null)
            {
                Root = new DirNode(root, location);
            } else
            {
                Root.AddInfoFromLocation(root, location);
            }

            return this;
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

            public NodeStatus Status { get; set; }

            public DifferencesStatus Differences { get; set; }

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

            public LocationEnum Location { get; protected set; }

            public AbstractNode(FileSystemInfo info, LocationEnum location)
            {
                AddInfoFromLocation(info, location);
            }

            public bool IsInLocation(LocationEnum location)
            {
                return (((int)location) & ((int)Location)) > 0;
            }

            protected void markFound(LocationEnum location)
            {
                Location = (LocationEnum)((int)Location | (int)location);
            }

            public void AddInfoFromLocation(FileSystemInfo info, LocationEnum location)
            {
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

            public DirNode(DirectoryInfo info, LocationEnum location)
                : base(info, location)
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
                var dirDiffNode = new DirNode(info, location);
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

            public void AddFile(FileInfo info, LocationEnum location)
            {
                Files.Add(new FileNode(info, location));
            }

        }

        public class FileNode : AbstractNode, IFilesystemTreeFileNode
        {
            public FileNode(FileInfo info, LocationEnum location)
                : base(info, location)
            {

            }

            public override void Accept(IFilesystemTreeVisitor visitor)
            {
                visitor.Visit(this);
            }
        }
    }
}
