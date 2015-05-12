namespace CoreLibrary.FilesystemTree.Enums
{
    /// <summary>
    /// Which version of diff item should be kept and used?
    /// </summary>
    public enum PreferedActionThreeWayEnum
    {
        /// <summary>
        /// Default action.
        /// </summary>
        Default, 
        
        /// <summary>
        /// Apply and keep local content.
        /// </summary>
        ApplyLocal,
        
        /// <summary>
        /// Apply and keep remote content.
        /// </summary>
        ApplyRemote, 
        
        /// <summary>
        /// Apply and keep base content.
        /// </summary>
        RevertToBase
    }
}