using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CoreLibrary.Enums;
using TextDiffAlgorithm.ThreeWay;

namespace TextDiffProcessors.DiffOutput.ThreeWay
{
    /// <summary>
    /// Prints differences between 3 files in Diff Normal Output.
    /// </summary>
    public class Diff3NormalOutput : DiffOutputAbstract<Diff3, Diff3Item>
    {
        private readonly FileInfo infoBase;

        public Diff3NormalOutput(FileInfo infoLocal, FileInfo infoBase, FileInfo infoRemote, Diff3 diff)
            : base(infoLocal, infoRemote, diff)
        {
            this.infoBase = infoBase;
        }

        public override IEnumerable<string> Print()
        {
            using (StreamReader localStream = InfoLocal.OpenText())
            using (StreamReader remoteStream = InfoRemote.OpenText())
            using (StreamReader baseStream = infoBase.OpenText())
            {
                int m = 0;
                int n = 0;
                int o = 0;

                foreach (Diff3Item diffItem in Diff.Items)
                {
                    CurrentDiffItem = diffItem;

                    yield return HunkHeader(diffItem.Differeces);

                    // skip same
                    for (; o < diffItem.BaseLineStart; o++) { baseStream.ReadLine(); }
                    for (; m < diffItem.LocalLineStart; m++) { localStream.ReadLine(); }
                    for (; n < diffItem.RemoteLineStart; n++) { remoteStream.ReadLine(); }

                    // DifferencesStatusEnum.LocalRemoteSame has a different order of blocks
                    if (diffItem.Differeces == DifferencesStatusEnum.LocalRemoteSame)
                    {
                        foreach (string l in PrintSection("1", diffItem.LocalLineStart, diffItem.LocalAffectedLines,
                            m, localStream, Diff.FilesLineCount.Local, Diff.FilesEndsWithNewLine.Local, false))
                            yield return l;

                        foreach (string l in PrintSection("3", diffItem.RemoteLineStart, diffItem.RemoteAffectedLines,
                            n, remoteStream, Diff.FilesLineCount.Remote, Diff.FilesEndsWithNewLine.Remote))
                            yield return l;

                        foreach (string l in PrintSection("2", diffItem.BaseLineStart, diffItem.BaseAffectedLines,
                            o, baseStream, Diff.FilesLineCount.Base, Diff.FilesEndsWithNewLine.Base))
                            yield return l;
                    } else
                    {
                        foreach (string l in PrintSection("1", diffItem.LocalLineStart, diffItem.LocalAffectedLines,
                            m, localStream, Diff.FilesLineCount.Local, Diff.FilesEndsWithNewLine.Local,
                                diffItem.Differeces != DifferencesStatusEnum.BaseLocalSame))
                            yield return l;

                        foreach (string l in PrintSection("2", diffItem.BaseLineStart, diffItem.BaseAffectedLines,
                            o, baseStream, Diff.FilesLineCount.Base, Diff.FilesEndsWithNewLine.Base,
                                diffItem.Differeces != DifferencesStatusEnum.BaseRemoteSame))
                            yield return l;

                        foreach (string l in PrintSection("3", diffItem.RemoteLineStart, diffItem.RemoteAffectedLines,
                            n, remoteStream, Diff.FilesLineCount.Remote, Diff.FilesEndsWithNewLine.Remote))
                            yield return l;
                    }

                    m += diffItem.LocalAffectedLines;
                    n += diffItem.RemoteAffectedLines;
                    o += diffItem.BaseAffectedLines;

                    DiffHasEnded = true;
                }
            }
        }

        private IEnumerable<string> PrintSection(string fileMark, int lineStart, int affectedLines,
            int c, TextReader stream, int fileLinesCount, bool endsNewLine, bool printLines = true)
        {
            yield return FileHeader(fileMark, lineStart, affectedLines);

            for (int p = 0; p < affectedLines; p++)
            {
                if (printLines)
                {
                    yield return "  " + stream.ReadLine();
                } else
                {
                    stream.ReadLine();
                }
                c++;
            }


            if (c == fileLinesCount && !endsNewLine)
                yield return "\\ No newline at end of file";
        }

        /// <summary>
        /// Returns diff chunk header with a number of file that is different
        /// </summary>
        /// <param name="diffStatus">Which files are same.</param>
        /// <returns>String header of the diff chunk.</returns>
        private string HunkHeader(DifferencesStatusEnum diffStatus)
        {
            switch (diffStatus)
            {
                case DifferencesStatusEnum.BaseLocalSame:
                    return "===3";
                case DifferencesStatusEnum.BaseRemoteSame:
                    return "===1";
                case DifferencesStatusEnum.LocalRemoteSame:
                    return "===2";
                case DifferencesStatusEnum.AllDifferent:
                    return "===";
                default:
                    throw new ApplicationException("Diff chunk cannot have this DifferenceStatusEnum.");
            }
        }

        /// <summary>
        /// String header of file diff section
        /// </summary>
        /// <param name="fileMark">String representation of file.</param>
        /// <param name="lineStart">Where does the diff start.</param>
        /// <param name="linesAffected">How many lines does diff affect.</param>
        /// <returns>String header of file diff section.</returns>
        private string FileHeader(string fileMark, int lineStart, int linesAffected)
        {
            return fileMark + ":" + CreateRange(lineStart, linesAffected);
        }

        /// <summary>
        /// Print range of how many lines were affected by the change.
        /// </summary>
        /// <param name="startingLine">Starting line in a file.</param>
        /// <param name="numberOfLines">Number of lines affected in a file.</param>
        /// <returns></returns>
        private string CreateRange(int startingLine, int numberOfLines)
        {
            if (numberOfLines == 0)
            {
                return startingLine + "a";
            }

            return numberOfLines > 1
                ? (startingLine + 1) + "," + ((startingLine + 1) + numberOfLines - 1) + "c"
                : (startingLine + 1) + "c";
        }
    }
}
