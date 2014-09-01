using CoreLibrary.Enums;
using CoreLibrary.Settings.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Processors.Preprocessors
{
    public class CsharpSourcesFilterProcessor : PreProcessorAbstract
    {
        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay; } }

        public override int Priority { get { return 50; } }

        [Settings("Filter for only C# source files.", "csharp-source-code", "C#")]
        public bool IsEnabled = false;

        public override void Process(Interfaces.IFilesystemTreeDirNode node)
        {
        }

        public override void Process(Interfaces.IFilesystemTreeFileNode node)
        {
            if (!checkModeAndStatus(node))
                return;

            if (!IsEnabled)
                return;

            checkModeAndStatus(node);

            if (node.Info.Extension.ToLowerInvariant() != ".cs")
                node.Status = NodeStatusEnum.IsIgnored;

            if (node.Info.Name == "AssemblyInfo.cs")
                node.Status = NodeStatusEnum.IsIgnored;

            if (node.Info.Name.StartsWith("TemporaryGeneratedFile_"))
                node.Status = NodeStatusEnum.IsIgnored;
        }
    }
}
