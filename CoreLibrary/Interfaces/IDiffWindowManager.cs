namespace CoreLibrary.Interfaces
{
    using TV = IFilesystemTreeVisitable;

    public interface IDiffWindowManager
    {
        IDiffWindow<TV> OpenNewTab(params string[] args);
        IDiffWindow<TV> OpenNewTab(TV diffNode, IDiffWindow<TV> treeNode = null);

        void RequestDiff(IDiffWindow<TV> window);

        void RequestMerge(IDiffWindow<TV> window);
    }
}
