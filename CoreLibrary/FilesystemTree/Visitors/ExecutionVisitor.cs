using System;
using System.Collections.Generic;
using System.Linq;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Plugins.Processors;

namespace CoreLibrary.FilesystemTree.Visitors
{
    /// <summary>
    /// This visitor executes all processors in a given order on a Node.
    /// 
    /// All files and folders are processed in series. 
    /// But one particular file is processed sequentially with all the processors one by one.
    /// </summary>
    public class ExecutionVisitor : IExecutionVisitor
    {
        readonly IEnumerable<IProcessor> processors;

        private bool isCancelled = false;

        /// <summary>
        /// Initializes new instance of the <see cref="ExecutionVisitor"/>
        /// </summary>
        /// <param name="processors">Enumerator for processors that will be run.</param>
        public ExecutionVisitor(IEnumerable<IProcessor> processors)
        {
            this.processors = processors;
        }

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

        private static void HandleError(INodeAbstractNode node, Exception e)
        {
            node.Status = NodeStatusEnum.HasError;
            node.Exception = e;
        }

        public void Wait()
        {
            // everything is in serial, no need to wait
        }

        public void Cancel()
        {
            isCancelled = true;
        }
    }
}
