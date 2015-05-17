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
        /// <summary>
        /// Processors that will be executed in this ExecutionVisitor.
        /// </summary>
        private readonly IEnumerable<IProcessor> processors;

        /// <summary>
        /// A list of currently active tasks used for asynchronous execution.
        /// </summary>
        private readonly List<Task> tasks = new List<Task>();

        /// <summary>
        /// Used for cancelling all asynchronous tasks executed by this visitor.
        /// </summary>
        private readonly CancellationTokenSource tokenSource;

        /// <summary>
        /// Initializes new instance of the <see cref="ParallelExecutionVisitor"/>
        /// </summary>
        /// <param name="processors">Enumerator for processors that will be run.</param>
        public ParallelExecutionVisitor(IEnumerable<IProcessor> processors)
        {
            this.processors = processors;
            tokenSource = new CancellationTokenSource();
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <summary>
        /// Handles an error that occures during the processing.
        /// </summary>
        /// <param name="node">Node in which the error occured.</param>
        /// <param name="e">Error exception.</param>
        private static void HandleError(INodeAbstractNode node, Exception e)
        {
            node.Status = NodeStatusEnum.HasError;
            node.Exception = e;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void Cancel()
        {
            tokenSource.Cancel();
        }
    }
}
