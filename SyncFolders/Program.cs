using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using CoreLibrary.Settings;
using CoreLibrary.FilesystemTree.Visitors;
using CoreLibrary.Exceptions;
using System.IO;
using System.Reflection;

namespace SyncFolders
{
    class Program
    {
        static void Main(string[] args)
        {
            // load all available processors
            IProcessorLoader loader = new ProcessorsLoader();
            try
            {
                loader.LoadAll();
            } catch (ProcessorPriorityColissionException e)
            {
                Console.WriteLine("Processor " + e.Message + "could not be loaded because of a priority collision.");
            }

            // if the help argument is passed - list all the available settings
            if (args.Length == 1 && args[0] == "--help")
            {
                SettingsPrinter printer = new SettingsPrinter(loader.GetSettings());
                printer.Print();
                return;
            }

            // pass the arguments
            SettingsParser parser = new SettingsParser(loader.GetSettings());
            try
            {
                args = parser.ParseSettings(args);
            } catch (SettingsNotFoundException e)
            {
                Console.WriteLine("This option has not been found: " + e.Message);
                return;
            }


            Crawler di = null;

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
                Console.WriteLine("Left directory was not found or is not readable.");
                return;
            } catch (RightDirectoryNotFoundException)
            {
                Console.WriteLine("Right directory was not found or is not readable.");
                return;
            } catch (BaseDirectoryNotFoundException)
            {
                Console.WriteLine("Base directory was not found or is not readable.");
                return;
            }

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            // traverse a filesystem tree (loads all the files)
            var tree = di.TraverseTree();

            System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds + "ms for tree bulding");
            sw.Restart();

            // execution visitor for filesystem tree processes the files and folders with loaded processors
            var ex = new ExecutionVisitor(loader);
            tree.Accept(ex);

            // wait until the filesystem tree is finished with processing
            ex.Wait();

            // print the filesystem tree
            tree.Accept(new PrinterVisitor());

            System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds + "ms for tree processing");
        }
    }
}
