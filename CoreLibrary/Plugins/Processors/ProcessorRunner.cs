using System.Threading.Tasks;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Visitors;

namespace CoreLibrary.Plugins.Processors
{
    public class ProcessorRunner
    {
        private readonly IExecutionVisitor diffVisitor;
        private readonly IExecutionVisitor mergeVisitor;
        private readonly IExecutionVisitor interactiveVisitor;

        public delegate void OnDiffCompleteDelegate();
        public delegate void OnMergeCompleteDelegate();

        public ProcessorRunner(IProcessorLoader processorLoader)
        {
            diffVisitor = new ParallelExecutionVisitor(processorLoader.GetProcessors(ProcessorTypeEnum.Diff));
            mergeVisitor = new ParallelExecutionVisitor(processorLoader.GetProcessors(ProcessorTypeEnum.Merge));
            interactiveVisitor = new ExecutionVisitor(processorLoader.GetProcessors(ProcessorTypeEnum.Interactive));
        }

        public async Task RunDiff(IFilesystemTreeVisitable diffTree)
        {
            await Task.Factory.StartNew(() =>
            {
                diffTree.Accept(diffVisitor);
                diffVisitor.Wait();
            }, TaskCreationOptions.LongRunning);
        }

        public async Task RunMerge(IFilesystemTreeVisitable diffTree)
        {
            await Task.Factory.StartNew(() =>
            {
                diffTree.Accept(mergeVisitor);
                mergeVisitor.Wait();
            }, TaskCreationOptions.LongRunning);
        }

        public void RunInteractiveResolving(IFilesystemTreeVisitable diffTree)
        {
            diffTree.Accept(interactiveVisitor);
        }
    }
}
