using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BasicMenus.ChangesMenu;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Helpers;
using CoreLibrary.Plugins.DiffWindow;

namespace DirectoryDiffWindows
{
    /// <summary>
    /// Interaction logic for DirectoryDiffTwoWay.xaml
    /// </summary>
    [DiffWindow(1000)]
    public partial class DirectoryDiffTwoWay : UserControl, IDiffWindow<FilesystemDiffTree>, IChangesMenu
    {
        public FilesystemDiffTree DiffNode { get; private set; }
        private readonly IDiffWindowManager manager;
        private INodeAbstractNode selectedNode;
        private bool isBusy = true;

        public static readonly DependencyProperty LocalFolderLocationProperty = DependencyProperty.Register("LocalFolderLocation", typeof(string), typeof(DirectoryDiffTwoWay));

        public string LocalFolderLocation
        {
            get
            {
                return (string)GetValue(LocalFolderLocationProperty);
            }
            set { SetValue(LocalFolderLocationProperty, value); }
        }

        public static readonly DependencyProperty RemoteFolderLocationProperty = DependencyProperty.Register("RemoteFolderLocation", typeof(string), typeof(DirectoryDiffTwoWay));

        public string RemoteFolderLocation
        {
            get { return (string)GetValue(RemoteFolderLocationProperty); }
            set { SetValue(RemoteFolderLocationProperty, value); }
        }

        public DirectoryDiffTwoWay(INodeVisitable diffNode, IDiffWindowManager manager)
        {
            DiffNode = (FilesystemDiffTree)diffNode;
            this.manager = manager;

            InitializeComponent();

            TreeView.ItemsSource = ((DirDiffNode)DiffNode.Root).FilesAndDirectories;
            TreeView.Loaded += (sender, args) =>
            {
                var t = sender as TreeView;

                if (t == null) return;

                if (t.SelectedItem == null && t.Items.Count > 0)
                {
                    ((TreeViewItem)t.ItemContainerGenerator.ContainerFromIndex(0)).IsSelected = true;
                }

                selectedNode = (INodeAbstractNode)t.SelectedItem;
            };
        }

        public static bool CanBeApplied(object instance)
        {
            var filesystemTree = instance as FilesystemDiffTree;

            if (filesystemTree == null)
                return false;

            return filesystemTree.DiffMode == DiffModeEnum.TwoWay;
        }

        public void OnDiffComplete(Task t)
        {
            isBusy = false;

            // Items use IPropertyChanged 
            //TreeView.Items.Refresh();
            //TreeView.InvalidateVisual();
        }

        public void OnMergeComplete(Task t)
        {
            isBusy = false;

            // Items use IPropertyChanged 
            //TreeView.Items.Refresh();
            //TreeView.InvalidateVisual();
        }

        private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var t = sender as TreeView;
            if (t != null && t.SelectedItem is INodeFileNode)
            {
                manager.OpenNewTab((INodeAbstractNode)t.SelectedItem, this);
            }
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            selectedNode = e.NewValue as INodeAbstractNode;
        }

        private void FolderDiff2Way_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFolderLocation = PathShortener.TrimPath(DiffNode.Root.InfoLocal.FullName, FilePathLabel);
            RemoteFolderLocation = PathShortener.TrimPath(DiffNode.Root.InfoRemote.FullName, FilePathLabel);
        }

        #region Iterating over TreeView

        public TreeViewItem GetItem(ItemsControl container, object itemToSelect)
        {
            foreach (object item in container.Items)
            {
                var itemContainer = (ItemsControl)container.ItemContainerGenerator.ContainerFromItem(item);

                if (item == itemToSelect)
                    return (TreeViewItem)itemContainer;

                if (itemContainer == null || itemContainer.Items.Count <= 0) continue;

                TreeViewItem treeViewItemFound = GetItem(itemContainer, itemToSelect);

                if (treeViewItemFound != null)
                    return treeViewItemFound;
            }
            return null;
        }

        public TreeViewItem GetPreviousDiffItem(ItemsControl container, object itemToSelect, Func<INodeFileNode, bool> f, ref TreeViewItem previous)
        {
            foreach (object item in container.Items)
            {
                var itemContainer = (TreeViewItem)container.ItemContainerGenerator.ContainerFromItem(item);

                if (item == itemToSelect)
                {
                    return previous;
                }

                var diffFileNode = item as INodeFileNode;
                if (diffFileNode != null && f(diffFileNode))
                {
                    previous = itemContainer;
                }

                if (itemContainer == null || itemContainer.Items.Count <= 0) continue;

                TreeViewItem treeViewItemFound = GetPreviousDiffItem(itemContainer, itemToSelect, f, ref previous);

                if (treeViewItemFound != null)
                    return treeViewItemFound;
            }

            return null;
        }

        public TreeViewItem GetNextDiffItem(ItemsControl container, object itemToSelect, Func<INodeFileNode, bool> f, ref bool wasFound)
        {
            foreach (object item in container.Items)
            {
                var itemContainer = (TreeViewItem)container.ItemContainerGenerator.ContainerFromItem(item);

                if (wasFound && item is INodeFileNode && f((INodeFileNode)item))
                    return itemContainer;

                if (item == itemToSelect) wasFound = true;

                if (itemContainer == null || itemContainer.Items.Count <= 0) continue;

                TreeViewItem treeViewItemFound = GetNextDiffItem(itemContainer, itemToSelect, f, ref wasFound);

                if (treeViewItemFound != null)
                    return treeViewItemFound;
            }

            return null;
        }
        #endregion

        #region Custom ChangesMenu commands

        private static readonly Func<INodeFileNode, bool> NextDiffFunc =
            node => node.Differences != DifferencesStatusEnum.AllSame;

        public CommandBinding PreviousCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    TreeViewItem prev = null;
                    TreeViewItem selectedItem = GetPreviousDiffItem(TreeView, selectedNode, NextDiffFunc, ref prev);
                    selectedItem.IsSelected = true;
                    selectedItem.BringIntoView();
                },
                (sender, args) =>
                {
                    TreeViewItem prev = null;
                    args.CanExecute = GetPreviousDiffItem(TreeView, selectedNode, NextDiffFunc, ref prev) != null;
                });
        }

        public CommandBinding NextCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    bool wasFound = false;
                    TreeViewItem selectedItem = GetNextDiffItem(TreeView, selectedNode, NextDiffFunc, ref wasFound);
                    selectedItem.IsSelected = true;
                    selectedItem.BringIntoView();
                },
                (sender, args) =>
                {
                    bool wasFound = false;
                    args.CanExecute = GetNextDiffItem(TreeView, selectedNode, NextDiffFunc, ref wasFound) != null;
                });
        }

        public CommandBinding RecalculateCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    isBusy = true;
                    manager.RequestDiff(this);
                },
                (sender, args) => args.CanExecute = !isBusy);
        }
        #endregion

    }
}
