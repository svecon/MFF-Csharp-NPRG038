using System;
using System.IO;
using System.Text;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Processors;
using DiffAlgorithm;
using DiffAlgorithm.Diff3;
using DiffIntegration.DiffFilesystemTree;

namespace DiffIntegration.DiffOutput
{
    /// <summary>
    /// Processor for printing out 3-way diff between three files.
    /// </summary>
    public class Diff3NormalOutput : ProcessorAbstract
    {
        public override int Priority { get { return 125035; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.ThreeWay; } }

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

            using (StreamReader streamL = ((FileInfo)dnode.InfoLocal).OpenText())
            using (StreamReader streamR = ((FileInfo)dnode.InfoRemote).OpenText())
            using (StreamReader streamO = ((FileInfo)dnode.InfoBase).OpenText())
            {
                int m = 0;
                int n = 0;
                int o = 0;

                foreach (Diff3Item diff in dnode.Diff3.Items)
                {
                    sb.AppendLine(HunkHeader(diff.Differeces));

                    // skip same
                    for (; o < diff.BaseLineStart; o++) { streamO.ReadLine(); }
                    for (; m < diff.LocalLineStart; m++) { streamL.ReadLine(); }
                    for (; n < diff.RemoteLineStart; n++) { streamR.ReadLine(); }

                    // DifferencesStatusEnum.LocalRemoteSame has a different order of blocks
                    if (diff.Differeces == DifferencesStatusEnum.LocalRemoteSame)
                    {
                        sb.Append(PrintSection("1", diff.LocalLineStart, diff.LocalAffectedLines,
                            ref m, streamL, dnode.Diff3.FilesLineCount.Local, dnode.Diff3.FilesEndsWithNewLine.Local, false));

                        sb.Append(PrintSection("3", diff.RemoteLineStart, diff.RemoteAffectedLines,
                            ref n, streamR, dnode.Diff3.FilesLineCount.Remote, dnode.Diff3.FilesEndsWithNewLine.Remote));

                        sb.Append(PrintSection("2", diff.BaseLineStart, diff.BaseAffectedLines,
                            ref o, streamO, dnode.Diff3.FilesLineCount.Base, dnode.Diff3.FilesEndsWithNewLine.Base));
                    } else
                    {
                        sb.Append(PrintSection("1", diff.LocalLineStart, diff.LocalAffectedLines,
                            ref m, streamL, dnode.Diff3.FilesLineCount.Local, dnode.Diff3.FilesEndsWithNewLine.Local,
                                diff.Differeces != DifferencesStatusEnum.BaseLocalSame));

                        sb.Append(PrintSection("2", diff.BaseLineStart, diff.BaseAffectedLines,
                            ref o, streamO, dnode.Diff3.FilesLineCount.Base, dnode.Diff3.FilesEndsWithNewLine.Base,
                                diff.Differeces != DifferencesStatusEnum.BaseRemoteSame));

                        sb.Append(PrintSection("3", diff.RemoteLineStart, diff.RemoteAffectedLines,
                            ref n, streamR, dnode.Diff3.FilesLineCount.Remote, dnode.Diff3.FilesEndsWithNewLine.Remote));
                    }

                    Console.Write(sb.ToString());
                    sb.Clear();
                }
            }
        }

        private string PrintSection(string fileMark, int lineStart, int affectedLines,
            ref int c, TextReader stream,
            int fileLinesCount, bool endsNewLine, bool printLines = true)
        {
            var sb = new StringBuilder();
            sb.AppendLine(FileHeader(fileMark, lineStart, affectedLines));

            if (printLines)
                for (int p = 0; p < affectedLines; p++)
                {
                    sb.AppendLine("  " + stream.ReadLine());
                    c++;
                }

            if (c == fileLinesCount && !endsNewLine)
                sb.AppendLine("\\ No newline at end of file");

            return sb.ToString();
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
                    throw new ApplicationException("Diff3 chunk cannot have this DifferenceStatusEnum.");
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
