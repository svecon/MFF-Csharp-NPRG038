
using System.Text;
using CoreLibrary.Enums;

namespace DiffAlgorithm
{
    /// <summary>
    /// Container for representing one diff change between three files.
    /// </summary>
    public class Diff3Item
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

        /// <summary>
        /// Type of the diff chunk - which files differ.
        /// </summary>
        public DifferencesStatusEnum Differeces;

        public Diff3Item(int oldLineStart, int newLineStart, int hisLineStart, int oldAffectedLines,
            int newAffectedLines, int hisAffectedLines, DifferencesStatusEnum diff)
        {
            OldLineStart = oldLineStart;
            NewLineStart = newLineStart;
            HisLineStart = hisLineStart;

            OldAffectedLines = oldAffectedLines;
            NewAffectedLines = newAffectedLines;
            HisAffectedLines = hisAffectedLines;

            Differeces = diff;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(OldLineStart)
                .Append(".")
                .Append(NewLineStart)
                .Append(".")
                .Append(HisLineStart)
                .Append("^")
                .Append(OldAffectedLines)
                .Append(".")
                .Append(NewAffectedLines)
                .Append(".")
                .Append(HisAffectedLines)
                ;

            if (Differeces == DifferencesStatusEnum.AllDifferent)
            {
                sb.Append("!!");
            }

            return sb.ToString();
        }

    }
}
