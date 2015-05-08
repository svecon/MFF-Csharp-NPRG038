using System;

namespace CoreLibrary.FilesystemTree.Enums
{
    /// <summary>
    /// Modes for FilesystemTree structure.
    /// 
    /// Some features are exclusive for different mode.
    /// </summary>
    [Flags]
    public enum DiffModeEnum
    {
        /// <summary>
        /// 2-way diffing mode.
        /// </summary>
        TwoWay      = 1 << 0,

        /// <summary>
        /// 3-way diffing mode.
        /// </summary>
        ThreeWay    = 1 << 1,
    }
}
