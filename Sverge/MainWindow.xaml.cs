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
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.DiffWindow;
using CoreLibrary.Plugins.Processors;
using DiffIntegration.DiffFilesystemTree;

namespace Sverge
{
    using TV = IFilesystemTreeVisitable;
    using DW = IDiffWindow<IFilesystemTreeVisitable>;

    /// <summary>
    /// Interaction logic for diffManager.xaml
    /// </summary>
    public partial class MainWindow : Window, IDiffWindowManager
    {
        private readonly ProcessorRunner runner;
        private readonly IProcessorLoader loader;
        private readonly DiffWindowLoader windowLoader;
        private readonly Dictionary<TV, int> tabsPositions;
        private readonly Dictionary<DW, DW> parentWindows;
        private int windowMenusAdded;
        private int windowMenusBindingsAdded;

        public MainWindow(IProcessorLoader processorLoader)
        {
            loader = processorLoader;
            runner = new ProcessorRunner(processorLoader);

            windowLoader = new DiffWindowLoader(this);
            windowLoader.LoadWindows();
            windowLoader.LoadWindowMenus();

            tabsPositions = new Dictionary<TV, int>();
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
            IFilesystemTreeVisitable diffTree;

            try
            {
                if (args.Length == 2 && areArgsFiles > 0)
                {
                    diffTree = new DiffFileNode(args[0], args[1]);
                } else if (args.Length == 3 && areArgsFiles > 0)
                {
                    diffTree = new DiffFileNode(args[0], args[1], args[2]);
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

        private void CallDiffContinuations(Task t, DW window)
        {
            window.OnDiffComplete(t);

            foreach (DW childrenWindow in parentWindows.Where(p => p.Value == window).Select(p => p.Key))
                childrenWindow.OnDiffComplete(t);

            if (!parentWindows.ContainsKey(window)) return;
            parentWindows[window].OnDiffComplete(t);
        }

        private void CallMergeContinuations(Task t, DW window)
        {
            window.OnMergeComplete(t);

            foreach (DW childrenWindow in parentWindows.Where(p => p.Value == window).Select(p => p.Key))
                childrenWindow.OnMergeComplete(t);

            if (!parentWindows.ContainsKey(window)) return;
            parentWindows[window].OnMergeComplete(t);
        }

#pragma warning disable 4014
        public void RequestDiff(DW window)
        {
            runner.RunDiff(window.DiffNode).ContinueWith(t => CallDiffContinuations(t, window)
                , TaskContinuationOptions.ExecuteSynchronously);
        }

        public void RequestMerge(DW window)
        {
            runner.RunMerge(window.DiffNode).ContinueWith(t =>
            {
                CallMergeContinuations(t, window);

                runner.RunDiff(window.DiffNode).ContinueWith(tt => CallDiffContinuations(tt, window)
                    , TaskContinuationOptions.ExecuteSynchronously);
            }, TaskContinuationOptions.ExecuteSynchronously);

        }
#pragma warning restore 4014

        public DW OpenNewTab(TV diffNode, DW parentWindow = null)
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
            var node = diffNode as IFilesystemTreeFileNode;
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

        private void RemoveWindow(int position)
        {
            tabsPositions.Remove(((DW)((TabItem)Tabs.Items.GetItemAt(position)).Content).DiffNode);
            Tabs.Items.RemoveAt(position);
        }

        private void RemoveWindow(TabItem item)
        {
            tabsPositions.Remove(((DW)item.Content).DiffNode);
            Tabs.Items.Remove(item);
        }

        public void OpenWindowDialog()
        {
            var open = new OpenDialogWindow(this);
            open.ShowDialog();
        }

        private void TabCloseImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            RemoveWindow(FindParent<TabItem>(sender as DependencyObject));
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            //check if the parent matches the type we're looking for
            var parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }


        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenWindowDialog();
        }

        public static readonly RoutedUICommand CloseWindow = new RoutedUICommand(
            "CloseWindow", "CloseWindow",
            typeof(MainWindow),
            new InputGestureCollection() { 
                new KeyGesture(Key.W, ModifierKeys.Control)
            }
        );

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Tabs.SelectedIndex != -1;
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RemoveCustomWindowMenuBindings();
            CloseCustomWindowMenus();

            RemoveWindow(Tabs.SelectedIndex);
        }

        public static readonly RoutedUICommand Exit = new RoutedUICommand(
            "Exit", "Exit",
            typeof(MainWindow),
            new InputGestureCollection() { 
                new KeyGesture(Key.F4, ModifierKeys.Alt)
            }
        );

        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown(); // @TODO Exit code
        }

        private void CloseCustomWindowMenus()
        {
            for (; windowMenusAdded > 0; windowMenusAdded--)
            {
                Menu.Items.RemoveAt(Menu.Items.Count - 1);
            }
        }

        private void RemoveCustomWindowMenuBindings()
        {
            for (; windowMenusBindingsAdded > 0; windowMenusBindingsAdded--)
            {
                CommandBindings.RemoveAt(CommandBindings.Count - 1);
            }
        }

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

        public static readonly RoutedUICommand ProcessorSettings = new RoutedUICommand(
            "ProcessorSettings", "ProcessorSettings",
            typeof(MainWindow),
            new InputGestureCollection() {
            }
        );

        private void ProcessorSettings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new ProcessorSettingsWindow(loader);
            window.ShowDialog();
        }
    }
}
