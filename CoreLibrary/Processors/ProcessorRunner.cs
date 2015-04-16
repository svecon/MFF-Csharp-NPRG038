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
        private readonly Dictionary<IFilesystemTreeVisitable, OnDiffCompleteDelegate> diffDelegates;
        private readonly Dictionary<IFilesystemTreeVisitable, OnMergeCompleteDelegate> mergeDelegates;

        public ProcessorRunner(IProcessorLoader processorLoader)
        {
            loader = processorLoader;
            diffDelegates = new Dictionary<IFilesystemTreeVisitable, OnDiffCompleteDelegate>();
            mergeDelegates = new Dictionary<IFilesystemTreeVisitable, OnMergeCompleteDelegate>();
        }

        public void AddOnDiffCompleteDelegate(IFilesystemTreeVisitable treeVisitable, OnDiffCompleteDelegate onCompleteDelegate)
        {
            if (diffDelegates.ContainsKey(treeVisitable))
            {
                diffDelegates[treeVisitable] += onCompleteDelegate;
            } else
            {
                diffDelegates.Add(treeVisitable, onCompleteDelegate);
            }
        }

        public void AddOnMergeCompleteDelegate(IFilesystemTreeVisitable treeVisitable, OnMergeCompleteDelegate onCompleteDelegate)
        {
            if (mergeDelegates.ContainsKey(treeVisitable))
            {
                mergeDelegates[treeVisitable] += onCompleteDelegate;
            } else
            {
                mergeDelegates.Add(treeVisitable, onCompleteDelegate);
            }
        }

        public async Task RunDiff(IFilesystemTreeVisitable diffTree)
        {
            IExecutionVisitor ex = new ParallelExecutionVisitor(loader.SplitUsingPreprocessors());

            await Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                diffTree.Accept(ex);
                ex.Wait();
            }, TaskCreationOptions.LongRunning);

            if (!diffDelegates.ContainsKey(diffTree) || diffDelegates[diffTree] == null) return;

            diffDelegates[diffTree]();
            diffDelegates.Remove(diffTree);
        }

        public async Task RunMerge(IFilesystemTreeVisitable diffTree)
        {
            IExecutionVisitor ex = new ParallelExecutionVisitor(loader.SplitUsingPostprocessors());

            await Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                diffTree.Accept(ex);
                ex.Wait();
            }, TaskCreationOptions.LongRunning);

            if (!mergeDelegates.ContainsKey(diffTree) || mergeDelegates[diffTree] == null) return;
            
            mergeDelegates[diffTree]();
            mergeDelegates.Remove(diffTree);
        }

        public void RunInteractiveResolving(IFilesystemTreeVisitable diffTree)
        {

        }
    }
}
