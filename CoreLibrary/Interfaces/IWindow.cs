namespace CoreLibrary.Interfaces
{
    public interface IWindow
    {
        void AddNewTab(params string[] args);
        void AddNewTab(IFilesystemTreeVisitable diffTree);
    }
}
