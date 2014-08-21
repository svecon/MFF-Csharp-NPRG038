using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilesystemCrawler.Interfaces;

namespace FilesystemCrawler
{
    public class DiffStructurePrinterVisitor : IDiffStructureVisitor
    {

        int currentLevel = 1;

        public void Visit(DiffStructure.DirDiffNode node)
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

        public void Visit(DiffStructure.FileDiffNode node)
        {
            switch (node.Location)
            {
                case DiffStructure.LocationEnum.OnLeft:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case DiffStructure.LocationEnum.OnRight:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case DiffStructure.LocationEnum.OnAll:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                default:
                    break;
            }

            for (int i = 0; i < currentLevel; i++)
            {
                Console.Write("|  ");
            }

            Console.WriteLine("+- " + node.Info.Name + ": " + node.Location);

            Console.ResetColor();
        }
    }
}
