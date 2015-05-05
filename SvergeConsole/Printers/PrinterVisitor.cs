using System;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Visitors;

namespace SvergeConsole.Printers
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

        int currentLevel = 0;

        private IFilesystemTreeDirNode directoryCache;

        private bool firstDirectoryVisit;

        private bool isOnlyFileNode = true;

        public void Visit(IFilesystemTreeDirNode node)
        {
            isOnlyFileNode = false;

            // Root directory
            if (node.RelativePath == "")
            {
                Console.WriteLine(@"+- {0} ({1:0.00}kB)", node.Info.FullName, node.GetSize());
            } else
            {
                // postpone printing to later
                directoryCache = node;
                firstDirectoryVisit = true;
            }

            foreach (IFilesystemTreeFileNode file in node.Files)
            {
                file.Accept(this);
            }

            foreach (IFilesystemTreeDirNode dir in node.Directories)
            {
                currentLevel++;
                dir.Accept(this);
                currentLevel--;
            }
        }

        private void PrintDirectory()
        {
            Console.WriteLine();

            for (int i = 0; i < currentLevel - 1; i++)
            {
                Console.Write("|  ");
            }

            Console.WriteLine(@"+- \{0} ({1:0.00}kB)", directoryCache.RelativePath, directoryCache.GetSize());
        }

        public void Visit(IFilesystemTreeFileNode node)
        {
            if (node.Status == NodeStatusEnum.IsIgnored)
                return;

            // print directory if this is the first file
            if (firstDirectoryVisit)
            {
                PrintDirectory();
                firstDirectoryVisit = false;
            }

            switch (node.Status)
            {
                case NodeStatusEnum.HasError:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case NodeStatusEnum.IsIgnored:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    return;
                default:
                    switch (node.Differences)
                    {
                        case DifferencesStatusEnum.Initial:
                            Console.ResetColor();
                            break;
                        case DifferencesStatusEnum.AllDifferent:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case DifferencesStatusEnum.BaseLocalSame:
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            break;
                        case DifferencesStatusEnum.BaseRemoteSame:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        case DifferencesStatusEnum.LocalRemoteSame:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            break;
                        case DifferencesStatusEnum.AllSame:
                            Console.ResetColor();
                            break;
                        default:
                            Console.ResetColor();
                            break;
                    }
                    break;
            }

            for (int i = 0; i < currentLevel; i++)
            {
                Console.Write("|  ");
            }

            if (!isOnlyFileNode)
            {
                Console.Write("+- ");
            }

            if (node.InfoLocal != null && node.InfoBase != null && node.InfoRemote != null && node.InfoLocal.Name != node.InfoBase.Name && node.InfoLocal.Name != node.InfoRemote.Name)
            {
                Console.WriteLine("{0} / {1} / {2}: {3}, {4}, {5}", node.InfoLocal.Name, node.InfoBase.Name, node.InfoRemote.Name,
                (LocationCombinationsEnum)node.Location, node.Differences, node.Status);
            } else if (node.InfoLocal != null && node.InfoRemote != null && node.InfoLocal.Name != node.InfoRemote.Name)
            {
                Console.WriteLine("{0} / {1}: {2}, {3}, {4}", node.InfoLocal.Name, node.InfoRemote.Name,
                (LocationCombinationsEnum)node.Location, node.Differences, node.Status);
            } else if (node.InfoLocal != null && node.InfoBase != null && node.InfoLocal.Name != node.InfoBase.Name)
            {
                Console.WriteLine("{0} / {1}: {2}, {3}, {4}", node.InfoLocal.Name, node.InfoBase.Name,
                (LocationCombinationsEnum)node.Location, node.Differences, node.Status);
            } else if (node.InfoBase != null && node.InfoRemote != null && node.InfoBase.Name != node.InfoRemote.Name)
            {
                Console.WriteLine("{0} / {1}: {2}, {3}, {4}", node.InfoBase.Name, node.InfoRemote.Name,
                (LocationCombinationsEnum)node.Location, node.Differences, node.Status);
            } else
            {
                Console.WriteLine("{0}: {1}, {2}, {3}", node.Info.Name,
                (LocationCombinationsEnum)node.Location, node.Differences, node.Status);
            }



#if DEBUG
            if (node.Status == NodeStatusEnum.HasError)
            {
                Console.WriteLine(node.Exception);
            }
#endif

            Console.ResetColor();
        }
    }
}
