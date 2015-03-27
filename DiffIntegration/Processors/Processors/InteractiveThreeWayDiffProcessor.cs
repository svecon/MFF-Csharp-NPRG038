using System;
using System.IO;
using System.Linq;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using CoreLibrary.Processors.Processors;
using CoreLibrary.Settings.Attributes;
using DiffAlgorithm.ThreeWay;
using DiffIntegration.DiffFilesystemTree;
using DiffIntegration.DiffOutput;
using DiffIntegration.DiffOutput.ThreeWay;

namespace DiffIntegration.Processors.Processors
{
    /// <summary>
    /// InteractiveTwoWayDiffProcessor is a console interface for choosing which version 
    /// of the file (local / remote) you want to keep.
    /// </summary>
    public class InteractiveThreeWayDiffProcessor : ProcessorAbstract
    {
        public override int Priority { get { return 1001; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.ThreeWay; } }

        [Settings("Interactive console differ.", "interactive", "i")]
        public bool IsEnabled = false;

        [Settings("Show help during the interactive process.", "interactive-help")]
        public bool ShowHelp = true;

        [Settings("Default action for interactive diff.", "3interactive-default")]
        public Diff3ItemActionEnum DefaultAction = Diff3ItemActionEnum.Default;

        private Diff3ItemActionEnum defaultFileAction;

        private bool applyToFile;

        private bool applyToAll;

        public override void Process(IFilesystemTreeDirNode node)
        {
        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            if (!CheckModeAndStatus(node))
                return;

            if (!IsEnabled)
                return;

            if (node.Differences == DifferencesStatusEnum.AllSame)
                return;

            var dnode = node as DiffFileNode;

            if (dnode == null)
                return;

            if (dnode.Diff3 == null)
                return;

            if (!applyToAll)
            {
                applyToFile = false;
                defaultFileAction = DefaultAction;
            }

            var output = new Diff3NormalOutput((FileInfo)node.InfoLocal, (FileInfo)node.InfoBase, (FileInfo)node.InfoRemote, dnode.Diff3);

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
                diff.Action = defaultFileAction;
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
                diff.Action = defaultFileAction;
                return;
            }

            Diff3ItemActionEnum chosenAction = defaultFileAction;
            switch (input.Substring(0, 1).ToUpperInvariant())
            {
                case "B":
                    diff.Action = chosenAction = Diff3ItemActionEnum.RevertToBase;
                    input = input.Substring(0, 1);
                    break;

                case "L":
                    diff.Action = chosenAction = Diff3ItemActionEnum.ApplyLocal;
                    input = input.Substring(0, 1);
                    break;

                case "R":
                    diff.Action = chosenAction = Diff3ItemActionEnum.ApplyRemote;
                    input = input.Substring(0, 1);
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
