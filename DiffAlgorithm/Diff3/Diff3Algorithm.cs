using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreLibrary.Enums;
using DiffAlgorithm.Diff;

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
        /// <summary>
        /// 2-way diff between new and old files.
        /// </summary>
        private readonly DiffItem[] diffOldNew;

        /// <summary>
        /// 2-way diff between new and his files.
        /// </summary>
        private readonly DiffItem[] diffOldHis;

        /// <summary>
        /// Hashed new file.
        /// 
        /// Used in checking conflicts.
        /// </summary>
        private readonly int[] leftFile;

        /// <summary>
        /// Hashed his file.
        /// 
        /// Used in checking conflicts.
        /// </summary>
        private readonly int[] rightFile;

        /// <summary>
        /// Temporary container for Diff3Items
        /// </summary>
        private List<Diff3Item> diff3Items;

        /// <summary>
        /// How many lines ahead/back is old file compared to new file.
        /// 
        /// Always in regard to start of the diff chunk.
        /// </summary>
        private int deltaToNew;

        /// <summary>
        /// How many lines ahead/back is old file compared to his file.
        /// 
        /// Always in regard to start of the diff chunk.
        /// </summary>
        private int deltaToHis;

        #region very simple iterators over the two-way diffs
        int newIterator = 0;
        int hisIterator = 0;
        private DiffItem? CurrentNew { get { if (newIterator < diffOldNew.Length) return diffOldNew[newIterator]; return null; } }
        private DiffItem? CurrentHis { get { if (hisIterator < diffOldHis.Length) return diffOldHis[hisIterator]; return null; } }
        #endregion

        /// <summary>
        /// Constructor for Diff3Algorithm.
        /// </summary>
        /// <param name="diffOldNew">Two-way diff between old and new file.</param>
        /// <param name="diffOldHis">Two-way diff between old and his file.</param>
        /// <param name="leftFile">Hashed new file.</param>
        /// <param name="rightFile">Hashed his file.</param>
        public Diff3Algorithm(DiffItem[] diffOldNew, DiffItem[] diffOldHis, int[] leftFile, int[] rightFile)
        {
            this.diffOldNew = diffOldNew;
            this.diffOldHis = diffOldHis;

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
                    RemoveLastDiff3();

                    if (wasHis)
                        hisIterator++;
                    else
                        newIterator++;

                    // create extended chunk
                    AddNewDiff3(new Diff3Item(
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

        /// <summary>
        /// Iterates over 2-way diffs and merge them into Diff3Items
        /// depending on their overlapping
        /// </summary>
        private void JoinDiffsIntoOne()
        {
            if (CurrentNew == null)
            // there are only a changes on his side remaining
            {
                AddNewDiff3(CreateFromHis());
                hisIterator++;
            } else if (CurrentHis == null)
            // there are only a changes on my side remaining
            {
                AddNewDiff3(CreateFromNew());
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

                    AddNewDiff3(areSame
                        ? CreateFullChunk(DifferencesStatusEnum.LeftRightSame)
                        : CreateFullChunk(DifferencesStatusEnum.AllDifferent));

                    newIterator++; hisIterator++;

                } else
                // adding different number of lines => conflicting
                {
                    AddNewDiff3(CreateAllDifferent());
                    newIterator++; hisIterator++;
                }
            } else if (AreOverlapping(CurrentNew, CurrentHis) || AreOverlapping(CurrentHis, CurrentNew))
            // check if they are overlapping
            {
                AddNewDiff3(CreateAllDifferent());
                newIterator++; hisIterator++;
            } else if (CurrentNew.Value.OldLineStart < CurrentHis.Value.OldLineStart)
            // take CurrentNew as it starts lower 
            {
                AddNewDiff3(CreateFromNew());
                newIterator++;
            } else if (CurrentNew.Value.OldLineStart > CurrentHis.Value.OldLineStart)
            // take CurrentHis as it starts lower
            {
                AddNewDiff3(CreateFromHis());
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
        /// Interface method for adding new Diff3Item.
        /// 
        /// Recalculates deltas for line numbers with regards to old file.
        /// </summary>
        /// <param name="item">Diff3Item to be added.</param>
        private void AddNewDiff3(Diff3Item item)
        {
            deltaToHis += item.HisAffectedLines - item.OldAffectedLines;
            deltaToNew += item.NewAffectedLines - item.OldAffectedLines;

            diff3Items.Add(item);
        }

        /// <summary>
        /// Interface method for removing Diff3Item.
        /// 
        /// Realculates deltas for line numbers with regards to old file.
        /// </summary>
        private void RemoveLastDiff3()
        {
            Diff3Item item = diff3Items.Last();
            deltaToHis -= item.HisAffectedLines - item.OldAffectedLines;
            deltaToNew -= item.NewAffectedLines - item.OldAffectedLines;

            diff3Items.RemoveAt(diff3Items.Count - 1);
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
                CurrentNew.Value.OldLineStart + deltaToHis,
                CurrentNew.Value.DeletedInOld,
                CurrentNew.Value.InsertedInNew,
                CurrentNew.Value.DeletedInOld,
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
                CurrentHis.Value.OldLineStart + deltaToNew,
                CurrentHis.Value.NewLineStart,
                CurrentHis.Value.DeletedInOld,
                CurrentHis.Value.DeletedInOld,
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
