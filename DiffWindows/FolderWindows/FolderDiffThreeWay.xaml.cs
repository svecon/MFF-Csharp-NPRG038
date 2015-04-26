using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.DiffWindow;
using DiffIntegration.DiffFilesystemTree;
using DiffWindows.Menus;
using DiffWindows.TextWindows.Controls;

namespace DiffWindows.FolderWindows
{
    using AN = IFilesystemTreeAbstractNode;

    /// <summary>
    /// Interaction logic for FolderDiffThreeWay.xaml
    /// </summary>
    [DiffWindow(1100)]
    public partial class FolderDiffThreeWay : UserControl, IDiffWindow<DiffFilesystemTree>, IChangesMenu, IMergeMenu
    {
        public DiffFilesystemTree DiffNode { get; private set; }
        private readonly IDiffWindowManager manager;

        public static readonly DependencyProperty LocalFolderLocationProperty
            = DependencyProperty.Register("LocalFolderLocation", typeof(string), typeof(FolderDiffThreeWay));

        public string LocalFolderLocation
        {
            get { return (string)GetValue(LocalFolderLocationProperty); }
            set { SetValue(LocalFolderLocationProperty, value); }
        }

        public static readonly DependencyProperty RemoteFolderLocationProperty
            = DependencyProperty.Register("RemoteFolderLocation", typeof(string), typeof(FolderDiffThreeWay));

        public string RemoteFolderLocation
        {
            get { return (string)GetValue(RemoteFolderLocationProperty); }
            set { SetValue(RemoteFolderLocationProperty, value); }
        }

        public static readonly DependencyProperty BaseFolderLocationProperty
            = DependencyProperty.Register("BaseFolderLocation", typeof(string), typeof(FolderDiffThreeWay));

        public string BaseFolderLocation
        {
            get { return (string)GetValue(BaseFolderLocationProperty); }
            set { SetValue(BaseFolderLocationProperty, value); }
        }

        private AN selectedNode;

        private bool isBusy = false;

        public FolderDiffThreeWay(IFilesystemTreeVisitable diffNode, IDiffWindowManager manager)
        {
            DiffNode = (DiffFilesystemTree)diffNode;
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

                selectedNode = (AN)t.SelectedItem;
            };
        }

        public static bool CanBeApplied(object instance)
        {
            var filesystemTree = instance as DiffFilesystemTree;

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
            if (t != null && t.SelectedItem is IFilesystemTreeFileNode)
            {
                manager.OpenNewTab((AN)t.SelectedItem, this);
            }
        }

        private void FolderDiffThreeWay_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFolderLocation = PathHelper.TrimPath(DiffNode.Root.InfoLocal.FullName, FilePathLabel);
            RemoteFolderLocation = PathHelper.TrimPath(DiffNode.Root.InfoRemote.FullName, FilePathLabel);
            BaseFolderLocation = PathHelper.TrimPath(DiffNode.Root.InfoBase.FullName, FilePathLabel);
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

        public TreeViewItem GetPreviousDiffItem(ItemsControl container, object itemToSelect, Func<DiffFileNode, bool> f, ref TreeViewItem previous)
        {
            foreach (object item in container.Items)
            {
                var itemContainer = (TreeViewItem)container.ItemContainerGenerator.ContainerFromItem(item);

                if (item == itemToSelect)
                {
                    return previous;
                }

                var diffFileNode = item as DiffFileNode;
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

        public TreeViewItem GetNextDiffItem(ItemsControl container, object itemToSelect, Func<DiffFileNode, bool> f, ref bool wasFound)
        {
            foreach (object item in container.Items)
            {
                var itemContainer = (TreeViewItem)container.ItemContainerGenerator.ContainerFromItem(item);

                if (wasFound && item is DiffFileNode && f((DiffFileNode)item))
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

        private static readonly Func<DiffFileNode, bool> NextDiffFunc =
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

        private static readonly Func<DiffFileNode, bool> NextConfFunc =
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

        private static readonly Func<DiffFileNode, bool> NextNonresolvedDiffFunc =
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
                        MessageBox.Show(DiffWindows.Resources.Popup_Conflicts_Resolve, DiffWindows.Resources.Popup_Conflicts, MessageBoxButton.OK);
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
                    ((DiffFileNode)selectedNode).Action = PreferedActionThreeWayEnum.ApplyLocal;
                },
                (sender, args) => args.CanExecute = selectedNode.IsInLocation(LocationCombinationsEnum.OnLocal)
                    && selectedNode is DiffFileNode
            );
        }

        public CommandBinding UseBaseCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    ((DiffFileNode)selectedNode).Action = PreferedActionThreeWayEnum.RevertToBase;
                },
                (sender, args) => args.CanExecute = selectedNode.IsInLocation(LocationCombinationsEnum.OnBase)
                    && selectedNode is DiffFileNode
            );
        }

        public CommandBinding UseRemoteCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    ((DiffFileNode)selectedNode).Action = PreferedActionThreeWayEnum.ApplyRemote;
                },
                (sender, args) => args.CanExecute = selectedNode.IsInLocation(LocationCombinationsEnum.OnRemote)
                    && selectedNode is DiffFileNode
            );
        }

        #endregion

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            selectedNode = e.NewValue as AN;
        }
    }

}