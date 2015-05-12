using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;

namespace CoreLibrary.Helpers
{
    /// <summary>
    /// Helper for determining if the FilesystemTree is conflicting.
    /// </summary>
    public static class IsConflictingHelper
    {
        /// <summary>
        /// Checks the FilesystemTree based on mode, locations and diff status for coflicts.
        /// </summary>
        /// <param name="node">FilesystemTree that is checked for conflicts.</param>
        /// <returns>Whether the FilesystemTree has conflicting changes.</returns>
        public static bool IsConflicting(INodeAbstractNode node)
        {
            if (node.Differences != DifferencesStatusEnum.AllDifferent) return false;

            return (node.Mode == DiffModeEnum.ThreeWay && (
                node.IsInLocation(LocationCombinationsEnum.OnAll3)
                || node.IsInLocation(LocationCombinationsEnum.OnBaseLocal)
                || node.IsInLocation(LocationCombinationsEnum.OnBaseRemote)
                || node.IsInLocation(LocationCombinationsEnum.OnLocalRemote)
                )) ||
                   (node.Mode == DiffModeEnum.TwoWay && node.IsInLocation(LocationCombinationsEnum.OnLocalRemote));
        }
    }
}
