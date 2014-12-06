using System.Collections.Generic;
using CoreLibrary.Enums;
using System.IO;

namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// Dir node of FilesystemTree.
    /// </summary>
    public interface IFilesystemTreeDirNode : IFilesystemTreeAbstractNode
    {
        /// <summary>
        /// All subdirectories for current directory.
        /// </summary>
        List<IFilesystemTreeDirNode> Directories { get; }

        /// <summary>
        /// All files located in current directory.
        /// </summary>
        List<IFilesystemTreeFileNode> Files { get; }

        /// <summary>
        /// Reference to the RootNode of the entire FilesystemTree
        /// 
        /// Root directory references itself.
        /// </summary>
        IFilesystemTreeDirNode RootNode { get; }

        /// <summary>
        /// Relative path to the root of the FilesystemTree root.
        /// 
        /// This combined with root path gives absolute path.
        /// </summary>
        string RelativePath { get; }

        /// <summary>
        /// Add new subdirectory.
        /// </summary>
        /// <param name="info">DirectoryInfo for subdirectory.</param>
        /// <param name="location">Where the subdirectory was found.</param>
        /// <returns>Node for new subdirectory.</returns>
        IFilesystemTreeDirNode AddDir(DirectoryInfo info, LocationEnum location);

        /// <summary>
        /// Add new file in current directory.
        /// </summary>
        /// <param name="info">FileInfo for new file.</param>
        /// <param name="location">Where the file was found.</param>
        /// <returns>Node for new file.</returns>
        IFilesystemTreeFileNode AddFile(FileInfo info, LocationEnum location);

        /// <summary>
        /// Search for existing subdirectory.
        /// </summary>
        /// <param name="info">Needle: DirectoryInfo</param>
        /// <returns>Null or existing DirNode.</returns>
        IFilesystemTreeDirNode SearchForDir(DirectoryInfo info);

        /// <summary>
        /// Search for existing file.
        /// </summary>
        /// <param name="info">Needle: FileInfo</param>
        /// <returns>Null or existing FileNode.</returns>
        IFilesystemTreeFileNode SearchForFile(FileInfo info);

        /// <summary>
        /// Returns a size of directory and all subdirectories in kB.
        /// </summary>
        /// <returns>Size in kB</returns>
        double GetSize();
    }
}
