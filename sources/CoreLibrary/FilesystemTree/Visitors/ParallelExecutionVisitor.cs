using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Plugins.Processors;

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
        readonly IEnumerable<IProcessor> processors;

        readonly List<Task> tasks = new List<Task>();

        readonly CancellationTokenSource tokenSource;

        /// <summary>
        /// Initializes new instance of the <see cref="ParallelExecutionVisitor"/>
        /// </summary>
        /// <param name="processors">Enumerator for processors that will be run.</param>
        public ParallelExecutionVisitor(IEnumerable<IProcessor> processors)
        {
            this.processors = processors;
            tokenSource = new CancellationTokenSource();
        }

        public void Visit(INodeDirNode node)
        {
            // create a completed task
            Task task = Task.FromResult(false);

            // run all the processors sequentially one after another on this FilesystemTree
            task = processors.Aggregate(task, (current, processor) => current.ContinueWith(_ =>
            {
                try
                {
                    processor.Process(node);
                } catch (Exception e)
                {
                    HandleError(node, e);
                }
            }, tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current));

            // add task with processors
            tasks.Add(task);

            // process files
            foreach (INodeFileNode file in node.Files)
                file.Accept(this);

            // process directories
            foreach (INodeDirNode dir in node.Directories)
                dir.Accept(this);
        }

        public void Visit(INodeFileNode node)
        {
            // create a completed task
            Task task = Task.FromResult(false);

            // run all the processors sequentially one after another on this FilesystemTree
            task = processors.Aggregate(task, (current, processor) => current.ContinueWith(_ =>
            {
                try
                {
                    processor.Process(node);
                } catch (Exception e)
                {
                    HandleError(node, e);
                }
            }, tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current));

            // add task with processors
            tasks.Add(task);
        }

        private static void HandleError(INodeAbstractNode node, Exception e)
        {
            node.Status = NodeStatusEnum.HasError;
            node.Exception = e;
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
                // wait for all nodes to finish processing
                Task.WaitAll(tasks.ToArray());
            } catch (AggregateException ae)
            {
                ae.Handle(x =>
                {
                    if (x is TaskCanceledException)
                        return true;

                    // TODO: log exceptions
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
