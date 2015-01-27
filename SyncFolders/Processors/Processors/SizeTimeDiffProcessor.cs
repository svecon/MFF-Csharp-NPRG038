using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Settings.Attributes;
using CoreLibrary.Processors.Processors;
using CoreLibrary.Processors;

namespace SyncFolders.Processors.Processors
{
    /// <summary>
    /// SizeTimeDiffProcessor checks whether two (or three) files are different based on size and modification time.
    /// </summary>
    class SizeTimeDiffProcessor : ProcessorAbstract
    {
        public override int Priority { get { return 1000; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay; } }

        [SettingsAttribute("Disable fast diff check.", "slow-diff", "D")]
        public bool IsEnabled = true;

        [Flags]
        public enum CompareModeEnum
        {
            Size                = 1 << 0,
            Modification        = 1 << 1,
            SizeModification    = Size | Modification,
        }

        [Settings("Attributes that will be checked during diff.", "fast-diff", "F")]
        public CompareModeEnum CompareMode = CompareModeEnum.SizeModification;

        public override void Process(IFilesystemTreeDirNode node)
        {

        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            if (!checkModeAndStatus(node))
                return;

            if (!IsEnabled)
                return;

            var threeWay = new ThreeWayDiffHelper();

            // check for existence
            if (node.IsInLocation(LocationEnum.OnBase))
                threeWay.AddBaseFilePossibility();

            if (node.IsInLocation(LocationEnum.OnLeft))
                threeWay.AddLeftFilePossibility();

            if (node.IsInLocation(LocationEnum.OnRight))
                threeWay.AddRightFilePossibility();

            // create combinations
            threeWay.RecalculatePossibleCombinations();

            var infoBase = (FileInfo)node.InfoBase;
            var infoLeft = (FileInfo)node.InfoLeft;
            var infoRight = (FileInfo)node.InfoRight;

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
            node.Status = NodeStatusEnum.WasDiffed;
        }
    }
}
