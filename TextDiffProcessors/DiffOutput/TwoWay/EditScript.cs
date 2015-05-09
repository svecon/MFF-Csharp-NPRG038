using System.Collections.Generic;
using System.IO;
using TextDiffAlgorithm.TwoWay;

namespace TextDiffProcessors.DiffOutput.TwoWay
{
    /// <summary>
    /// Processor for printing out edit script between two files.
    /// </summary>
    public class EditScript : DiffOutputAbstract<Diff, DiffItem>
    {
        /// <summary>
        /// Initializes new instance of the <see cref="EditScript"/>
        /// </summary>
        /// <param name="infoLocal">Info for the local file.</param>
        /// <param name="infoRemote">Info for the remote file.</param>
        /// <param name="diff">Calculated 2-way text diff.</param>
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
                    for (; n < diffItem.LocalLineStart; n++) { localStream.ReadLine(); }
                    for (; m < diffItem.RemoteLineStart; m++) { remoteStream.ReadLine(); }

                    // deleted
                    for (int p = 0; p < diffItem.LocalAffectedLines; p++) { yield return "< " + localStream.ReadLine(); n++; }

                    // missing newline at end of old file
                    if (n == Diff.FilesLineCount.Local && !Diff.FilesEndsWithNewLine.Local)
                        yield return @"\ No newline at end of file";

                    if (diffItem.LocalAffectedLines > 0 && diffItem.RemoteAffectedLines > 0)
                        yield return "---";

                    // inserted
                    for (int p = 0; p < diffItem.RemoteAffectedLines; p++) { yield return "> " + remoteStream.ReadLine(); m++; }

                    // missing newline at end of old file
                    if (m == Diff.FilesLineCount.Remote && !Diff.FilesEndsWithNewLine.Remote)
                        yield return @"\ No newline at end of file";

                    DiffHasEnded = true;
                }
            }
        }

        /// <summary>
        /// Type of a diffItem chunk. PreferedAction that was performed on an old file to get new file.
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
            if (diffItem.LocalAffectedLines > 0 && diffItem.RemoteAffectedLines > 0)
                return DiffType.Change;

            if (diffItem.LocalAffectedLines > 0)
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
                    header += CreateRange(diffItem.LocalLineStart, 1)
                        + "a"
                        + CreateRange(diffItem.RemoteLineStart + 1, diffItem.RemoteAffectedLines);
                    break;
                case DiffType.Delete:
                    header += CreateRange(diffItem.LocalLineStart + 1, diffItem.LocalAffectedLines)
                        + "d"
                        + CreateRange(diffItem.RemoteLineStart, 1);
                    break;
                case DiffType.Change:
                    header += CreateRange(diffItem.LocalLineStart + 1, diffItem.LocalAffectedLines)
                        + "c"
                        + CreateRange(diffItem.RemoteLineStart + 1, diffItem.RemoteAffectedLines);
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
