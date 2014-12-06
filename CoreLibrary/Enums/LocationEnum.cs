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
        OnLeft      = 1 << 1,
        OnRight     = 1 << 2,
    };
}
