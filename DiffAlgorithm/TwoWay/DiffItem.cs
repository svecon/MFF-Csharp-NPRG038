using System.Text;
using CoreLibrary.Enums;

namespace DiffAlgorithm.TwoWay
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
