using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoreLibrary.Enums;
using DiffIntegration.DiffFilesystemTree;
using Sverge.Control;
using Sverge.Control.LineMarkers;

namespace Sverge.DiffWindow
{
    /// <summary>
    /// Interaction logic for FolderDiffTwoWay.xaml
    /// </summary>
    public partial class FolderDiffTwoWay : UserControl, IDiffWindow
    {
        private DiffFilesystemTree node;
        private MainWindow window;

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

        public FolderDiffTwoWay(object diffNode)
        {
            node = (DiffFilesystemTree)diffNode;

            InitializeComponent();

            TreeView.ItemsSource = ((DiffDirNode)node.Root).FilesAndDirectories;
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

            window = Application.Current.Windows.OfType<MainWindow>().SingleOrDefault(x => x.IsActive);
            // TODO - predat si window pres nejaky interface?
            window.AddNewTab(diffnode);
        }

        private void FolderDiff2Way_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFolderLocation = PathHelper.TrimPath(node.Root.InfoLocal.FullName, FilePathLabel);
            RemoteFolderLocation = PathHelper.TrimPath(node.Root.InfoRemote.FullName, FilePathLabel);
        }
    }
}
