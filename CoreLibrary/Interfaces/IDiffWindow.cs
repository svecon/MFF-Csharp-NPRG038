namespace CoreLibrary.Interfaces
{
    public interface IDiffWindow<out TNode>
    {
        TNode DiffNode { get; }
    }
}
