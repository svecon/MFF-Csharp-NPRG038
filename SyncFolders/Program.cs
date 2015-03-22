using System;
using System.IO;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using CoreLibrary.Settings;
using CoreLibrary.FilesystemTree.Visitors;
using CoreLibrary.Exceptions;
using DiffIntegration.DiffFilesystemTree;
using DiffIntegration.DiffOutput;

namespace SyncFolders
{
    class Program
    {
        static void Main(string[] args)
        {
            //var lloader = new ProcessorsLoader();
            ////var x = new DiffFileNode(new FileInfo("C:/Users/svecon/Downloads/a.txt"), new FileInfo("C:/Users/svecon/Downloads/b.txt"));
            ////var x = new DiffFileNode(new FileInfo("C:/Users/svecon/Downloads/lao.txt"), new FileInfo("C:/Users/svecon/Downloads/tzu.txt"));
            ////var x = new DiffFileNode(new FileInfo("C:/Program Files/KDiff3/bin/d0.txt"), new FileInfo("C:/Program Files/KDiff3/bin/d1.txt"), new FileInfo("C:/Program Files/KDiff3/bin/d2.txt")); // , new FileInfo("C:/Program Files/KDiff3/bin/f2.txt")
            //x.RecalculateDiff();
            //var ou = new UnifiedDiffOutput();
            //var ed = new EditScript();
            //var no = new Diff3NormalOutput();
            //var nx = new Diff3Merge();
            //var vis = new ExecutionVisitor(lloader);
            ////lloader.LoadAll();
            ////lloader.Print();
            //return;
            //;
            //lloader.AddProcessor(ou);
            //x.Accept(vis);
            //vis.Wait();
            //ou.Process(x);
            //ed.Process(x);
            //nx.Process(x);
            return;






            //// load all available processors
            //IProcessorLoader loader = new ProcessorsLoader();
            //try
            //{
            //    loader.LoadAll();
            //} catch (ProcessorPriorityColissionException e)
            //{
            //    Console.WriteLine("Processor " + e.Message + "could not be loaded because of a priority collision.");
            //}

            //// if the help argument is passed - list all the available settings
            //if (args.Length == 1 && args[0] == "--help")
            //{
            //    var printer = new SettingsPrinter(loader.GetSettings());
            //    printer.Print();
            //    return;
            //}

            //// pass the arguments
            //var parser = new SettingsParser(loader.GetSettings());
            //try
            //{
            //    args = parser.ParseSettings(args);
            //} catch (SettingsNotFoundException e)
            //{
            //    Console.WriteLine("This option has not been found: " + e.Message);
            //    return;
            //} catch (SettingsUnknownValue e)
            //{
            //    Console.WriteLine("This value for given option is invalid: " + e.Message);
            //    return;
            //}


            //Crawler di = null;

            //// create crawler depending on how many path arguments were specified
            //try
            //{
            //    if (args.Length == 3)
            //    {
            //        di = new Crawler(args[0], args[1], args[2]);
            //    } else if (args.Length == 2)
            //    {
            //        di = new Crawler(args[0], args[1]);
            //    } else
            //    {
            //        Console.WriteLine("Please specify either 3 or 2 folders for processing.");
            //        return;
            //    }
            //} catch (LocalDirectoryNotFoundException)
            //{
            //    Console.WriteLine("Old directory was not found or is not readable.");
            //    return;
            //} catch (RemoteDirectoryNotFoundException)
            //{
            //    Console.WriteLine("New directory was not found or is not readable.");
            //    return;
            //} catch (BaseDirectoryNotFoundException)
            //{
            //    Console.WriteLine("Base directory was not found or is not readable.");
            //    return;
            //}

            //var sw = new System.Diagnostics.Stopwatch();
            //sw.Start();

            //// traverse a filesystem tree (loads all the files)
            //IFilesystemTree tree = di.TraverseTree();

            //System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds + "ms for tree bulding");
            //sw.Restart();

            //// execution visitor for filesystem tree processes the files and folders with loaded processors
            //var ex = new ExecutionVisitor(loader);
            //tree.Accept(ex);

            //// wait until the filesystem tree is finished with processing
            //ex.Wait();

            //// print the filesystem tree
            //tree.Accept(new PrinterVisitor());

            //System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds + "ms for tree processing");
        }
    }
}
