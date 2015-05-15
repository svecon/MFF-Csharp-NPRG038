﻿using System;
using System.IO;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;

namespace BasicProcessors.InteractiveProcessors
{
    /// <summary>
    /// Interactive processor used to resolve and print differences between 2 files.
    /// </summary>
    [Processor(ProcessorTypeEnum.Interactive, 9900, DiffModeEnum.TwoWay)]
    public class InteractiveTwoWayActionProcessor : ProcessorAbstract
    {
        /// <summary>
        /// Show help for resolving conflicts.
        /// </summary>
        [Settings("Show help during the interactive process.", "interactive-help")]
        public bool ShowHelp = false;

        [Settings("Default action for interactive diff.", "2interactive-default")]
        public PreferedActionTwoWayEnum DefaultAction = PreferedActionTwoWayEnum.ApplyRemote;

        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        protected override bool CheckStatus(INodeFileNode node)
        {
            if (node.Status != NodeStatusEnum.IsConflicting)
                return false;

            return base.CheckStatus(node);
        }

        protected override void ProcessChecked(INodeFileNode node)
        {
            var diffNode = node as FileDiffNode;

            if (diffNode == null)
                return;

            if (node.IsInLocation(LocationCombinationsEnum.OnLocal))
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                var infoLocal = (FileInfo)node.InfoLocal;
                Console.WriteLine("Local file:");
                Console.ResetColor();
                Console.WriteLine("{0} ({1}, {2:0.#}kB)", infoLocal.FullName, infoLocal.LastWriteTime, infoLocal.Length / 1024.0);
            }

            if (node.IsInLocation(LocationCombinationsEnum.OnRemote))
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                var infoRemote = (FileInfo)node.InfoRemote;
                Console.WriteLine("Remote file:");
                Console.ResetColor();
                Console.WriteLine("{0} ({1}, {2:0.#}kB)", infoRemote.FullName, infoRemote.LastWriteTime, infoRemote.Length / 1024.0);
            }

            Console.ForegroundColor = ConsoleColor.DarkCyan;

            if (ShowHelp)
            {
                Console.WriteLine("[R] to select remote file and [L] to select local file. Default is [R].");
            }

            string input = Console.ReadLine();
            Console.ResetColor();

            if (string.IsNullOrEmpty(input))
            {
                diffNode.Action = (PreferedActionThreeWayEnum)((int)DefaultAction);
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