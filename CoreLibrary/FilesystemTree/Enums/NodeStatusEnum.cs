
namespace CoreLibrary.FilesystemTree.Enums
{
    /// <summary>
    /// Status of a FilesystemNode. 
    /// 
    /// Processors may act differently for each status.
    /// </summary>
    public enum NodeStatusEnum
    {
        /// <summary>
        /// Initial status. The FilesystemTree was not processed.
        /// </summary>
        Initial,

        /// <summary>
        /// There was an error during processing the FilesystemTree.
        /// </summary>
        HasError,

        /// <summary>
        /// The FilesystemTree is ignored.
        /// </summary>
        IsIgnored,

        /// <summary>
        /// The FilesystemTree has calculated diff.
        /// </summary>
        WasDiffed,

        /// <summary>
        /// The FilesystemTree was merged.
        /// </summary>
        WasMerged,

        /// <summary>
        /// The diff in FilesystemTree has conflicts.
        /// </summary>
        HasConflicts,

        /// <summary>
        /// The files in the FilesystemTree are conflicting.
        /// </summary>
        IsConflicting,
    }
}
