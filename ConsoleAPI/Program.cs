using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MergeLibrary;

namespace ConsoleAPI {
    class Program {
        static void Main(string[] args)
        {
            DirectoryIterator di = new DirectoryIterator(args[0], args[1]);

            di.TraverseTree().Root.Print();
        }
    }
}
