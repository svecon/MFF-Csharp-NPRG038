using System;

namespace CoreLibrary.Enums
{
    /// <summary>
    /// Locations where the files were found from.
    /// </summary>
    [Flags]
    public enum LocationEnum
    {
        OnBase      = 1 << 0,
        OnLocal      = 1 << 1,
        OnRemote     = 1 << 2,
    };
}
