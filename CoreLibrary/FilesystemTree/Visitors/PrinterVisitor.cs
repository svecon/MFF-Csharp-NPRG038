using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Interfaces;
using CoreLibrary.Enums;

namespace CoreLibrary.FilesystemTree.Visitors
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

            Console.WriteLine("+- " + node.Info.FullName + " (" + node.GetSize().ToString("#.00") + ")");

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
            if (node.Status == NodeStatusEnum.HasError)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("E ");
            } else
                switch (node.Differences)
                {
                    case DifferencesStatusEnum.Initial:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case DifferencesStatusEnum.AllDifferent:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case DifferencesStatusEnum.BaseLeftSame:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case DifferencesStatusEnum.BaseRight:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case DifferencesStatusEnum.LeftRightSame:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case DifferencesStatusEnum.AllSame:
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
