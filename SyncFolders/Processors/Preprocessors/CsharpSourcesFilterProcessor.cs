using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Preprocessors;
using CoreLibrary.Settings.Attributes;

namespace SyncFolders.Processors.Preprocessors
{
    /// <summary>
    /// Filter for C# source codes (manually typed ones).
    /// 
    /// Leaves out everything else.
    /// </summary>
    public class CsharpSourcesFilterProcessor : PreProcessorAbstract
    {
        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay; } }

        public override int Priority { get { return 50; } }

        [Settings("Filter for only C# source files.", "csharp-source-code", "C#")]
        public bool IsEnabled = false;

        public override void Process(IFilesystemTreeDirNode node)
        {
        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            if (!CheckModeAndStatus(node))
                return;

            if (!IsEnabled)
                return;

            CheckModeAndStatus(node);

            if (node.Info.Extension.ToLowerInvariant() != ".cs")
                node.Status = NodeStatusEnum.IsIgnored;

            if (node.Info.Name == "AssemblyInfo.cs")
                node.Status = NodeStatusEnum.IsIgnored;

            if (node.Info.Name.StartsWith("TemporaryGeneratedFile_"))
                node.Status = NodeStatusEnum.IsIgnored;
        }
    }
}
