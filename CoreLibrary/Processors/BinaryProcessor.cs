using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CoreLibrary.Interfaces;
using CoreLibrary.Enums;

namespace CoreLibrary.Processors
{
    class BinaryProcessor : IProcessor
    {
        const int BUFFER_SIZE = 4096;

        public bool Process(IFilesystemTreeDirNode node)
        {
            return false;
        }

        public bool Process(IFilesystemTreeFileNode node)
        {
            if (node.Status == NodeStatus.WasDiffed)
                return false;

            const int BASE_LEFT = 0x1;
            const int BASE_RIGHT = 0x2;
            const int LEFT_RIGHT = 0x4;

            const int BASE = 0x1;
            const int LEFT = 0x2;
            const int RIGHT = 0x4;

            int possibleCombinations = 0x0;
            int filesToBeChecked = 0x0;

            try
            {
                StreamReader[] readers = new StreamReader[3];

                char[][] buffers = new char[3][];

                // initialize readers
                if (node.IsInLocation(LocationEnum.OnBase))
                {
                    readers[0] = new StreamReader(node.InfoBase.FullName);
                    buffers[0] = new char[BUFFER_SIZE];
                    filesToBeChecked |= BASE;
                }

                if (node.IsInLocation(LocationEnum.OnLeft))
                {
                    readers[1] = new StreamReader(node.InfoLeft.FullName);
                    buffers[1] = new char[BUFFER_SIZE];
                    filesToBeChecked |= LEFT;
                }

                if (node.IsInLocation(LocationEnum.OnRight))
                {
                    readers[2] = new StreamReader(node.InfoRight.FullName);
                    buffers[2] = new char[BUFFER_SIZE];
                    filesToBeChecked |= RIGHT;
                }

                // create combinations
                if ((filesToBeChecked & (BASE | LEFT)) == (BASE | LEFT))
                    possibleCombinations |= BASE_LEFT;

                if ((filesToBeChecked & (BASE | RIGHT)) == (BASE | RIGHT))
                    possibleCombinations |= BASE_RIGHT;

                if ((filesToBeChecked & (LEFT | RIGHT)) == (LEFT | RIGHT))
                    possibleCombinations |= LEFT_RIGHT;

                while (possibleCombinations > 0)
                {
                    int[] bufferLengths = new int[3];

                    // load buffers
                    if ((filesToBeChecked & BASE) > 0)
                        bufferLengths[0] = readers[0].Read(buffers[0], 0, buffers[0].Length);

                    if ((filesToBeChecked & LEFT) > 0)
                        bufferLengths[1] = readers[1].Read(buffers[1], 0, buffers[1].Length);

                    if ((filesToBeChecked & RIGHT) > 0)
                        bufferLengths[2] = readers[2].Read(buffers[2], 0, buffers[2].Length);

                    // check buffer lengths
                    if ((possibleCombinations & BASE_LEFT) > 0 && bufferLengths[0] != bufferLengths[1])
                        possibleCombinations &= ~BASE_LEFT;

                    if ((possibleCombinations & BASE_RIGHT) > 0 && bufferLengths[0] != bufferLengths[2])
                        possibleCombinations &= ~BASE_RIGHT;

                    if ((possibleCombinations & LEFT_RIGHT) > 0 && bufferLengths[1] != bufferLengths[2])
                        possibleCombinations &= ~LEFT_RIGHT;

                    int maxLength = bufferLengths.Max();

                    if (maxLength == 0) // files reached end and are the same
                        break;

                    for (int i = 0; i < maxLength; i++)
                    {
                        if ((possibleCombinations & BASE_LEFT) > 0 && buffers[0][i] != buffers[1][i])
                            possibleCombinations &= ~BASE_LEFT;

                        if ((possibleCombinations & BASE_RIGHT) > 0 && buffers[0][i] != buffers[2][i])
                            possibleCombinations &= ~BASE_RIGHT;

                        if ((possibleCombinations & LEFT_RIGHT) > 0 && buffers[1][i] != buffers[2][i])
                            possibleCombinations &= ~LEFT_RIGHT;

                        if (possibleCombinations == 0)
                        {
                            break;
                        }
                    }

                    // recheck filesToBeChecked
                    if ((possibleCombinations & (BASE_LEFT | BASE_RIGHT)) == 0)
                        filesToBeChecked &= ~BASE;

                    if ((possibleCombinations & (BASE_LEFT | LEFT_RIGHT)) == 0)
                        filesToBeChecked &= ~LEFT;

                    if ((possibleCombinations & (BASE_RIGHT | LEFT_RIGHT)) == 0)
                        filesToBeChecked &= ~RIGHT;
                }

                // recheck filesToBeChecked
                if ((possibleCombinations & (BASE_LEFT | BASE_RIGHT)) == 0)
                    filesToBeChecked &= ~BASE;

                if ((possibleCombinations & (BASE_LEFT | LEFT_RIGHT)) == 0)
                    filesToBeChecked &= ~LEFT;

                if ((possibleCombinations & (BASE_RIGHT | LEFT_RIGHT)) == 0)
                    filesToBeChecked &= ~RIGHT;

                node.Differences = (DifferencesStatus)(filesToBeChecked);
                node.Status = NodeStatus.WasDiffed;

            } catch (IOException e)
            {
                Console.WriteLine(e);
                node.Status = NodeStatus.HasError;
            }

            return true;
        }

        public int Priority { get { return 10; } }

        public DiffModeEnum Mode { get { return DiffModeEnum.ThreeWay; } }
    }
}
