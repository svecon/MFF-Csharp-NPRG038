using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Enums
{
    /// <summary>
    /// Modes for FilesystemDiff structure.
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
