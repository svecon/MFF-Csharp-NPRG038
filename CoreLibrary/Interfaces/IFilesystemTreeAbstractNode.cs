using System;
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
        FileSystemInfo InfoLocal { get; }

        /// <summary>
        /// Info from Right directory.
        /// </summary>
        FileSystemInfo InfoRemote { get; }

        /// <summary>
        /// What has already been done with this node.
        /// </summary>
        NodeStatusEnum Status { get; set; }

        /// <summary>
        /// Raised exceptions during the processing 
        /// </summary>
        Exception Exception { get; set; }

        /// <summary>
        /// Mode for given filesystem tree.
        /// 
        /// Processors can act differently for each mode.
        /// </summary>
        DiffModeEnum Mode { get; }

        /// <summary>
        /// File type for given file.
        /// </summary>
        FileTypeEnum FileType { get; set; }

        /// <summary>
        /// Which files are different from each other.
        /// </summary>
        DifferencesStatusEnum Differences { get; set; }

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
        /// Check whether a file has been found in given location combination.
        /// </summary>
        /// <param name="location">Location combination you want to check.</param>
        /// <returns>True if files has been found from that location.</returns>
        bool IsInLocation(LocationCombinationsEnum location);

        /// <summary>
        /// Adds Info from new Location (new file has been found).
        /// </summary>
        /// <param name="info">Info about the file.</param>
        /// <param name="location">Location of the file.</param>
        void AddInfoFromLocation(FileSystemInfo info, LocationEnum location);

        void RemoveInfoFromLocation(LocationEnum location);

        /// <summary>
        /// Absolute path of a file (or directory) for given location.
        /// </summary>
        /// <returns>Absolute path of the file on the disk.</returns>
        string GetAbsolutePath(LocationEnum location);
    }
}
