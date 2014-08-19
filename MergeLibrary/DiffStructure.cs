using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace MergeLibrary {
    public class DiffStructure {

        public DirDiffNode Root { get; protected set; }

        public enum LocationEnum { OnLeft = 1, OnRight = 2, OnBoth = 3 };

        public DiffStructure(DirectoryInfo root)
        {
            Root = new DirDiffNode(root, LocationEnum.OnBoth);
        }

        public abstract class AbstractDiffNode {

            public FileSystemInfo Info { get; protected set; }

            public LocationEnum Location;

            public AbstractDiffNode(FileSystemInfo info, LocationEnum location)
            {
                Info = info;
                Location = location;
            }

            public void MarkFound(LocationEnum location)
            {
                Location = (LocationEnum)((int)Location | (int)location);
            }

            public abstract void Print();

        }

        public class DirDiffNode : AbstractDiffNode {

            List<DirDiffNode> directories;

            List<FileDiffNode> files;

            public DirDiffNode(DirectoryInfo info, LocationEnum location)
                : base(info, location)
            {
                directories = new List<DirDiffNode>();
                files = new List<FileDiffNode>();
            }

            public DirDiffNode SearchForDir(DirectoryInfo info)
            {
                int i = 0;
                int comparison = -1;
                while (i < directories.Count && (comparison = directories[i].Info.Name.CompareTo(info.Name)) == -1)
                {
                    i++;
                }

                if (comparison == 0)
                {
                    return directories[i];
                }
                else
                {
                    return null;
                }

            }

            public void AddDir(DirectoryInfo info, LocationEnum location)
            {
                directories.Add(new DirDiffNode(info, location));
            }

            public FileDiffNode SearchForFile(FileInfo info)
            {
                int i = 0;
                int comparison = -1;
                while (i < files.Count && (comparison = files[i].Info.Name.CompareTo(info.Name)) == -1)
                {
                    i++;
                }

                if (comparison == 0)
                {
                    return files[i];
                }
                else
                {
                    return null;
                }

            }

            public void AddFile(FileInfo info, LocationEnum location)
            {
                files.Add(new FileDiffNode(info, location));
            }

            public override void Print()
            {
                Console.WriteLine("===");

                Console.WriteLine(Info.FullName);

                foreach (var item in files)
                {
                    item.Print();
                }

                Console.WriteLine("---");

                foreach (var item in directories)
                {
                    item.Print();
                }

            }
        }

        public class FileDiffNode : AbstractDiffNode {
            public FileDiffNode(FileInfo info, LocationEnum location)
                : base(info, location)
            {

            }

            public override void Print()
            {
                Console.WriteLine(Info.FullName + ": " + Location);
            }
        }
    }
}
