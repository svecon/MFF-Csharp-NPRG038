using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreLibrary.FilesystemTree.Visitors;
using CoreLibrary.Interfaces;

namespace CoreLibrary.Processors
{
    public class ProcessorRunner
    {
        private readonly IProcessorLoader loader;

        public delegate void OnDiffCompleteDelegate();
        public delegate void OnMergeCompleteDelegate();

        public ProcessorRunner(IProcessorLoader processorLoader)
        {
            loader = processorLoader;
        }

        public async Task RunDiff(IFilesystemTreeVisitable diffTree)
        {
            IExecutionVisitor ex = new ParallelExecutionVisitor(loader.SplitUsingPreprocessors());

            await Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                diffTree.Accept(ex);
                ex.Wait();
            }, TaskCreationOptions.LongRunning);
        }

        public async Task RunMerge(IFilesystemTreeVisitable diffTree)
        {
            IExecutionVisitor ex = new ParallelExecutionVisitor(loader.SplitUsingPostprocessors());

            await Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                diffTree.Accept(ex);
                ex.Wait();
            }, TaskCreationOptions.LongRunning);
        }

        public void RunInteractiveResolving(IFilesystemTreeVisitable diffTree)
        {

        }
    }
}
