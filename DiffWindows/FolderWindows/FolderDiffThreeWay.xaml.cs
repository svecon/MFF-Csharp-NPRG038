using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
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
            if (sender is TreeView)
            {
                var t = sender as TreeView;
                if (t.SelectedItem is DiffFileNode)
                {
                    manager.OpenNewTab((AN)t.SelectedItem, this);
                }
            }

            var item = sender as TreeViewItem;

            if (item == null)
                return;

            var newDiffNode = item.Header as DiffFileNode;

            if (newDiffNode == null)
                return;

            manager.OpenNewTab(newDiffNode, this);
        }

        private void FolderDiffThreeWay_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFolderLocation = PathHelper.TrimPath(DiffNode.Root.InfoLocal.FullName, FilePathLabel);
            RemoteFolderLocation = PathHelper.TrimPath(DiffNode.Root.InfoRemote.FullName, FilePathLabel);
            BaseFolderLocation = PathHelper.TrimPath(DiffNode.Root.InfoBase.FullName, FilePathLabel);
        }

        #region Custom ChangesMenu commands

        public TreeViewItem GetPreviousDiffItem(ItemsControl container, object itemToSelect, Func<DiffFileNode, bool> f, ref TreeViewItem previous)
        {
            foreach (object item in container.Items)
            {
                var itemContainer = (TreeViewItem)container.ItemContainerGenerator.ContainerFromItem(item);

                if (item == itemToSelect)
                {
                    return previous;
                }

                if (item is DiffFileNode && f((DiffFileNode)item))
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

        public RoutedUICommand PreviousCommand()
        {
            return Previous;
        }

        private static readonly Func<DiffFileNode, bool> NextDiffFunc =
            node => node.Differences != DifferencesStatusEnum.AllSame;

        public CommandBinding PreviousCommandBinding()
        {
            return new CommandBinding(Previous,
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

        public static RoutedUICommand Previous = new RoutedUICommand("Previous", "Previous", typeof(TextDiff3Area),
                new InputGestureCollection() { new KeyGesture(Key.F7) }
            );


        public RoutedUICommand NextCommand()
        {
            return Next;
        }

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

        public CommandBinding NextCommandBinding()
        {
            return new CommandBinding(Next,
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

        public static RoutedUICommand Next = new RoutedUICommand("Next", "Next", typeof(TextDiff3Area),
                new InputGestureCollection() { new KeyGesture(Key.F8) }
            );

        #endregion

        #region Custom MergeMenu commands

        public RoutedUICommand PreviousConflictCommand() { return PreviousConflict; }

        public CommandBinding PreviousConflictCommandBinding()
        {
            return new CommandBinding(PreviousConflict,
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

        public static RoutedUICommand PreviousConflict = new RoutedUICommand("PreviousConflict", "PreviousConflict", typeof(TextDiff3Area),
            new InputGestureCollection() { new KeyGesture(Key.D9, ModifierKeys.Control) }
        );

        public RoutedUICommand NextConflictCommand() { return NextConflict; }

        private static readonly Func<DiffFileNode, bool> NextConfFunc =
            node => node.Differences != DifferencesStatusEnum.AllSame && node.Status == NodeStatusEnum.IsConflicting;

        public CommandBinding NextConflictCommandBinding()
        {
            return new CommandBinding(NextConflict, (sender, args) =>
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

        public static RoutedUICommand NextConflict = new RoutedUICommand("NextConflict", "NextConflict", typeof(TextDiff3Area),
            new InputGestureCollection() { new KeyGesture(Key.D0, ModifierKeys.Control) }
        );

        private static readonly Func<DiffFileNode, bool> NextNonresolvedDiffFunc =
            node => node.Differences != DifferencesStatusEnum.AllSame
                && node.Status == NodeStatusEnum.IsConflicting
                && node.Action == PreferedActionEnum.Default;

        public RoutedUICommand MergeCommand() { return Merge; }

        public CommandBinding MergeCommandBinding()
        {
            return new CommandBinding(Merge,
                (sender, args) =>
                {
                    bool wasFound = false;
                    TreeViewItem selectedItem = GetNextDiffItem(TreeView, DiffNode.Root.FilesAndDirectories.First(), NextNonresolvedDiffFunc, ref wasFound);

                    if (selectedItem != null)
                    {
                        selectedItem.IsSelected = true;
                        selectedItem.BringIntoView();
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

        public static RoutedUICommand Merge = new RoutedUICommand("Merge", "Merge", typeof(TextDiff3Area),
            new InputGestureCollection() { new KeyGesture(Key.M, ModifierKeys.Control) }
        );

        public RoutedUICommand UseLocalCommand() { return UseLocal; }

        public static RoutedUICommand UseLocal = new RoutedUICommand("UseLocal", "UseLocal", typeof(TextDiff3Area),
            new InputGestureCollection() { new KeyGesture(Key.I, ModifierKeys.Control) }
        );

        public CommandBinding UseLocalCommandBinding()
        {
            return new CommandBinding(UseLocal,
                (sender, args) =>
                {
                    ((DiffFileNode)selectedNode).Action = PreferedActionEnum.ApplyLocal;
                },
                (sender, args) => args.CanExecute = selectedNode.IsInLocation(LocationCombinationsEnum.OnLocal)
                    && selectedNode is DiffFileNode
            );
        }

        public RoutedUICommand UseBaseCommand() { return UseBase; }

        public static RoutedUICommand UseBase = new RoutedUICommand("UseBase", "UseBase", typeof(TextDiff3Area),
            new InputGestureCollection() { new KeyGesture(Key.O, ModifierKeys.Control) }
        );

        public CommandBinding UseBaseCommandBinding()
        {
            return new CommandBinding(UseBase,
                (sender, args) =>
                {
                    ((DiffFileNode)selectedNode).Action = PreferedActionEnum.RevertToBase;
                },
                (sender, args) => args.CanExecute = selectedNode.IsInLocation(LocationCombinationsEnum.OnBase)
                    && selectedNode is DiffFileNode
            );
        }

        public RoutedUICommand UseRemoteCommand() { return UseRemote; }

        public static RoutedUICommand UseRemote = new RoutedUICommand("UseRemote", "UseRemote", typeof(TextDiff3Area),
            new InputGestureCollection() { new KeyGesture(Key.P, ModifierKeys.Control) }
        );

        public CommandBinding UseRemoteCommandBinding()
        {
            return new CommandBinding(UseRemote,
                (sender, args) =>
                {
                    ((DiffFileNode)selectedNode).Action = PreferedActionEnum.ApplyRemote;
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