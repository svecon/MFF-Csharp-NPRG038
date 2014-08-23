﻿using System;
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
            switch ((LocationCombinationsEnum)node.Location)
            {
                case LocationCombinationsEnum.OnBase:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LocationCombinationsEnum.OnLeft:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LocationCombinationsEnum.OnRight:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LocationCombinationsEnum.OnAll2:
                case LocationCombinationsEnum.OnAll3:
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