using CoreLibrary.FilesystemTree;

namespace CoreLibrary.Plugins.DiffWindow
{
    /// <summary>
    /// Interface for DiffWindows manager that shows the individual windows.
    /// </summary>
    public interface IDiffWindowManager
    {
        /// <summary>
        /// Open new visualisation using paths as string arguments.
        /// </summary>
        /// <param name="args">Paths to the files/folders that will be diffed.</param>
        /// <returns><see cref="IDiffWindow{TNode}"/> that visualizes differences.</returns>
        IDiffWindow<INodeVisitable> OpenNewTab(params string[] args);

        /// <summary>
        /// Open new visualisation using FilesystemTree.
        /// </summary>
        /// <param name="diffNode">FilesystemTree that holds the differences.</param>
        /// <param name="parentWindow">Parent <see cref="IDiffWindow{TNode}"/> that requested opening new diff.</param>
        /// <returns><see cref="IDiffWindow{TNode}"/> that visualizes differences.</returns>
        IDiffWindow<INodeVisitable> OpenNewTab(INodeVisitable diffNode, IDiffWindow<INodeVisitable> parentWindow = null);

        /// <summary>
        /// Requests recalculation of the diff for given <see cref="IDiffWindow{TNode}"/>
        /// </summary>
        /// <param name="window"><see cref="IDiffWindow{TNode}"/> that requests the diff calculation.</param>
        void RequestDiff(IDiffWindow<INodeVisitable> window);

        /// <summary>
        /// Requests merge for given <see cref="IDiffWindow{TNode}"/>
        /// </summary>
        /// <param name="window"><see cref="IDiffWindow{TNode}"/> that requests the merging.</param>
        void RequestMerge(IDiffWindow<INodeVisitable> window);
    }
}
