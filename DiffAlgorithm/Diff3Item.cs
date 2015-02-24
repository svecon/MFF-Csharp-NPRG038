
using CoreLibrary.Enums;

namespace DiffAlgorithm
{
    /// <summary>
    /// Container for representing one diff change between three files.
    /// </summary>
    internal class Diff3Item
    {
        /// <summary>
        /// Line number of old file where change starts.
        /// </summary>
        public readonly int OldLineStart;

        /// <summary>
        /// Line number of new file where change starts.
        /// </summary>
        public readonly int NewLineStart;

        /// <summary>
        /// Line number of his file where change starts.
        /// </summary>
        public readonly int HisLineStart;

        /// <summary>
        /// Number of lines affected in the old file.
        /// </summary>
        public readonly int OldAffectedLines;

        /// <summary>
        /// Number of lines affected in the new file.
        /// </summary>
        public readonly int NewAffectedLines;

        /// <summary>
        /// Number of lines affected in his file.
        /// </summary>
        public readonly int HisAffectedLines;

        private DifferencesStatusEnum differeces;

        public Diff3Item(int oldLineStart, int newLineStart, int hisLineStart, int oldAffectedLines,
            int newAffectedLines, int hisAffectedLines, DifferencesStatusEnum diff)
        {
            OldLineStart = oldLineStart;
            NewLineStart = newLineStart;
            HisLineStart = HisLineStart;

            OldAffectedLines = oldAffectedLines;
            NewAffectedLines = newAffectedLines;
            HisAffectedLines = hisAffectedLines;

            differeces = diff;
        }


    }
}
