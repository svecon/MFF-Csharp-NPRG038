
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
        private DiffItem? CurLeft { get { if (leftI < leftBase.Length) return leftBase[leftI]; return null; } }
        private DiffItem? CurRight { get { if (rightI < rightBase.Length) return rightBase[rightI]; return null; } }

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

            while (CurLeft != null || CurRight != null)
            {
                if (CurLeft == null)
                {
                    diff3Items.Add(CreateFromRight());
                    rightI++;
                } else if (CurRight == null)
                {
                    diff3Items.Add(CreateFromLeft());
                    leftI++;
                } else if (CurLeft.Value.OldLineStart <= CurRight.Value.OldLineStart)
                {
                    if (CurLeft.Value.OldLineStart + CurLeft.Value.DeletedInOld + 1 <= CurRight.Value.OldLineStart)
                    {
                        // diff3
                    } else
                    {
                        diff3Items.Add(CreateFromLeft());
                        leftI++;
                    }
                } else
                {
                    if (CurRight.Value.OldLineStart + CurRight.Value.DeletedInOld + 1 <= CurLeft.Value.OldLineStart)
                    {
                        // diff3
                    } else
                    {
                        diff3Items.Add(CreateFromRight());
                        rightI++;
                    }
                }

            }

            return diff3Items.ToArray();
        }

        private Diff3Item CreateFromLeft()
        {
            return new Diff3Item(
                CurLeft.Value.OldLineStart,
                -1,
                CurLeft.Value.NewLineStart,
                CurLeft.Value.DeletedInOld,
                0,
                CurLeft.Value.InsertedInNew,
                DifferencesStatusEnum.BaseRightSame
            );
        }

        private Diff3Item CreateFromRight()
        {
            return new Diff3Item(
                CurRight.Value.OldLineStart,
                -1,
                CurRight.Value.NewLineStart,
                CurRight.Value.DeletedInOld,
                0,
                CurRight.Value.InsertedInNew,
                DifferencesStatusEnum.BaseLeftSame
            );
        }
    }
}
