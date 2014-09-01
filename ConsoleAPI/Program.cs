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
using System.IO;

namespace ConsoleAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            IProcessorLoader loader = new ProcessorsLoader();
            loader.Load();

            if (args.Length == 1 && args[0] == "--help")
            {
                SettingsPrinter printer = new SettingsPrinter(loader.GetSettings());
                printer.Print();
                return;
            }

            SettingsParser parser = new SettingsParser(loader.GetSettings());
            args = parser.ParseSettings(args);

            Crawler di = null;

            if (args.Length == 3)
            {
                di = new Crawler(args[0], args[1], args[2]);
            } else if (args.Length == 2)
            {
                di = new Crawler(args[0], args[1]);
            } else
            {
                throw new ArgumentException("please specify 3 or 2 folders");
            }

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var tree = di.TraverseTree();

            System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds + "ms for tree bulding");
            sw.Restart();

            var ex = new ExecutionVisitor(loader);

            tree.Accept(ex);
            ex.Wait();
            tree.Accept(new PrinterVisitor());

            System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds + "ms for tree processing");
        }
    }
}
