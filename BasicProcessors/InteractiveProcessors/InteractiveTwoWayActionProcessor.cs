using System;
using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;

namespace BasicProcessors.InteractiveProcessors
{
    [Processor(ProcessorTypeEnum.Interactive, 9901, DiffModeEnum.TwoWay)]
    public class InteractiveTwoWayActionProcessor : ProcessorAbstract
    {

        [Settings("Hide help during the interactive process.", "interactive-help")]
        public bool ShowHelp = true;

        [Settings("Default action for interactive diff.", "2interactive-default")]
        public PreferedActionTwoWayEnum DefaultAction = PreferedActionTwoWayEnum.ApplyRemote;

        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
        }

        protected override bool CheckStatus(IFilesystemTreeFileNode node)
        {
            if (node.Status != NodeStatusEnum.IsConflicting)
                return false;

            return base.CheckStatus(node);
        }

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
        {
            var diffNode = node as FileDiffNode;

            if (diffNode == null)
                return;

            Console.ForegroundColor = ConsoleColor.DarkCyan;

            if (node.IsInLocation(LocationCombinationsEnum.OnLocal))
            {
                var infoLocal = (FileInfo)node.InfoLocal;
                Console.WriteLine("Local file:");
                Console.WriteLine("{0} ({1}, {2}kB)", infoLocal.FullName, infoLocal.LastWriteTime, infoLocal.Length / 1024.0);
            }

            if (node.IsInLocation(LocationCombinationsEnum.OnRemote))
            {
                var infoRemote = (FileInfo)node.InfoRemote;
                Console.WriteLine("Remote file:");
                Console.WriteLine("{0} ({1}, {2}kB)", infoRemote.FullName, infoRemote.LastWriteTime, infoRemote.Length / 1024.0);    
            }

            if (ShowHelp)
            {
                Console.WriteLine("[R] to select remote file and [L] to select local file. Enter nothing to keep default action.");
            }

            string input = Console.ReadLine();
            Console.ResetColor();

            if (string.IsNullOrEmpty(input))
            {
                return;
            }

            switch (input.Substring(0, 1).ToUpperInvariant())
            {
                case "R":
                    diffNode.Action = PreferedActionThreeWayEnum.ApplyRemote;
                    return;

                case "L":
                    diffNode.Action = PreferedActionThreeWayEnum.ApplyLocal;
                    return;
            }

            diffNode.Action = (PreferedActionThreeWayEnum)((int)DefaultAction);

        }
    }
}
