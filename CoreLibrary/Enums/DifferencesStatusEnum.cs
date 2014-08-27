using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Enums
{
    /// <summary>
    /// Enum for having the FilesystemNode know which files are same and which are different.
    /// </summary>
    public enum DifferencesStatusEnum
    {
        Initial = -1, AllDifferent = 0, BaseLeftSame = 3, BaseRight = 5, LeftRightSame = 6, AllSame = 7
    }
}
