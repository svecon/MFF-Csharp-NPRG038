using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BasicMenus.ChangesMenu;
using BasicMenus.MergeMenu;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Helpers;
using CoreLibrary.Plugins.DiffWindow;

namespace DirectoryDiffWindows
{
    /// <summary>
    /// Interaction logic for DirectoryDiffThreeWay.xaml
    /// </summary>
    [DiffWindow(1100)]
    public partial class DirectoryDiffThreeWay : UserControl, IDiffWindow<FilesystemDiffTree>, IChangesMenu, IMergeMenu
    {
        public FilesystemDiffTree DiffNode { get; private set; }
        private readonly IDiffWindowManager manager;

        public static readonly DependencyProperty LocalFolderLocationProperty
            = DependencyProperty.Register("LocalFolderLocation", typeof(string), typeof(DirectoryDiffThreeWay));

        public string LocalFolderLocation
        {
            get { return (string)GetValue(LocalFolderLocationProperty); }
            set { SetValue(LocalFolderLocationProperty, value); }
        }

        public static readonly DependencyProperty RemoteFolderLocationProperty
            = DependencyProperty.Register("RemoteFolderLocation", typeof(string), typeof(DirectoryDiffThreeWay));

        public string RemoteFolderLocation
        {
            get { return (string)GetValue(RemoteFolderLocationProperty); }
            set { SetValue(RemoteFolderLocationProperty, value); }
        }

        public static readonly DependencyProperty BaseFolderLocationProperty
            = DependencyProperty.Register("BaseFolderLocation", typeof(string), typeof(DirectoryDiffThreeWay));

        public string BaseFolderLocation
        {
            get { return (string)GetValue(BaseFolderLocationProperty); }
            set { SetValue(BaseFolderLocationProperty, value); }
        }

        private INodeAbstractNode selectedNode;

        private bool isBusy = false;

        public DirectoryDiffThreeWay(INodeVisitable diffNode, IDiffWindowManager manager)
        {
            DiffNode = (FilesystemDiffTree)diffNode;
            this.manager = manager;

            InitializeComponent();

            TreeView.ItemsSource = DiffNode.Root.FilesAndDirectories;
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

            return filesystemTree.DiffMode == DiffModeEnum.ThreeWay;
        }

        public void OnDiffComplete(Task t)
        {
            // Items use IPropertyChanged 
            isBusy = false;
        }

        public void OnMergeComplete(Task t)
        {
            // Items use IPropertyChanged 
            //TreeView.Items.Refresh();
            //TreeView.InvalidateVisual();
            isBusy = false;
        }

        private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var t = sender as TreeView;
            if (t != null && t.SelectedItem is INodeFileNode)
            {
                manager.OpenNewTab((INodeAbstractNode)t.SelectedItem, this);
            }
        }

        private void FolderDiffThreeWay_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFolderLocation = PathShortener.TrimPath(DiffNode.Root.InfoLocal.FullName, FilePathLabel);
            RemoteFolderLocation = PathShortener.TrimPath(DiffNode.Root.InfoRemote.FullName, FilePathLabel);
            BaseFolderLocation = PathShortener.TrimPath(DiffNode.Root.InfoBase.FullName, FilePathLabel);
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

        public TreeViewItem GetPreviousDiffItem(ItemsControl container, object itemToSelect, Func<FileDiffNode, bool> f, ref TreeViewItem previous)
        {
            foreach (object item in container.Items)
            {
                var itemContainer = (TreeViewItem)container.ItemContainerGenerator.ContainerFromItem(item);

                if (item == itemToSelect)
                {
                    return previous;
                }

                var diffFileNode = item as FileDiffNode;
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

        public TreeViewItem GetNextDiffItem(ItemsControl container, object itemToSelect, Func<FileDiffNode, bool> f, ref bool wasFound)
        {
            foreach (object item in container.Items)
            {
                var itemContainer = (TreeViewItem)container.ItemContainerGenerator.ContainerFromItem(item);

                if (wasFound && item is FileDiffNode && f((FileDiffNode)item))
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

        private static readonly Func<FileDiffNode, bool> NextDiffFunc =
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

        #region Custom MergeMenu commands

        public CommandBinding PreviousConflictCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    TreeViewItem prev = null;
                    TreeViewItem selectedItem = GetPreviousDiffItem(TreeView, selectedNode, NextConfFunc, ref prev);
                    selectedItem.IsSelected = true;
                    selectedItem.BringIntoView();
                },
                (sender, args) =>
                {
                    TreeViewItem prev = null;
                    args.CanExecute = GetPreviousDiffItem(TreeView, selectedNode, NextConfFunc, ref prev) != null;
                });
        }

        private static readonly Func<FileDiffNode, bool> NextConfFunc =
            node => node.Differences != DifferencesStatusEnum.AllSame && node.Status == NodeStatusEnum.IsConflicting;

        public CommandBinding NextConflictCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    bool wasFound = false;
                    TreeViewItem selectedItem = GetNextDiffItem(TreeView, selectedNode, NextConfFunc, ref wasFound);
                    selectedItem.IsSelected = true;
                    selectedItem.BringIntoView();
                },
                (sender, args) =>
                {
                    bool wasFound = false;
                    args.CanExecute = GetNextDiffItem(TreeView, selectedNode, NextConfFunc, ref wasFound) != null;
                });
        }

        private static readonly Func<FileDiffNode, bool> NextNonresolvedDiffFunc =
            node => node.Differences != DifferencesStatusEnum.AllSame
                && node.Status == NodeStatusEnum.IsConflicting
                && node.Action == PreferedActionThreeWayEnum.Default;

        public CommandBinding MergeCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    bool wasFound = false;
                    TreeViewItem selectedItem = GetNextDiffItem(TreeView, DiffNode.Root.FilesAndDirectories.First(), NextNonresolvedDiffFunc, ref wasFound);

                    if (selectedItem != null)
                    {
                        selectedItem.IsSelected = true;
                        selectedItem.BringIntoView();
                        MessageBox.Show(DirectoryDiffWindows.Resources.Popup_Conflicts_Resolve, DirectoryDiffWindows.Resources.Popup_Conflicts, MessageBoxButton.OK);
                        return;
                    }

                    isBusy = true;
                    manager.RequestMerge(this);
                },
                (sender, args) =>
                {
                    args.CanExecute = !isBusy && DiffNode.Root.FilesAndDirectories.Any();
                }
            );
        }

        public CommandBinding UseLocalCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    ((FileDiffNode)selectedNode).Action = PreferedActionThreeWayEnum.ApplyLocal;
                },
                (sender, args) => args.CanExecute = selectedNode.IsInLocation(LocationCombinationsEnum.OnLocal)
                    && selectedNode is FileDiffNode
            );
        }

        public CommandBinding UseBaseCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    ((FileDiffNode)selectedNode).Action = PreferedActionThreeWayEnum.RevertToBase;
                },
                (sender, args) => args.CanExecute = selectedNode.IsInLocation(LocationCombinationsEnum.OnBase)
                    && selectedNode is FileDiffNode
            );
        }

        public CommandBinding UseRemoteCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    ((FileDiffNode)selectedNode).Action = PreferedActionThreeWayEnum.ApplyRemote;
                },
                (sender, args) => args.CanExecute = selectedNode.IsInLocation(LocationCombinationsEnum.OnRemote)
                    && selectedNode is FileDiffNode
            );
        }

        #endregion

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            selectedNode = e.NewValue as INodeAbstractNode;
        }
    }

}