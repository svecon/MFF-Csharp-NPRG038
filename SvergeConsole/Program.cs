using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Exceptions;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Visitors;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using CoreLibrary.Settings;
using CoreLibrary.Settings.Attributes;
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
            try
            {
                _loader = new ProcessorsLoader();
                // Load available processors and their settings
                _loader.LoadAll();
                // Add special settings from this Program
                _loader.RetrieveSettings(typeof(Program), true);
            } catch (ProcessorPriorityColissionException e)
            {
                Console.WriteLine("Processor " + e.Message + "could not be loaded because of a priority collision.");
            }

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

            // if the help argument is passed - list all the available settings
            if (ShowHelp)
            {
                Console.WriteLine("Sverge – A Flexible Tool for Comparing & Merging [{0}]", GetVersion());
                Console.WriteLine("Usage: [OPTION]... <LOCAL> [BASE] <REMOTE>");

                Console.WriteLine("\nListing all found Processors and their parameters:");
                var processorPrinter = new ProcessorsPrinter(_loader);
                processorPrinter.Print();
                return;
            }

            if (args.Length != 2 && args.Length != 3)
            {
                Console.WriteLine("You need to specify 2 or 3 arguments.");
                Console.WriteLine("Show more information using '--help' option.");
                return;
            }

            Crawler di;

            int areArgsFiles = 0;

            try
            {
                foreach (string s in args)
                {
                    areArgsFiles <<= 1;
                    areArgsFiles |= IsFile(s);
                }
            } catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            } catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            if ((args.Length == 2 && areArgsFiles == 3) || (args.Length == 3 && areArgsFiles == 7))
            {
                Console.WriteLine("Diffing files");
            } else if (areArgsFiles == 0)
            {
                Console.WriteLine("Diffing folders");
            }
            return;

            // create crawler depending on how many path arguments were specified
            try
            {
                if (args.Length == 3)
                {
                    di = new Crawler(args[0], args[1], args[2]);
                } else if (args.Length == 2)
                {
                    di = new Crawler(args[0], args[1]);
                } else
                {
                    Console.WriteLine("Please specify either 3 or 2 folders for processing.");
                    return;
                }
            } catch (LeftDirectoryNotFoundException)
            {
                Console.WriteLine("Old directory was not found or is not readable.");
                return;
            } catch (RightDirectoryNotFoundException)
            {
                Console.WriteLine("New directory was not found or is not readable.");
                return;
            } catch (BaseDirectoryNotFoundException)
            {
                Console.WriteLine("Base directory was not found or is not readable.");
                return;
            }

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            // traverse a filesystem tree (loads all the files)
            IFilesystemTree tree = di.TraverseTree();

            System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds + "ms for tree bulding");
            sw.Restart();

            // execution visitor for filesystem tree processes the files and folders with loaded processors
            var ex = new ExecutionVisitor(_loader);
            tree.Accept(ex);

            // wait until the filesystem tree is finished with processing
            ex.Wait();

            // print the filesystem tree
            tree.Accept(new PrinterVisitor());

            System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds + "ms for tree processing");

        }

        private static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        private static int IsFile(string path)
        {
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(@path);

            //detect whether its a directory or file
            return (attr & FileAttributes.Directory) == FileAttributes.Directory ? 0 : 1;
        }
    }
}
