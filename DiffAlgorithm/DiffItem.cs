using System.Text;

namespace DiffAlgorithm
{
    public struct DiffItem
    {
        public readonly int OldLineStart;
        public readonly int NewLineStart;

        public readonly int DeletedInOld;
        public readonly int InsertedInNew;

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
