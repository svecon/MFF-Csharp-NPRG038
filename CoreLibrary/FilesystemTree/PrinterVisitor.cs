using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Interfaces;
using CoreLibrary.Enums;

namespace CoreLibrary.FilesystemTree
{
    public class PrinterVisitor : IFilesystemTreeVisitor
    {

        int currentLevel = 1;

        public void Visit(IFilesystemTreeDirNode node)
        {
            Console.WriteLine();

            for (int i = 0; i < currentLevel - 1; i++)
            {
                Console.Write("|  ");
            }

            Console.WriteLine("+- " + node.Info.FullName);

            foreach (var file in node.Files)
            {
                file.Accept(this);
            }

            foreach (var dir in node.Directories)
            {
                currentLevel++;
                dir.Accept(this);
                currentLevel--;
            }
        }

        public void Visit(IFilesystemTreeFileNode node)
        {
            switch (node.Differences)
            {
                case DifferencesStatus.Initial:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case DifferencesStatus.AllDifferent:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case DifferencesStatus.BaseLeftSame:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case DifferencesStatus.BaseRight:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case DifferencesStatus.LeftRight:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case DifferencesStatus.AllSame:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    throw new NotImplementedException("This difference type was not implemented.");
            }

            for (int i = 0; i < currentLevel; i++)
            {
                Console.Write("|  ");
            }

            Console.WriteLine("+- " + node.Info.Name + ": " + node.Location + " # " + node.Differences);

            Console.ResetColor();
        }
    }
}
