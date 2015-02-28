using System;
using System.Collections.Generic;
using System.Linq;
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

        private List<Diff3Item> diff3Items;

        #region very simple iterators over the two-way diffs
        int leftI = 0;
        int rightI = 0;
        private DiffItem? CurrentNew { get { if (leftI < leftBase.Length) return leftBase[leftI]; return null; } }
        private DiffItem? CurrentHis { get { if (rightI < rightBase.Length) return rightBase[rightI]; return null; } }
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
            diff3Items = new List<Diff3Item>();

            while (CurrentNew != null || CurrentHis != null)
            {
                #region Solve partially overlapping diffs
                if (diff3Items.Any())
                {
                    #region Find lower (in terms of OldLineStart) diff
                    DiffItem lowerDiff;
                    bool wasHis = false;
                    if (CurrentNew == null)
                    {
                        lowerDiff = CurrentHis.Value;
                        wasHis = true;
                    }
                    else if (CurrentHis == null)
                        lowerDiff = CurrentNew.Value;
                    else if (CurrentNew.Value.OldLineStart < CurrentHis.Value.OldLineStart)
                        lowerDiff = CurrentNew.Value;
                    else
                    {
                        lowerDiff = CurrentHis.Value;
                        wasHis = true;
                    }

                    #endregion

                    Diff3Item old = diff3Items.Last();

                    // are they overlapping?
                    if (old.OldLineStart + old.OldAffectedLines >= lowerDiff.OldLineStart)
                    {
                        // get and remove last diff item
                        diff3Items.RemoveAt(diff3Items.Count - 1);

                        #region calculate which lines to extend
                        int addNew = 0;
                        int addHis = 0;
                        if (wasHis)
                        {
                            addHis = lowerDiff.InsertedInNew;
                            addNew = lowerDiff.DeletedInOld;
                            rightI++;
                        } else {
                            addHis = lowerDiff.DeletedInOld;
                            addNew = lowerDiff.InsertedInNew;
                            leftI++;
                        }
                        #endregion

                        diff3Items.Add(new Diff3Item(
                                old.OldLineStart,
                                old.NewLineStart,
                                old.HisLineStart,
                                old.OldAffectedLines + lowerDiff.DeletedInOld,
                                old.NewAffectedLines + addNew,
                                old.HisAffectedLines + addHis,
                                DifferencesStatusEnum.AllDifferent
                            ));
                        
                        continue;
                    }
                }
                #endregion

                ParseCycle();
            }

            return diff3Items.ToArray();
        }

        private void ParseCycle()
        {
            if (CurrentNew == null)
            // there are only a changes on his side remaining
            {
                diff3Items.Add(CreateFromHis());
                rightI++;
            } else if (CurrentHis == null)
            // there are only a changes on my side remaining
            {
                diff3Items.Add(CreateFromNew());
                leftI++;

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

                    leftI++; rightI++;

                } else
                // adding different number of lines => conflicting
                {
                    diff3Items.Add(CreateAllDifferent());
                    leftI++; rightI++;
                }
            } else if (AreOverlapping(CurrentNew, CurrentHis) || AreOverlapping(CurrentHis, CurrentNew))
            // check if they are overlapping
            {
                diff3Items.Add(CreateAllDifferent());
                leftI++; rightI++;
            } else if (CurrentNew.Value.OldLineStart < CurrentHis.Value.OldLineStart)
            // take CurrentNew as it starts lower 
            {
                diff3Items.Add(CreateFromNew());
                leftI++;
            } else if (CurrentNew.Value.OldLineStart > CurrentHis.Value.OldLineStart)
            // take CurrentHis as it starts lower
            {
                diff3Items.Add(CreateFromHis());
                rightI++;
            } else
            {
                throw new ApplicationException("This should never happen.");
            }
        }

        bool AreOverlapping(DiffItem? bottom, DiffItem? top)
        {
            return (bottom.Value.OldLineStart < top.Value.OldLineStart
                    && bottom.Value.OldLineStart + bottom.Value.DeletedInOld >= top.Value.OldLineStart);
        }

        /// <summary>
        /// Creates a Diff3Item between Old and New file.
        /// </summary>
        /// <returns>Diff3Item marking the change.</returns>
        private Diff3Item CreateFromNew()
        {
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
