using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Interfaces;
using System.Threading;

namespace CoreLibrary.FilesystemTree.Visitors
{
    /// <summary>
    /// This visitor executes all processors in a given order on a FilesystemTree.
    /// 
    /// All files and folders are processed in parallel. 
    /// But one particular file is processed sequentially with all the processors one by one.
    /// </summary>
    public class ExecutionVisitor : IExecutionVisitor
    {

        IProcessorLoader loader;

        List<Task> tasks = new List<Task>();

        CancellationTokenSource tokenSource;

        public ExecutionVisitor(IProcessorLoader loader)
        {
            this.loader = loader;
            tokenSource = new CancellationTokenSource();
        }

        public void Visit(IFilesystemTreeDirNode node)
        {
            // create a completed task
            Task task = Task.FromResult(false);

            foreach (var processor in loader.GetPreProcessors())
                task = task.ContinueWith(_ => processor.Process(node), tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
            foreach (var processor in loader.GetProcessors())
                task = task.ContinueWith(_ => processor.Process(node), tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
            foreach (var processor in loader.GetPostProcessors())
                task = task.ContinueWith(_ => processor.Process(node), tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);

            // add task which runs when there is an error
            tasks.Add(task.ContinueWith(_ => handleError(node, _), TaskContinuationOptions.NotOnRanToCompletion));
            // add task with processors
            tasks.Add(task);

            foreach (var file in node.Files)
                file.Accept(this);

            foreach (var dir in node.Directories)
                dir.Accept(this);
        }

        public void Visit(IFilesystemTreeFileNode node)
        {
            // create a completed task
            Task task = Task.FromResult(false);

            foreach (var processor in loader.GetPreProcessors())
                task = task.ContinueWith(_ => processor.Process(node), tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
            foreach (var processor in loader.GetProcessors())
                task = task.ContinueWith(_ => processor.Process(node), tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
            foreach (var processor in loader.GetPostProcessors())
                task = task.ContinueWith(_ => processor.Process(node), tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);

            // add task which runs when there is an error
            tasks.Add(task.ContinueWith(_ => handleError(node, _), TaskContinuationOptions.NotOnRanToCompletion));
            // add task with processors
            tasks.Add(task);
        }

        private void handleError(IFilesystemTreeAbstractNode node, Task task)
        {
            node.Status = Enums.NodeStatusEnum.HasError;
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

                    System.Diagnostics.Debug.WriteLine(x);
                    //TODO log exception
                    return true;
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
