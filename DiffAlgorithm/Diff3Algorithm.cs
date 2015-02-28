using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreLibrary.Enums;

namespace DiffAlgorithm
{
    /// <summary>
    /// This class implements Diff3 algorithm.
    /// 
    /// Takes two 2-way diffs and merges them into 3-way chunks.
    /// Checks overlapping and conflicts.
    /// </summary>
    class Diff3Algorithm
    {
        private readonly DiffItem[] leftBase;
        private readonly DiffItem[] rightBase;

        private readonly int[] leftFile;
        private readonly int[] rightFile;

        private List<Diff3Item> diff3Items;

        #region very simple iterators over the two-way diffs
        int newIterator = 0;
        int hisIterator = 0;
        private DiffItem? CurrentNew { get { if (newIterator < leftBase.Length) return leftBase[newIterator]; return null; } }
        private DiffItem? CurrentHis { get { if (hisIterator < rightBase.Length) return rightBase[hisIterator]; return null; } }
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
        public Diff3Item[] MergeIntoDiff3Chunks()
        {
            diff3Items = new List<Diff3Item>();

            while (CurrentNew != null || CurrentHis != null)
            {
                #region Solve partially overlapping diffs by extending them

                bool wasHis; 
                DiffItem lowerDiff;
                Diff3Item old;

                if (diff3Items.Any() &&
                    (old = diff3Items.Last()).OldLineStart
                                        + old.OldAffectedLines
                    >= (lowerDiff = FindLowerDiff(out wasHis)).OldLineStart)
                // are they overlapping?
                {
                    // remove last diff item -- alias "old"
                    diff3Items.RemoveAt(diff3Items.Count - 1);

                    if (wasHis)
                        hisIterator++;
                    else
                        newIterator++;

                    // create extended chunk
                    diff3Items.Add(new Diff3Item(
                            old.OldLineStart,
                            old.NewLineStart,
                            old.HisLineStart,
                            old.OldAffectedLines + lowerDiff.DeletedInOld,
                            old.NewAffectedLines + (wasHis ? lowerDiff.DeletedInOld : lowerDiff.InsertedInNew),
                            old.HisAffectedLines + (wasHis ? lowerDiff.InsertedInNew : lowerDiff.DeletedInOld),
                            DifferencesStatusEnum.AllDifferent
                        ));

                    continue;
                }

                #endregion

                JoinDiffsIntoOne();
            }

            return diff3Items.ToArray();
        }

        private void JoinDiffsIntoOne()
        {
            if (CurrentNew == null)
            // there are only a changes on his side remaining
            {
                diff3Items.Add(CreateFromHis());
                hisIterator++;
            } else if (CurrentHis == null)
            // there are only a changes on my side remaining
            {
                diff3Items.Add(CreateFromNew());
                newIterator++;

            } else if (CurrentNew.Value.OldLineStart == CurrentHis.Value.OldLineStart)
            // starts on the same line
            {
                if (CurrentNew.Value.DeletedInOld == CurrentHis.Value.DeletedInOld &&
                    CurrentNew.Value.InsertedInNew == CurrentHis.Value.InsertedInNew)
                // changes the same lines in old and adds same lines in new
                {
                    // check if the new lines are same => non-conflicting
                    bool areSame = true;
                    for (int i = 0; i < CurrentNew.Value.InsertedInNew; i++)
                    {
                        if (leftFile[CurrentNew.Value.NewLineStart + i] == rightFile[CurrentHis.Value.NewLineStart + i])
                            continue;

                        areSame = false;
                        break;
                    }

                    diff3Items.Add(areSame
                        ? CreateFullChunk(DifferencesStatusEnum.LeftRightSame)
                        : CreateFullChunk(DifferencesStatusEnum.AllDifferent));

                    newIterator++; hisIterator++;

                } else
                // adding different number of lines => conflicting
                {
                    diff3Items.Add(CreateAllDifferent());
                    newIterator++; hisIterator++;
                }
            } else if (AreOverlapping(CurrentNew, CurrentHis) || AreOverlapping(CurrentHis, CurrentNew))
            // check if they are overlapping
            {
                diff3Items.Add(CreateAllDifferent());
                newIterator++; hisIterator++;
            } else if (CurrentNew.Value.OldLineStart < CurrentHis.Value.OldLineStart)
            // take CurrentNew as it starts lower 
            {
                diff3Items.Add(CreateFromNew());
                newIterator++;
            } else if (CurrentNew.Value.OldLineStart > CurrentHis.Value.OldLineStart)
            // take CurrentHis as it starts lower
            {
                diff3Items.Add(CreateFromHis());
                hisIterator++;
            } else
            {
                throw new ApplicationException("This should never happen.");
            }
        }


        /// <summary>
        /// Find lower (in terms of OldLineStart) diff
        /// </summary>
        /// <param name="wasHis">Which file does it come from?</param>
        /// <returns>DiffItem with lower OldLineStart</returns>
        private DiffItem FindLowerDiff(out bool wasHis)
        {
            wasHis = false;

            if (CurrentHis == null)
            {
                Debug.Assert(CurrentNew != null, "CurrentNew != null");
                return CurrentNew.Value;
            }

            if (CurrentNew == null)
            {
                wasHis = true;
                return CurrentHis.Value;
            }

            if (CurrentNew.Value.OldLineStart < CurrentHis.Value.OldLineStart)
                return CurrentNew.Value;

            wasHis = true;
            return CurrentHis.Value;
        }

        /// <summary>
        /// Checks if two 2-way diffs are overlapping.
        /// </summary>
        /// <param name="bottom">Bottom 2-way diff.</param>
        /// <param name="top">Top 2-way diff.</param>
        /// <returns>Yes if they are ovelapping.</returns>
        bool AreOverlapping(DiffItem? bottom, DiffItem? top)
        {
            Debug.Assert(bottom != null, "bottom != null");
            Debug.Assert(top != null, "top != null");
            return (bottom.Value.OldLineStart < top.Value.OldLineStart
                    && bottom.Value.OldLineStart + bottom.Value.DeletedInOld >= top.Value.OldLineStart);
        }

        /// <summary>
        /// Creates a Diff3Item between Old and New file.
        /// </summary>
        /// <returns>Diff3Item marking the change.</returns>
        private Diff3Item CreateFromNew()
        {
            Debug.Assert(CurrentNew != null, "CurrentNew != null");
            return new Diff3Item(
                CurrentNew.Value.OldLineStart,
                CurrentNew.Value.NewLineStart,
                -1, // todo: not needed, but can be calculated for a check
                CurrentNew.Value.DeletedInOld,
                CurrentNew.Value.InsertedInNew,
                0,
                DifferencesStatusEnum.BaseRightSame
            );
        }

        /// <summary>
        /// Creates a Diff3Item between Old and His file.
        /// </summary>
        /// <returns>Diff3Item marking the change.</returns>
        private Diff3Item CreateFromHis()
        {
            Debug.Assert(CurrentHis != null, "CurrentHis != null");
            return new Diff3Item(
                CurrentHis.Value.OldLineStart,
                -1, // todo: not needed, but can be calculated for a check
                CurrentHis.Value.NewLineStart,
                CurrentHis.Value.DeletedInOld,
                0,
                CurrentHis.Value.InsertedInNew,
                DifferencesStatusEnum.BaseLeftSame
            );
        }

        /// <summary>
        /// Creates a Diff3Item between all three files.
        /// </summary>
        /// <returns>Diff3Item marking the change.</returns>
        private Diff3Item CreateFullChunk(DifferencesStatusEnum diff)
        {
            Debug.Assert(CurrentHis != null, "CurrentHis != null");
            Debug.Assert(CurrentNew != null, "CurrentNew != null");
            return new Diff3Item(
                CurrentHis.Value.OldLineStart,
                CurrentNew.Value.NewLineStart,
                CurrentHis.Value.NewLineStart,
                CurrentHis.Value.DeletedInOld,
                CurrentNew.Value.InsertedInNew,
                CurrentHis.Value.InsertedInNew,
                diff
            );
        }

        /// <summary>
        /// All three files are different.
        /// 
        /// This block is created even for only partly overflowing chunks.
        /// That means that we need to shift some lines to cover all the lines for every one of them.
        /// </summary>
        /// <returns>Diff3Item marking the change.</returns>
        private Diff3Item CreateAllDifferent()
        {
            Debug.Assert(CurrentNew != null, "CurrentNew != null");
            Debug.Assert(CurrentHis != null, "CurrentHis != null");
            int minOldStart = Math.Min(CurrentNew.Value.OldLineStart, CurrentHis.Value.OldLineStart);
            int maxOldStart = Math.Max(CurrentNew.Value.OldLineStart + CurrentNew.Value.DeletedInOld,
                        CurrentHis.Value.OldLineStart + CurrentHis.Value.DeletedInOld);
            int oldSpan = maxOldStart - minOldStart;

            return new Diff3Item(
                    minOldStart,
                    CurrentNew.Value.NewLineStart + (minOldStart - CurrentNew.Value.OldLineStart),
                    CurrentHis.Value.NewLineStart + (minOldStart - CurrentHis.Value.OldLineStart),
                    oldSpan,
                    CurrentNew.Value.InsertedInNew + (oldSpan - CurrentNew.Value.DeletedInOld),
                    CurrentHis.Value.InsertedInNew + (oldSpan - CurrentHis.Value.DeletedInOld),
                    DifferencesStatusEnum.AllDifferent
                );
        }
    }
}
