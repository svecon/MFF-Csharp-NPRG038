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
    /// Plugin for visualising differences between three directories.
    /// </summary>
    [DiffWindow(1100)]
    public partial class DirectoryDiffThreeWay : UserControl, IDiffWindow<FilesystemDiffTree>, IChangesMenu, IMergeMenu
    {
        /// <inheritdoc />
        public FilesystemDiffTree DiffNode { get; private set; }

        /// <summary>
        /// An instance of <see cref="IDiffWindowManager"/> that manages current window.
        /// </summary>
        private readonly IDiffWindowManager manager;

        #region Dependency properties

        /// <summary>
        /// Dependency property for <see cref="LocalFolderLocation"/>
        /// </summary>
        public static readonly DependencyProperty LocalFolderLocationProperty
            = DependencyProperty.Register("LocalFolderLocation", typeof(string), typeof(DirectoryDiffThreeWay));

        /// <summary>
        /// String path to the local folder.
        /// </summary>
        public string LocalFolderLocation
        {
            get { return (string)GetValue(LocalFolderLocationProperty); }
            set { SetValue(LocalFolderLocationProperty, value); }
        }

        /// <summary>
        /// Dependency property for <see cref="RemoteFolderLocation"/>
        /// </summary>
        public static readonly DependencyProperty RemoteFolderLocationProperty
            = DependencyProperty.Register("RemoteFolderLocation", typeof(string), typeof(DirectoryDiffThreeWay));


        /// <summary>
        /// String path to the remote folder.
        /// </summary>
        public string RemoteFolderLocation
        {
            get { return (string)GetValue(RemoteFolderLocationProperty); }
            set { SetValue(RemoteFolderLocationProperty, value); }
        }

        /// <summary>
        /// Dependency property for <see cref="BaseFolderLocation"/>
        /// </summary>
        public static readonly DependencyProperty BaseFolderLocationProperty
            = DependencyProperty.Register("BaseFolderLocation", typeof(string), typeof(DirectoryDiffThreeWay));

        /// <summary>
        /// String path to the base folder.
        /// </summary>
        public string BaseFolderLocation
        {
            get { return (string)GetValue(BaseFolderLocationProperty); }
            set { SetValue(BaseFolderLocationProperty, value); }
        }

        #endregion

        /// <summary>
        /// Selected node in the treeview.
        /// </summary>
        private INodeAbstractNode selectedNode;

        /// <summary>
        /// Is the actual diff being recalculated?
        /// </summary>
        private bool isBusy = true;

        /// <summary>
        /// Initializes new instance of the <see cref="DirectoryDiffThreeWay"/>
        /// </summary>
        /// <param name="diffNode">Diff node holding the calculated diff.</param>
        /// <param name="manager">Manager for this window.</param>
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

            FilePathLocal.ToolTip = DiffNode.Root.InfoLocal.FullName;
            FilePathBase.ToolTip = DiffNode.Root.InfoBase.FullName;
            FilePathRemote.ToolTip = DiffNode.Root.InfoRemote.FullName;
        }

        /// <summary>
        /// Can this <see cref="IDiffWindow{TNode}"/> be applied to given instance?
        /// </summary>
        /// <param name="instance">Instance holding the calculated diff.</param>
        /// <returns>True if this plugin can be applied.</returns>
        public static bool CanBeApplied(object instance)
        {
            var filesystemTree = instance as FilesystemDiffTree;

            if (filesystemTree == null)
                return false;

            return filesystemTree.DiffMode == DiffModeEnum.ThreeWay;
        }

        /// <inheritdoc />
        public void OnDiffComplete(Task t)
        {
            // Items use IPropertyChanged 
            isBusy = false;
        }

        /// <inheritdoc />
        public void OnMergeComplete(Task t)
        {
            // Items use IPropertyChanged 
            //TreeView.Items.Refresh();
            //TreeView.InvalidateVisual();
            isBusy = false;
        }

        /// <summary>
        /// Opens a new window when double-clicking on a TreeView
        /// </summary>
        /// <param name="sender">TreeView</param>
        /// <param name="e">Event arguments</param>
        private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var t = sender as TreeView;
            if (t != null && t.SelectedItem is INodeFileNode)
            {
                manager.OpenNewTab((INodeAbstractNode)t.SelectedItem, this);
            }
        }

        #region Iterating over TreeView

        /// <summary>
        /// Finds a TreeViewItem container with an instance.
        /// </summary>
        /// <param name="container">Container where to search from.</param>
        /// <param name="itemToSelect">Needle</param>
        /// <returns>TreeViewItem</returns>
        private TreeViewItem GetItem(ItemsControl container, object itemToSelect)
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

        /// <summary>
        /// Find a previous TreeViewItem container to a given instance.
        /// </summary>
        /// <param name="container">Container where to search from.</param>
        /// <param name="itemToSelect">Needle</param>
        /// <param name="f">Additional criteria for needle.</param>
        /// <param name="previous">Container for previous instance.</param>
        /// <returns>Container for the given instance.</returns>
        private TreeViewItem GetPreviousDiffItem(ItemsControl container, object itemToSelect, Func<FileDiffNode, bool> f, ref TreeViewItem previous)
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

        /// <summary>
        /// Find a next TreeViewItem container to a given instance.
        /// </summary>
        /// <param name="container">Container where to search from.</param>
        /// <param name="itemToSelect">Needle</param>
        /// <param name="f">Additional criteria for needle.</param>
        /// <param name="wasFound">Was next container found?</param>
        /// <returns>Container for the given instance.</returns>
        private TreeViewItem GetNextDiffItem(ItemsControl container, object itemToSelect, Func<FileDiffNode, bool> f, ref bool wasFound)
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

        /// <summary>
        /// Criteria function for finding next node that is different.
        /// </summary>
        private static readonly Func<FileDiffNode, bool> NextDiffFunc =
            node => node.Differences != DifferencesStatusEnum.AllSame;

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <summary>
        /// Criteria function for finding next node that is conflicting.
        /// </summary>
        private static readonly Func<FileDiffNode, bool> NextConfFunc =
            node => node.Differences != DifferencesStatusEnum.AllSame && node.Status == NodeStatusEnum.IsConflicting;

        /// <inheritdoc />
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

        /// <summary>
        /// Criteria function for finding node that has not been resolved yet.
        /// </summary>
        private static readonly Func<FileDiffNode, bool> NextNonresolvedDiffFunc =
            node => node.Differences != DifferencesStatusEnum.AllSame
                && node.Status == NodeStatusEnum.IsConflicting
                && node.Action == PreferedActionThreeWayEnum.Default;

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <summary>
        /// Update selected node when a new TreeView item is changed.
        /// </summary>
        /// <param name="sender">TreeView</param>
        /// <param name="e">Changed event args</param>
        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            selectedNode = e.NewValue as INodeAbstractNode;
        }

        /// <summary>
        /// Update text paths when the window is loaded and sizes are calculated.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void DirectoryDiffThreeWay_OnLoaded(object sender, RoutedEventArgs e)
        {
            LocalFolderLocation = PathShortener.TrimPath(DiffNode.Root.InfoLocal.FullName, FilePathLocal);
            RemoteFolderLocation = PathShortener.TrimPath(DiffNode.Root.InfoRemote.FullName, FilePathBase);
            BaseFolderLocation = PathShortener.TrimPath(DiffNode.Root.InfoBase.FullName, FilePathRemote);
        }
    }

}