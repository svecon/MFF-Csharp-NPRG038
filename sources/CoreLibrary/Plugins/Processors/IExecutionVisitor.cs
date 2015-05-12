
using CoreLibrary.FilesystemTree.Visitors;

namespace CoreLibrary.Plugins.Processors
{
    /// <summary>
    /// Interface for a FilesystemTree Execution visitor.
    /// 
    /// Containes method for task manipulations.
    /// </summary>
    public interface IExecutionVisitor : IFilesystemTreeVisitor
    {

        /// <summary>
        /// Wait for all processes to finish.
        /// </summary>
        void Wait();

        /// <summary>
        /// Cancel execution of all processes.
        /// </summary>
        void Cancel();
    }
}
