using System;
using System.IO;
using System.Linq;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using CoreLibrary.Processors.Processors;
using CoreLibrary.Settings.Attributes;
using DiffAlgorithm.Diff;
using DiffIntegration.DiffFilesystemTree;
using DiffIntegration.DiffOutput;

namespace DiffIntegration.Processors.Processors
{
    /// <summary>
    /// InteractiveDiffProcessor is a console interface for choosing which version 
    /// of the file (local / remote) you want to keep.
    /// </summary>
    public class InteractiveDiffProcessor : ProcessorAbstract
    {
        public override int Priority { get { return 1000; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay; } }

        [Settings("Interactive console differ.", "interactive", "i")]
        public bool IsEnabled = false;

        private bool applyToFile;

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

            if (dnode.Diff == null)
                return;

            applyToFile = false;

            var output = new UnifiedDiffOutput((FileInfo)node.InfoLocal, (FileInfo)node.InfoRemote, dnode.Diff, 4);

            Console.WriteLine();
            foreach (string line in output.Print())
            {
                if (output.DiffHasEnded)
                {
                    ParseUserInput(output.CurrentDiffItem);
                }

                Console.WriteLine(line);
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
                diff.Action = DiffItemActionEnum.ApplyRemote;
                return;
            }

            ConsoleColor tempColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkCyan;

            Console.WriteLine("Would you like to revert changes? [R] Or apply remote for this file? [K]");

            string input = Console.ReadLine();
            Console.ForegroundColor = tempColor;

            if (input != null && input.Trim().Equals("r", StringComparison.InvariantCultureIgnoreCase))
            {
                diff.Action = DiffItemActionEnum.RevertToLocal;
            } else
            {
                diff.Action = DiffItemActionEnum.ApplyRemote;

                if (input != null && input.Trim().Equals("k", StringComparison.InvariantCultureIgnoreCase))
                    applyToFile = true;
            }
        }
    }
}
