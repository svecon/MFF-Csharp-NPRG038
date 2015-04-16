using System.IO;
using System.Linq;
using CoreLibrary.Enums;
using CoreLibrary.Helpers;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using CoreLibrary.Settings.Attributes;

namespace BasicProcessors.Processors.DiffProcessors
{
    /// <summary>
    /// BinaryDiffProcessors processes any files and checks for differences byte by byte.
    /// </summary>
    [Processor(ProcessorTypeEnum.Diff, 1200, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class BinaryDiffProcessor : ProcessorAbstract
    {
        /// <summary>
        /// Size of an array buffer for reading files.
        /// </summary>
        const int BUFFER_SIZE = 4096;

        [Settings("Force binary diff check.", "binary-check", "BC")]
        public bool IsEnabled = false;

        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
        }

        protected override bool CheckStatus(IFilesystemTreeFileNode node)
        {
            return IsEnabled && base.CheckStatus(node);
        }

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
        {
            var threeWay = new ThreeWayDiffHelper();
            var readers = new BinaryReader[3];

            try
            {
                var buffers = new byte[3][];

                // initialize readers
                if (node.IsInLocation(LocationEnum.OnBase))
                {
                    readers[0] = new BinaryReader(File.OpenRead(node.InfoBase.FullName));
                    buffers[0] = new byte[BUFFER_SIZE];
                    threeWay.AddBaseFilePossibility();
                }

                if (node.IsInLocation(LocationEnum.OnLocal))
                {
                    readers[1] = new BinaryReader(File.OpenRead(node.InfoLocal.FullName));
                    buffers[1] = new byte[BUFFER_SIZE];
                    threeWay.AddLocalFilePossibility();
                }

                if (node.IsInLocation(LocationEnum.OnRemote))
                {
                    readers[2] = new BinaryReader(File.OpenRead(node.InfoRemote.FullName));
                    buffers[2] = new byte[BUFFER_SIZE];
                    threeWay.AddRemoteFilePossibility();
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
                foreach (BinaryReader reader in readers.Where(reader => reader != null))
                {
                    reader.Close();
                }
            }

        }
    }
}
