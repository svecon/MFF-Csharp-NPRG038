using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;

namespace CoreLibrary.FilesystemTree.Visitors
{
    /// <summary>
    /// This visitor executes all processors in a given order on a FilesystemTree.
    /// 
    /// All files and folders are processed in parallel. 
    /// But one particular file is processed sequentially with all the processors one by one.
    /// </summary>
    public class ParallelExecutionVisitor : IExecutionVisitor
    {
        readonly IProcessorLoader loader;

        readonly List<Task> tasks = new List<Task>();

        readonly CancellationTokenSource tokenSource;

        /// <summary>
        /// Constructor for ParallelExecutionVisitor.
        /// </summary>
        /// <param name="loader">Loader for all Processors.</param>
        public ParallelExecutionVisitor(IProcessorLoader loader)
        {
            this.loader = loader;
            tokenSource = new CancellationTokenSource();
        }

        public void Visit(IFilesystemTreeDirNode node)
        {
            // create a completed task
            Task task = Task.FromResult(false);

            foreach (IPreProcessor processor in loader.GetPreProcessors())
                task = task.ContinueWith(_ => processor.Process(node), tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
            foreach (IProcessor processor in loader.GetProcessors())
                task = task.ContinueWith(_ => processor.Process(node), tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
            foreach (IPostProcessor processor in loader.GetPostProcessors())
                task = task.ContinueWith(_ => processor.Process(node), tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);

            // add task which runs when there is an error
            tasks.Add(task.ContinueWith(_ => HandleError(node, _), TaskContinuationOptions.NotOnRanToCompletion));
            // add task with processors
            tasks.Add(task);

            foreach (IFilesystemTreeFileNode file in node.Files)
                file.Accept(this);

            foreach (IFilesystemTreeDirNode dir in node.Directories)
                dir.Accept(this);
        }

        public void Visit(IFilesystemTreeFileNode node)
        {
            // create a completed task
            Task task = Task.FromResult(false);

            foreach (IPreProcessor processor in loader.GetPreProcessors())
                task = task.ContinueWith(_ => processor.Process(node), tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
            foreach (IProcessor processor in loader.GetProcessors())
                task = task.ContinueWith(_ => processor.Process(node), tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
            foreach (IPostProcessor processor in loader.GetPostProcessors())
                task = task.ContinueWith(_ => processor.Process(node), tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);

            // add task which runs when there is an error
            tasks.Add(task.ContinueWith(_ => HandleError(node, _), TaskContinuationOptions.NotOnRanToCompletion));
            // add task with processors
            tasks.Add(task);
        }

        private static void HandleError(IFilesystemTreeAbstractNode node, Task task)
        {
            node.Status = NodeStatusEnum.HasError;
            node.Exception = task.Exception;
        }

        /// <summary>
        /// Wait for all processing to finish.
        /// </summary>
        public void Wait()
        {
            if (tokenSource.IsCancellationRequested)
                return;

            try
            {
                Task.WaitAll(tasks.ToArray());
            } catch (AggregateException ae)
            {
                ae.Handle(x =>
                {
                    if (x is TaskCanceledException)
                        return true;

                    Debug.WriteLine(x);
                    return false;
                });
            }
        }

        /// <summary>
        /// Cancel all processing.
        /// 
        /// All nodes end up in error state!
        /// </summary>
        public void Cancel()
        {
            tokenSource.Cancel();
        }
    }
}
