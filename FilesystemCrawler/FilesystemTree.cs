using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using FilesystemCrawler.Interfaces;
using FilesystemCrawler.Enums;

namespace FilesystemCrawler
{

    public class FilesystemTree
    {

        public DiffModeEnum DiffMode { get; protected set; }

        public DirDiffNode Root { get; protected set; }

        public FilesystemTree(DiffModeEnum mode)
        {
            DiffMode = mode;
        }

        public FilesystemTree AddDirToRoot(DirectoryInfo root, LocationEnum location)
        {
            if (Root == null)
            {
                Root = new DirDiffNode(root, location);
            } else
            {
                Root.AddDir(root, location);
            }

            return this;
        }

        public void Accept(IFilesystemTreeVisitor visitor)
        {
            visitor.Visit(Root);
        }

        public abstract class AbstractDiffNode
        {
            public FileSystemInfo InfoBase { get; protected set; }

            public FileSystemInfo InfoLeft { get; protected set; }

            public FileSystemInfo InfoRight { get; protected set; }

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

            public AbstractDiffNode(FileSystemInfo info, LocationEnum location)
            {
                AddInfoFromLocation(info, location);
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

        public class DirDiffNode : AbstractDiffNode
        {

            public List<DirDiffNode> Directories { get; protected set; }

            public List<FileDiffNode> Files { get; protected set; }

            public DirDiffNode(DirectoryInfo info, LocationEnum location)
                : base(info, location)
            {
                Directories = new List<DirDiffNode>();
                Files = new List<FileDiffNode>();
            }

            public override void Accept(IFilesystemTreeVisitor visitor)
            {
                visitor.Visit(this);
            }

            public DirDiffNode SearchForDir(DirectoryInfo info)
            {
                int i = 0;
                int comparison = -1;
                while (i < Directories.Count && (comparison = Directories[i].Info.Name.CompareTo(info.Name)) == -1)
                {
                    i++;
                }

                if (comparison == 0)
                {
                    return Directories[i];
                } else
                {
                    return null;
                }

            }

            public DirDiffNode AddDir(DirectoryInfo info, LocationEnum location)
            {
                var dirDiffNode = new DirDiffNode(info, location);
                Directories.Add(dirDiffNode);
                return dirDiffNode;
            }

            public FileDiffNode SearchForFile(FileInfo info)
            {
                int i = 0;
                int comparison = -1;
                while (i < Files.Count && (comparison = Files[i].Info.Name.CompareTo(info.Name)) == -1)
                {
                    i++;
                }

                if (comparison == 0)
                {
                    return Files[i];
                } else
                {
                    return null;
                }

            }

            public void AddFile(FileInfo info, LocationEnum location)
            {
                Files.Add(new FileDiffNode(info, location));
            }

        }

        public class FileDiffNode : AbstractDiffNode
        {
            public FileDiffNode(FileInfo info, LocationEnum location)
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
