using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Helpers;
using CoreLibrary.Plugins.Processors;

namespace BasicProcessors.DiffProcessors
{
    /// <summary>
    /// Processor for checking whether the file is a text file.
    /// </summary>
    [Processor(ProcessorTypeEnum.Diff, 300, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class FileTypeProcessor : ProcessorAbstract
    {
        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        protected override bool CheckStatus(INodeFileNode node)
        {
            if (node.FileType != FileTypeEnum.Unknown)
                return false;

            return base.CheckStatus(node);
        }

        protected override void ProcessChecked(INodeFileNode node)
        {
            node.FileType = node.Info.FullName.IsTextFile()
                ? FileTypeEnum.Text
                : FileTypeEnum.Binary;
        }
    }
}
