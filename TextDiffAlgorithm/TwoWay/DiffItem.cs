using System.Text;
using CoreLibrary.FilesystemTree.Enums;

namespace TextDiffAlgorithm.TwoWay
{
    /// <summary>
    /// Container for representing one diff change between two files.
    /// </summary>
    public class DiffItem
    {
        /// <summary>
        /// Line number of old file where change starts.
        /// </summary>
        public readonly int LocalLineStart;

        /// <summary>
        /// Line number of new file where change starts.
        /// </summary>
        public readonly int RemoteLineStart;

        /// <summary>
        /// Number of lines deleted in the old file.
        /// </summary>
        public readonly int LocalAffectedLines;

        /// <summary>
        /// Number of lines inserted in the new file.
        /// </summary>
        public readonly int RemoteAffectedLines;

        /// <summary>
        /// Default action to do with this diff.
        /// </summary>
        public PreferedActionTwoWayEnum PreferedAction;

        /// <summary>
        /// Initializes new instance of the <see cref="DiffItem"/>
        /// </summary>
        /// <param name="localLineStart">A line number where the difference starts in local file.</param>
        /// <param name="remoteLineStart">A line number where the difference starts in remote file.</param>
        /// <param name="localAffectedLines">Number of lines affected in local file.</param>
        /// <param name="remoteAffectedLines">Number of lines affected in remote file.</param>
        public DiffItem(int localLineStart, int remoteLineStart, int localAffectedLines, int remoteAffectedLines)
        {
            LocalLineStart = localLineStart;
            RemoteLineStart = remoteLineStart;
            LocalAffectedLines = localAffectedLines;
            RemoteAffectedLines = remoteAffectedLines;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(LocalAffectedLines.ToString())
                .Append(".")
                .Append(RemoteAffectedLines.ToString())
                .Append(".")
                .Append(LocalLineStart.ToString())
                .Append(".")
                .Append(RemoteLineStart.ToString())
                .Append("*")
                ;

            return sb.ToString();
        }
    }
}
