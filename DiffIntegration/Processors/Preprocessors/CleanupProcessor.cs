using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Preprocessors;
using DiffIntegration.DiffFilesystemTree;

namespace DiffIntegration.Processors.Preprocessors
{
    public class CleanupProcessor : PreProcessorAbstract
    {
        public override int Priority { get { return 0; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay; } }

        public override void Process(IFilesystemTreeDirNode node)
        {

        }

        public override void Process(IFilesystemTreeFileNode node)
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
