using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;

namespace BasicProcessors.DiffProcessors
{
    /// <summary>
    /// Filter for C# source codes (manually typed ones).
    /// 
    /// Leaves out everything else.
    /// </summary>
    [Processor(ProcessorTypeEnum.Diff, 50, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class CsharpSourcesFilterProcessor : ProcessorAbstract
    {
        [Settings("Filter for only C# source files.", "csharp-source-code", "C#")]
        public bool IsEnabled = false;

        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        protected override bool CheckStatus(INodeFileNode node)
        {
            return IsEnabled && base.CheckStatus(node);
        }

        protected override void ProcessChecked(INodeFileNode node)
        {
            if (node.Info.Extension.ToLowerInvariant() != ".cs")
                node.Status = NodeStatusEnum.IsIgnored;

            if (node.Info.Name == "AssemblyInfo.cs")
                node.Status = NodeStatusEnum.IsIgnored;

            if (node.Info.Name.EndsWith(".g.cs"))
                node.Status = NodeStatusEnum.IsIgnored;

            if (node.Info.Name.EndsWith(".g.i.cs"))
                node.Status = NodeStatusEnum.IsIgnored;

            if (node.Info.Name.StartsWith("TemporaryGeneratedFile_"))
                node.Status = NodeStatusEnum.IsIgnored;
        }
    }
}
