using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CoreLibrary.Exceptions.NotFound;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.DiffWindow;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;

namespace Sverge
{
    using DW = IDiffWindow<INodeVisitable>;

    /// <summary>
    /// Main window of the application that shows different visualisations.
    /// </summary>
    public partial class MainWindow : Window, IDiffWindowManager
    {
        /// <summary>
        /// ProcessorRunner for executing processors asynchronously.
        /// </summary>
        private readonly ProcessorRunner runner;

        /// <summary>
        /// Loader containing all available processors.
        /// </summary>
        private readonly IProcessorLoader loader;

        /// <summary>
        /// DiffWindow loader for loading all visualisations plugins.
        /// </summary>
        private readonly DiffWindowLoader windowLoader;

        /// <summary>
        /// Dictionary for nodes and their tab positions.
        /// </summary>
        private readonly Dictionary<INodeVisitable, int> tabsPositions;

        /// <summary>
        /// Dictionary for relations of windows and their parent windows.
        /// </summary>
        private readonly Dictionary<DW, DW> parentWindows;

        /// <summary>
        /// Number of plugin menus added for current window.
        /// </summary>
        private int windowMenusAdded;

        /// <summary>
        /// Number of plugin menu actions added to the current scope.
        /// </summary>
        private int windowMenusBindingsAdded;

        /// <summary>
        /// Settings for closing application after merge.
        /// </summary>
        [Settings("Close the application after merging is completed.", "close-merge")]
        public static bool CloseAfterMerge;

        /// <summary>
        /// Initializes new node of the <see cref="MainWindow"/>
        /// </summary>
        /// <param name="processorLoader">Processor loader containing all loaded processors.</param>
        public MainWindow(IProcessorLoader processorLoader)
        {
            loader = processorLoader;
            runner = new ProcessorRunner(processorLoader);

            windowLoader = new DiffWindowLoader(this);
            windowLoader.LoadWindows();
            windowLoader.LoadWindowMenus();

            tabsPositions = new Dictionary<INodeVisitable, int>();
            parentWindows = new Dictionary<DW, DW>();

            InitializeComponent();
        }

        /// <summary>
        /// Determine if the path is an existing file.
        /// </summary>
        /// <param name="path">Path to the file to be checked</param>
        /// <returns>1 if the path is an existing file.</returns>
        private static int IsFile(string path)
        {
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(@path);

            //detect whether its a directory or file
            return (attr & FileAttributes.Directory) == FileAttributes.Directory ? 0 : 1;
        }

        /// <inheritdoc />
        public DW OpenNewTab(params string[] args)
        {
            #region Wrong number of arguments
            if (args.Length != 2 && args.Length != 3)
            {
                MessageBox.Show(
                    Properties.Resources.App_NewTab_ErrorArgumentsNumberText,
                    Properties.Resources.App_NewTab_ErrorArguments
                );
                return null;
            }
            #endregion

            #region Constructs simple mask - 1 if path is a file

            int areArgsFiles = 0;

            try
            {
                foreach (string s in args)
                {
                    areArgsFiles <<= 1;
                    areArgsFiles |= IsFile(s);
                }
            } catch (FileNotFoundException)
            { // do nothing, we will catch it later
            } catch (DirectoryNotFoundException)
            { // do nothing, we will catch it later
            }
            #endregion

            #region Creating main structure
            INodeVisitable diffTree;

            try
            {
                if (args.Length == 2 && areArgsFiles > 0)
                {
                    diffTree = new FileDiffNode(args[0], args[1]);
                } else if (args.Length == 3 && areArgsFiles > 0)
                {
                    diffTree = new FileDiffNode(args[0], args[1], args[2]);
                } else if (args.Length == 2 && areArgsFiles == 0)
                {
                    diffTree = new DiffCrawler().InitializeCrawler(args[0], args[1]).TraverseTree();
                } else if (args.Length == 3 && areArgsFiles == 0)
                {
                    diffTree = new DiffCrawler().InitializeCrawler(args[0], args[1], args[2]).TraverseTree();
                } else
                {
                    MessageBox.Show(
                        Properties.Resources.App_NewTab_ErrorArgumentsNumberText,
                        Properties.Resources.App_NewTab_MixingFilesAndFolders
                    );
                    return null;
                }

                #region Catch all possible NotFoundExceptions
            } catch (LocationFileNotFoundException e)
            {
                MessageBox.Show(
                    String.Format(Properties.Resources.App_NewTab_FileNotFound, e.Info.FullName),
                    Properties.Resources.App_NewTab_MixingFilesAndFolders
                );
                return null;
            } catch (LocationDirectoryNotFoundException e)
            {
                MessageBox.Show(
                    String.Format(Properties.Resources.App_NewTab_DirectoryNotFound, e.Info.FullName),
                    Properties.Resources.App_NewTab_MixingFilesAndFolders
                );
                return null;
            }
                #endregion

            #endregion

            DW newWindow = OpenNewTab(diffTree);
            RequestDiff(newWindow);

            return newWindow;
        }

        /// <summary>
        /// Calls OnDiffComplete method on all related windows.
        /// </summary>
        /// <param name="t">Task used to calculate the diff.</param>
        /// <param name="window">Window that requestes the calculation.</param>
        private void CallDiffContinuations(Task t, DW window)
        {
            window.OnDiffComplete(t);

            foreach (DW childrenWindow in parentWindows.Where(p => p.Value == window).Select(p => p.Key))
                childrenWindow.OnDiffComplete(t);

            if (!parentWindows.ContainsKey(window)) return;
            parentWindows[window].OnDiffComplete(t);
        }

        /// <summary>
        /// Calls OnMergeComplete method on all related windows.
        /// </summary>
        /// <param name="t">Task used to merge.</param>
        /// <param name="window">Window that requestes the merge.</param>
        private void CallMergeContinuations(Task t, DW window)
        {
            window.OnMergeComplete(t);

            foreach (DW childrenWindow in parentWindows.Where(p => p.Value == window).Select(p => p.Key))
                childrenWindow.OnMergeComplete(t);

            if (!parentWindows.ContainsKey(window)) return;
            parentWindows[window].OnMergeComplete(t);
        }

#pragma warning disable 4014

        /// <inheritdoc />
        public void RequestDiff(DW window)
        {
            runner.RunDiff(window.DiffNode).ContinueWith(t => CallDiffContinuations(t, window)
                , TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <inheritdoc />
        public void RequestMerge(DW window)
        {
            runner.RunMerge(window.DiffNode).ContinueWith(t =>
            {
                if (CloseAfterMerge)
                {
                    Close();
                    return;
                }

                CallMergeContinuations(t, window);

                runner.RunDiff(window.DiffNode).ContinueWith(tt => CallDiffContinuations(tt, window)
                    , TaskContinuationOptions.ExecuteSynchronously);
            }, TaskContinuationOptions.ExecuteSynchronously);

        }
#pragma warning restore 4014

        /// <inheritdoc />
        public DW OpenNewTab(INodeVisitable diffNode, DW parentWindow = null)
        {
            int tabPosition;
            if (tabsPositions.TryGetValue(diffNode, out tabPosition))
            {
                Tabs.SelectedIndex = tabPosition;
                return null;
            }

            DW newWindow = windowLoader.CreateWindowFor(diffNode);

            if (parentWindow != null)
                parentWindows.Add(newWindow, parentWindow);

            string header;
            var node = diffNode as INodeFileNode;
            if (node != null)
            {
                header = node.Info.Name;
            } else
            {
                var tree = diffNode as IFilesystemTree;
                header = tree != null ? tree.Root.Info.Name : "Unknown";
            }

            Tabs.Items.Add(new TabItem {
                Header = header,
                Content = newWindow,
                IsSelected = true
            });

            tabsPositions.Add(diffNode, Tabs.SelectedIndex);

            return newWindow;
        }

        /// <summary>
        /// Removes a window from tab.
        /// </summary>
        /// <param name="position">Position of a window.</param>
        private void RemoveWindow(int position)
        {
            tabsPositions.Remove(((DW)((TabItem)Tabs.Items.GetItemAt(position)).Content).DiffNode);
            Tabs.Items.RemoveAt(position);
        }

        /// <summary>
        /// Removes a window from tab.
        /// </summary>
        /// <param name="item">Instance of tab item.</param>
        private void RemoveWindow(TabItem item)
        {
            tabsPositions.Remove(((DW)item.Content).DiffNode);
            Tabs.Items.Remove(item);
        }

        /// <summary>
        /// Opens the <see cref="OpenDialogWindow"/>
        /// </summary>
        public void OpenWindowDialog()
        {
            var open = new OpenDialogWindow(this);
            open.ShowDialog();
        }

        /// <summary>
        /// Event handler for closing button on tab items.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Mouse event args.</param>
        private void TabCloseImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            RemoveCustomWindowMenuBindings();
            CloseCustomWindowMenus();

            RemoveWindow(FindParent<TabItem>(sender as DependencyObject));
        }

        /// <summary>
        /// Find parent in the VisualTree.
        /// </summary>
        /// <typeparam name="T">Type of the parent.</typeparam>
        /// <param name="child">Instance from where to start searching from.</param>
        /// <returns>Instance of a parent.</returns>
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            //check if the parent matches the type we're looking for
            var parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }

        /// <summary>
        /// Can the new command be executed?
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Can execute event args.</param>
        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !CloseAfterMerge;
        }

        /// <summary>
        /// Opens a window for opening new comparison.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Executed event args.</param>
        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenWindowDialog();
        }

        /// <summary>
        /// Command for closing a visualisation window.
        /// </summary>
        public static readonly RoutedUICommand CloseWindow = new RoutedUICommand(
            "CloseWindow", "CloseWindow",
            typeof(MainWindow),
            new InputGestureCollection() { 
                new KeyGesture(Key.W, ModifierKeys.Control)
            }
        );

        /// <summary>
        /// Checks whether any tab window is opened.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Executed event args.</param>
        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Tabs.SelectedIndex != -1;
        }

        /// <summary>
        /// Closes current visualisation window.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Executed event args.</param>
        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RemoveCustomWindowMenuBindings();
            CloseCustomWindowMenus();

            RemoveWindow(Tabs.SelectedIndex);
        }

        /// <summary>
        /// Command for exiting the application.
        /// </summary>
        public static readonly RoutedUICommand Exit = new RoutedUICommand(
            "Exit", "Exit",
            typeof(MainWindow),
            new InputGestureCollection() { 
                new KeyGesture(Key.F4, ModifierKeys.Alt)
            }
        );

        /// <summary>
        /// Exits the application.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Executed event args.</param>
        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown(); // TODO Exit code
        }

        /// <summary>
        /// Closes all menus which were implemented by current diffwindow.
        /// </summary>
        private void CloseCustomWindowMenus()
        {
            for (; windowMenusAdded > 0; windowMenusAdded--)
            {
                Menu.Items.RemoveAt(Menu.Items.Count - 1);
            }
        }

        /// <summary>
        /// Removes custom menu actins which were defined by current diffwindow..
        /// </summary>
        private void RemoveCustomWindowMenuBindings()
        {
            for (; windowMenusBindingsAdded > 0; windowMenusBindingsAdded--)
            {
                CommandBindings.RemoveAt(CommandBindings.Count - 1);
            }
        }

        /// <summary>
        /// Changes current visualisation window.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void Tabs_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.Source is TabControl))
                return;

            if (Tabs.SelectedIndex == -1)
                return;

            RemoveCustomWindowMenuBindings();
            CloseCustomWindowMenus();

            IEnumerable<IDiffWindowMenu> windows = windowLoader.CreateDiffWindowMenus(
                ((TabItem)Tabs.Items.GetItemAt(Tabs.SelectedIndex)).Content
            );

            foreach (IDiffWindowMenu diffWindowMenu in windows)
            {
                MenuItem item = diffWindowMenu.CreateMenuItem();
                Menu.Items.Add(item);

                item.Style = (Style)Resources["WindowMenuItemStyle"];

                windowMenusAdded++;

                foreach (CommandBinding commandBinding in diffWindowMenu.CommandBindings())
                {
                    CommandBindings.Add(commandBinding);

                    windowMenusBindingsAdded++;
                }
            }
        }

        /// <summary>
        /// Command for showin the processor window.
        /// </summary>
        public static readonly RoutedUICommand ProcessorSettings = new RoutedUICommand(
            "ProcessorSettings", "ProcessorSettings",
            typeof(MainWindow),
            new InputGestureCollection());

        /// <summary>
        /// Opens a window for processor settings.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Executed event args.</param>
        private void ProcessorSettings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new ProcessorSettingsWindow(loader);
            window.ShowDialog();
        }
    }
}
