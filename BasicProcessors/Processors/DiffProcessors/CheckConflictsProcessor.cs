using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using DiffIntegration.DiffFilesystemTree;

namespace BasicProcessors.Processors.DiffProcessors
{
    [Processor(ProcessorTypeEnum.Diff, 9999, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class CheckConflictsProcessor : ProcessorAbstract
    {
        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
        }

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
        {
            var diffNode = node as DiffFileNode;

            if (diffNode == null)
                return;

            switch ((LocationCombinationsEnum)node.Location)
            {
                case LocationCombinationsEnum.OnBaseLocal:

                    if (node.Differences != DifferencesStatusEnum.BaseRemoteSame && node.Differences != DifferencesStatusEnum.AllSame)
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
