using CoreLibrary.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// Interface for a FilesystemTree Execution visitor.
    /// 
    /// Containes method for task manipulations.
    /// </summary>
    public interface IExecutionVisitor : IFilesystemTreeVisitor
    {

        /// <summary>
        /// Wait for all processes to finish.
        /// </summary>
        void Wait();

        /// <summary>
        /// Cancel execution of all processes.
        /// </summary>
        void Cancel();
    }
}
