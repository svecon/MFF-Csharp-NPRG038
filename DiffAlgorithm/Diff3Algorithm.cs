using System.Collections.Generic;
using CoreLibrary.Enums;

namespace DiffAlgorithm
{
    /// <summary>
    /// This class implements Diff3 algorithm.
    /// </summary>
    class Diff3Algorithm
    {
        private readonly DiffItem[] leftBase;
        private readonly DiffItem[] rightBase;

        private int[] leftFile;
        private int[] rightFile;

        #region very simple iterators over the two-way diffs
        int leftI = 0;
        int rightI = 0;
        private DiffItem? CurLeft { get { if (leftI < leftBase.Length) return leftBase[leftI]; return null; } }
        private DiffItem? CurRight { get { if (rightI < rightBase.Length) return rightBase[rightI]; return null; } }
        #endregion

        /// <summary>
        /// Constructor for Diff3Algorithm.
        /// </summary>
        /// <param name="leftBase">Two-way diff between old and new file.</param>
        /// <param name="rightBase">Two-way diff between old and his file.</param>
        /// <param name="leftFile">Hashed new file.</param>
        /// <param name="rightFile">Hashed his file.</param>
        public Diff3Algorithm(DiffItem[] leftBase, DiffItem[] rightBase, int[] leftFile, int[] rightFile)
        {
            this.leftBase = leftBase;
            this.rightBase = rightBase;

            this.leftFile = leftFile;
            this.rightFile = rightFile;
        }

        /// <summary>
        /// Main algorithm function.
        /// 
        /// Iterates over all two-way diffs and tries to decide if they are conflicting or not.
        /// If they are conflicting then they are merged into a one bigger chunk.
        /// </summary>
        /// <returns>Array of Diff3Items.</returns>
        public Diff3Item[] Parse()
        {
            var diff3Items = new List<Diff3Item>();

            while (CurLeft != null || CurRight != null)
            {
                if (CurLeft == null)
                {
                    diff3Items.Add(CreateFromHis());
                    rightI++;
                } else if (CurRight == null)
                {
                    diff3Items.Add(CreateFromNew());
                    leftI++;
                } else if (CurLeft.Value.OldLineStart <= CurRight.Value.OldLineStart)
                {
                    if (CurLeft.Value.OldLineStart + CurLeft.Value.DeletedInOld + 1 <= CurRight.Value.OldLineStart)
                    {
                        // diff3
                    } else
                    {
                        diff3Items.Add(CreateFromNew());
                        leftI++;
                    }
                } else
                {
                    if (CurRight.Value.OldLineStart + CurRight.Value.DeletedInOld + 1 <= CurLeft.Value.OldLineStart)
                    {
                        // diff3
                    } else
                    {
                        diff3Items.Add(CreateFromHis());
                        rightI++;
                    }
                }

            }

            return diff3Items.ToArray();
        }

        /// <summary>
        /// Creates a Diff3Item between Old and New file.
        /// </summary>
        /// <returns>Diff3Item diff.</returns>
        private Diff3Item CreateFromNew()
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

        /// <summary>
        /// Creates a Diff3Item between Old and His file.
        /// </summary>
        /// <returns>Diff3Item diff.</returns>
        private Diff3Item CreateFromHis()
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
