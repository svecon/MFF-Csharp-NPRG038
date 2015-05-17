using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Plugins.Processors;

namespace BasicProcessors.DiffProcessors
{
    /// <summary>
    /// Initialization processor ensures that the data is consistent between multiple diff calculations.
    /// </summary>
    [Processor(ProcessorTypeEnum.Diff, 0, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class InitializationProcessor : ProcessorAbstract
    {
        /// <inheritdoc />
        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        /// <inheritdoc />
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
