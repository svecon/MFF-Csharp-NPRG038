
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.Plugins.Processors
{
    /// <summary>
    /// Processor interface that can process nodes from FilesystemTree.
    /// 
    /// This is the base interface for all processor types.
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Processes a directory FilesystemTree.
        /// </summary>
        /// <param name="node">Directory node from the FilesystemTree</param>
        void Process(INodeDirNode node);

        /// <summary>
        /// Processes a file FilesystemTree.
        /// </summary>
        /// <param name="node">File node from the FilesystemTree</param>
        void Process(INodeFileNode node);
    }
}
