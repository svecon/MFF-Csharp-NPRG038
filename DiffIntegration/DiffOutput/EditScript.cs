using System;
using System.IO;
using System.Text;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Processors;
using DiffAlgorithm;
using DiffAlgorithm.Diff;
using DiffIntegration.DiffFilesystemTree;

namespace DiffIntegration.DiffOutput
{
    /// <summary>
    /// Processor for printing out edit script between two files.
    /// </summary>
    public class EditScript : ProcessorAbstract
    {
        public override int Priority { get { return 125010; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay; } }

        public override void Process(IFilesystemTreeDirNode node)
        {
            // empty
        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            var dnode = node as DiffFileNode;

            if (dnode == null)
                return;

            var sb = new StringBuilder();

            using (StreamReader streamA = ((FileInfo)dnode.InfoLocal).OpenText())
            using (StreamReader streamB = ((FileInfo)dnode.InfoRemote).OpenText())
            {
                int n = 0;
                int m = 0;

                foreach (DiffItem diff in dnode.Diff.Items)
                {
                    sb.AppendLine(CreateHeader(diff));

                    // skip same
                    for (; n < diff.OldLineStart; n++) { streamA.ReadLine(); }
                    for (; m < diff.NewLineStart; m++) { streamB.ReadLine(); }

                    // deleted
                    for (int p = 0; p < diff.DeletedInOld; p++) { sb.AppendLine("< " + streamA.ReadLine()); n++; }

                    // missing newline at end of old file
                    if (n == dnode.Diff.FilesLineCount.Old && !dnode.Diff.FilesEndsWithNewLine.Old)
                        sb.AppendLine("\\ No newline at end of file");

                    if (diff.DeletedInOld > 0 && diff.InsertedInNew > 0)
                        sb.AppendLine("---");

                    // inserted
                    for (int p = 0; p < diff.InsertedInNew; p++) { sb.AppendLine("> " + streamB.ReadLine()); m++; }

                    // missing newline at end of old file
                    if (m == dnode.Diff.FilesLineCount.New && !dnode.Diff.FilesEndsWithNewLine.New)
                        sb.AppendLine("\\ No newline at end of file");
                }
            }

            Console.WriteLine(sb.ToString());
        }

        /// <summary>
        /// Type of a diff chunk. Action that was performed on an old file to get new file.
        /// </summary>
        private enum DiffType
        {
            Delete,
            Append,
            Change
        }

        /// <summary>
        /// Calculate type of diff depending on number of rows changed.
        /// </summary>
        /// <param name="diff">Diff Item for given lines change.</param>
        /// <returns>DiffType of the change between two files.</returns>
        private DiffType FindDiffType(DiffItem diff)
        {
            if (diff.DeletedInOld > 0 && diff.InsertedInNew > 0)
                return DiffType.Change;

            if (diff.DeletedInOld > 0)
                return DiffType.Delete;

            return DiffType.Append;
        }

        /// <summary>
        /// Print header of a diff change.
        /// </summary>
        /// <param name="diff">DiffItem chunk.</param>
        /// <returns>String header for a diff chunk.</returns>
        private string CreateHeader(DiffItem diff)
        {
            string header = "";

            switch (FindDiffType(diff))
            {
                case DiffType.Append:
                    header += CreateRange(diff.OldLineStart, 1)
                        + "a"
                        + CreateRange(diff.NewLineStart + 1, diff.InsertedInNew);
                    break;
                case DiffType.Delete:
                    header += CreateRange(diff.OldLineStart + 1, diff.DeletedInOld)
                        + "d"
                        + CreateRange(diff.NewLineStart, 1);
                    break;
                case DiffType.Change:
                    header += CreateRange(diff.OldLineStart + 1, diff.DeletedInOld)
                        + "c"
                        + CreateRange(diff.NewLineStart + 1, diff.InsertedInNew);
                    break;
            }

            return header;
        }

        /// <summary>
        /// Print range of how many lines were affected by the change.
        /// </summary>
        /// <param name="startingLine">Starting line in a file.</param>
        /// <param name="numberOfLines">Number of lines affected in a file.</param>
        /// <returns></returns>
        private string CreateRange(int startingLine, int numberOfLines)
        {
            return numberOfLines > 1
                ? startingLine + "," + (startingLine + numberOfLines - 1)
                : startingLine.ToString();
        }
    }
}
