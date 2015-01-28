using System;
using System.IO;
using System.Linq;
using System.Text;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Processors;
using DiffAlgorithm;
using DiffIntegration.DiffFilesystemTree;

namespace DiffIntegration.DiffOutput
{
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

            using (StreamReader streamA = ((FileInfo)dnode.InfoLeft).OpenText())
            using (StreamReader streamB = ((FileInfo)dnode.InfoRight).OpenText())
            {
                int n = 0;
                int m = 0;

                foreach (DiffItem diff in dnode.Diff.Items)
                {
                    sb.AppendLine(createHeader(diff));

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

        private enum DiffType
        {
            Delete,
            Append,
            Change
        }

        private DiffType findDiffType(DiffItem diff)
        {
            if (diff.DeletedInOld > 0 && diff.InsertedInNew > 0)
                return DiffType.Change;

            if (diff.DeletedInOld > 0)
                return DiffType.Delete;

            return DiffType.Append;
        }

        private string createHeader(DiffItem diff)
        {
            int moveFirst = 0;
            int moveSecond = 0;

            string header = "";

            switch (findDiffType(diff))
            {
                case DiffType.Append:
                    header += createRange(diff.OldLineStart, 1)
                        + "a"
                        + createRange(diff.NewLineStart + 1, diff.InsertedInNew);
                    break;
                case DiffType.Delete:
                    header += createRange(diff.OldLineStart + 1, diff.DeletedInOld)
                        + "d"
                        + createRange(diff.NewLineStart, 1);
                    break;
                case DiffType.Change:
                    header += createRange(diff.OldLineStart + 1, diff.DeletedInOld)
                        + "c"
                        + createRange(diff.NewLineStart + 1, diff.InsertedInNew);
                    break;
            }

            return header;
        }

        private string createRange(int startingLine, int numberOfLines)
        {
            return numberOfLines > 1
                ? startingLine + "," + (startingLine + numberOfLines - 1)
                : startingLine.ToString();
        }
    }
}
