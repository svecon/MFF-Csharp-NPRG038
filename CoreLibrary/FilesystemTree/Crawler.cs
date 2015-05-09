using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CoreLibrary.Exceptions.NotFound;
using CoreLibrary.FilesystemTree.Enums;

namespace CoreLibrary.FilesystemTree
{
    /// <summary>
    /// Crawler traverses given directories (2 or 3) 
    /// and builds a IFilesystemTree with files and directories as Nodes.
    /// 
    /// TODO try to parallelize the crawling to read the related directories from different revisions at the same time
    /// This might not be faster as the structure still needs to do only one related directory at a time
    /// Paralallizing without waiting for related directories to finish scanning would make finding related files and directories much slower.
    /// </summary>
    public class Crawler : ICrawler
    {
        /// <summary>
        /// Struct that holds all needed information for filesystem traversal:
        /// DirectoryInfo, Pointer to a tree FilesystemTree, Path Location.
        /// </summary>
        private struct DirectoryForIteration
        {
            public readonly DirectoryInfo Info;
            public readonly INodeDirNode ParentDiffNode;
            public readonly LocationEnum Location;

            public DirectoryForIteration(DirectoryInfo info, INodeDirNode parent, LocationEnum location)
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
        /// Initializes new instance of the <see cref="Crawler"/>
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
                    INodeDirNode diffNode = currentDir.ParentDiffNode.SearchForDir(info);
                    if (diffNode == null)
                    { // no related directory found, create a new FilesystemTree
                        diffNode = currentDir.ParentDiffNode.AddDir(info, currentDir.Location);
                    } else
                    { // related directory found, add this location
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
                        INodeFileNode diffNode = currentDir.ParentDiffNode.SearchForFile(info);
                        if (diffNode == null)
                        { // no related file found, create a new FilesystemTree
                            currentDir.ParentDiffNode.AddFile(info, currentDir.Location);
                        } else
                        { // related file found, add this location
                            diffNode.AddInfoFromLocation(info, currentDir.Location);
                        }
                    } catch (FileNotFoundException e)
                    {
                        // If file was deleted by a separate application 
                        //  or thread since the call to TraverseTree() 
                        // then just continue.
                        Debug.WriteLine(e.Message);
                        continue;
                    }
                }
            }

            return FilesystemTree;
        }
    }
}
