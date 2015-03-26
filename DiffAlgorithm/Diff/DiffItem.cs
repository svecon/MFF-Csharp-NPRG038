using System.Text;

namespace DiffAlgorithm.Diff
{
    /// <summary>
    /// Container for representing one diff change between two files.
    /// </summary>
    public class DiffItem
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
        /// Number of lines deleted in the old file.
        /// </summary>
        public readonly int DeletedInOld;

        /// <summary>
        /// Number of lines inserted in the new file.
        /// </summary>
        public readonly int InsertedInNew;

        /// <summary>
        /// Default action to do with this diff.
        /// </summary>
        public DiffItemActionEnum Action;

        public DiffItem(int oldLineStart, int newLineStart, int deletedInOld, int insertedInNew)
        {
            OldLineStart = oldLineStart;
            NewLineStart = newLineStart;
            DeletedInOld = deletedInOld;
            InsertedInNew = insertedInNew;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(DeletedInOld.ToString())
                .Append(".")
                .Append(InsertedInNew.ToString())
                .Append(".")
                .Append(OldLineStart.ToString())
                .Append(".")
                .Append(NewLineStart.ToString())
                .Append("*")
                ;

            return sb.ToString();
        }
    }
}
