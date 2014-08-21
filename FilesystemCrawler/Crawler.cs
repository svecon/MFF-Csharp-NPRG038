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

        DiffStructure diff;

        /// <summary>
        /// Data structure to hold Info of subfolders to be examined for Files.
        /// </summary>
        Stack<DirectoryForIteration> dirs;

        protected Crawler()
        {
            dirs = new Stack<DirectoryForIteration>(15);
        }

        public Crawler(string baseDirPath, string leftDirPath, string rightDirPath)
            : this()
        {
            if (baseDirPath == null)
            {
                diff = new DiffStructure(DiffModeEnum.TwoWay);
            } else
            {
                diff = new DiffStructure(DiffModeEnum.ThreeWay);

                #region AddingBaseDir

                baseDir = new DirectoryInfo(baseDirPath);

                if (!baseDir.Exists)
                    throw new BaseDirectoryNotFoundException(baseDir);

                diff.AddDirToRoot(baseDir, LocationEnum.OnBase);
                dirs.Push(new DirectoryForIteration(baseDir, diff.Root, LocationEnum.OnBase));

                #endregion
            }

            #region AddingLeftDir

            leftDir = new DirectoryInfo(leftDirPath);

            if (!leftDir.Exists)
                throw new LeftDirectoryNotFoundException(leftDir);

            diff.AddDirToRoot(leftDir, LocationEnum.OnLeft);
            dirs.Push(new DirectoryForIteration(leftDir, diff.Root, LocationEnum.OnLeft));

            #endregion

            #region AddingRightDir

            rightDir = new DirectoryInfo(rightDirPath);

            if (!rightDir.Exists)
                throw new RightDirectoryNotFoundException(rightDir);

            diff.AddDirToRoot(rightDir, LocationEnum.OnRight);
            dirs.Push(new DirectoryForIteration(rightDir, diff.Root, LocationEnum.OnRight));

            #endregion
        }

        public Crawler(string leftDirPath, string rightDirPath)
            : this(null, leftDirPath, rightDirPath)
        {
        }

        public DiffStructure TraverseTree()
        {
            while (dirs.Count > 0)
            {
                DirectoryForIteration currentDir = dirs.Pop();
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

                // Get first in dir (alphabetically) in our structure.


                // Push the subdirectories onto the stack for traversal. 
                // This could also be done before handing the Files.
                foreach (DirectoryInfo info in subDirs)
                {
                    DiffStructure.DirDiffNode diffNode = currentDir.ParentDiffNode.SearchForDir(info);
                    if (diffNode == null)
                    {
                        diffNode = currentDir.ParentDiffNode.AddDir(info, currentDir.Location);
                    } else
                    {
                        diffNode.AddInfoFromLocation(info, currentDir.Location);
                    }

                    dirs.Push(new DirectoryForIteration(info, diffNode, currentDir.Location));
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
                // Modify this block to perform your required task. 
                foreach (FileInfo info in files)
                {
                    try
                    {
                        DiffStructure.FileDiffNode diffNode = currentDir.ParentDiffNode.SearchForFile(info);
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

            return diff;
        }

        struct DirectoryForIteration
        {
            public DirectoryInfo Info;
            public DiffStructure.DirDiffNode ParentDiffNode;
            public LocationEnum Location;

            public DirectoryForIteration(DirectoryInfo info, DiffStructure.DirDiffNode parent, LocationEnum location)
            {
                Info = info;
                ParentDiffNode = parent;
                Location = location;
            }
        }
    }
}
