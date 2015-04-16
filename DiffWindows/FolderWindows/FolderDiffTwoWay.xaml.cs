using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Plugins.DiffWindow;
using DiffIntegration.DiffFilesystemTree;

namespace DiffWindows.FolderWindows
{
    /// <summary>
    /// Interaction logic for FolderDiffTwoWay.xaml
    /// </summary>
    [DiffWindow(1000)]
    public partial class FolderDiffTwoWay : UserControl, IDiffWindow<DiffFilesystemTree>
    {
        public DiffFilesystemTree DiffNode { get; private set; }
        private readonly IDiffWindowManager manager;

        public static readonly DependencyProperty LocalFolderLocationProperty = DependencyProperty.Register("LocalFolderLocation", typeof(string), typeof(FolderDiffTwoWay));

        public string LocalFolderLocation
        {
            get
            {
                return (string)GetValue(LocalFolderLocationProperty);
            }
            set { SetValue(LocalFolderLocationProperty, value); }
        }

        public static readonly DependencyProperty RemoteFolderLocationProperty = DependencyProperty.Register("RemoteFolderLocation", typeof(string), typeof(FolderDiffTwoWay));

        public string RemoteFolderLocation
        {
            get { return (string)GetValue(RemoteFolderLocationProperty); }
            set { SetValue(RemoteFolderLocationProperty, value); }
        }

        public FolderDiffTwoWay(IFilesystemTreeVisitable diffNode, IDiffWindowManager manager)
        {
            DiffNode = (DiffFilesystemTree)diffNode;
            this.manager = manager;

            InitializeComponent();

            TreeView.ItemsSource = ((DiffDirNode)DiffNode.Root).FilesAndDirectories;
        }

        public static bool CanBeApplied(object instance)
        {
            var filesystemTree = instance as DiffFilesystemTree;

            if (filesystemTree == null)
                return false;

            return filesystemTree.DiffMode == DiffModeEnum.TwoWay;
        }

        public void OnDiffComplete(Task t)
        {
            TreeView.Items.Refresh();
            TreeView.InvalidateVisual();
        }

        public void OnMergeComplete(Task t)
        {
            TreeView.Items.Refresh();
            TreeView.InvalidateVisual();
        }

        private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as TreeViewItem;

            if (item == null)
                return;

            var newDiffNode = item.Header as DiffFileNode;

            if (newDiffNode == null)
                return;

            manager.OpenNewTab(newDiffNode, this);
        }

        private void FolderDiff2Way_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFolderLocation = PathHelper.TrimPath(DiffNode.Root.InfoLocal.FullName, FilePathLabel);
            RemoteFolderLocation = PathHelper.TrimPath(DiffNode.Root.InfoRemote.FullName, FilePathLabel);
        }
    }
}
