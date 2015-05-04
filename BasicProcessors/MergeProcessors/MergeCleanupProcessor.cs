using CoreLibrary.Enums;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.Processors;

namespace BasicProcessors.MergeProcessors
{
    [Processor(ProcessorTypeEnum.Merge, 9999, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class MergeCleanupProcessor : ProcessorAbstract
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
            if (diffNode.InfoBase != null) diffNode.InfoBase.Refresh();
            if (diffNode.InfoLocal != null) diffNode.InfoLocal.Refresh();
            if (diffNode.InfoRemote != null) diffNode.InfoRemote.Refresh();
        }

    }
}
