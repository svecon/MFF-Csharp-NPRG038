﻿using System.Text;
using CoreLibrary.FilesystemTree.Enums;

namespace TextDiffAlgorithm.ThreeWay
{
    /// <summary>
    /// Container for representing one diff change between three files.
    /// </summary>
    public class Diff3Item
    {
        /// <summary>
        /// Line number in base file where change starts.
        /// </summary>
        public readonly int BaseLineStart;

        /// <summary>
        /// Line number in local file where change starts.
        /// </summary>
        public readonly int LocalLineStart;

        /// <summary>
        /// Line number in remote file where change starts.
        /// </summary>
        public readonly int RemoteLineStart;

        /// <summary>
        /// Number of lines affected in the base file.
        /// </summary>
        public readonly int BaseAffectedLines;

        /// <summary>
        /// Number of lines affected in the local file.
        /// </summary>
        public readonly int LocalAffectedLines;

        /// <summary>
        /// Number of lines affected in the remote file.
        /// </summary>
        public readonly int RemoteAffectedLines;

        /// <summary>
        /// Type of the diff chunk - which files differ.
        /// </summary>
        public DifferencesStatusEnum Differeces;

        /// <summary>
        /// Default action to do with this diff.
        /// </summary>
        public PreferedActionThreeWayEnum PreferedAction;

        /// <summary>
        /// Initializes new instance of the <see cref="Diff3Item"/>
        /// </summary>
        /// <param name="baseLineStart">A line number where the difference starts in base file.</param>
        /// <param name="localLineStart">A line number where the difference starts in local file.</param>
        /// <param name="remoteLineStart">A line number where the difference starts in remote file.</param>
        /// <param name="baseAffectedLines">Number of lines affected in base file.</param>
        /// <param name="localAffectedLines">Number of lines affected in local file.</param>
        /// <param name="remoteAffectedLines">Number of lines affected in remote file.</param>
        /// <param name="diff">Which differences are same.</param>
        public Diff3Item(int baseLineStart, int localLineStart, int remoteLineStart, int baseAffectedLines,
            int localAffectedLines, int remoteAffectedLines, DifferencesStatusEnum diff)
        {
            BaseLineStart = baseLineStart;
            LocalLineStart = localLineStart;
            RemoteLineStart = remoteLineStart;

            BaseAffectedLines = baseAffectedLines;
            LocalAffectedLines = localAffectedLines;
            RemoteAffectedLines = remoteAffectedLines;

            Differeces = diff;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(BaseLineStart)
                .Append(".")
                .Append(LocalLineStart)
                .Append(".")
                .Append(RemoteLineStart)
                .Append("^")
                .Append(BaseAffectedLines)
                .Append(".")
                .Append(LocalAffectedLines)
                .Append(".")
                .Append(RemoteAffectedLines)
                ;

            if (Differeces == DifferencesStatusEnum.AllDifferent)
            {
                sb.Append("!!");
            }

            return sb.ToString();
        }

    }
}
