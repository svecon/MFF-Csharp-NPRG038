using System;
using System.Collections.Generic;
using System.IO;
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
using CoreLibrary.Exceptions.NotFound;
using CoreLibrary.FilesystemTree.Visitors;
using CoreLibrary.Interfaces;
using DiffIntegration.DiffFilesystemTree;
using DiffIntegration.Processors.Preprocessors;
using DiffIntegration.Processors.Processors;

namespace Sverge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IProcessorLoader loader;
        private readonly DiffWindowLoader windowLoader;

        public MainWindow(IProcessorLoader processorLoader)
        {
            loader = processorLoader;
            windowLoader = new DiffWindowLoader();

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

        public void AddNewTab(params string[] args)
        {
            #region Wrong number of arguments
            if (args.Length != 2 && args.Length != 3)
            {
                MessageBox.Show(
                    Properties.Resources.App_NewTab_ErrorArgumentsNumberText,
                    Properties.Resources.App_NewTab_ErrorArgumentsNumber
                );
                return;
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
                    Console.WriteLine("You can not mix folders and files together as arguments.");
                    return;
                }

                #region Catch all possible NotFoundExceptions
            } catch (LocalFileNotFoundException e)
            {
                Console.WriteLine(e);
                return;
            } catch (BaseFileNotFoundException e)
            {
                Console.WriteLine(e);
                return;
            } catch (RemoteFileNotFoundException e)
            {
                Console.WriteLine(e);
                return;
            } catch (LocalDirectoryNotFoundException e)
            {
                Console.WriteLine(e);
                return;
            } catch (BaseDirectoryNotFoundException e)
            {
                Console.WriteLine(e);
                return;
            } catch (RemoteDirectoryNotFoundException e)
            {
                Console.WriteLine(e);
                return;
            }
                #endregion

            #endregion

            #region Run Processors
            // run preprocessors and calculate diffs in parallel 
            IExecutionVisitor ex = new ParallelExecutionVisitor(loader.SplitLoaderUsing(
                  typeof(ExtensionFilterProcessor)
                , typeof(RegexFilterProcessor)
                , typeof(CsharpSourcesFilterProcessor)
                , typeof(FileTypeProcessor)

                , typeof(SizeTimeDiffProcessor)
                , typeof(ChecksumDiffProcessor)
                , typeof(BinaryDiffProcessor)

                , typeof(CalculateDiffProcessor)
            ));
            diffTree.Accept(ex);
            ex.Wait();


            #endregion

            AddNewTab(diffTree);
        }

        public void AddNewTab(IFilesystemTreeVisitable diffTree)
        {
            Tabs.Items.Add(new TabItem {
                Header = "New Header",
                Content = windowLoader.CreateWindowFor(diffTree),
                //IsSelected = Tabs.Items.Count == 0
                IsSelected = true
            });
        }

        public void WrongNumberOfArguments()
        {
            var x = MessageBox.Show("You need to specify 2 or 3 arguments.");
        }

        private void TabCloseImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var x = FindParent<TabItem>(sender as DependencyObject);
            Tabs.Items.Remove(x);
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
    }


}
