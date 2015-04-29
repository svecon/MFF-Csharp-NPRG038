using System.IO;
using System.Linq;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;
using DiffAlgorithm;
using DiffAlgorithm.ThreeWay;
using DiffIntegration.DiffFilesystemTree;

namespace BasicProcessors.Processors.DiffProcessors
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

        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
        }

        protected override bool CheckStatus(IFilesystemTreeFileNode node)
        {
            if (node.FileType != FileTypeEnum.Text)
                return false;

            if (node.Differences == DifferencesStatusEnum.AllSame)
                return false;

            return base.CheckStatus(node);
        }

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
        {
            var dnode = node as DiffFileNode;

            if (dnode == null)
                return;

            var diff = new DiffRunHelper(TrimSpace, IgnoreWhitespace, IgnoreCase);

            switch ((LocationCombinationsEnum)node.Location)
            {
                case LocationCombinationsEnum.OnBase:
                case LocationCombinationsEnum.OnLocal:
                case LocationCombinationsEnum.OnRemote:
                    return; // do nothing
                case LocationCombinationsEnum.OnLocalRemote:
                    if (node.Mode == DiffModeEnum.TwoWay)
                    {
                        dnode.Diff = diff.DiffFiles((FileInfo)dnode.InfoLocal, (FileInfo)dnode.InfoRemote);
                    }
                    break;
                default:
                    switch (node.Mode)
                    {
                        case DiffModeEnum.TwoWay:
                            dnode.Diff = diff.DiffFiles((FileInfo)dnode.InfoLocal, (FileInfo)dnode.InfoRemote);
                            break;
                        case DiffModeEnum.ThreeWay:
                            dnode.Diff3 = diff.DiffFiles((FileInfo)dnode.InfoBase, (FileInfo)dnode.InfoLocal, (FileInfo)dnode.InfoRemote);
                            break; // do nothing
                    }
                    break;
            }

            if (dnode.Diff != null && dnode.Diff.Items.Length == 0)
                dnode.Differences = DifferencesStatusEnum.AllSame;

            if (dnode.Diff3 != null && dnode.Diff3.Items.Length == 0)
                dnode.Differences = DifferencesStatusEnum.AllSame;

            if (dnode.Diff3 == null) return;

            foreach (Diff3Item diff3Item in dnode.Diff3.Items
                .Where(diff3Item => diff3Item.Differeces == DifferencesStatusEnum.AllDifferent))
            {
                dnode.Status = NodeStatusEnum.IsConflicting;
                break;
            }
        }
    }
}
