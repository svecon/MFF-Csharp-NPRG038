using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffAlgorithm
{
    public struct Item
    {
        public Item(int lineStartA, int lineStartB, int deletedInA, int insertedInB)
        {
            this.LineStartA = lineStartA;
            this.LineStartB = lineStartB;
            this.DeletedInA = deletedInA;
            this.InsertedInB = insertedInB;
        }

        public readonly int LineStartA;
        public readonly int LineStartB;

        public readonly int DeletedInA;
        public readonly int InsertedInB;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
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
