
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
        /// <summary>
        /// Binary mask for current possible combinations of the files.
        /// </summary>
        private int possibleCombinations = 0x0;

        /// <summary>
        /// Binary mask for current possibles fiesl.
        /// </summary>
        private int possibleFiles = 0x0;

        #region Constants

        /// <summary>
        /// Binary combination of base and local file.
        /// </summary>
        const int BASE_LOCAL = 0x1;

        /// <summary>
        /// Binary combination of base and remote file.
        /// </summary>
        const int BASE_REMOTE = 0x2;

        /// <summary>
        /// Binary combination of local and remote file.
        /// </summary>
        const int LOCAL_REMOTE = 0x4;

        /// <summary>
        /// Binary value for base file.
        /// </summary>
        const int BASE = 0x1;

        /// <summary>
        /// Binary value for local file.
        /// </summary>
        const int LOCAL = 0x2;

        /// <summary>
        /// Binary value for remote file.
        /// </summary>
        const int REMOTE = 0x4;

        #endregion

        #region Getters

        /// <summary>
        /// Recalculate possible files and return.
        /// </summary>
        /// <returns>Possible files</returns>
        public int GetSameFiles()
        {
            RecalculatePossibleFiles();

            return possibleFiles;
        }

        /// <summary>
        /// Getter for the possible combinations.
        /// </summary>
        /// <returns>Possible combinations</returns>
        public int GetPossibleCombinations()
        {
            return possibleCombinations;
        }

        #endregion

        #region Adding files as possibilities

        /// <summary>
        /// Add given file mask as a possibility.
        /// </summary>
        /// <param name="file">Possible file mask.</param>
        protected void AddGivenFileAsPossibility(int file)
        {
            possibleFiles |= file;
        }

        /// <summary>
        /// Add base file as a possibility.
        /// </summary>
        public void AddBaseFilePossibility()
        {
            AddGivenFileAsPossibility(BASE);
        }

        /// <summary>
        /// Add local file as a possibility.
        /// </summary>
        public void AddLocalFilePossibility()
        {
            AddGivenFileAsPossibility(LOCAL);
        }

        /// <summary>
        /// Add remote file as a possibility.
        /// </summary>
        public void AddRemoteFilePossibility()
        {
            AddGivenFileAsPossibility(REMOTE);
        }

        #endregion

        #region Checking combinations

        /// <summary>
        /// Checks the given combination against the possible combinations.
        /// </summary>
        /// <param name="combination">Combination to be checked.</param>
        /// <param name="isDifferent">Whether the input is different.</param>
        protected void CheckGivenCombination(int combination, bool isDifferent)
        {
            if ((possibleCombinations & combination) > 0 && isDifferent)
                possibleCombinations &= ~combination;
        }

        /// <summary>
        /// Check combination of base and local.
        /// </summary>
        /// <param name="isDifferent">Whether base and local are different.</param>
        public void CheckCombinationBaseLocal(bool isDifferent)
        {
            CheckGivenCombination(BASE_LOCAL, isDifferent);
        }

        /// <summary>
        /// Check combination of base and remote.
        /// </summary>
        /// <param name="isDifferent">Whether base and remote are different.</param>
        public void CheckCombinationBaseRemote(bool isDifferent)
        {
            CheckGivenCombination(BASE_REMOTE, isDifferent);
        }

        /// <summary>
        /// Checks combination of local and remote.
        /// </summary>
        /// <param name="isDifferent">Whether local and remote are different.</param>
        public void CheckCombinationLocalRemote(bool isDifferent)
        {
            CheckGivenCombination(LOCAL_REMOTE, isDifferent);
        }

        #endregion

        #region Files possibility checks

        /// <summary>
        /// Can give file be the same?
        /// </summary>
        /// <param name="file">Mask of the file to be checked.</param>
        /// <param name="forceRecheck">Force recheck of the possible files.</param>
        /// <returns>True when the file can be same.</returns>
        protected bool CanGivenFileBeSame(int file, bool forceRecheck = false)
        {
            if (forceRecheck)
                RecalculatePossibleFiles();

            return (possibleFiles & file) > 0;
        }

        /// <summary>
        /// Can base file be same?
        /// </summary>
        /// <param name="forceRecheck">Force recheck of the possible files.</param>
        /// <returns>True when the file can be same.</returns>
        public bool CanBaseFileBeSame(bool forceRecheck = false)
        {
            return CanGivenFileBeSame(BASE, forceRecheck);
        }

        /// <summary>
        /// Can local file be same?
        /// </summary>
        /// <param name="forceRecheck">Force recheck of the possible files.</param>
        /// <returns>True when the file can be same.</returns>
        public bool CanLocalFileBeSame(bool forceRecheck = false)
        {
            return CanGivenFileBeSame(LOCAL, forceRecheck);
        }

        /// <summary>
        /// Can remote file be same?
        /// </summary>
        /// <param name="forceRecheck">Force recheck of the possible files.</param>
        /// <returns>True when the file can be same.</returns>
        public bool CanRemoteFileBeSame(bool forceRecheck = false)
        {
            return CanGivenFileBeSame(REMOTE, forceRecheck);
        }

        #endregion

        #region Combinations possibility checks

        /// <summary>
        /// Helper method for checking possible combinations.
        /// </summary>
        /// <param name="combination">Mask for the combination.</param>
        /// <returns>True if the combination is still possible.</returns>
        protected bool CanGivenCombinationBeSame(int combination)
        {
            return (possibleCombinations & combination) > 0;
        }

        /// <summary>
        /// Helper method for checking possible combinations.
        /// </summary>
        /// <returns>True if base and local can be same.</returns>
        public bool CanBaseLocalBeSame()
        {
            return CanGivenCombinationBeSame(BASE_LOCAL);
        }

        /// <summary>
        /// Helper method for checking possible combinations.
        /// </summary>
        /// <returns>True if base and remote can be same.</returns>
        public bool CanBaseRemoteBeSame()
        {
            return CanGivenCombinationBeSame(BASE_REMOTE);
        }

        /// <summary>
        /// Helper method for checking possible combinations.
        /// </summary>
        /// <returns>True if local and remote can be same.</returns>
        public bool CanLocalRemoteBeSame()
        {
            return CanGivenCombinationBeSame(LOCAL_REMOTE);
        }

        #endregion

        #region Recalculating possibilities

        /// <summary>
        /// Recalculates the possible combinations based on the possible files.
        /// </summary>
        public void RecalculatePossibleCombinations()
        {
            if ((possibleFiles & (BASE | LOCAL)) == (BASE | LOCAL))
                possibleCombinations |= BASE_LOCAL;

            if ((possibleFiles & (BASE | REMOTE)) == (BASE | REMOTE))
                possibleCombinations |= BASE_REMOTE;

            if ((possibleFiles & (LOCAL | REMOTE)) == (LOCAL | REMOTE))
                possibleCombinations |= LOCAL_REMOTE;
        }

        /// <summary>
        /// Recalculates the possible files based on the possible combinations.
        /// </summary>
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
