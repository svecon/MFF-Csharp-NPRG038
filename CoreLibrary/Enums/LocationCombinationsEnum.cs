
namespace CoreLibrary.Enums
{
    /// <summary>
    /// All possible file locations and their combinations.
    /// 
    /// Behaves like an extension for <see cref="LocationEnum"/>
    /// </summary>
    public enum LocationCombinationsEnum
    { OnBase = 1, OnLocal = 2, OnRemote = 4, OnBaseLocal = 3, OnBaseRemote = 5, OnLocalRemote = 6, OnAll3 = 7 };
}
