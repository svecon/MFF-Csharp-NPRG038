using System;
using System.IO;
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
            {
                using (StreamReader streamB = ((FileInfo)dnode.InfoRight).OpenText())
                {
                    int n = 0;
                    int m = 0;

                    foreach (DiffItem diff in dnode.Diff.Items)
                    {
                        sb.AppendLine(createHeader(diff));

                        // skip same
                        for (; n < diff.LineStartA; n++) { streamA.ReadLine(); }
                        for (; m < diff.LineStartB; m++) { streamB.ReadLine(); }

                        // deleted
                        for (int p = 0; p < diff.DeletedInA; p++) { sb.AppendLine("< " + streamA.ReadLine()); n++; }

                        if (diff.DeletedInA > 0 && diff.InsertedInB > 0)
                            sb.AppendLine("---");

                        // inserted
                        for (int p = 0; p < diff.InsertedInB; p++) { sb.AppendLine("> " + streamB.ReadLine()); m++; }
                    }
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
            if (diff.DeletedInA > 0 && diff.InsertedInB > 0)
                return DiffType.Change;

            if (diff.DeletedInA > 0)
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
                    header += createRange(diff.LineStartA, 1)
                        + "a"
                        + createRange(diff.LineStartB + 1, diff.InsertedInB);
                    break;
                case DiffType.Delete:
                    header += createRange(diff.LineStartA + 1, diff.DeletedInA)
                        + "d"
                        + createRange(diff.LineStartB, 1);
                    break;
                case DiffType.Change:
                    header += createRange(diff.LineStartA + 1, diff.DeletedInA)
                        + "c"
                        + createRange(diff.LineStartB + 1, diff.InsertedInB);
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
