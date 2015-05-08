using System.Text.RegularExpressions;
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
    [Processor(ProcessorTypeEnum.Diff, 150, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class RegexFilterProcessor : ProcessorAbstract
    {
        [Settings("Include only file names that are matching Regex.", "include-regex", "iR")]
        public Regex IncludeRegex = null;

        [Settings("Exclude file name that are matching Regex.", "exclude-regex", "eR")]
        public Regex ExcludeRegex = null;

        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        protected override void ProcessChecked(INodeFileNode node)
        {
            if (ExcludeRegex != null && ExcludeRegex.IsMatch(node.Info.Name))
            {
                node.Status = NodeStatusEnum.IsIgnored;
            }

            if (IncludeRegex != null && !IncludeRegex.IsMatch(node.Info.Name))
            {
                node.Status = NodeStatusEnum.IsIgnored;
            }
        }
    }
}
