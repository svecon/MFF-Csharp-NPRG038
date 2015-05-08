
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.Plugins.Processors
{
    /// <summary>
    /// Processor interface that can process nodes from Node.
    /// 
    /// This is the base interface for all processor types.
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Processes a directory node.
        /// </summary>
        /// <param name="node"></param>
        void Process(INodeDirNode node);

        /// <summary>
        /// Processes a file node.
        /// </summary>
        /// <param name="node">File node from the Node</param>
        void Process(INodeFileNode node);
    }
}
