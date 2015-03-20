using System.Collections.Generic;
using System.Linq;
using DiffAlgorithm;
using DiffAlgorithm.Diff;

namespace DiffIntegration.DiffOutput
{
    /// <summary>
    /// DiffChunk is a container for diff changes that are very near to each other (in terms of line numbers).
    /// </summary>
    internal struct DiffChunk
    {
        /// <summary>
        /// All diffs close to each other
        /// </summary>
        private readonly List<DiffItem> diffs;

        /// <summary>
        /// Simple iterator number
        /// </summary>
        private int currentDiff;

        /// <summary>
        /// Structure holding number of lines in each file.
        /// </summary>
        private Diff.FilesLineCountStruct lineCounts;

        /// <summary>
        /// Maximum padding between diffs to be considered "close".
        /// </summary>
        private readonly int padding;

        /// <summary>
        /// Constructor for a DiffChunk container.
        /// </summary>
        /// <param name="diff">First Diff that the chunk is based on.</param>
        /// <param name="filesLineCount">Line numbers of all diffed files.</param>
        /// <param name="padding">Padding around diffs determining how close the diffs are.</param>
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
            int end = diffs.Last().NewLineStart + diffs.Last().InsertedInNew - 1 + padding;

            return (end > lineCounts.New - 1) ? lineCounts.New - 1 : end;
        }

        #endregion

        #region Joining close chunks
        public bool ChuckOverflows(DiffChunk b)
        {
            return (LeftLineEnd() + 1 >= b.LeftLineStart());
        }

        public void JoinChunk(DiffChunk b)
        {
            diffs.AddRange(b.diffs);
        }
        #endregion

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
            int x = LeftLineEnd();
            int y = LeftLineStart();

            int c = RightLineEnd();
            int d = RightLineStart();

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