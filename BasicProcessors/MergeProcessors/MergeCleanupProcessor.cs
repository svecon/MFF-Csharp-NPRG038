using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Plugins.Processors;

namespace BasicProcessors.MergeProcessors
{
    /// <summary>
    /// Merge cleanup processor ensures consistent data between multiple runs.
    /// </summary>
    [Processor(ProcessorTypeEnum.Merge, 9999, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class MergeCleanupProcessor : ProcessorAbstract
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
            if (diffNode.InfoBase != null) diffNode.InfoBase.Refresh();
            if (diffNode.InfoLocal != null) diffNode.InfoLocal.Refresh();
            if (diffNode.InfoRemote != null) diffNode.InfoRemote.Refresh();
        }

    }
}
