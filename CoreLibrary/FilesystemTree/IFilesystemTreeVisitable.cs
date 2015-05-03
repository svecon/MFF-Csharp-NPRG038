﻿
using CoreLibrary.FilesystemTree.Visitors;

namespace CoreLibrary.FilesystemTree
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