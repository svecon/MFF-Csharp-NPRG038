
namespace CoreLibrary.Helpers
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

        const int BASE_LOCAL = 0x1;
        const int BASE_REMOTE = 0x2;
        const int LOCAL_REMOTE = 0x4;

        const int BASE = 0x1;
        const int LOCAL = 0x2;
        const int REMOTE = 0x4;

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

        public void AddLocalFilePossibility()
        {
            AddGivenFileAsPossibility(LOCAL);
        }

        public void AddRemoteFilePossibility()
        {
            AddGivenFileAsPossibility(REMOTE);
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
            CheckGivenCombination(BASE_LOCAL, isDifferent);
        }

        public void CheckCombinationBaseRight(bool isDifferent)
        {
            CheckGivenCombination(BASE_REMOTE, isDifferent);
        }

        public void CheckCombinationLeftRight(bool isDifferent)
        {
            CheckGivenCombination(LOCAL_REMOTE, isDifferent);
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
            return CanGivenFileBeSame(LOCAL, forceRecheck);
        }

        public bool CanRightFileBeSame(bool forceRecheck = false)
        {
            return CanGivenFileBeSame(REMOTE, forceRecheck);
        }

        #endregion

        #region Combinations possibility checks

        protected bool CanGivenCombinationBeSame(int combination)
        {
            return (possibleCombinations & combination) > 0;
        }

        public bool CanCombinationBaseLeftBeSame()
        {
            return CanGivenCombinationBeSame(BASE_LOCAL);
        }

        public bool CanCombinationBaseRightBeSame()
        {
            return CanGivenCombinationBeSame(BASE_REMOTE);
        }

        public bool CanCombinationLeftRightBeSame()
        {
            return CanGivenCombinationBeSame(LOCAL_REMOTE);
        }

        #endregion

        #region Recalculating possibilities

        public void RecalculatePossibleCombinations()
        {
            if ((possibleFiles & (BASE | LOCAL)) == (BASE | LOCAL))
                possibleCombinations |= BASE_LOCAL;

            if ((possibleFiles & (BASE | REMOTE)) == (BASE | REMOTE))
                possibleCombinations |= BASE_REMOTE;

            if ((possibleFiles & (LOCAL | REMOTE)) == (LOCAL | REMOTE))
                possibleCombinations |= LOCAL_REMOTE;
        }

        public void RecalculatePossibleFiles()
        {
            if ((possibleCombinations & (BASE_LOCAL | BASE_REMOTE)) == 0)
                possibleFiles &= ~BASE;

            if ((possibleCombinations & (BASE_LOCAL | LOCAL_REMOTE)) == 0)
                possibleFiles &= ~LOCAL;

            if ((possibleCombinations & (BASE_REMOTE | LOCAL_REMOTE)) == 0)
                possibleFiles &= ~REMOTE;
        }

        #endregion
    }
}
