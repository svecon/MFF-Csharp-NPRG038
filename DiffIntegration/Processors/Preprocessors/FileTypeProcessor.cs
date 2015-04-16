using CoreLibrary.Enums;
using CoreLibrary.Helpers;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Preprocessors;

namespace DiffIntegration.Processors.Preprocessors
{
    /// <summary>
    /// Filter for C# source codes (manually typed ones).
    /// 
    /// Leaves out everything else.
    /// </summary>
    public class FileTypeProcessor : PreProcessorAbstract
    {
        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay; } }

        public override int Priority { get { return 300; } }

        public override void Process(IFilesystemTreeDirNode node)
        {
        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            if (!CheckModeAndStatus(node))
                return;

            if (node.Status == NodeStatusEnum.IsIgnored)
                return;

            node.FileType = node.Info.FullName.IsTextFile()
                ? FileTypeEnum.Text
                : FileTypeEnum.Binary;

        }
    }
}
