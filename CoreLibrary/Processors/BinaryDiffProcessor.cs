using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CoreLibrary.Interfaces;
using CoreLibrary.Enums;
using System.Threading;
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.Processors
{
    class BinaryDiffProcessor : IPreProcessor
    {
        const int BUFFER_SIZE = 4096;

        public void Process(IFilesystemTreeDirNode node)
        {
        }

        public void Process(IFilesystemTreeFileNode node)
        {
            if (node.Status == NodeStatusEnum.WasDiffed)
                return;

            var threeWay = new ThreeWayDiffHelper();

            try
            {
                StreamReader[] readers = new StreamReader[3];

                char[][] buffers = new char[3][];

                // initialize readers
                if (node.IsInLocation(LocationEnum.OnBase))
                {
                    readers[0] = new StreamReader(node.InfoBase.FullName);
                    buffers[0] = new char[BUFFER_SIZE];
                    threeWay.AddBaseFilePossibility();
                }

                if (node.IsInLocation(LocationEnum.OnLeft))
                {
                    readers[1] = new StreamReader(node.InfoLeft.FullName);
                    buffers[1] = new char[BUFFER_SIZE];
                    threeWay.AddLeftFilePossibility();
                }

                if (node.IsInLocation(LocationEnum.OnRight))
                {
                    readers[2] = new StreamReader(node.InfoRight.FullName);
                    buffers[2] = new char[BUFFER_SIZE];
                    threeWay.AddRightFilePossibility();
                }

                // create combinations
                threeWay.RecalculatePossibleCombinations();

                while (threeWay.GetPossibleCombinations() > 0)
                {
                    int[] bufferLengths = new int[3];

                    // load buffers
                    if (threeWay.CanBaseFileBeSame(true))
                        bufferLengths[0] = readers[0].Read(buffers[0], 0, buffers[0].Length);

                    if (threeWay.CanLeftFileBeSame())
                        bufferLengths[1] = readers[1].Read(buffers[1], 0, buffers[1].Length);

                    if (threeWay.CanRightFileBeSame())
                        bufferLengths[2] = readers[2].Read(buffers[2], 0, buffers[2].Length);

                    // check buffer lengths
                    if (threeWay.CanCombinationBaseLeftBeSame())
                        threeWay.CheckCombinationBaseLeft(bufferLengths[0] != bufferLengths[1]);
                    if (threeWay.CanCombinationBaseRightBeSame())
                        threeWay.CheckCombinationBaseRight(bufferLengths[0] != bufferLengths[2]);
                    if (threeWay.CanCombinationLeftRightBeSame())
                        threeWay.CheckCombinationLeftRight(bufferLengths[1] != bufferLengths[2]);

                    int maxLength = bufferLengths.Max();

                    if (maxLength == 0) // files reached end and are the same
                        break;

                    for (int i = 0; i < maxLength; i++)
                    {
                        if (threeWay.CanCombinationBaseLeftBeSame())
                            threeWay.CheckCombinationBaseLeft(buffers[0][i] != buffers[1][i]);
                        if (threeWay.CanCombinationBaseRightBeSame())
                            threeWay.CheckCombinationBaseRight(buffers[0][i] != buffers[2][i]);
                        if (threeWay.CanCombinationLeftRightBeSame())
                            threeWay.CheckCombinationLeftRight(buffers[1][i] != buffers[2][i]);

                        if (threeWay.GetPossibleCombinations() == 0)
                            break;
                    }
                }

                node.Differences = (DifferencesStatusEnum)(threeWay.GetSameFiles());
                node.Status = NodeStatusEnum.WasDiffed;

            } catch (IOException e)
            {
                Console.WriteLine(e);
                node.Status = NodeStatusEnum.HasError;
            }

        }

        public int Priority { get { return 10; } }

        public DiffModeEnum Mode { get { return DiffModeEnum.ThreeWay; } }
    }
}
