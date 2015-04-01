using System;
using System.Collections.Generic;
using System.Linq;
using CoreLibrary.Enums;
using DiffAlgorithm.TwoWay;

namespace DiffAlgorithm.ThreeWay
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
        private readonly DiffItem[] diffBaseLocal;

        /// <summary>
        /// 2-way diff between new and his files.
        /// </summary>
        private readonly DiffItem[] diffBaseRemote;

        /// <summary>
        /// Hashed new file.
        /// 
        /// Used in checking conflicts.
        /// </summary>
        private readonly int[] localFile;

        /// <summary>
        /// Hashed his file.
        /// 
        /// Used in checking conflicts.
        /// </summary>
        private readonly int[] remoteFile;

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
        private DiffItem CurrentNew
        {
            get
            {
                return newIterator < diffBaseLocal.Length ? diffBaseLocal[newIterator] : null;
            }
        }
        private DiffItem CurrentHis
        {
            get
            {
                return hisIterator < diffBaseRemote.Length ? diffBaseRemote[hisIterator] : null;
            }
        }
        #endregion

        /// <summary>
        /// Constructor for Diff3Algorithm.
        /// </summary>
        /// <param name="diffBaseLocal">Two-way diff between old and new file.</param>
        /// <param name="diffBaseRemote">Two-way diff between old and his file.</param>
        /// <param name="localFile">Hashed new file.</param>
        /// <param name="remoteFile">Hashed his file.</param>
        public Diff3Algorithm(DiffItem[] diffBaseLocal, DiffItem[] diffBaseRemote, int[] localFile, int[] remoteFile)
        {
            this.diffBaseLocal = diffBaseLocal;
            this.diffBaseRemote = diffBaseRemote;

            this.localFile = localFile;
            this.remoteFile = remoteFile;
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
                    (old = diff3Items.Last()).BaseLineStart
                                        + old.BaseAffectedLines
                    >= (lowerDiff = FindLowerDiff(out wasHis)).LocalLineStart)
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
                            old.BaseLineStart,
                            old.LocalLineStart,
                            old.RemoteLineStart,
                            old.BaseAffectedLines + lowerDiff.LocalAffectedLines,
                            old.LocalAffectedLines + (wasHis ? lowerDiff.LocalAffectedLines : lowerDiff.RemoteAffectedLines),
                            old.RemoteAffectedLines + (wasHis ? lowerDiff.RemoteAffectedLines : lowerDiff.LocalAffectedLines),
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

            } else if (CurrentNew.LocalLineStart == CurrentHis.LocalLineStart)
            // starts on the same line
            {
                if (CurrentNew.LocalAffectedLines == CurrentHis.LocalAffectedLines &&
                    CurrentNew.RemoteAffectedLines == CurrentHis.RemoteAffectedLines)
                // changes the same lines in old and adds same lines in new
                {
                    // check if the new lines are same => non-conflicting
                    bool areSame = true;
                    for (int i = 0; i < CurrentNew.RemoteAffectedLines; i++)
                    {
                        if (localFile[CurrentNew.RemoteLineStart + i] == remoteFile[CurrentHis.RemoteLineStart + i])
                            continue;

                        areSame = false;
                        break;
                    }

                    AddNewDiff3(areSame
                        ? CreateFullChunk(DifferencesStatusEnum.LocalRemoteSame)
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
            } else if (CurrentNew.LocalLineStart < CurrentHis.LocalLineStart)
            // take CurrentNew as it starts lower 
            {
                AddNewDiff3(CreateFromNew());
                newIterator++;
            } else if (CurrentNew.LocalLineStart > CurrentHis.LocalLineStart)
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
        /// Find lower (in terms of BaseLineStart) diff
        /// </summary>
        /// <param name="wasHis">Which file does it come from?</param>
        /// <returns>DiffItem with lower BaseLineStart</returns>
        private DiffItem FindLowerDiff(out bool wasHis)
        {
            wasHis = false;

            if (CurrentHis == null)
            {
                return CurrentNew;
            }

            if (CurrentNew == null)
            {
                wasHis = true;
                return CurrentHis;
            }

            if (CurrentNew.LocalLineStart < CurrentHis.LocalLineStart)
                return CurrentNew;

            wasHis = true;
            return CurrentHis;
        }

        /// <summary>
        /// Interface method for adding new Diff3Item.
        /// 
        /// Recalculates deltas for line numbers with regards to old file.
        /// </summary>
        /// <param name="item">Diff3Item to be added.</param>
        private void AddNewDiff3(Diff3Item item)
        {
            deltaToHis += item.RemoteAffectedLines - item.BaseAffectedLines;
            deltaToNew += item.LocalAffectedLines - item.BaseAffectedLines;

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
            deltaToHis -= item.RemoteAffectedLines - item.BaseAffectedLines;
            deltaToNew -= item.LocalAffectedLines - item.BaseAffectedLines;

            diff3Items.RemoveAt(diff3Items.Count - 1);
        }

        /// <summary>
        /// Checks if two 2-way diffs are overlapping.
        /// </summary>
        /// <param name="bottom">Bottom 2-way diff.</param>
        /// <param name="top">Top 2-way diff.</param>
        /// <returns>Yes if they are ovelapping.</returns>
        bool AreOverlapping(DiffItem bottom, DiffItem top)
        {
            return (bottom.LocalLineStart < top.LocalLineStart
                    && bottom.LocalLineStart + bottom.LocalAffectedLines >= top.LocalLineStart);
        }

        /// <summary>
        /// Creates a Diff3Item between Local and Remote file.
        /// </summary>
        /// <returns>Diff3Item marking the change.</returns>
        private Diff3Item CreateFromNew()
        {
            return new Diff3Item(
                CurrentNew.LocalLineStart,
                CurrentNew.RemoteLineStart,
                CurrentNew.LocalLineStart + deltaToHis,
                CurrentNew.LocalAffectedLines,
                CurrentNew.RemoteAffectedLines,
                CurrentNew.LocalAffectedLines,
                DifferencesStatusEnum.BaseRemoteSame
            );
        }

        /// <summary>
        /// Creates a Diff3Item between Local and Remote file.
        /// </summary>
        /// <returns>Diff3Item marking the change.</returns>
        private Diff3Item CreateFromHis()
        {
            return new Diff3Item(
                CurrentHis.LocalLineStart,
                CurrentHis.LocalLineStart + deltaToNew,
                CurrentHis.RemoteLineStart,
                CurrentHis.LocalAffectedLines,
                CurrentHis.LocalAffectedLines,
                CurrentHis.RemoteAffectedLines,
                DifferencesStatusEnum.BaseLocalSame
            );
        }

        /// <summary>
        /// Creates a Diff3Item between all three files.
        /// </summary>
        /// <returns>Diff3Item marking the change.</returns>
        private Diff3Item CreateFullChunk(DifferencesStatusEnum diff)
        {
            return new Diff3Item(
                CurrentHis.LocalLineStart,
                CurrentNew.RemoteLineStart,
                CurrentHis.RemoteLineStart,
                CurrentHis.LocalAffectedLines,
                CurrentNew.RemoteAffectedLines,
                CurrentHis.RemoteAffectedLines,
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
            int minOldStart = Math.Min(CurrentNew.LocalLineStart, CurrentHis.LocalLineStart);
            int maxOldStart = Math.Max(CurrentNew.LocalLineStart + CurrentNew.LocalAffectedLines,
                        CurrentHis.LocalLineStart + CurrentHis.LocalAffectedLines);
            int oldSpan = maxOldStart - minOldStart;

            return new Diff3Item(
                    minOldStart,
                    CurrentNew.RemoteLineStart + (minOldStart - CurrentNew.LocalLineStart),
                    CurrentHis.RemoteLineStart + (minOldStart - CurrentHis.LocalLineStart),
                    oldSpan,
                    CurrentNew.RemoteAffectedLines + (oldSpan - CurrentNew.LocalAffectedLines),
                    CurrentHis.RemoteAffectedLines + (oldSpan - CurrentHis.LocalAffectedLines),
                    DifferencesStatusEnum.AllDifferent
                );
        }
    }
}
