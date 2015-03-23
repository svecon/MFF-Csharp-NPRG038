using System;
using System.IO;
using System.Linq;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using CoreLibrary.Processors.Processors;
using CoreLibrary.Settings.Attributes;
using DiffAlgorithm.Diff;
using DiffAlgorithm.Diff3;
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

            if (node.Differences == DifferencesStatusEnum.AllSame)
                return;

            var dnode = node as DiffFileNode;

            if (dnode == null)
                return;

            var diff = new DiffHelper(TrimSpace, IgnoreWhitespace, IgnoreCase);

            switch (node.Mode)
            {
                case DiffModeEnum.TwoWay:

                    if ((LocationCombinationsEnum)node.Location != LocationCombinationsEnum.OnLocalRemote)
                        return;

                    dnode.Diff = diff.DiffFiles((FileInfo)dnode.InfoLocal, (FileInfo)dnode.InfoRemote);

                    if (dnode.Diff.Items.Length == 0)
                        dnode.Differences = DifferencesStatusEnum.AllSame;

                    break;
                case DiffModeEnum.ThreeWay:

                    // TODO: WHAT IF FILES ARE MISSING!!!!!

                    dnode.Diff3 = diff.DiffFiles((FileInfo)dnode.InfoBase, (FileInfo)dnode.InfoLocal, (FileInfo)dnode.InfoRemote);

                    if (dnode.Diff3.Items.Length == 0)
                        dnode.Differences = DifferencesStatusEnum.AllSame;

                    break;
            }
        }
    }
}
