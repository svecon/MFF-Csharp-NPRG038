
namespace CoreLibrary.Enums
{
    /// <summary>
    /// Status of a FilesystemNode. 
    /// 
    /// Processors may act differently for each status.
    /// </summary>
    public enum NodeStatusEnum
    {
        Initial,

        HasError,
        IsIgnored,
        
        WasDiffed,

        WasMerged,

        HasConflicts,
        IsConflicting,
    }
}
