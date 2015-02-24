using System.Collections.Generic;
using System.Linq;
using DiffAlgorithm;

namespace DiffIntegration.DiffOutput
{
    internal struct DiffChunk
    {
        private readonly List<DiffItem> diffs;
        private int currentDiff;
        private Diff.FilesLineCountStruct lineCounts;
        private readonly int padding;

        public DiffChunk(DiffItem diff, Diff.FilesLineCountStruct filesLineCount, int padding)
        {
            lineCounts = filesLineCount;
            diffs = new List<DiffItem> { diff };

            this.padding = padding;
            currentDiff = 0;
        }

        #region Computing chunk size

        public int LeftLineStart()
        {
            int start = diffs[0].OldLineStart - padding;

            return (start < 0) ? 0 : start;
        }

        public int LeftLineEnd()
        {
            int end = diffs.Last().OldLineStart + diffs.Last().DeletedInOld - 1 + padding;

            return (end > lineCounts.Old - 1) ? lineCounts.Old - 1 : end;
        }

        public int RightLineStart()
        {
            int start = diffs[0].NewLineStart - padding;

            return (start < 0) ? 0 : start;
        }

        public int RightLineEnd()
        {
            int end = diffs.Last().NewLineStart + diffs.Last().InsertedInNew -1 + padding;

            return (end > lineCounts.New - 1) ? lineCounts.New - 1 : end;
        }

        #endregion

        public bool ChuckOverflows(DiffChunk b)
        {
            return (LeftLineEnd() + 1 >= b.LeftLineStart());
        }

        public void JoinChunk(DiffChunk b)
        {
            diffs.AddRange(b.diffs);
        }

        #region Ultra-simple iterator

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

        #endregion

        public string Header()
        {
            var x = LeftLineEnd();
            var y = LeftLineStart();

            var c = RightLineEnd();
            var d = RightLineStart();

            return "@@ -"
                   + (LeftLineStart() + 1)
                   + ","
                   + (LeftLineEnd() - LeftLineStart() + 1)
                   + " +"
                   + (RightLineStart() + 1)
                   + ","
                   + (RightLineEnd() - RightLineStart() + 1)
                   + " @@";
        }

    }
}