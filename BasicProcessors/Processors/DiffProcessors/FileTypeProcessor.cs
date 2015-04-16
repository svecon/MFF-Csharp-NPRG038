using CoreLibrary.Enums;
using CoreLibrary.Helpers;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;

namespace BasicProcessors.Processors.DiffProcessors
{
    /// <summary>
    /// Filter for C# source codes (manually typed ones).
    /// 
    /// Leaves out everything else.
    /// </summary>
    [Processor(ProcessorTypeEnum.Diff, 300, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class FileTypeProcessor : ProcessorAbstract
    {
        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
        }

        protected override bool CheckStatus(IFilesystemTreeFileNode node)
        {
            if (node.FileType != FileTypeEnum.Unknown)
                return false;

            return base.CheckStatus(node);
        }

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
        {
            node.FileType = node.Info.FullName.IsTextFile()
                ? FileTypeEnum.Text
                : FileTypeEnum.Binary;
        }
    }
}
