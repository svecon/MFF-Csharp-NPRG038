using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TextDiffAlgorithm.TwoWay;

namespace TextDiffProcessors.DiffOutput.TwoWay
{
    /// <summary>
    /// Prints differences between two files in Unified Diff Output
    /// </summary>
    public class UnifiedDiffOutput : DiffOutputAbstract<Diff, DiffItem>
    {
        private readonly int contextLinesCount;

        /// <summary>
        /// Initializes new instance of the <see cref="EditScript"/>
        /// </summary>
        /// <param name="infoLocal">Info for the local file.</param>
        /// <param name="infoRemote">Info for the remote file.</param>
        /// <param name="diff">Calculated 2-way text diff.</param>
        /// <param name="contextLinesCount">Number of lines to show as a context.</param>
        public UnifiedDiffOutput(FileInfo infoLocal, FileInfo infoRemote, Diff diff, int contextLinesCount = 3)
            : base(infoLocal, infoRemote, diff)
        {
            this.contextLinesCount = contextLinesCount;
        }

        public override IEnumerable<string> Print()
        {
            // print headers
            Console.ForegroundColor = ConsoleColor.DarkGray;
            yield return "--- " + CreateHeader(InfoLocal.FullName, InfoLocal.LastWriteTime);
            yield return "+++ " + CreateHeader(InfoRemote.FullName, InfoRemote.LastWriteTime);
            Console.ResetColor();

            // create and merge ovelapping diffs into chunks
            var chunks = new List<DiffChunk>(Diff.Items.Length);
            foreach (DiffChunk newChunk in Diff.Items.Select(diffItem => new DiffChunk(diffItem, Diff.FilesLineCount, contextLinesCount)))
            {
                if (chunks.Any() && chunks.Last().ChuckOverflows(newChunk))
                    chunks.Last().JoinChunk(newChunk);
                else
                    chunks.Add(newChunk);
            }

            // print chunks
            using (StreamReader localStream = InfoLocal.OpenText())
            using (StreamReader remoteStream = InfoRemote.OpenText())
            {
                int n = 0;
                int m = 0;

                foreach (DiffChunk chunk in chunks)
                {
                    CurrentDiffItem = chunk.CurrentDiff();

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    yield return chunk.Header();
                    Console.ResetColor();

                    // skip same
                    for (; n < chunk.LeftLineStart(); n++) { localStream.ReadLine(); }
                    for (; m < chunk.RightLineStart(); m++) { remoteStream.ReadLine(); }

                    do
                    {
                        // context between diffs
                        while (chunk.CurrentDiff().LocalLineStart > n)
                        {
                            yield return " " + localStream.ReadLine();
                            remoteStream.ReadLine();
                            n++;
                            m++;
                        }

                        // deleted
                        Console.ForegroundColor = ConsoleColor.Red;
                        for (int p = 0; p < chunk.CurrentDiff().LocalAffectedLines; p++) { yield return "-" + localStream.ReadLine(); n++; }
                        Console.ResetColor();

                        // missing newline at end of old file
                        if (n == Diff.FilesLineCount.Local && !Diff.FilesEndsWithNewLine.Local)
                            yield return @"\ No newline at end of file";

                        // inserted
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        for (int p = 0; p < chunk.CurrentDiff().RemoteAffectedLines; p++) { yield return "+" + remoteStream.ReadLine(); m++; }
                        Console.ResetColor();

                        // missing newline at end of new file
                        if (m == Diff.FilesLineCount.Remote && !Diff.FilesEndsWithNewLine.Remote)
                            yield return @"\ No newline at end of file";

                        // context between diffs
                        while (chunk.HasNextDiff() && chunk.NextDiff().LocalLineStart > n)
                        {
                            yield return " " + localStream.ReadLine();
                            remoteStream.ReadLine();
                            n++;
                            m++;
                        }

                        if (chunk.HasNextDiff())
                            DiffHasEnded = true;

                    } while (chunk.HasNextDiff() && chunk.MoveNextDiff());

                    // context after all diffs
                    for (; n <= chunk.LeftLineEnd(); n++) { yield return " " + localStream.ReadLine(); }

                    DiffHasEnded = true;
                }
            }
        }

        private static string CreateHeader(string filename, DateTime date)
        {
            return filename + "\t" + date.ToString("ddd MMM d H:mm:ss yyyy");
        }

    }
}
