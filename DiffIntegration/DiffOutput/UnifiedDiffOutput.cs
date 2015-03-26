using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Processors;
using CoreLibrary.Settings.Attributes;
using DiffAlgorithm.Diff;
using DiffIntegration.DiffFilesystemTree;

namespace DiffIntegration.DiffOutput
{
    /// <summary>
    /// Processor for printing out unified diff output between two files.
    /// </summary>
    public class UnifiedDiffOutput
    {
        //[Settings("Context ContextLinesCount in unified diff output.", "context-ContextLinesCount-output", "PO")]
        public int ContextLinesCount;// = 3;

        private readonly FileInfo infoLocal;
        private readonly FileInfo infoRemote;
        private readonly Diff diff;


        private bool diffHasEnded;

        public bool DiffHasEnded
        {
            get
            {
                bool temp = diffHasEnded;
                diffHasEnded = false;
                return temp;
            }
            private set { diffHasEnded = value; }
        }

        public DiffItem CurrentDiffItem { get; private set; }

        public UnifiedDiffOutput(FileInfo infoLocal, FileInfo infoRemote, Diff diff, int contextLinesCount = 3)
        {
            this.infoLocal = infoLocal;
            this.diff = diff;
            this.infoRemote = infoRemote;
            ContextLinesCount = contextLinesCount;
        }

        public IEnumerable<string> Print()
        {
            // print headers
            Console.ForegroundColor = ConsoleColor.White;
            yield return "--- " + CreateHeader(infoLocal.FullName, infoLocal.LastWriteTime);
            yield return "+++ " + CreateHeader(infoRemote.FullName, infoRemote.LastWriteTime);
            Console.ResetColor();

            // create and merge ovelapping diffs into chunks
            var chunks = new List<DiffChunk>(diff.Items.Length);
            foreach (DiffChunk newChunk in diff.Items.Select(diffItem => new DiffChunk(diffItem, diff.FilesLineCount, ContextLinesCount)))
            {
                if (chunks.Any() && chunks.Last().ChuckOverflows(newChunk))
                    chunks.Last().JoinChunk(newChunk);
                else
                    chunks.Add(newChunk);
            }

            // print chunks
            using (StreamReader localStream = infoLocal.OpenText())
            using (StreamReader remoteStream = infoRemote.OpenText())
            {
                int n = 0;
                int m = 0;

                foreach (DiffChunk chunk in chunks)
                {
                    CurrentDiffItem = chunk.CurrentDiff();

                    Console.ForegroundColor = ConsoleColor.White;
                    yield return chunk.Header();
                    Console.ResetColor();

                    // skip same
                    for (; n < chunk.LeftLineStart(); n++) { localStream.ReadLine(); }
                    for (; m < chunk.RightLineStart(); m++) { remoteStream.ReadLine(); }

                    do
                    {
                        // context between diffs
                        while (chunk.CurrentDiff().OldLineStart > n)
                        {
                            yield return " " + localStream.ReadLine();
                            remoteStream.ReadLine();
                            n++;
                            m++;
                        }
                        
                        // deleted
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        for (int p = 0; p < chunk.CurrentDiff().DeletedInOld; p++) { yield return "-" + localStream.ReadLine(); n++; }
                        Console.ResetColor();

                        // missing newline at end of old file
                        if (n == diff.FilesLineCount.Local && !diff.FilesEndsWithNewLine.Local)
                            yield return @"\ No newline at end of file";

                        // inserted
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        for (int p = 0; p < chunk.CurrentDiff().InsertedInNew; p++) { yield return "+" + remoteStream.ReadLine(); m++; }
                        Console.ResetColor();

                        // missing newline at end of new file
                        if (m == diff.FilesLineCount.Remote && !diff.FilesEndsWithNewLine.Remote)
                            yield return @"\ No newline at end of file";

                        // context between diffs
                        while (chunk.HasNextDiff() && chunk.NextDiff().OldLineStart > n)
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
