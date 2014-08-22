using System;
using System.Collections.Generic;
using System.IO;
using FilesystemCrawler.Enums;
using FilesystemCrawler.Exceptions;

namespace FilesystemCrawler
{

    public class Crawler
    {

        DirectoryInfo baseDir;

        DirectoryInfo leftDir;

        DirectoryInfo rightDir;

        /// <summary>
        /// FilesystemDiff represents all found files in a tree structure.
        /// </summary>
        public FilesystemTree FilesystemDiff { get; protected set; }

        /// <summary>
        /// Data structure to hold Info of subfolders to be examined for Files.
        /// </summary>
        Stack<DirectoryForIteration> dirsToBeSearched;

        /// <summary>
        /// Create crawler for 3-way diffing.
        /// </summary>
        /// <param name="baseDirPath">Base directory (origin)</param>
        /// <param name="leftDirPath">Path to the left directory</param>
        /// <param name="rightDirPath">Path to the right directory</param>
        public Crawler(string baseDirPath, string leftDirPath, string rightDirPath)
        {
            dirsToBeSearched = new Stack<DirectoryForIteration>(15);

            if (baseDirPath == null)
            {
                FilesystemDiff = new FilesystemTree(DiffModeEnum.TwoWay);
            } else
            {
                FilesystemDiff = new FilesystemTree(DiffModeEnum.ThreeWay);

                #region AddingBaseDir

                baseDir = new DirectoryInfo(baseDirPath);

                if (!baseDir.Exists)
                    throw new BaseDirectoryNotFoundException(baseDir);

                FilesystemDiff.AddDirToRoot(baseDir, LocationEnum.OnBase);
                dirsToBeSearched.Push(new DirectoryForIteration(baseDir, FilesystemDiff.Root, LocationEnum.OnBase));

                #endregion
            }

            #region AddingLeftDir

            leftDir = new DirectoryInfo(leftDirPath);

            if (!leftDir.Exists)
                throw new LeftDirectoryNotFoundException(leftDir);

            FilesystemDiff.AddDirToRoot(leftDir, LocationEnum.OnLeft);
            dirsToBeSearched.Push(new DirectoryForIteration(leftDir, FilesystemDiff.Root, LocationEnum.OnLeft));

            #endregion

            #region AddingRightDir

            rightDir = new DirectoryInfo(rightDirPath);

            if (!rightDir.Exists)
                throw new RightDirectoryNotFoundException(rightDir);

            FilesystemDiff.AddDirToRoot(rightDir, LocationEnum.OnRight);
            dirsToBeSearched.Push(new DirectoryForIteration(rightDir, FilesystemDiff.Root, LocationEnum.OnRight));

            #endregion
        }

        /// <summary>
        /// Create crawler for 2-way diffing.
        /// </summary>
        /// <param name="leftDirPath">Path to the left directory</param>
        /// <param name="rightDirPath">Path to the right directory</param>
        public Crawler(string leftDirPath, string rightDirPath)
            : this(null, leftDirPath, rightDirPath)
        {
        }

        /// <summary>
        /// Traverses filesystem directories specified in constructor.
        /// Creates a filesystem tree with all files from all paths.
        /// </summary>
        /// <returns></returns>
        public FilesystemTree TraverseTree()
        {
            while (dirsToBeSearched.Count > 0)
            {
                DirectoryForIteration currentDir = dirsToBeSearched.Pop();
                DirectoryInfo[] subDirs;
                try
                {
                    subDirs = currentDir.Info.GetDirectories();
                }
                    // An UnauthorizedAccessException exception will be thrown if we do not have 
                    // discovery permission on a folder or file. It may or may not be acceptable  
                    // to ignore the exception and continue enumerating the remaining Files and  
                    // folders. It is also possible (but unlikely) that a DirectoryNotFound exception  
                    // will be raised. This will happen if currentDir has been deleted by 
                    // another application or thread after our call to Directory.Exists. The  
                    // choice of which exceptions to catch depends entirely on the specific task  
                    // you are intending to perform and also on how much you know with certainty  
                    // about the systems on which this code will run. 
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                } catch (System.IO.DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                // Push the subdirectories onto the stack for traversal. 
                foreach (DirectoryInfo info in subDirs)
                {
                    FilesystemTree.DirNode diffNode = currentDir.ParentDiffNode.SearchForDir(info);
                    if (diffNode == null)
                    {
                        diffNode = currentDir.ParentDiffNode.AddDir(info, currentDir.Location);
                    } else
                    {
                        diffNode.AddInfoFromLocation(info, currentDir.Location);
                    }

                    dirsToBeSearched.Push(new DirectoryForIteration(info, diffNode, currentDir.Location));
                }

                FileInfo[] files = null;
                try
                {
                    files = currentDir.Info.GetFiles();
                } catch (UnauthorizedAccessException e)
                {

                    Console.WriteLine(e.Message);
                    continue;
                } catch (System.IO.DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                // Perform the required action on each file here. 
                foreach (FileInfo info in files)
                {
                    try
                    {
                        FilesystemTree.FileNode diffNode = currentDir.ParentDiffNode.SearchForFile(info);
                        if (diffNode == null)
                        {
                            currentDir.ParentDiffNode.AddFile(info, currentDir.Location);
                        } else
                        {
                            diffNode.AddInfoFromLocation(info, currentDir.Location);
                        }
                    } catch (FileNotFoundException e)
                    {
                        // If file was deleted by a separate application 
                        //  or thread since the call to TraverseTree() 
                        // then just continue.
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }
            }

            return FilesystemDiff;
        }

        /// <summary>
        /// Struct that holds all needed information for filesystem traversal:
        /// DirectoryInfo, Pointer to a tree node, Path Location.
        /// </summary>
        struct DirectoryForIteration
        {
            public DirectoryInfo Info;
            public FilesystemTree.DirNode ParentDiffNode;
            public LocationEnum Location;

            public DirectoryForIteration(DirectoryInfo info, FilesystemTree.DirNode parent, LocationEnum location)
            {
                Info = info;
                ParentDiffNode = parent;
                Location = location;
            }
        }
    }
}
