﻿using System.Collections.Generic;
using System.IO;
using DiffAlgorithm.TwoWay;

namespace DiffIntegration.DiffOutput.TwoWay
{
    /// <summary>
    /// Processor for printing out edit script between two files.
    /// </summary>
    public class EditScript : DiffOutputAbstract<Diff, DiffItem>
    {
        public EditScript(FileInfo infoLocal, FileInfo infoRemote, Diff diff)
            : base(infoLocal, infoRemote, diff)
        {
        }

        public override IEnumerable<string> Print()
        {
            using (StreamReader localStream = InfoLocal.OpenText())
            using (StreamReader remoteStream = InfoRemote.OpenText())
            {
                int n = 0;
                int m = 0;

                foreach (DiffItem diffItem in Diff.Items)
                {
                    CurrentDiffItem = diffItem;

                    yield return CreateHeader(diffItem);

                    // skip same
                    for (; n < diffItem.OldLineStart; n++) { localStream.ReadLine(); }
                    for (; m < diffItem.NewLineStart; m++) { remoteStream.ReadLine(); }

                    // deleted
                    for (int p = 0; p < diffItem.DeletedInOld; p++) { yield return "< " + localStream.ReadLine(); n++; }

                    // missing newline at end of old file
                    if (n == Diff.FilesLineCount.Local && !Diff.FilesEndsWithNewLine.Local)
                        yield return @"\ No newline at end of file";

                    if (diffItem.DeletedInOld > 0 && diffItem.InsertedInNew > 0)
                        yield return "---";

                    // inserted
                    for (int p = 0; p < diffItem.InsertedInNew; p++) { yield return "> " + remoteStream.ReadLine(); m++; }

                    // missing newline at end of old file
                    if (m == Diff.FilesLineCount.Remote && !Diff.FilesEndsWithNewLine.Remote)
                        yield return @"\ No newline at end of file";

                    DiffHasEnded = true;
                }
            }
        }

        /// <summary>
        /// Type of a diffItem chunk. Action that was performed on an old file to get new file.
        /// </summary>
        private enum DiffType
        {
            Delete,
            Append,
            Change
        }

        /// <summary>
        /// Calculate type of diffItem depending on number of rows changed.
        /// </summary>
        /// <param name="diffItem">Diff Item for given lines change.</param>
        /// <returns>DiffType of the change between two files.</returns>
        private DiffType FindDiffType(DiffItem diffItem)
        {
            if (diffItem.DeletedInOld > 0 && diffItem.InsertedInNew > 0)
                return DiffType.Change;

            if (diffItem.DeletedInOld > 0)
                return DiffType.Delete;

            return DiffType.Append;
        }

        /// <summary>
        /// Print header of a diffItem change.
        /// </summary>
        /// <param name="diffItem">DiffItem chunk.</param>
        /// <returns>String header for a diffItem chunk.</returns>
        private string CreateHeader(DiffItem diffItem)
        {
            string header = "";

            switch (FindDiffType(diffItem))
            {
                case DiffType.Append:
                    header += CreateRange(diffItem.OldLineStart, 1)
                        + "a"
                        + CreateRange(diffItem.NewLineStart + 1, diffItem.InsertedInNew);
                    break;
                case DiffType.Delete:
                    header += CreateRange(diffItem.OldLineStart + 1, diffItem.DeletedInOld)
                        + "d"
                        + CreateRange(diffItem.NewLineStart, 1);
                    break;
                case DiffType.Change:
                    header += CreateRange(diffItem.OldLineStart + 1, diffItem.DeletedInOld)
                        + "c"
                        + CreateRange(diffItem.NewLineStart + 1, diffItem.InsertedInNew);
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