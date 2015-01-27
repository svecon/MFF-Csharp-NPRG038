using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Preprocessors;
using CoreLibrary.Settings.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SyncFolders.Processors.Preprocessors
{
    /// <summary>
    /// Filter for C# source codes (manually typed ones).
    /// 
    /// Leaves out everything else.
    /// </summary>
    public class RegexFilterProcessor : PreProcessorAbstract
    {
        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay; } }

        public override int Priority { get { return 150; } }

        [Settings("Include only file names that are matching Regex.", "include-regex", "iR")]
        public Regex IncludeRegex = null;

        [Settings("Exclude file name that are matching Regex.", "exclude-regex", "eR")]
        public Regex ExcludeRegex = null;

        public override void Process(IFilesystemTreeDirNode node)
        {
        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            if (!checkModeAndStatus(node))
                return;

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
