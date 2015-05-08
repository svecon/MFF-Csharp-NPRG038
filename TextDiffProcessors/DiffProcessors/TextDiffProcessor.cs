using System.IO;
using System.Linq;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;
using TextDiffAlgorithm;
using TextDiffAlgorithm.ThreeWay;
using TextDiffAlgorithm.TwoWay;

namespace TextDiffProcessors.DiffProcessors
{
    /// <summary>
    /// TextDiffProcessor calculates diff between files that we know are different
    /// </summary>
    [Processor(ProcessorTypeEnum.Diff, 1500, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class TextDiffProcessor : ProcessorAbstract
    {
        [Settings("Diff algorithm will ignore leading and trailing whitespace", "trim-space", "ts")]
        public bool TrimSpace = false;

        [Settings("Diff algorithm will ignore all white space", "ignore-whitespace", "iw")]
        public bool IgnoreWhitespace = false;

        [Settings("Diff algorithm will ignore case senstivity", "ignore-case", "ic")]
        public bool IgnoreCase = false;

        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        protected override bool CheckStatus(INodeFileNode node)
        {
            if (node.FileType != FileTypeEnum.Text)
                return false;

            if (node.Differences == DifferencesStatusEnum.AllSame)
                return false;

            return base.CheckStatus(node);
        }

        protected override void ProcessChecked(INodeFileNode node)
        {
            var dnode = node as FileDiffNode;

            if (dnode == null)
                return;

            var diffRunHelper = new DiffRunHelper(TrimSpace, IgnoreWhitespace, IgnoreCase);

            switch ((LocationCombinationsEnum)node.Location)
            {
                case LocationCombinationsEnum.OnBase:
                case LocationCombinationsEnum.OnLocal:
                case LocationCombinationsEnum.OnRemote:
                    return; // do nothing
                case LocationCombinationsEnum.OnLocalRemote:
                    if (node.Mode == DiffModeEnum.TwoWay)
                    {
                        dnode.Diff = diffRunHelper.DiffFiles((FileInfo)dnode.InfoLocal, (FileInfo)dnode.InfoRemote);
                    }
                    break;
                default:
                    switch (node.Mode)
                    {
                        case DiffModeEnum.TwoWay:
                            dnode.Diff = diffRunHelper.DiffFiles((FileInfo)dnode.InfoLocal, (FileInfo)dnode.InfoRemote);
                            break;
                        case DiffModeEnum.ThreeWay:
                            dnode.Diff = diffRunHelper.DiffFiles((FileInfo)dnode.InfoBase, (FileInfo)dnode.InfoLocal, (FileInfo)dnode.InfoRemote);
                            break; // do nothing
                    }
                    break;
            }

            var diff = dnode.Diff as Diff;
            if (diff != null && diff.Items.Length == 0)
                dnode.Differences = (DifferencesStatusEnum)dnode.Location;

            var diff3 = dnode.Diff as Diff3;
            if (diff3 != null && diff3.Items.Length == 0)
                dnode.Differences = (DifferencesStatusEnum)dnode.Location;

            if (diff3 == null) return;

            foreach (Diff3Item diff3Item in ((Diff3)dnode.Diff).Items
                .Where(diff3Item => diff3Item.Differeces == DifferencesStatusEnum.AllDifferent))
            {
                dnode.Status = NodeStatusEnum.IsConflicting;
                break;
            }
        }
    }
}
