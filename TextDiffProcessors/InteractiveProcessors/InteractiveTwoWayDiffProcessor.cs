using System;
using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;
using TextDiffAlgorithm.TwoWay;
using TextDiffProcessors.DiffOutput.TwoWay;

namespace TextDiffProcessors.InteractiveProcessors
{
    /// <summary>
    /// InteractiveTwoWayDiffProcessor is a console interface for choosing which version 
    /// of the file (local / remote) you want to keep.
    /// </summary>
    [Processor(ProcessorTypeEnum.Interactive, 100, DiffModeEnum.TwoWay)]
    public class InteractiveTwoWayDiffProcessor : ProcessorAbstract
    {
        //[Settings("Interactive console differ.", "interactive", "i")]
        //public bool IsEnabled = false;

        [Settings("Show help during the interactive process.", "interactive-help")]
        public bool ShowHelp = false;

        [Settings("Default action for interactive diff.", "2interactive-default")]
        public PreferedActionTwoWayEnum DefaultAction = PreferedActionTwoWayEnum.ApplyRemote;

        private PreferedActionTwoWayEnum defaultFileAction;

        private bool applyToFile;

        private bool applyToAll;

        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
        }

        protected override bool CheckStatus(IFilesystemTreeFileNode node)
        {
            //if (!IsEnabled)
            //    return false;

            if (node.Differences == DifferencesStatusEnum.AllSame)
                return false;

            if ((LocationCombinationsEnum)node.Location != LocationCombinationsEnum.OnLocalRemote)
                return false;

            return base.CheckStatus(node);
        }

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
        {
            var dnode = node as FileDiffNode;

            if (dnode == null)
                return;

            if (dnode.Diff == null)
                return;

            if (!applyToAll)
            {
                applyToFile = false;
                defaultFileAction = DefaultAction;
            }

            var output = new UnifiedDiffOutput((FileInfo)node.InfoLocal, (FileInfo)node.InfoRemote, (Diff)dnode.Diff, 4);

            DiffItem currentDiffItem = null;

            Console.WriteLine();
            foreach (string line in output.Print())
            {
                if (output.DiffHasEnded)
                {
                    ParseUserInput(currentDiffItem);
                }

                Console.WriteLine(line);
                currentDiffItem = output.CurrentDiffItem;
            }

            if (output.DiffHasEnded)
            {
                ParseUserInput(output.CurrentDiffItem);
            }
        }

        private void ParseUserInput(DiffItem diff)
        {
            if (applyToFile)
            {
                diff.PreferedAction = defaultFileAction;
                return;
            }

            ConsoleColor tempColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkCyan;

            if (ShowHelp)
            {
                Console.WriteLine("[R] to revert and [K] to keep changes. Enter nothing to keep default action.");
                Console.WriteLine("[RFILE] to revert and [KFILE] to keep changes in this file.");
                Console.WriteLine("[RALL] to revert and [KALL] to keep all changes across all files.");
            }

            string input = Console.ReadLine();
            Console.ForegroundColor = tempColor;

            if (string.IsNullOrEmpty(input))
            {
                diff.PreferedAction = defaultFileAction;
                return;
            }

            PreferedActionTwoWayEnum chosenAction = defaultFileAction;
            switch (input.Substring(0, 1).ToUpperInvariant())
            {
                case "R":
                    diff.PreferedAction = chosenAction = PreferedActionTwoWayEnum.ApplyLocal;
                    input = input.Substring(1);
                    break;

                case "K":
                    diff.PreferedAction = chosenAction = PreferedActionTwoWayEnum.ApplyRemote;
                    input = input.Substring(1);
                    break;
            }

            if (string.IsNullOrEmpty(input))
                return;

            switch (input.ToUpperInvariant())
            {
                case "FILE":
                    applyToFile = true;
                    defaultFileAction = chosenAction;
                    break;

                case "ALL":
                    applyToAll = applyToFile = true;
                    DefaultAction = defaultFileAction = chosenAction;
                    break;
            }
        }
    }
}
