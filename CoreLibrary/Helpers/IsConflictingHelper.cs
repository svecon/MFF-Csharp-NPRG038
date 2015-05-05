using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.Helpers
{
    public static class IsConflictingHelper
    {
        public static bool IsConflicting(IFilesystemTreeAbstractNode node)
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
