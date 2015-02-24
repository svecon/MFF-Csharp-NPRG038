
using System.Collections;
using System.Collections.Generic;
using CoreLibrary.Enums;

namespace DiffAlgorithm
{
    class Diff3Algorithm
    {
        private DiffItem[] leftBase;
        private DiffItem[] rightBase;

        private int[] leftFile;
        private int[] rightFile;

        int leftI = 0;
        int rightI = 0;
        private DiffItem? curLeft { get { if (leftI < leftBase.Length) return leftBase[leftI]; return null; } }
        private DiffItem? curRight { get { if (rightI < rightBase.Length) return rightBase[rightI]; return null; } }

        public Diff3Algorithm(DiffItem[] leftBase, DiffItem[] rightBase, int[] leftFile, int[] rightFile)
        {
            this.leftBase = leftBase;
            this.rightBase = rightBase;

            this.leftFile = leftFile;
            this.rightFile = rightFile;
        }

        public Diff3Item[] Parse()
        {
            var diff3Items = new List<Diff3Item>();

            while (curLeft != null || curRight != null)
            {
                if (curLeft == null)
                {
                    diff3Items.Add(createFromRight());
                    rightI++;
                } else if (curRight == null)
                {
                    diff3Items.Add(createFromLeft());
                    leftI++;
                } else if (curLeft.Value.OldLineStart <= curRight.Value.OldLineStart)
                {
                    if (curLeft.Value.OldLineStart + curLeft.Value.DeletedInOld + 1 <= curRight.Value.OldLineStart)
                    {
                        // diff3
                    } else
                    {
                        diff3Items.Add(createFromLeft());
                        leftI++;
                    }
                } else
                {
                    if (curRight.Value.OldLineStart + curRight.Value.DeletedInOld + 1 <= curLeft.Value.OldLineStart)
                    {
                        // diff3
                    } else
                    {
                        diff3Items.Add(createFromRight());
                        rightI++;
                    }
                }

            }

            return diff3Items.ToArray();
        }

        private Diff3Item createFromLeft()
        {
            return new Diff3Item(
                curLeft.Value.OldLineStart,
                -1,
                curLeft.Value.NewLineStart,
                curLeft.Value.DeletedInOld,
                0,
                curLeft.Value.InsertedInNew,
                DifferencesStatusEnum.BaseRightSame
            );
        }

        private Diff3Item createFromRight()
        {
            return new Diff3Item(
                curRight.Value.OldLineStart,
                -1,
                curRight.Value.NewLineStart,
                curRight.Value.DeletedInOld,
                0,
                curRight.Value.InsertedInNew,
                DifferencesStatusEnum.BaseLeftSame
            );
        }
    }
}
