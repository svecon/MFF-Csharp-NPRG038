using System.Collections.Generic;
using DiffAlgorithm.TwoWay;
using DiffIntegration.DiffFilesystemTree;

namespace Sverge.Control.LineMarkers
{
    class LineMarkersTwoWayElement : LineMarkersElementAbstract<DiffItem>
    {

        public LineMarkersTwoWayElement(DiffFileNode fileNode, TextAreaAbstract leftText, TextAreaAbstract rightText)
            : base(fileNode, leftText, rightText)
        {
        }

        protected override IEnumerable<DiffItem> VisibleDiffItems()
        {
            if (Node.Diff == null)
                yield break;

            foreach (DiffItem diffItem in Node.Diff.Items)
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
