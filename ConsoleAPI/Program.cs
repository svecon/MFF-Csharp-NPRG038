using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;

namespace ConsoleAPI
{
    class Program
    {
        static void Main(string[] args)
        {
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


            IProcessorLoader loader = new ProcessorsLoader();
            loader.Load();

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var tree = di.TraverseTree();

            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Restart();

            var ex = new ExecutionVisitor(loader);

            tree.Accept(ex);
            ex.Wait();
            tree.Accept(new PrinterVisitor());

            Console.WriteLine(sw.ElapsedMilliseconds);
        }
    }
}
