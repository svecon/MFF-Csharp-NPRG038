using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Enums
{
    /// <summary>
    /// All possible file locations and their combinations.
    /// 
    /// Behaves like an extension for <see cref="LocationEnum"/>
    /// </summary>
    public enum LocationCombinationsEnum
    { OnBase = 1, OnLeft = 2, OnRight = 4, OnBaseLeft = 3, OnBaseRight = 5, OnLeftRight = 6, OnAll3 = 7 };
}
