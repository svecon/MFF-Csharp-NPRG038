
namespace CoreLibrary.Enums
{
    /// <summary>
    /// Enum for having the FilesystemNode know which files are same and which are different.
    /// </summary>
    public enum DifferencesStatusEnum
    {
        /// <summary>
        /// Initial difference status. Unknown differences.
        /// </summary>
        Initial = -1,
        
        /// <summary>
        /// All files are different.
        /// </summary>
        AllDifferent = 0,
        
        /// <summary>
        /// Base and local files are same.
        /// </summary>
        BaseLocalSame = 3,
        
        /// <summary>
        /// Base and remote files are same.
        /// </summary>
        BaseRemoteSame = 5,
        
        /// <summary>
        /// Local and remote files are same.
        /// </summary>
        LocalRemoteSame = 6,
        
        /// <summary>
        /// All files are same.
        /// </summary>
        AllSame = 7
    }
}
