﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using FilesystemCrawler.Interfaces;

namespace FilesystemCrawler
{

    public class DiffStructure
    {

        public DirDiffNode Root { get; protected set; }

        public enum LocationEnum { OnBase = 1, OnLeft = 2, OnRight = 4, OnAll = 7 };

        public DiffStructure(DirectoryInfo root)
        {
            Root = new DirDiffNode(root, LocationEnum.OnAll);
        }

        public abstract class AbstractDiffNode
        {

            public FileSystemInfo Info { get; protected set; }

            public FileSystemInfo InfoBase { get; protected set; }

            public FileSystemInfo InfoLeft { get; protected set; }

            public FileSystemInfo InfoRight { get; protected set; }

            public LocationEnum Location { get; protected set; }

            public AbstractDiffNode(FileSystemInfo info, LocationEnum location)
            {
                Info = info;
                Location = location;
            }

            protected void markFound(LocationEnum location)
            {
                Location = (LocationEnum)((int)Location | (int)location);
            }

            public void AddInfoFromLocation(FileSystemInfo info, LocationEnum location) {
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
                    case LocationEnum.OnAll:
                        throw new ArgumentException("Cannot add Info from all locations at once.");
                    default:
                        throw new ArgumentException("Cannot add Info from this location.");
                }
            }

            public abstract void Accept(IDiffStructureVisitor visitor);

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

            public override void Accept(IDiffStructureVisitor visitor)
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

            public override void Accept(IDiffStructureVisitor visitor)
            {
                visitor.Visit(this);
            }
        }
    }
}
