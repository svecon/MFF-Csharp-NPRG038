using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Plugins.Processors;

namespace TextDiffProcessors.DiffProcessors
{
    [Processor(ProcessorTypeEnum.Diff, 9999, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class CheckConflictsProcessor : ProcessorAbstract
    {
        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        protected override void ProcessChecked(INodeFileNode node)
        {
            var diffNode = node as FileDiffNode;

            if (diffNode == null)
                return;

            switch ((LocationCombinationsEnum)node.Location)
            {
                case LocationCombinationsEnum.OnBaseLocal:

                    if (node.Differences != DifferencesStatusEnum.BaseLocalSame && node.Differences != DifferencesStatusEnum.AllSame)
                        node.Status = NodeStatusEnum.IsConflicting;

                    break;
                case LocationCombinationsEnum.OnBaseRemote:

                    if (node.Differences != DifferencesStatusEnum.BaseRemoteSame && node.Differences != DifferencesStatusEnum.AllSame)
                        node.Status = NodeStatusEnum.IsConflicting;

                    break;
                case LocationCombinationsEnum.OnLocalRemote:

                    if (node.Differences != DifferencesStatusEnum.LocalRemoteSame && node.Differences != DifferencesStatusEnum.AllSame)
                        node.Status = NodeStatusEnum.IsConflicting;

                    break;
            }
        }

    }
}
