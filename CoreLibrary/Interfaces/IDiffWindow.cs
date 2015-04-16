using System.Threading.Tasks;

namespace CoreLibrary.Interfaces
{
    public interface IDiffWindow<out TNode> where TNode : IFilesystemTreeVisitable
    {
        TNode DiffNode { get; }

        void OnDiffComplete(Task task);

        void OnMergeComplete(Task t);
    }
}
