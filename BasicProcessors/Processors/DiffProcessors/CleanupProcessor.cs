using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using DiffIntegration.DiffFilesystemTree;

namespace BasicProcessors.Processors.DiffProcessors
{
    [Processor(ProcessorTypeEnum.Diff, 0, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class CleanupProcessor : ProcessorAbstract
    {
        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
        }

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
        {
            var diffNode = node as DiffFileNode;

            if (diffNode == null)
                return;

            diffNode.Diff = null;
            diffNode.Diff3 = null;
            diffNode.Differences = DifferencesStatusEnum.Initial;
            diffNode.Status = NodeStatusEnum.Initial;
        }

    }
}
