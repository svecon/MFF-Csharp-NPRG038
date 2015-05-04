using System;
using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;
using TextDiffAlgorithm.ThreeWay;
using TextDiffProcessors.DiffOutput.ThreeWay;

namespace TextDiffProcessors.InteractiveResolveProcessors
{
    /// <summary>
    /// InteractiveTwoWayDiffProcessor is a console interface for choosing which version 
    /// of the file (local / remote) you want to keep.
    /// </summary>
    [Processor(ProcessorTypeEnum.InteractiveResolving, 200, DiffModeEnum.ThreeWay)]
    public class InteractiveThreeWayDiffProcessor : ProcessorAbstract
    {
        [Settings("Interactive console differ.", "interactive", "i")]
        public bool IsEnabled = false;

        [Settings("Show help during the interactive process.", "interactive-help")]
        public bool ShowHelp = true;

        [Settings("Default action for interactive diff.", "3interactive-default")]
        public PreferedActionThreeWayEnum DefaultPreferedAction = PreferedActionThreeWayEnum.Default;

        private PreferedActionThreeWayEnum defaultFilePreferedAction;

        private bool applyToFile;

        private bool applyToAll;

        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
        }

        protected override bool CheckStatus(IFilesystemTreeFileNode node)
        {
            return base.CheckStatus(node) && IsEnabled && node.Differences != DifferencesStatusEnum.AllSame;
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
                defaultFilePreferedAction = DefaultPreferedAction;
            }

            var output = new Diff3NormalOutput((FileInfo)node.InfoLocal, (FileInfo)node.InfoBase, (FileInfo)node.InfoRemote, (Diff3)dnode.Diff);

            Diff3Item currentDiffItem = null;

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

        private void ParseUserInput(Diff3Item diff)
        {
            if (applyToFile)
            {
                diff.PreferedAction = defaultFilePreferedAction;
                return;
            }

            ConsoleColor tempColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkCyan;

            if (ShowHelp)
            {
                Console.WriteLine("[B] to choose base [L] for local and [R] for remote changes. Enter nothing to keep default action.");
                Console.WriteLine("Append FILE to the option to apply that option for the whole file. [FILE|BFILE|LFILE|RFILE]");
                Console.WriteLine("Append ALL  to the option to apply that option for all files. [ALL|BALL|LALL|RALL]");
            }

            string input = Console.ReadLine();
            Console.ForegroundColor = tempColor;

            if (string.IsNullOrEmpty(input))
            {
                diff.PreferedAction = defaultFilePreferedAction;
                return;
            }

            PreferedActionThreeWayEnum chosenPreferedAction = defaultFilePreferedAction;
            switch (input.Substring(0, 1).ToUpperInvariant())
            {
                case "B":
                    diff.PreferedAction = chosenPreferedAction = PreferedActionThreeWayEnum.RevertToBase;
                    input = input.Substring(0, 1);
                    break;

                case "L":
                    diff.PreferedAction = chosenPreferedAction = PreferedActionThreeWayEnum.ApplyLocal;
                    input = input.Substring(0, 1);
                    break;

                case "R":
                    diff.PreferedAction = chosenPreferedAction = PreferedActionThreeWayEnum.ApplyRemote;
                    input = input.Substring(0, 1);
                    break;
            }

            if (string.IsNullOrEmpty(input))
                return;

            switch (input.ToUpperInvariant())
            {
                case "FILE":
                    applyToFile = true;
                    defaultFilePreferedAction = chosenPreferedAction;
                    break;

                case "ALL":
                    applyToAll = applyToFile = true;
                    DefaultPreferedAction = defaultFilePreferedAction = chosenPreferedAction;
                    break;
            }
        }
    }
}
