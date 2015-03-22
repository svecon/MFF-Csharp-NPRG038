﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Processors;
using CoreLibrary.Settings.Attributes;
using DiffIntegration.DiffFilesystemTree;

namespace DiffIntegration.DiffOutput
{
    /// <summary>
    /// Processor for printing out unified diff output between two files.
    /// </summary>
    public class UnifiedDiffOutput : ProcessorAbstract
    {
        [Settings("Context padding in unified diff output.", "context-padding-output", "PO")]
        public int Padding = 3;

        public override int Priority { get { return 125005; } }

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

            if (!dnode.Diff.Items.Any())
                return;

            var sb = new StringBuilder();

            // print headers
            sb.AppendLine("--- " + CreateHeader(dnode.InfoLocal.FullName, dnode.InfoLocal.LastWriteTime));
            sb.AppendLine("+++ " + CreateHeader(dnode.InfoRemote.FullName, dnode.InfoRemote.LastWriteTime));

            // create and merge ovelapping diffs into chunks
            var chunks = new List<DiffChunk>(dnode.Diff.Items.Length);
            foreach (DiffChunk newChunk in dnode.Diff.Items.Select(diff => new DiffChunk(diff, dnode.Diff.FilesLineCount, Padding)))
            {
                if (chunks.Any() && chunks.Last().ChuckOverflows(newChunk))
                    chunks.Last().JoinChunk(newChunk);
                else
                    chunks.Add(newChunk);
            }

            // print chunks
            using (StreamReader streamA = ((FileInfo)dnode.InfoLocal).OpenText())
            using (StreamReader streamB = ((FileInfo)dnode.InfoRemote).OpenText())
            {
                int n = 0;
                int m = 0;

                foreach (DiffChunk chunk in chunks)
                {
                    sb.AppendLine(chunk.Header());

                    // skip same
                    for (; n < chunk.LeftLineStart(); n++) { streamA.ReadLine(); }
                    for (; m < chunk.RightLineStart(); m++) { streamB.ReadLine(); }

                    do
                    {
                        // context between diffs
                        while (chunk.CurrentDiff().OldLineStart > n)
                        {
                            sb.AppendLine(" " + streamA.ReadLine());
                            streamB.ReadLine();
                            n++;
                            m++;
                        }
                        // deleted
                        for (int p = 0; p < chunk.CurrentDiff().DeletedInOld; p++) { sb.AppendLine("-" + streamA.ReadLine()); n++; }

                        // missing newline at end of old file
                        if (n == dnode.Diff.FilesLineCount.Old && !dnode.Diff.FilesEndsWithNewLine.Old)
                            sb.AppendLine("\\ No newline at end of file");

                        // inserted
                        for (int p = 0; p < chunk.CurrentDiff().InsertedInNew; p++) { sb.AppendLine("+" + streamB.ReadLine()); m++; }

                        // missing newline at end of new file
                        if (m == dnode.Diff.FilesLineCount.New && !dnode.Diff.FilesEndsWithNewLine.New)
                            sb.AppendLine("\\ No newline at end of file");

                        // context between diffs
                        while (chunk.HasNextDiff() && chunk.NextDiff().OldLineStart > n)
                        {
                            sb.AppendLine(" " + streamA.ReadLine());
                            streamB.ReadLine();
                            n++;
                            m++;
                        }
                         
                    } while (chunk.HasNextDiff() && chunk.MoveNextDiff());

                    // context after all diffs
                    for (; n <= chunk.LeftLineEnd(); n++) { sb.AppendLine(" " + streamA.ReadLine()); }
                }
            }

            Console.WriteLine(sb.ToString());
        }

        private string CreateHeader(string filename, DateTime date)
        {
            return filename + "\t" + date.ToString("ddd MMM d H:mm:ss yyyy");
        }

    }
}
