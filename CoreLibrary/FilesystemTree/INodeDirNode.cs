using System.Collections.Generic;
using System.IO;
using CoreLibrary.FilesystemTree.Enums;

namespace CoreLibrary.FilesystemTree
{
    /// <summary>
    /// Dir node of Node.
    /// </summary>
    public interface INodeDirNode : INodeAbstractNode
    {
        /// <summary>
        /// All subdirectories for current directory.
        /// </summary>
        List<INodeDirNode> Directories { get; }

        /// <summary>
        /// All files located in current directory.
        /// </summary>
        List<INodeFileNode> Files { get; }

        /// <summary>
        /// Reference to the RootNode of the entire Node
        /// 
        /// Root directory references itself.
        /// </summary>
        INodeDirNode RootNode { get; }

        /// <summary>
        /// Relative path to the root of the Node root.
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
        INodeDirNode AddDir(DirectoryInfo info, LocationEnum location);

        /// <summary>
        /// Add new file in current directory.
        /// </summary>
        /// <param name="info">FileInfo for new file.</param>
        /// <param name="location">Where the file was found.</param>
        /// <returns>Node for new file.</returns>
        INodeFileNode AddFile(FileInfo info, LocationEnum location);

        /// <summary>
        /// Search for existing subdirectory.
        /// </summary>
        /// <param name="info">Needle: DirectoryInfo</param>
        /// <returns>Null or existing DirNode.</returns>
        INodeDirNode SearchForDir(DirectoryInfo info);

        /// <summary>
        /// Search for existing file.
        /// </summary>
        /// <param name="info">Needle: FileInfo</param>
        /// <returns>Null or existing FileNode.</returns>
        INodeFileNode SearchForFile(FileInfo info);

        /// <summary>
        /// Returns a size of directory and all subdirectories in kB.
        /// </summary>
        /// <returns>Size in kB</returns>
        double GetSize();

        /// <summary>
        /// Enumerator for all files and directories in the current node.
        /// 
        /// First iterates over directories.
        /// </summary>
        IEnumerable<INodeAbstractNode> FilesAndDirectories { get; }
    }
}
