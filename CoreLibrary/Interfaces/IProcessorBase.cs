using System;
using System.IO;
using CoreLibrary.Enums;

namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// Processor interface that can process nodes from FilesystemTree.
    /// </summary>
    public interface IProcessorBase
    {
        void Process(IFilesystemTreeDirNode node);

        void Process(IFilesystemTreeFileNode node);

        /// <summary>
        /// Priority ensures correct order of execution between multiple processors.
        /// </summary>
        int Priority { get; }

        DiffModeEnum Mode { get; }
    }
}
