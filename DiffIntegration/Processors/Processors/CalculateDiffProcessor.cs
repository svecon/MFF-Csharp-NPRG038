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

            var diff = new DiffHelper();

            switch (node.Mode)
            {
                case DiffModeEnum.TwoWay:
                    dnode.Diff = diff.DiffFiles((FileInfo)dnode.InfoLocal, (FileInfo)dnode.InfoRemote);
                    break;
                case DiffModeEnum.ThreeWay:
                    dnode.Diff3 = diff.DiffFiles((FileInfo)dnode.InfoBase, (FileInfo)dnode.InfoLocal, (FileInfo)dnode.InfoRemote);
                    break;
            }
        }
    }
}
