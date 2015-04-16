using System;
using System.Collections.Generic;
using System.Linq;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;

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
        readonly IEnumerable<IProcessor> processors;

        private bool isCancelled = false;

        /// <summary>
        /// Constructor for ExecutionVisitorInSerial.
        /// </summary>
        /// <param name="processors">Processors to be run.</param>
        public ExecutionVisitor(IEnumerable<IProcessor> processors)
        {
            this.processors = processors;
        }

        public void Visit(IFilesystemTreeDirNode node)
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

            foreach (IFilesystemTreeFileNode file in node.Files.Where(processor => !isCancelled))
                file.Accept(this);

            foreach (IFilesystemTreeDirNode dir in node.Directories.Where(processor => !isCancelled))
                dir.Accept(this);
        }

        public void Visit(IFilesystemTreeFileNode node)
        {
            try
            {
                foreach (IProcessor processor in processors.Where(processor => !isCancelled))
                    processor.Process(node);

            } catch (Exception e)
            {
                HandleError(node, e);
            }
        }

        private static void HandleError(IFilesystemTreeAbstractNode node, Exception e)
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
