using System;

namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// Interface for a visitor that can traverse FilesystemTree.
    /// </summary>
    public interface IFilesystemTreeVisitor
    {
        void Visit(IFilesystemTreeDirNode node);
        void Visit(IFilesystemTreeFileNode node);
    }
}
