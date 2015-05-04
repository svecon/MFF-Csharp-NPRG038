﻿using System.Collections.Generic;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemDiffTree;
using TextDiffAlgorithm.ThreeWay;

namespace TextDiffWindows.Controls.LineMarkers
{
    class LineMarkersThreeWayElement : LineMarkersElementAbstract<Diff3Item>
    {

        public enum MarkerTypeEnum { BaseLeft, BaseRight }

        private readonly MarkerTypeEnum type;

        public LineMarkersThreeWayElement(FileDiffNode node, TextAreaAbstract leftText, TextAreaAbstract rightText, MarkerTypeEnum markerType)
            : base(node, leftText, rightText)
        {
            type = markerType;
        }

        protected override IEnumerable<Diff3Item> VisibleDiffItems()
        {
            if (Node.Diff == null)
                yield break;

            if (type == MarkerTypeEnum.BaseLeft && !Node.IsInLocation(LocationCombinationsEnum.OnBaseLocal))
                yield break;

            if (type == MarkerTypeEnum.BaseRight && !Node.IsInLocation(LocationCombinationsEnum.OnBaseRemote))
                yield break;

            foreach (Diff3Item diffItem in ((Diff3)Node.Diff).Items)
            {
                if (type == MarkerTypeEnum.BaseLeft)
                {
                    // both are behind
                    if (diffItem.LocalLineStart + diffItem.LocalAffectedLines < PositionToLine(LeftOffset)
                        && diffItem.BaseLineStart + diffItem.BaseAffectedLines < PositionToLine(RightOffset))
                    {
                        continue;
                    }

                    // both are ahead
                    if (diffItem.LocalLineStart > PositionToLine(LeftOffset + ActualHeight)
                        && diffItem.BaseLineStart > PositionToLine(RightOffset + ActualHeight))
                    {
                        yield break;
                    }
                } else
                { // MarkerTypeEnum.BaseRight

                    // both are behind
                    if (diffItem.BaseLineStart + diffItem.BaseAffectedLines < PositionToLine(LeftOffset)
                        && diffItem.RemoteLineStart + diffItem.RemoteAffectedLines < PositionToLine(RightOffset))
                    {
                        continue;
                    }

                    // both are ahead
                    if (diffItem.BaseLineStart > PositionToLine(LeftOffset + ActualHeight)
                        && diffItem.RemoteLineStart > PositionToLine(RightOffset + ActualHeight))
                    {
                        yield break;
                    }
                }

                yield return diffItem;
            }
        }

        protected override double GetLeftStart(Diff3Item diffItem)
        {
            return type == MarkerTypeEnum.BaseLeft
            ? PositionYLeft(diffItem.LocalLineStart)
            : PositionYLeft(diffItem.BaseLineStart);
        }

        protected override double GetLeftEnd(Diff3Item diffItem)
        {
            return type == MarkerTypeEnum.BaseLeft
            ? PositionYLeft(diffItem.LocalLineStart + diffItem.LocalAffectedLines)
            : PositionYLeft(diffItem.BaseLineStart + diffItem.BaseAffectedLines);
        }

        protected override double GetRightStart(Diff3Item diffItem)
        {
            return type == MarkerTypeEnum.BaseLeft
            ? PositionYRight(diffItem.BaseLineStart)
            : PositionYRight(diffItem.RemoteLineStart);
        }

        protected override double GetRightEnd(Diff3Item diffItem)
        {
            return type == MarkerTypeEnum.BaseLeft
            ? PositionYRight(diffItem.BaseLineStart + diffItem.BaseAffectedLines)
            : PositionYRight(diffItem.RemoteLineStart + diffItem.RemoteAffectedLines);
        }
    }
}