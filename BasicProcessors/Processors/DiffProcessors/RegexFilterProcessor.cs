using System.Text.RegularExpressions;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using CoreLibrary.Settings.Attributes;

namespace BasicProcessors.Processors.DiffProcessors
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

        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
        }

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
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
