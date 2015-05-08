
namespace CoreLibrary.Enums
{
    /// <summary>
    /// All possible file locations and their combinations.
    /// 
    /// Behaves like an extension for <see cref="LocationEnum"/>
    /// </summary>
    public enum LocationCombinationsEnum
    {
        /// <summary>
        /// File is only on base.
        /// </summary>
        OnBase = 1,

        /// <summary>
        /// File is only on local.
        /// </summary>
        OnLocal = 2,

        /// <summary>
        /// File is only on remote.
        /// </summary>
        OnRemote = 4,

        /// <summary>
        /// File is on base and local.
        /// </summary>
        OnBaseLocal = 3,

        /// <summary>
        /// File is on base and remote.
        /// </summary>
        OnBaseRemote = 5,

        /// <summary>
        /// File is on local and remote.
        /// </summary>
        OnLocalRemote = 6,

        /// <summary>
        /// File is on local, base and remote.
        /// </summary>
        OnAll3 = 7
    };
}
