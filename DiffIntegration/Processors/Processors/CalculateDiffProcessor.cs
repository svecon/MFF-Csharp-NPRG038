using System;
using System.IO;
using System.Linq;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using CoreLibrary.Processors.Processors;
using CoreLibrary.Settings.Attributes;
using DiffAlgorithm.TwoWay;
using DiffIntegration.DiffFilesystemTree;

namespace DiffIntegration.Processors.Processors
{
    /// <summary>
    /// CalculateDiffProcessor calculates diff between files that we know are different
    /// </summary>
    public class CalculateDiffProcessor : ProcessorAbstract
    {
        public override int Priority { get { return 500; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay; } }

        [Settings("Diff algorithm will ignore leading and trailing whitespace", "trim-space", "ts")]
        public bool TrimSpace = false;

        [Settings("Diff algorithm will ignore all white space", "ignore-whitespace", "iw")]
        public bool IgnoreWhitespace = false;

        [Settings("Diff algorithm will ignore case senstivity", "ignore-case", "ic")]
        public bool IgnoreCase = false;

        public override void Process(IFilesystemTreeDirNode node)
        {
        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            if (!CheckModeAndStatus(node))
                return;

            if (node.FileType != FileTypeEnum.Text)
                return;

            if (node.Differences == DifferencesStatusEnum.AllSame)
                return;

            var dnode = node as DiffFileNode;

            if (dnode == null)
                return;

            var diff = new DiffHelper(TrimSpace, IgnoreWhitespace, IgnoreCase);

            switch ((LocationCombinationsEnum)node.Location)
            {
                case LocationCombinationsEnum.OnBase:
                case LocationCombinationsEnum.OnLocal:
                case LocationCombinationsEnum.OnRemote:
                    return; // do nothing

                case LocationCombinationsEnum.OnLocalRemote:

                    switch (node.Mode)
                    {
                        case DiffModeEnum.TwoWay:
                            dnode.Diff = diff.DiffFiles((FileInfo)dnode.InfoLocal, (FileInfo)dnode.InfoRemote);
                            break;
                        case DiffModeEnum.ThreeWay:
                            break; // do nothing
                    }

                    break;
                case LocationCombinationsEnum.OnBaseLocal:

                    dnode.Diff = diff.DiffFiles((FileInfo)dnode.InfoBase, (FileInfo)dnode.InfoLocal);
                    break;

                case LocationCombinationsEnum.OnBaseRemote:

                    dnode.Diff = diff.DiffFiles((FileInfo)dnode.InfoBase, (FileInfo)dnode.InfoRemote);
                    break;

                case LocationCombinationsEnum.OnAll3:

                    dnode.Diff3 = diff.DiffFiles((FileInfo)dnode.InfoBase, (FileInfo)dnode.InfoLocal, (FileInfo)dnode.InfoRemote);
                    break;
            }

            if (dnode.Diff != null && dnode.Diff.Items.Length == 0)
                dnode.Differences = DifferencesStatusEnum.AllSame;

            if (dnode.Diff3 != null && dnode.Diff3.Items.Length == 0)
                dnode.Differences = DifferencesStatusEnum.AllSame;

        }
    }
}
