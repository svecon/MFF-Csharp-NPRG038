using System;
using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;

namespace BasicProcessors.InteractiveProcessors
{
    [Processor(ProcessorTypeEnum.Interactive, 9900, DiffModeEnum.ThreeWay)]
    public class InteractiveThreeWayActionProcessor : ProcessorAbstract
    {

        [Settings("Hide help during the interactive process.", "interactive-help")]
        public bool ShowHelp = true;

        [Settings("Default action for interactive diff.", "3interactive-default")]
        public PreferedActionThreeWayEnum DefaultAction = PreferedActionThreeWayEnum.Default;

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

            if (node.IsInLocation(LocationCombinationsEnum.OnBase))
            {
                var infoBase = (FileInfo)node.InfoBase;
                Console.WriteLine("Base file:");
                Console.WriteLine("{0} ({1}, {2}kB)", infoBase.FullName, infoBase.LastWriteTime, infoBase.Length / 1024.0);
            }

            if (node.IsInLocation(LocationCombinationsEnum.OnRemote))
            {
                var infoRemote = (FileInfo)node.InfoRemote;
                Console.WriteLine("Remote file:");
                Console.WriteLine("{0} ({1}, {2}kB)", infoRemote.FullName, infoRemote.LastWriteTime, infoRemote.Length / 1024.0);
            }

            if (ShowHelp)
            {
                Console.WriteLine("[B] to choose base [L] for local and [R] for remote changes. Enter nothing to keep default action.");
            }

            string input = Console.ReadLine();
            Console.ResetColor();

            if (string.IsNullOrEmpty(input))
            {
                diffNode.Action = DefaultAction;
                return;
            }

            switch (input.Substring(0, 1).ToUpperInvariant())
            {
                case "B":
                    diffNode.Action = PreferedActionThreeWayEnum.RevertToBase;
                    return;

                case "L":
                    diffNode.Action = PreferedActionThreeWayEnum.ApplyLocal;
                    return;

                case "R":
                    diffNode.Action = PreferedActionThreeWayEnum.ApplyRemote;
                    return;
            }

            diffNode.Action = DefaultAction;

        }
    }
}
