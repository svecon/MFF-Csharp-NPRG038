using System;
using System.IO;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
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
        /// <summary>
        /// Settings for showing help during resolving of the conflicts.
        /// </summary>
        [Settings("Show help during the interactive process.", "interactive-help")]
        public bool ShowHelp = false;

        /// <summary>
        /// Setting for default action used to resolve conflicts.
        /// </summary>
        [Settings("Default action for interactive diff.", "2interactive-default")]
        public PreferedActionTwoWayEnum DefaultAction = PreferedActionTwoWayEnum.ApplyRemote;

        /// <summary>
        /// Prefered action for current file. Will reset after the file is processed.
        /// </summary>
        private PreferedActionTwoWayEnum defaultFileAction;

        /// <summary>
        /// Apply default action to whole file?
        /// </summary>
        private bool applyToFile;

        /// <summary>
        /// Apply default action to all files?
        /// </summary>
        private bool applyToAll;

        /// <inheritdoc />
        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        /// <inheritdoc />
        protected override bool CheckStatus(INodeFileNode node)
        {
            //if (!IsEnabled)
            //    return false;

            if (node.Differences == DifferencesStatusEnum.AllSame)
                return false;

            if ((LocationCombinationsEnum)node.Location != LocationCombinationsEnum.OnLocalRemote)
                return false;

            return base.CheckStatus(node);
        }

        /// <inheritdoc />
        protected override void ProcessChecked(INodeFileNode node)
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

            node.Status = NodeStatusEnum.WasDiffed;
        }

        /// <summary>
        /// Parses the user input and assings an action to givend diff.
        /// </summary>
        /// <param name="diff">Diff that will have the action set.</param>
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
                Console.WriteLine("[R] to select remote file or [L] to select local file. Enter nothing to keep default action.");
                Console.WriteLine("Append FILE to the option to apply that option for the whole file. [FILE|BFILE|LFILE|RFILE]");
                Console.WriteLine("Append ALL  to the option to apply that option for all files. [ALL|BALL|LALL|RALL]");
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
                    diff.PreferedAction = chosenAction = PreferedActionTwoWayEnum.ApplyRemote;
                    input = input.Substring(1);
                    break;

                case "L":
                    diff.PreferedAction = chosenAction = PreferedActionTwoWayEnum.ApplyLocal;
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
