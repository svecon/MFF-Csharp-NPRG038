using System.IO;
using CoreLibrary.Enums;

namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// Abstract node in FilesystemTree.
    /// </summary>
    public interface IFilesystemTreeAbstractNode : IFilesystemTreeVisitable
    {
        /// <summary>
        /// What has already been done with this node.
        /// </summary>
        NodeStatusEnum Status { get; set; }

        /// <summary>
        /// Mode for given filesystem tree.
        /// 
        /// Processors can act differently for each mode.
        /// </summary>
        DiffModeEnum Mode { get; }

        /// <summary>
        /// Which files are different from each other.
        /// </summary>
        DifferencesStatusEnum Differences { get; set; }

        /// <summary>
        /// Info from any of matched files that is not null.
        /// </summary>
        FileSystemInfo Info { get; }

        /// <summary>
        /// Info from Base directory.
        /// </summary>
        FileSystemInfo InfoBase { get; }

        /// <summary>
        /// Info from Left directory.
        /// </summary>
        FileSystemInfo InfoLeft { get; }

        /// <summary>
        /// Info from Right directory.
        /// </summary>
        FileSystemInfo InfoRight { get; }

        /// <summary>
        /// Binary mask that hold information where the file (or directory) has been found.
        /// </summary>
        int Location { get; }

        /// <summary>
        /// Check whether a file has been found in given location.
        /// </summary>
        /// <param name="location">Location you want to check.</param>
        /// <returns>True if files has been found from that location.</returns>
        bool IsInLocation(LocationEnum location);

        /// <summary>
        /// Adds Info from new Location (new file has been found).
        /// </summary>
        /// <param name="info">Info about the file.</param>
        /// <param name="location">Location of the file.</param>
        /// <param name="markIsFound">Mark that file has been found.</param>
        void AddInfoFromLocation(FileSystemInfo info, LocationEnum location, bool markIsFound = true);

        /// <summary>
        /// Absolute path of a file (or directory) for given location.
        /// </summary>
        /// <returns>Absolute path of the file on the disk.</returns>
        string GetAbsolutePath(LocationEnum location);
    }
}
