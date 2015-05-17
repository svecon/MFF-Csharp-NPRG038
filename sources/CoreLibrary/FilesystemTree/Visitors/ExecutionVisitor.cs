using System;
using System.Collections.Generic;
using System.Linq;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Plugins.Processors;

namespace CoreLibrary.FilesystemTree.Visitors
{
    /// <summary>
    /// This visitor executes all processors in a given order on a FilesystemTree.
    /// 
    /// All files and folders are processed in series. 
    /// But one particular file is processed sequentially with all the processors one by one.
    /// </summary>
    public class ExecutionVisitor : IExecutionVisitor
    {
        /// <summary>
        /// Processors that will be executed in this ExecutionVisitor.
        /// </summary>
        private readonly IEnumerable<IProcessor> processors;

        /// <summary>
        /// Was the execution cancelled?
        /// </summary>
        private bool isCancelled = false;

        /// <summary>
        /// Initializes new instance of the <see cref="ExecutionVisitor"/>
        /// </summary>
        /// <param name="processors">Enumerator for processors that will be run.</param>
        public ExecutionVisitor(IEnumerable<IProcessor> processors)
        {
            this.processors = processors;
        }

        /// <inheritdoc />
        public void Visit(INodeDirNode node)
        {
            try
            {
                // run processors unless this Visitor isCancelled
                foreach (IProcessor processor in processors.Where(processor => !isCancelled))
                    processor.Process(node);

            } catch (Exception e)
            {
                HandleError(node, e);
            }

            // process files
            foreach (INodeFileNode file in node.Files.Where(processor => !isCancelled))
                file.Accept(this);

            // process subdirectories
            foreach (INodeDirNode dir in node.Directories.Where(processor => !isCancelled))
                dir.Accept(this);
        }

        /// <inheritdoc />
        public void Visit(INodeFileNode node)
        {
            try
            {
                // run processors unless this Visitor isCancelled
                foreach (IProcessor processor in processors.Where(processor => !isCancelled))
                    processor.Process(node);

            } catch (Exception e)
            {
                HandleError(node, e);
            }
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

            // TODO: log exceptions
        }

        /// <inheritdoc />
        public void Wait()
        {
            // everything is in serial, no need to wait
        }

        /// <inheritdoc />
        public void Cancel()
        {
            isCancelled = true;
        }
    }
}
