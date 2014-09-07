using System;

namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// Interface for a visitor that can traverse FilesystemTree.
    /// </summary>
    public interface IFilesystemTreeVisitor
    {
        /// <summary>
        /// Visit and process a directory node.
        /// </summary>
        /// <param name="node">FilesystemTree directory node</param>
        void Visit(IFilesystemTreeDirNode node);
        /// <summary>
        /// Visit and process a file node.
        /// </summary>
        /// <param name="node">FilesystemTree file node</param>
        void Visit(IFilesystemTreeFileNode node);
    }
}
