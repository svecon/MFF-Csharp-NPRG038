using System.Collections.Generic;
using CoreLibrary.FilesystemDiffTree;
using TextDiffAlgorithm.TwoWay;

namespace TextDiffWindows.Controls.LineMarkers
{
    /// <summary>
    /// An element for connecting related differences for two way.
    /// </summary>
    class LineMarkersTwoWayElement : LineMarkersElementAbstract<DiffItem>
    {
        /// <summary>
        /// Initializes new instance of the <see cref="LineMarkersTwoWayElement"/>
        /// </summary>
        /// <param name="node">A node that contains the files and differences.</param>
        /// <param name="leftText">A pointer to the text element on the left.</param>
        /// <param name="rightText">A pointer to the text element on the right.</param>
        public LineMarkersTwoWayElement(FileDiffNode node, TextAreaAbstract leftText, TextAreaAbstract rightText)
            : base(node, leftText, rightText)
        {
        }

        protected override IEnumerable<DiffItem> VisibleDiffItems()
        {
            if (Node.Diff == null)
                yield break;

            foreach (DiffItem diffItem in ((Diff)Node.Diff).Items)
            {
                if (diffItem.LocalLineStart + diffItem.LocalAffectedLines < PositionToLine(LeftOffset)
                    && diffItem.RemoteLineStart + diffItem.RemoteAffectedLines < PositionToLine(RightOffset))
                {
                    continue;
                }

                if (diffItem.LocalLineStart > PositionToLine(LeftOffset + ActualHeight)
                    && diffItem.RemoteLineStart > PositionToLine(RightOffset + ActualHeight))
                {
                    break;
                }

                yield return diffItem;
            }
        }

        protected override double GetLeftStart(DiffItem diffItem)
        {
            return PositionYLeft(diffItem.LocalLineStart);
        }

        protected override double GetLeftEnd(DiffItem diffItem)
        {
            return PositionYLeft(diffItem.LocalLineStart + diffItem.LocalAffectedLines);
        }

        protected override double GetRightStart(DiffItem diffItem)
        {
            return PositionYRight(diffItem.RemoteLineStart);
        }

        protected override double GetRightEnd(DiffItem diffItem)
        {
            return PositionYRight(diffItem.RemoteLineStart + diffItem.RemoteAffectedLines);
        }
    }
}
