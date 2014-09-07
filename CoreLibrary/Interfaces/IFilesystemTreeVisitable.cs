using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// Interface for a FilesystemTree structure that can accept a Visitor
    /// </summary>
    public interface IFilesystemTreeVisitable
    {
        /// <summary>
        /// Accepting the visitor means that the visitor will be called 
        /// and it will process all information about the tree.
        /// </summary>
        /// <param name="visitor">Visitor pattern class.</param>
        void Accept(IFilesystemTreeVisitor visitor);
    }
}
