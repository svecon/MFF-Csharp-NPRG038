
using CoreLibrary.Enums;

namespace DiffAlgorithm
{
    internal class Diff3Item
    {
        public readonly int OldLineStart;
        public readonly int NewLineStart;
        public readonly int HisLineStart;

        public readonly int OldAffectedLines;
        public readonly int NewAffectedLines;
        public readonly int HisAffectedLines;

        private DifferencesStatusEnum Differeces;

        public Diff3Item(int oldLineStart, int newLineStart, int hisLineStart, int oldAffectedLines,
            int newAffectedLines, int hisAffectedLines, DifferencesStatusEnum diff)
        {
            OldLineStart = oldLineStart;
            NewLineStart = newLineStart;
            HisLineStart = HisLineStart;

            OldAffectedLines = oldAffectedLines;
            NewAffectedLines = newAffectedLines;
            HisAffectedLines = hisAffectedLines;

            Differeces = diff;
        }


    }
}
