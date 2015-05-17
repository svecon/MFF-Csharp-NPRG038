using System.Text.RegularExpressions;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;

namespace BasicProcessors.DiffProcessors
{
    /// <summary>
    /// Filter processor for filtering based on Regex
    /// </summary>
    [Processor(ProcessorTypeEnum.Diff, 150, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class RegexFilterProcessor : ProcessorAbstract
    {
        /// <summary>
        /// Includes only files matching given Regex.
        /// </summary>
        [Settings("Include only file names that are matching Regex.", "include-regex", "iR")]
        public Regex IncludeRegex = null;

        /// <summary>
        /// Excludes all files matching given Regex.
        /// </summary>
        [Settings("Exclude file name that are matching Regex.", "exclude-regex", "eR")]
        public Regex ExcludeRegex = null;

        /// <inheritdoc />
        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        /// <inheritdoc />
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
