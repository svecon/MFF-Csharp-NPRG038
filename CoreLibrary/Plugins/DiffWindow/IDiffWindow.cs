using System.Threading.Tasks;
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.Plugins.DiffWindow
{
    /// <summary>
    /// Interface for visualising windows.
    /// </summary>
    /// <typeparam name="TNode">Type of the <see cref="INodeVisitable"/> which is stored in the structure.</typeparam>
    public interface IDiffWindow<out TNode> where TNode : INodeVisitable
    {
        /// <summary>
        /// Node for holding the differences.
        /// </summary>
        TNode DiffNode { get; }

        /// <summary>
        /// Method that is called after the calculation of the diff is completed.
        /// </summary>
        /// <param name="task">Task that was used to run the diff calculation.</param>
        void OnDiffComplete(Task task);

        /// <summary>
        /// Method that is called after the merging is completed.
        /// </summary>
        /// <param name="task">Task that was used to run the merge.</param>
        void OnMergeComplete(Task task);
    }
}
