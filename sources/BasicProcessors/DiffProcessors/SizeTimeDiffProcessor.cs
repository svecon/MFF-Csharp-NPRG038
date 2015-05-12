using System;
using System.IO;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Helpers;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;

namespace BasicProcessors.DiffProcessors
{
    /// <summary>
    /// SizeTimeDiffProcessor checks whether two (or three) files are different based on size and modification time.
    /// </summary>
    [Processor(ProcessorTypeEnum.Diff, 9500, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class SizeTimeDiffProcessor : ProcessorAbstract
    {
        /// <summary>
        /// Enables <see cref="SizeTimeDiffProcessor"/>
        /// </summary>
        [Settings("Disable fast diff check.", "slow-diff", "D")]
        public bool IsEnabled = true;

        /// <summary>
        /// Enum for different types of comparisons.
        /// </summary>
        [Flags]
        public enum CompareModeEnum
        {
            /// <summary>
            /// Compare the files based on their file size.
            /// </summary>
            Size = 1 << 0,

            /// <summary>
            /// Compare the files based on their last modification time.
            /// </summary>
            Modification = 1 << 1,

            /// <summary>
            /// Compare on both size and last modification time.
            /// </summary>
            SizeModification = Size | Modification,
        }

        /// <summary>
        /// Compare mode setting.
        /// </summary>
        [Settings("Attributes that will be checked during diff.", "fast-diff", "F")]
        public CompareModeEnum CompareMode = CompareModeEnum.SizeModification;

        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        protected override bool CheckStatus(INodeFileNode node)
        {
            return base.CheckStatus(node) && IsEnabled && node.Status != NodeStatusEnum.WasDiffed;
        }

        protected override void ProcessChecked(INodeFileNode node)
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
            if (threeWay.CanBaseLocalBeSame())
                threeWay.CheckCombinationBaseLocal(infoBase.Length != infoLeft.Length);
            if (threeWay.CanBaseRemoteBeSame())
                threeWay.CheckCombinationBaseRemote(infoBase.Length != infoRight.Length);
            if (threeWay.CanLocalRemoteBeSame())
                threeWay.CheckCombinationLocalRemote(infoLeft.Length != infoRight.Length);

            // check for modifications
            if (threeWay.CanBaseLocalBeSame())
                threeWay.CheckCombinationBaseLocal(infoBase.LastWriteTime != infoLeft.LastWriteTime);
            if (threeWay.CanBaseRemoteBeSame())
                threeWay.CheckCombinationBaseRemote(infoBase.LastWriteTime != infoRight.LastWriteTime);
            if (threeWay.CanLocalRemoteBeSame())
                threeWay.CheckCombinationLocalRemote(infoLeft.LastWriteTime != infoRight.LastWriteTime);

            node.Differences = (DifferencesStatusEnum)threeWay.GetSameFiles();
            node.Status = IsConflictingHelper.IsConflicting(node)
                ? NodeStatusEnum.IsConflicting 
                : NodeStatusEnum.WasDiffed;
        }
    }
}
