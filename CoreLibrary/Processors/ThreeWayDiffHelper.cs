
namespace CoreLibrary.Processors
{
    /// <summary>
    /// ThreeWayDiffHelper uses binary masks to help you gradually differentiate between three different files
    /// with more and more comparisons.
    /// </summary>
    ///
    /// <remarks>
    /// In every single moment you only need to compare those files that still might be the same.
    ///
    /// Those files that had some differences in the very beginning will not be compared any further.
    ///
    /// After all comparisons were done, you can check whether the files are still the same (based on the previous comparisons).
    /// </remarks>
    public class ThreeWayDiffHelper
    {
        int possibleCombinations = 0x0;
        int possibleFiles = 0x0;

        #region Constants

        const int BASE_LEFT = 0x1;
        const int BASE_RIGHT = 0x2;
        const int LEFT_RIGHT = 0x4;

        const int BASE = 0x1;
        const int LEFT = 0x2;
        const int RIGHT = 0x4;

        #endregion

        #region Getters

        public int GetSameFiles()
        {
            RecalculatePossibleFiles();

            return possibleFiles;
        }

        public int GetPossibleCombinations()
        {
            return possibleCombinations;
        }

        #endregion

        #region Adding files as possibilities

        protected void AddGivenFileAsPossibility(int file)
        {
            possibleFiles |= file;
        }

        public void AddBaseFilePossibility()
        {
            AddGivenFileAsPossibility(BASE);
        }

        public void AddLeftFilePossibility()
        {
            AddGivenFileAsPossibility(LEFT);
        }

        public void AddRightFilePossibility()
        {
            AddGivenFileAsPossibility(RIGHT);
        }

        #endregion

        #region Checking combinations

        protected void CheckGivenCombination(int combination, bool isDifferent)
        {
            if ((possibleCombinations & combination) > 0 && isDifferent)
                possibleCombinations &= ~combination;
        }

        public void CheckCombinationBaseLeft(bool isDifferent)
        {
            CheckGivenCombination(BASE_LEFT, isDifferent);
        }

        public void CheckCombinationBaseRight(bool isDifferent)
        {
            CheckGivenCombination(BASE_RIGHT, isDifferent);
        }

        public void CheckCombinationLeftRight(bool isDifferent)
        {
            CheckGivenCombination(LEFT_RIGHT, isDifferent);
        }

        #endregion

        #region Files possibility checks

        protected bool CanGivenFileBeSame(int file, bool forceRecheck = false)
        {
            if (forceRecheck)
                RecalculatePossibleFiles();

            return (possibleFiles & file) > 0;
        }

        public bool CanBaseFileBeSame(bool forceRecheck = false)
        {
            return CanGivenFileBeSame(BASE, forceRecheck);
        }

        public bool CanLeftFileBeSame(bool forceRecheck = false)
        {
            return CanGivenFileBeSame(LEFT, forceRecheck);
        }

        public bool CanRightFileBeSame(bool forceRecheck = false)
        {
            return CanGivenFileBeSame(RIGHT, forceRecheck);
        }

        #endregion

        #region Combinations possibility checks

        protected bool CanGivenCombinationBeSame(int combination)
        {
            return (possibleCombinations & combination) > 0;
        }

        public bool CanCombinationBaseLeftBeSame()
        {
            return CanGivenCombinationBeSame(BASE_LEFT);
        }

        public bool CanCombinationBaseRightBeSame()
        {
            return CanGivenCombinationBeSame(BASE_RIGHT);
        }

        public bool CanCombinationLeftRightBeSame()
        {
            return CanGivenCombinationBeSame(LEFT_RIGHT);
        }

        #endregion

        #region Recalculating possibilities

        public void RecalculatePossibleCombinations()
        {
            if ((possibleFiles & (BASE | LEFT)) == (BASE | LEFT))
                possibleCombinations |= BASE_LEFT;

            if ((possibleFiles & (BASE | RIGHT)) == (BASE | RIGHT))
                possibleCombinations |= BASE_RIGHT;

            if ((possibleFiles & (LEFT | RIGHT)) == (LEFT | RIGHT))
                possibleCombinations |= LEFT_RIGHT;
        }

        public void RecalculatePossibleFiles()
        {
            if ((possibleCombinations & (BASE_LEFT | BASE_RIGHT)) == 0)
                possibleFiles &= ~BASE;

            if ((possibleCombinations & (BASE_LEFT | LEFT_RIGHT)) == 0)
                possibleFiles &= ~LEFT;

            if ((possibleCombinations & (BASE_RIGHT | LEFT_RIGHT)) == 0)
                possibleFiles &= ~RIGHT;
        }

        #endregion
    }
}
