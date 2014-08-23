using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary;
using CoreLibrary.FilesystemTree;

namespace ConsoleAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            Crawler di = new Crawler(args[0], args[1]);

            di.TraverseTree().Accept(new PrinterVisitor());
        }
    }
}
