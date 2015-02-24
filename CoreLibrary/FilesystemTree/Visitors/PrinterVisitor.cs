using System;
using CoreLibrary.Interfaces;
using CoreLibrary.Enums;

namespace CoreLibrary.FilesystemTree.Visitors
{
    /// <summary>
    /// Prints FilesystemTree on the console. 
    /// 
    /// Also prints all available information about the node (status, differences, locations).
    /// 
    /// Uses colors for different Node states.
    /// </summary>
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

            Console.WriteLine("+- " + node.Info.FullName + " (" + node.GetSize().ToString("#.00") + "kB)");

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
            } else if (node.Status == NodeStatusEnum.IsIgnored)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
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
                    case DifferencesStatusEnum.BaseRightSame:
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

            Console.WriteLine("+- " + node.Info.Name
                + ": " + (LocationCombinationsEnum)node.Location
                + " # " + node.Differences
                + " # " + node.Status);

            Console.ResetColor();
        }
    }
}
