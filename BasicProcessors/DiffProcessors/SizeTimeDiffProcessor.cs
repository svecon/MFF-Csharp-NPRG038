using System;
using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Helpers;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;

namespace BasicProcessors.DiffProcessors
{
    /// <summary>
    /// SizeTimeDiffProcessor checks whether two (or three) files are different based on size and modification time.
    /// </summary>
    [Processor(ProcessorTypeEnum.Diff, 1050, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class SizeTimeDiffProcessor : ProcessorAbstract
    {
        [Settings("Disable fast diff check.", "slow-diff", "D")]
        public bool IsEnabled = true;

        [Flags]
        public enum CompareModeEnum
        {
            Size = 1 << 0,
            Modification = 1 << 1,
            SizeModification = Size | Modification,
        }

        [Settings("Attributes that will be checked during diff.", "fast-diff", "F")]
        public CompareModeEnum CompareMode = CompareModeEnum.SizeModification;

        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
        }

        protected override bool CheckStatus(IFilesystemTreeFileNode node)
        {
            return base.CheckStatus(node) && IsEnabled;
        }

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
        {
            var threeWay = new ThreeWayDiffHelper();

            // check for existence
            if (node.IsInLocation(LocationEnum.OnBase))
                threeWay.AddBaseFilePossibility();

            if (node.IsInLocation(LocationEnum.OnLocal))
                threeWay.AddLocalFilePossibility();

            if (node.IsInLocation(LocationEnum.OnRemote))
                threeWay.AddRemoteFilePossibility();

            // create combinations
            threeWay.RecalculatePossibleCombinations();

            var infoBase = (FileInfo)node.InfoBase;
            var infoLeft = (FileInfo)node.InfoLocal;
            var infoRight = (FileInfo)node.InfoRemote;

            // check for sizes
            if (threeWay.CanCombinationBaseLeftBeSame())
                threeWay.CheckCombinationBaseLeft(infoBase.Length != infoLeft.Length);
            if (threeWay.CanCombinationBaseRightBeSame())
                threeWay.CheckCombinationBaseRight(infoBase.Length != infoRight.Length);
            if (threeWay.CanCombinationLeftRightBeSame())
                threeWay.CheckCombinationLeftRight(infoLeft.Length != infoRight.Length);

            // check for modifications
            if (threeWay.CanCombinationBaseLeftBeSame())
                threeWay.CheckCombinationBaseLeft(infoBase.LastWriteTime != infoLeft.LastWriteTime);
            if (threeWay.CanCombinationBaseRightBeSame())
                threeWay.CheckCombinationBaseRight(infoBase.LastWriteTime != infoRight.LastWriteTime);
            if (threeWay.CanCombinationLeftRightBeSame())
                threeWay.CheckCombinationLeftRight(infoLeft.LastWriteTime != infoRight.LastWriteTime);

            node.Differences = (DifferencesStatusEnum)threeWay.GetSameFiles();
            node.Status = IsConflictingHelper.IsConflicting(node)
                ? NodeStatusEnum.IsConflicting 
                : NodeStatusEnum.WasDiffed;
        }
    }
}
