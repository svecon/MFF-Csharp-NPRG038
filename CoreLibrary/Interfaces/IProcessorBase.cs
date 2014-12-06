
namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// Processor interface that can process nodes from FilesystemTree.
    /// 
    /// This is the base interface for all processor types.
    /// </summary>
    public interface IProcessorBase
    {
        /// <summary>
        /// Processes a directory node.
        /// </summary>
        /// <param name="node"></param>
        void Process(IFilesystemTreeDirNode node);

        /// <summary>
        /// Processes a file node.
        /// </summary>
        /// <param name="node">File node from the FilesystemTree</param>
        void Process(IFilesystemTreeFileNode node);

        /// <summary>
        /// Priority ensures correct order of execution between multiple processors.
        /// </summary>
        int Priority { get; }
    }
}
