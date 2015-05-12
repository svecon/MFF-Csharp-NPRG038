using System;

namespace CoreLibrary.FilesystemTree.Enums
{
    /// <summary>
    /// Locations where the files were found from.
    /// </summary>
    [Flags]
    public enum LocationEnum
    {
        /// <summary>
        /// File is on base.
        /// </summary>
        OnBase       = 1 << 0,

        /// <summary>
        /// File is on local.
        /// </summary>
        OnLocal      = 1 << 1,

        /// <summary>
        /// File is on remote.
        /// </summary>
        OnRemote     = 1 << 2,
    };
}
