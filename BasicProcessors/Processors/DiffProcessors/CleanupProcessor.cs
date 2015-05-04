using CoreLibrary.Enums;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.Processors;

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
