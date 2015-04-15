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
        private readonly IWindow window;

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

        public FolderDiffTwoWay(object diffNode, IWindow window)
        {
            DiffNode = (DiffFilesystemTree)diffNode;
            this.window = window;

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

        private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as TreeViewItem;

            if (item == null)
                return;

            var diffnode = item.Header as DiffFileNode;

            if (diffnode == null)
                return;

            window.AddNewTab(diffnode);
        }

        private void FolderDiff2Way_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFolderLocation = PathHelper.TrimPath(DiffNode.Root.InfoLocal.FullName, FilePathLabel);
            RemoteFolderLocation = PathHelper.TrimPath(DiffNode.Root.InfoRemote.FullName, FilePathLabel);
        }
    }
}
