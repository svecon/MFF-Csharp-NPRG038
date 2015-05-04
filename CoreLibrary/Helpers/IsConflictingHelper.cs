using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.Helpers
{
    public static class IsConflictingHelper
    {
        public static bool IsConflicting(IFilesystemTreeAbstractNode node)
        {
            if (node.Differences != DifferencesStatusEnum.AllDifferent) return false;

            return node.Mode == DiffModeEnum.ThreeWay ||
                   (node.Mode == DiffModeEnum.TwoWay && node.IsInLocation(LocationCombinationsEnum.OnLocalRemote));
        }
    }
}
