using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Plugins.Processors;

namespace BasicProcessors.DiffProcessors
{
    /// <summary>
    /// Processor for checking whether the file is conlicting based on the differences and locations.
    /// </summary>
    [Processor(ProcessorTypeEnum.Diff, 9999, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class CheckConflictsProcessor : ProcessorAbstract
    {
        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        protected override void ProcessChecked(INodeFileNode node)
        {
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
