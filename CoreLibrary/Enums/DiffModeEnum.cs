using System;

namespace CoreLibrary.Enums
{
    /// <summary>
    /// Modes for FilesystemTree structure.
    /// 
    /// Some features are exclusive for different mode.
    /// </summary>
    [Flags]
    public enum DiffModeEnum
    {
        TwoWay      = 1 << 0,
        ThreeWay    = 1 << 1,
    }
}
