using System.Text;

namespace DiffAlgorithm
{
    public struct DiffItem
    {
        public DiffItem(int lineStartA, int lineStartB, int deletedInA, int insertedInB)
        {
            LineStartA = lineStartA;
            LineStartB = lineStartB;
            DeletedInA = deletedInA;
            InsertedInB = insertedInB;
        }

        public readonly int LineStartA;
        public readonly int LineStartB;

        public readonly int DeletedInA;
        public readonly int InsertedInB;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(DeletedInA.ToString())
                .Append(".")
                .Append(InsertedInB.ToString())
                .Append(".")
                .Append(LineStartA.ToString())
                .Append(".")
                .Append(LineStartB.ToString())
                .Append("*")
                ;

            return sb.ToString();
        }
    }
}
