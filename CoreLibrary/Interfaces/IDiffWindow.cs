namespace CoreLibrary.Interfaces
{
    public interface IDiffWindow<out TNode> where TNode : IFilesystemTreeVisitable
    {
        TNode DiffNode { get; }

        void OnDiffComplete();

        void OnMergeComplete();
    }
}
