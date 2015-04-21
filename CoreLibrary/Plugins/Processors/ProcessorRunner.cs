using System.Threading;
using System.Threading.Tasks;
using CoreLibrary.FilesystemTree.Visitors;
using CoreLibrary.Interfaces;

namespace CoreLibrary.Processors
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
            interactiveVisitor = new ParallelExecutionVisitor(processorLoader.GetProcessors(ProcessorTypeEnum.InteractiveResolving));
        }

        public async Task RunDiff(IFilesystemTreeVisitable diffTree)
        {
            await Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                diffTree.Accept(diffVisitor);
                diffVisitor.Wait();
            }, TaskCreationOptions.LongRunning);
        }

        public async Task RunMerge(IFilesystemTreeVisitable diffTree)
        {
            await Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
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
