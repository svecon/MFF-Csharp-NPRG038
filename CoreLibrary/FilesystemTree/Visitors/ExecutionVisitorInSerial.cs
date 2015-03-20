﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreLibrary.Interfaces;
using System.Threading;

namespace CoreLibrary.FilesystemTree.Visitors
{
    /// <summary>
    /// This visitor executes all processors in a given order on a FilesystemTree.
    /// 
    /// All files and folders are processed in series. 
    /// But one particular file is processed sequentially with all the processors one by one.
    /// </summary>
    public class ExecutionVisitorInSerial : IExecutionVisitor
    {
        readonly IProcessorLoader loader;

        private bool cancelled = false;

        /// <summary>
        /// Constructor for ExecutionVisitorInSerial.
        /// </summary>
        /// <param name="loader">Loader for all Processors.</param>
        public ExecutionVisitorInSerial(IProcessorLoader loader)
        {
            this.loader = loader;
        }

        public void Visit(IFilesystemTreeDirNode node)
        {
            try
            {
                foreach (IPreProcessor processor in loader.GetPreProcessors().Where(processor => !cancelled))
                    processor.Process(node);
                foreach (IProcessor processor in loader.GetProcessors().Where(processor => !cancelled))
                    processor.Process(node);
                foreach (IPostProcessor processor in loader.GetPostProcessors().Where(processor => !cancelled))
                    processor.Process(node);
            } catch (Exception e)
            {
                HandleError(node);
            }

            foreach (IFilesystemTreeFileNode file in node.Files.Where(processor => !cancelled))
                file.Accept(this);

            foreach (IFilesystemTreeDirNode dir in node.Directories.Where(processor => !cancelled))
                dir.Accept(this);
        }

        public void Visit(IFilesystemTreeFileNode node)
        {
            try
            {
                foreach (IPreProcessor processor in loader.GetPreProcessors().Where(processor => !cancelled))
                    processor.Process(node);
                foreach (IProcessor processor in loader.GetProcessors().Where(processor => !cancelled))
                    processor.Process(node);
                foreach (IPostProcessor processor in loader.GetPostProcessors().Where(processor => !cancelled))
                    processor.Process(node);
            } catch (Exception e)
            {
                HandleError(node);
            }
        }

        private static void HandleError(IFilesystemTreeAbstractNode node)
        {
            node.Status = Enums.NodeStatusEnum.HasError;
        }

        public void Wait()
        {
            // everything is in serial, no need to wait
        }

        public void Cancel()
        {
            cancelled = true;
        }
    }
}
