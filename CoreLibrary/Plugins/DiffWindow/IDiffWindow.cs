using System.Threading.Tasks;
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.Plugins.DiffWindow
{
    public interface IDiffWindow<out TNode> where TNode : IFilesystemTreeVisitable
    {
        TNode DiffNode { get; }

        void OnDiffComplete(Task task);

        void OnMergeComplete(Task task);
    }
}
