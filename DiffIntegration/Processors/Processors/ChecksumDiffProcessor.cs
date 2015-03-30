using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using CoreLibrary.Processors.Processors;
using CoreLibrary.Settings.Attributes;
using DiffIntegration.RollingChecksums;

namespace DiffIntegration.Processors.Processors
{
    /// <summary>
    /// ChecksumDiffProcessor calculates multiple checksums for the files and checks if they are all same.
    /// </summary>
    public class ChecksumDiffProcessor : ProcessorAbstract
    {
        public override int Priority { get { return 100; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay; } }

        [Settings("Force checksum diff check.", "check-sum", "CS")]
        public bool IsEnabled = false;

        /// <summary>
        /// Size of an array buffer for reading files.
        /// </summary>
        const int BUFFER_SIZE = 4096;

        public override void Process(IFilesystemTreeDirNode node)
        {

        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            if (!CheckModeAndStatus(node))
                return;

            if (!IsEnabled)
                return;

            var threeWay = new ThreeWayDiffHelper();
            var readers = new BinaryReader[3];

            try
            {
                var adlerchecksums = new uint[3];
                var adlers = new Adler32[3];
                bool[] isEndOfFile = Enumerable.Repeat(true, 3).ToArray();

                // initialize readers
                if (node.IsInLocation(LocationEnum.OnBase))
                {
                    readers[0] = new BinaryReader(File.OpenRead(node.InfoBase.FullName));
                    adlers[0] = new Adler32();
                    threeWay.AddBaseFilePossibility();
                }

                if (node.IsInLocation(LocationEnum.OnLocal))
                {
                    readers[1] = new BinaryReader(File.OpenRead(node.InfoLocal.FullName));
                    adlers[1] = new Adler32();
                    threeWay.AddLocalFilePossibility();
                }

                if (node.IsInLocation(LocationEnum.OnRemote))
                {
                    readers[2] = new BinaryReader(File.OpenRead(node.InfoRemote.FullName));
                    adlers[2] = new Adler32();
                    threeWay.AddRemoteFilePossibility();
                }

                // create combinations
                threeWay.RecalculatePossibleCombinations();

                if (threeWay.GetPossibleCombinations() > 0)
                {
                    // load initial buffer window
                    if (threeWay.CanBaseFileBeSame(true))
                    {
                        adlerchecksums[0] = adlers[0].Fill(readers[0].ReadBytes(BUFFER_SIZE));
                        isEndOfFile[0] = readers[0].PeekChar() == -1;
                    }

                    if (threeWay.CanLeftFileBeSame())
                    {
                        adlerchecksums[1] = adlers[1].Fill(readers[1].ReadBytes(BUFFER_SIZE));
                        isEndOfFile[1] = readers[1].PeekChar() == -1;
                    }

                    if (threeWay.CanRightFileBeSame())
                    {
                        adlerchecksums[2] = adlers[2].Fill(readers[2].ReadBytes(BUFFER_SIZE));
                        isEndOfFile[2] = readers[2].PeekChar() == -1;
                    }
                }

                // check initial checksums
                CheckAllHelper(threeWay, adlers, p => p.Checksum);
                // check possible end of files => one file is shorter
                CheckAllHelper(threeWay, isEndOfFile);
                
                // continue until there is no chance for any file and not all files reached end
                while (threeWay.GetPossibleCombinations() > 0 && !isEndOfFile.All(x => x))
                {
                    var eadlerchecksums = new IEnumerator<uint>[3];
                    bool[] isEndOfChecksums = Enumerable.Repeat(true, 3).ToArray();

                    if (threeWay.CanBaseFileBeSame(true))
                    {
                        eadlerchecksums[0] = adlers[0].Roll(readers[0].ReadBytes(BUFFER_SIZE)).GetEnumerator();
                        isEndOfChecksums[0] = !eadlerchecksums[0].MoveNext();
                        isEndOfFile[0] = readers[0].PeekChar() == -1;
                    }

                    if (threeWay.CanLeftFileBeSame())
                    {
                        eadlerchecksums[1] = adlers[1].Roll(readers[1].ReadBytes(BUFFER_SIZE)).GetEnumerator();
                        isEndOfChecksums[1] = !eadlerchecksums[1].MoveNext();
                        isEndOfFile[1] = readers[1].PeekChar() == -1;
                    }

                    if (threeWay.CanRightFileBeSame())
                    {
                        eadlerchecksums[2] = adlers[2].Roll(readers[2].ReadBytes(BUFFER_SIZE)).GetEnumerator();
                        isEndOfChecksums[2] = !eadlerchecksums[2].MoveNext();
                        isEndOfFile[2] = readers[2].PeekChar() == -1;
                    }

                    // are there still any checksums to examine?
                    while (isEndOfChecksums.Where(x => !x).Any())
                    {
                        CheckAllHelper(threeWay, eadlerchecksums, p => p.Current);

                        if (eadlerchecksums[0] != null)
                            isEndOfChecksums[0] = !eadlerchecksums[0].MoveNext();
                        if (eadlerchecksums[1] != null)
                            isEndOfChecksums[1] = !eadlerchecksums[1].MoveNext();
                        if (eadlerchecksums[2] != null)
                            isEndOfChecksums[2] = !eadlerchecksums[2].MoveNext();

                        // check possible end of checksum => one file is shorter
                        CheckAllHelper(threeWay, isEndOfChecksums);
                    } 
                    
                }

                // check final checksums
                CheckAllHelper(threeWay, adlers, p => p.Checksum);

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

        private static void CheckAllHelper<T>(ThreeWayDiffHelper threeWay, T[] x, Func<T, uint> f = null)
        {
            if (f == null)
            {
                if (threeWay.CanCombinationBaseLeftBeSame())
                    threeWay.CheckCombinationBaseLeft(!Equals(x[0], x[1]));
                if (threeWay.CanCombinationBaseRightBeSame())
                    threeWay.CheckCombinationBaseRight(!Equals(x[0], x[2]));
                if (threeWay.CanCombinationLeftRightBeSame())
                    threeWay.CheckCombinationLeftRight(!Equals(x[1], x[2]));
            }
            else
            {
                if (threeWay.CanCombinationBaseLeftBeSame())
                    threeWay.CheckCombinationBaseLeft(!Equals(f(x[0]), f(x[1])));
                if (threeWay.CanCombinationBaseRightBeSame())
                    threeWay.CheckCombinationBaseRight(!Equals(f(x[0]), f(x[2])));
                if (threeWay.CanCombinationLeftRightBeSame())
                    threeWay.CheckCombinationLeftRight(!Equals(f(x[1]), f(x[2])));
            }
        }
    }
}
