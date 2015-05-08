
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
        /// Initial status. The node was not processed.
        /// </summary>
        Initial,

        /// <summary>
        /// There was an error during processing the node.
        /// </summary>
        HasError,

        /// <summary>
        /// The node is ignored.
        /// </summary>
        IsIgnored,

        /// <summary>
        /// The node has calculated diff.
        /// </summary>
        WasDiffed,

        /// <summary>
        /// The node was merged.
        /// </summary>
        WasMerged,

        /// <summary>
        /// The diff in node has conflicts.
        /// </summary>
        HasConflicts,

        /// <summary>
        /// The files in the node are conflicting.
        /// </summary>
        IsConflicting,
    }
}
