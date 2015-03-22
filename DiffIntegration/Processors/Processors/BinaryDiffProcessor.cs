using System.Linq;
using System.IO;
using CoreLibrary.Interfaces;
using CoreLibrary.Enums;
using CoreLibrary.Processors.Processors;
using CoreLibrary.Processors;

namespace SyncFolders.Processors.Processors
{
    /// <summary>
    /// BinaryDiffProcessors processes any files and checks for differences byte by byte.
    /// </summary>
    class BinaryDiffProcessor : ProcessorAbstract
    {
        /// <summary>
        /// Size of an array buffer for reading files.
        /// </summary>
        const int BUFFER_SIZE = 4096;

        public override int Priority { get { return 100000; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay; } }

        public override void Process(IFilesystemTreeDirNode node)
        {
        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            if (!CheckModeAndStatus(node))
                return;

            var threeWay = new ThreeWayDiffHelper();

            var readers = new StreamReader[3];

            try
            {
                var buffers = new char[3][];

                // initialize readers
                if (node.IsInLocation(LocationEnum.OnBase))
                {
                    readers[0] = new StreamReader(node.InfoBase.FullName);
                    buffers[0] = new char[BUFFER_SIZE];
                    threeWay.AddBaseFilePossibility();
                }

                if (node.IsInLocation(LocationEnum.OnLocal))
                {
                    readers[1] = new StreamReader(node.InfoLocal.FullName);
                    buffers[1] = new char[BUFFER_SIZE];
                    threeWay.AddLeftFilePossibility();
                }

                if (node.IsInLocation(LocationEnum.OnRemote))
                {
                    readers[2] = new StreamReader(node.InfoRemote.FullName);
                    buffers[2] = new char[BUFFER_SIZE];
                    threeWay.AddRightFilePossibility();
                }

                // create combinations
                threeWay.RecalculatePossibleCombinations();

                while (threeWay.GetPossibleCombinations() > 0)
                {
                    var bufferLengths = new int[3];

                    // load new buffers
                    if (threeWay.CanBaseFileBeSame(true))
                        bufferLengths[0] = readers[0].Read(buffers[0], 0, buffers[0].Length);

                    if (threeWay.CanLeftFileBeSame())
                        bufferLengths[1] = readers[1].Read(buffers[1], 0, buffers[1].Length);

                    if (threeWay.CanRightFileBeSame())
                        bufferLengths[2] = readers[2].Read(buffers[2], 0, buffers[2].Length);

                    // check buffered lengths
                    if (threeWay.CanCombinationBaseLeftBeSame())
                        threeWay.CheckCombinationBaseLeft(bufferLengths[0] != bufferLengths[1]);
                    if (threeWay.CanCombinationBaseRightBeSame())
                        threeWay.CheckCombinationBaseRight(bufferLengths[0] != bufferLengths[2]);
                    if (threeWay.CanCombinationLeftRightBeSame())
                        threeWay.CheckCombinationLeftRight(bufferLengths[1] != bufferLengths[2]);

                    int maxLength = bufferLengths.Max();

                    // files reached end and are the same
                    if (maxLength == 0)
                        break;

                    // check bytes
                    for (int i = 0; i < maxLength; i++)
                    {
                        if (threeWay.CanCombinationBaseLeftBeSame())
                            threeWay.CheckCombinationBaseLeft(buffers[0][i] != buffers[1][i]);
                        if (threeWay.CanCombinationBaseRightBeSame())
                            threeWay.CheckCombinationBaseRight(buffers[0][i] != buffers[2][i]);
                        if (threeWay.CanCombinationLeftRightBeSame())
                            threeWay.CheckCombinationLeftRight(buffers[1][i] != buffers[2][i]);

                        // all files are different
                        if (threeWay.GetPossibleCombinations() == 0)
                            break;
                    }
                }

                node.Differences = (DifferencesStatusEnum)(threeWay.GetSameFiles());
                node.Status = NodeStatusEnum.WasDiffed;

            } finally
            {
                // close readers if any
                foreach (StreamReader reader in readers.Where(reader => reader != null))
                {
                    reader.Close();
                }
            }

        }
    }
}
