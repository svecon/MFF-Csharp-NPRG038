using System.Threading.Tasks;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Visitors;

namespace CoreLibrary.Plugins.Processors
{
    /// <summary>
    /// Processor runner is a helper class that runs processors.
    /// </summary>
    public class ProcessorRunner
    {
        private readonly IExecutionVisitor diffVisitor;
        private readonly IExecutionVisitor mergeVisitor;
        private readonly IExecutionVisitor interactiveVisitor;

        /// <summary>
        /// Initializes new instance of the <see cref="ProcessorRunner"/>
        /// </summary>
        /// <param name="processorLoader">Processor loader with loaded processors.</param>
        public ProcessorRunner(IProcessorLoader processorLoader)
        {
            diffVisitor = new ParallelExecutionVisitor(processorLoader.GetProcessors(ProcessorTypeEnum.Diff));
            mergeVisitor = new ParallelExecutionVisitor(processorLoader.GetProcessors(ProcessorTypeEnum.Merge));
            interactiveVisitor = new ExecutionVisitor(processorLoader.GetProcessors(ProcessorTypeEnum.Interactive));
        }

        /// <summary>
        /// Runs a diff processors for a given structure
        /// </summary>
        /// <param name="structure">Structure on which the processors will run.</param>
        /// <returns>Task for the async process.</returns>
        public async Task RunDiff(INodeVisitable structure)
        {
            await Task.Factory.StartNew(() =>
            {
                structure.Accept(diffVisitor);
                diffVisitor.Wait();
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Runs a merge processors for a given structure
        /// </summary>
        /// <param name="structure">Structure on which the processors will run.</param>
        /// <returns>Task for the async process.</returns>
        public async Task RunMerge(INodeVisitable structure)
        {
            await Task.Factory.StartNew(() =>
            {
                structure.Accept(mergeVisitor);
                mergeVisitor.Wait();
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Runs interactive processors for a given structure
        /// </summary>
        /// <param name="structure">Structure on which the processors will run.</param>
        public void RunInteractiveResolving(INodeVisitable structure)
        {
            structure.Accept(interactiveVisitor);
        }
    }
}
