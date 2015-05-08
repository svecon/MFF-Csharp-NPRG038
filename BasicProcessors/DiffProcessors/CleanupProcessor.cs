using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Plugins.Processors;

namespace BasicProcessors.DiffProcessors
{
    [Processor(ProcessorTypeEnum.Diff, 0, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class CleanupProcessor : ProcessorAbstract
    {
        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        protected override void ProcessChecked(INodeFileNode node)
        {
            var diffNode = node as FileDiffNode;

            if (diffNode == null)
                return;

            diffNode.Diff = null;
            diffNode.Diff = null;
            diffNode.Differences = DifferencesStatusEnum.Initial;
            diffNode.Status = NodeStatusEnum.Initial;
            if (diffNode.InfoBase != null) diffNode.InfoBase.Refresh();
            if (diffNode.InfoLocal != null) diffNode.InfoLocal.Refresh();
            if (diffNode.InfoRemote != null) diffNode.InfoRemote.Refresh();
        }

    }
}
