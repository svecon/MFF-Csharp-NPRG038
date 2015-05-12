namespace CoreLibrary.Plugins.Processors
{
    /// <summary>
    /// Type of a processor.
    /// </summary>
    public enum ProcessorTypeEnum
    {
        /// <summary>
        /// Diff processor calcualtes diff.
        /// </summary>
        Diff, 
        
        /// <summary>
        /// Interactive processor runs in console user interface and can resolve conflicts.
        /// </summary>
        Interactive, 
        
        /// <summary>
        /// Merge processor merges the differences.
        /// </summary>
        Merge
    }
}
