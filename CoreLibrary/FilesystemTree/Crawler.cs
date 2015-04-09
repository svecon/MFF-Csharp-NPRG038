using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.Exceptions.NotFound;
using CoreLibrary.Interfaces;

namespace CoreLibrary.FilesystemTree
{
    /// <summary>
    /// Crawler traverses given directories (2 or 3) 
    /// and builds a IFilesystemTree with files and directories as Nodes.
    /// 
    /// //TODO maybe try to parallelize the crawling?
    /// </summary>
    public class Crawler : ICrawler
    {
        /// <summary>
        /// Struct that holds all needed information for filesystem traversal:
        /// DirectoryInfo, Pointer to a tree node, Path Location.
        /// </summary>
        private struct DirectoryForIteration
        {
            public readonly DirectoryInfo Info;
            public readonly IFilesystemTreeDirNode ParentDiffNode;
            public readonly LocationEnum Location;

            public DirectoryForIteration(DirectoryInfo info, IFilesystemTreeDirNode parent, LocationEnum location)
            {
                Info = info;
                ParentDiffNode = parent;
                Location = location;
            }
        }

        /// <summary>
        /// FilesystemTree represents all found files in a tree structure.
        /// </summary>
        protected IFilesystemTree FilesystemTree;

        /// <summary>
        /// Data structure to hold Info of subfolders to be examined for Files.
        /// </summary>
        readonly Stack<DirectoryForIteration> dirsToBeSearched;

        /// <summary>
        /// Create crawler that can traverse directories.
        /// </summary>
        public Crawler()
        {
            dirsToBeSearched = new Stack<DirectoryForIteration>(15);
        }

        public ICrawler InitializeCrawler(string localDirPath, string remoteDirPath)
        {
            dirsToBeSearched.Clear();

            FilesystemTree = CreateFilesystemTree(DiffModeEnum.TwoWay);

            AddDirToRoot(localDirPath, LocationEnum.OnLocal);
            AddDirToRoot(remoteDirPath, LocationEnum.OnRemote);

            return this;
        }

        public ICrawler InitializeCrawler(string localDirPath, string baseDirPath, string remoteDirPath)
        {
            dirsToBeSearched.Clear();

            FilesystemTree = CreateFilesystemTree(DiffModeEnum.ThreeWay);

            AddDirToRoot(localDirPath, LocationEnum.OnLocal);
            AddDirToRoot(baseDirPath, LocationEnum.OnBase);
            AddDirToRoot(remoteDirPath, LocationEnum.OnRemote);

            return this;
        }

        /// <summary>
        /// Virtual creating of FilesystemTree to allow instantiation of different type of trees.
        /// </summary>
        /// <param name="mode">Mode to instantiate the tree with</param>
        /// <returns>Empty FilesystemTree container</returns>
        protected virtual IFilesystemTree CreateFilesystemTree(DiffModeEnum mode)
        {
            return new FilesystemTree(mode);
        }

        /// <summary>
        /// Helper method that initializes crawler and adds root directories to be searched further
        /// </summary>
        /// <param name="dirPath">Path to root directory</param>
        /// <param name="location">Location of the directory</param>
        private void AddDirToRoot(string dirPath, LocationEnum location)
        {
            var dir = new DirectoryInfo(dirPath);

            if (!dir.Exists)
                switch (location)
                {
                    case LocationEnum.OnBase:
                        throw new BaseDirectoryNotFoundException(dir);
                    case LocationEnum.OnLocal:
                        throw new LocalDirectoryNotFoundException(dir);
                    case LocationEnum.OnRemote:
                        throw new RemoteDirectoryNotFoundException(dir);
                }

            FilesystemTree.AddDirToRoot(dir, location);
            dirsToBeSearched.Push(new DirectoryForIteration(dir, FilesystemTree.Root, location));
        }

        public IFilesystemTree TraverseTree()
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
                    // discovery permission on a folder or file.
                catch (UnauthorizedAccessException e)
                {
                    Debug.WriteLine(e.Message);
                    continue;
                    // It is also possible (but unlikely) that a DirectoryNotFound exception  
                    // will be raised. This will happen if currentDir has been deleted by 
                    // another application or thread after our call to Directory.Exists.
                } catch (DirectoryNotFoundException e)
                {
                    Debug.WriteLine(e.Message);
                    continue;
                }

                // Push the subdirectories onto the stack for traversal. 
                foreach (DirectoryInfo info in subDirs)
                {
                    IFilesystemTreeDirNode diffNode = currentDir.ParentDiffNode.SearchForDir(info);
                    if (diffNode == null)
                    {
                        diffNode = currentDir.ParentDiffNode.AddDir(info, currentDir.Location);
                    } else
                    {
                        diffNode.AddInfoFromLocation(info, currentDir.Location);
                    }

                    dirsToBeSearched.Push(new DirectoryForIteration(info, diffNode, currentDir.Location));
                }

                FileInfo[] files;
                try
                {
                    files = currentDir.Info.GetFiles();
                } catch (UnauthorizedAccessException e)
                {
                    Debug.WriteLine(e.Message);
                    continue;
                } catch (DirectoryNotFoundException e)
                {
                    Debug.WriteLine(e.Message);
                    continue;
                }

                // Processing files
                foreach (FileInfo info in files)
                {
                    try
                    {
                        IFilesystemTreeFileNode diffNode = currentDir.ParentDiffNode.SearchForFile(info);
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
                        Debug.WriteLine(e.Message);
                    }
                }
            }

            return FilesystemTree;
        }
    }
}
