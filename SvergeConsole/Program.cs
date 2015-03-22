using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Exceptions;
using CoreLibrary.Exceptions.NotFound;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Visitors;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using CoreLibrary.Settings;
using CoreLibrary.Settings.Attributes;
using DiffIntegration.DiffFilesystemTree;
using DiffIntegration.DiffOutput;

namespace SvergeConsole
{
    class Program
    {
        [Settings("Show help about using the console.", "help", "h")]
        public static bool ShowHelp = false;

        private static IProcessorLoader _loader;

        static void Main(string[] args)
        {
            #region Load all available processors and their settings
            try
            {
                _loader = new ProcessorLoader();
                // Load available processors and their settings
                _loader.LoadAll();
                // Add special settings from this Program
                _loader.RetrieveSettings(typeof(Program), true);
            } catch (ProcessorPriorityColissionException e)
            {
                Console.WriteLine("Processor " + e.Message + "could not be loaded because of a priority collision.");
            }
            #endregion

            #region Parse arguments as settings
            try
            {
                // pass the arguments and parse them
                var parser = new SettingsParser(_loader.GetSettings());
                args = parser.ParseSettings(args);
            } catch (SettingsNotFoundException e)
            {
                Console.WriteLine("This option has not been found: " + e.Message);
                return;
            } catch (SettingsUnknownValue e)
            {
                Console.WriteLine("This value for given option is invalid: " + e.Message);
                return;
            }
            #endregion

            #region Show help
            // if the help argument is passed - list all the available settings
            if (ShowHelp)
            {
                Console.WriteLine("Sverge – A Flexible Tool for Comparing & Merging [{0}]", GetVersion());
                Console.WriteLine("Usage: [OPTION]... <LOCAL> [BASE] <REMOTE>");

                Console.WriteLine("\nListing all found Processors and their parameters:");
                var processorPrinter = new ProcessorPrinter(_loader);
                processorPrinter.Print();
                return;
            }
            #endregion

            #region Wrong number of arguments
            if (args.Length != 2 && args.Length != 3)
            {
                Console.WriteLine("You need to specify 2 or 3 arguments.");
                Console.WriteLine("Show more information using '--help' option.");
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
            } catch (FileNotFoundException e)
            { // do nothing, we will catch it later
            } catch (DirectoryNotFoundException e)
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
            // execution visitor for filesystem tree processes the files and folders with loaded processors
            var ex = new ExecutionVisitor(_loader);
            diffTree.Accept(ex);

            // wait until the filesystem tree is finished with processing
            ex.Wait();

            // print the filesystem tree
            diffTree.Accept(new PrinterVisitor());
            #endregion
        }

        /// <summary>
        /// Return assembly version for current program
        /// </summary>
        /// <returns></returns>
        private static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
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
    }
}
