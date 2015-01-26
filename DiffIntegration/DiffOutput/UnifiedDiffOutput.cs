using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Processors;
using CoreLibrary.Settings.Attributes;
using DiffAlgorithm;
using DiffIntegration.DiffFilesystemTree;

namespace DiffIntegration.DiffOutput
{
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

        struct Chunk
        {
            private readonly List<DiffItem> diffs;
            private int currentDiff;
            private DiffFileNode.NumberOfLinesStruct fileLines;
            private readonly int padding;

            public Chunk(DiffItem diff, DiffFileNode.NumberOfLinesStruct numberOfLines, int padding)
            {
                fileLines = numberOfLines;
                diffs = new List<DiffItem> { diff };

                this.padding = padding;
                currentDiff = 0;
            }

            public int StartsAOnLine()
            {
                int start = diffs[0].LineStartA - padding;

                return (start < 0) ? 0 : start;
            }

            public int EndsAOnLine()
            {
                int end = diffs[diffs.Count - 1].LineStartA + diffs[diffs.Count - 1].DeletedInA + padding;

                return (end > fileLines.Left) ? fileLines.Left : end;
            }

            public int StartsBOnLine()
            {
                int start = diffs[0].LineStartB - padding;

                return (start < 0) ? 0 : start;
            }

            public int EndsBOnLine()
            {
                int end = diffs[diffs.Count - 1].LineStartB + diffs[diffs.Count - 1].InsertedInB + padding;

                return (end > fileLines.Right) ? fileLines.Right : end;
            }

            public bool ChuckOverflows(Chunk b)
            {
                return (EndsAOnLine() + 1 >= b.StartsAOnLine());
            }

            public void JoinChunk(Chunk b)
            {
                diffs.AddRange(b.GetDiffsList());
            }

            public DiffItem CurrentDiff()
            {
                return diffs[currentDiff];
            }

            public bool HasNextDiff()
            {
                return currentDiff + 1 < diffs.Count;
            }

            public DiffItem NextDiff()
            {
                return diffs[currentDiff + 1];
            }

            public bool MoveNextDiff()
            {
                if (HasNextDiff())
                    currentDiff++;

                return true;
            }


            private IEnumerable<DiffItem> GetDiffsList()
            {
                return diffs;
            }

            public string Header()
            {
                return "@@ -"
                    + (StartsAOnLine() + 1)
                    + ","
                    + (EndsAOnLine() - StartsAOnLine())
                    + " +"
                    + (StartsBOnLine() + 1)
                    + ","
                    + (EndsBOnLine() - StartsBOnLine())
                    + " @@";
            }

        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            var dnode = node as DiffFileNode;

            if (dnode == null)
                return;

            var sb = new StringBuilder();
            sb.AppendLine("--- " + createHeader(dnode.InfoLeft.FullName, dnode.InfoLeft.CreationTime));
            sb.AppendLine("+++ " + createHeader(dnode.InfoRight.FullName, dnode.InfoRight.CreationTime));

            // create and merge ovelapping diffs into chunks
            var chunks = new List<Chunk>(dnode.Diff.Length);
            foreach (Chunk newChunk in dnode.Diff.Select(diff => new Chunk(diff, dnode.NumberOfLines, Padding)))
            {
                if (chunks.Any() && chunks.Last().ChuckOverflows(newChunk))
                    chunks.Last().JoinChunk(newChunk);
                else
                    chunks.Add(newChunk);
            }

            using (StreamReader streamA = ((FileInfo)dnode.InfoLeft).OpenText())
            {
                using (StreamReader streamB = ((FileInfo)dnode.InfoRight).OpenText())
                {
                    int n = 0;
                    int m = 0;

                    foreach (Chunk chunk in chunks)
                    {
                        sb.AppendLine(chunk.Header());

                        // skip same
                        for (; n < chunk.StartsAOnLine(); n++) { streamA.ReadLine(); }
                        for (; m < chunk.StartsBOnLine(); m++) { streamB.ReadLine(); }

                        do
                        {
                            // context between diffs
                            while (chunk.CurrentDiff().LineStartA > n)
                            {
                                sb.AppendLine(" " + streamA.ReadLine());
                                streamB.ReadLine();
                                n++;
                                m++;
                            }
                            // deleted
                            for (int p = 0; p < chunk.CurrentDiff().DeletedInA; p++) { sb.AppendLine("-" + streamA.ReadLine()); n++; }
                            // inserted
                            for (int p = 0; p < chunk.CurrentDiff().InsertedInB; p++) { sb.AppendLine("+" + streamB.ReadLine()); m++; }

                            // context between diffs
                            while (chunk.HasNextDiff() && chunk.NextDiff().LineStartA > n)
                            {
                                sb.AppendLine(" " + streamA.ReadLine());
                                streamB.ReadLine();
                                n++;
                                m++;
                            }

                        } while (chunk.HasNextDiff() && chunk.MoveNextDiff());

                        // context after all diffs
                        for (; n < chunk.EndsAOnLine(); n++) { sb.AppendLine(" " + streamA.ReadLine()); }
                    }
                }
            }

            Console.WriteLine(sb.ToString());
        }

        private string createHeader(string filename, DateTime date)
        {
            return filename + "\t" + date.ToString("ddd MMM d H:mm:ss yyyy");
        }

    }
}
