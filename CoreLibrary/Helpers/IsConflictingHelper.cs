using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.Helpers
{
    /// <summary>
    /// Helper for determining if the node is conflicting.
    /// </summary>
    public static class IsConflictingHelper
    {
        /// <summary>
        /// Checks the node based on mode, locations and diff status for coflicts.
        /// </summary>
        /// <param name="node">Node that is checked for conflicts.</param>
        /// <returns>Whether the node has conflicting changes.</returns>
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
