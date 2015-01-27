
using System;

namespace DiffAlgorithm
{
    public class Diff
    {
        public DiffItem[] Items { get; protected set; }

        private DateTime diffedTime;

        private System.IO.FileInfo fileA;
        private System.IO.FileInfo fileB;

        public Diff(System.IO.FileInfo fileA, System.IO.FileInfo fileB)
        {
            this.fileA = fileA;
            this.fileB = fileB;

            diffedTime = new DateTime();
        }

        public void SetDiffItems(DiffItem[] diffItems)
        {
            Items = diffItems;
        }

    }
}
