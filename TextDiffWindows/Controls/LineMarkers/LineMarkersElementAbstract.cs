using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using CoreLibrary.FilesystemDiffTree;

namespace TextDiffWindows.Controls.LineMarkers
{
    abstract class LineMarkersElementAbstract<TU> : PositioningAreaAbstract
    {
        protected readonly FileDiffNode Node;
        protected readonly TextAreaAbstract Left;
        protected readonly TextAreaAbstract Right;

        protected double LeftOffset;
        protected double RightOffset;

        protected const double HORIZONTAL_DIFF_LINE_LENGTH = 7;

        protected LineMarkersElementAbstract(FileDiffNode node, TextAreaAbstract leftText, TextAreaAbstract rightText)
        {
            Node = node;
            Left = leftText;
            Right = rightText;

            Left.OnVerticalScroll += offset =>
            {
                LeftOffset = offset;
                InvalidateVisual();
            };

            Right.OnVerticalScroll += offset =>
            {
                RightOffset = offset;
                InvalidateVisual();
            };
        }

        #region Arrange and Measure

        protected override Size MeasureOverride(Size availableSize)
        {
            if (double.IsPositiveInfinity(availableSize.Width))
                availableSize.Width = 1;
            if (double.IsPositiveInfinity(availableSize.Height))
                availableSize.Height = 1;

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }

        #endregion

        #region Diff Position

        protected double PositionYLeft(int lineNumber)
        {
            return lineNumber * LineHeight - LeftOffset + PaddingTop;
        }

        protected double PositionYRight(int lineNumber)
        {
            return lineNumber * LineHeight - RightOffset + PaddingTop;
        }

        protected abstract double GetLeftStart(TU diffItem);

        protected abstract double GetLeftEnd(TU diffItem);

        protected abstract double GetRightStart(TU diffItem);

        protected abstract double GetRightEnd(TU diffItem);

        #endregion

        #region Rendering

        protected override void OnRender(DrawingContext dc)
        {
            foreach (TU diffItem in VisibleDiffItems())
            {
                DrawDiffLines(
                    dc,
                    GetLeftStart(diffItem),
                    GetLeftEnd(diffItem),
                    GetRightStart(diffItem),
                    GetRightEnd(diffItem)
                );
            }
        }

        protected abstract IEnumerable<TU> VisibleDiffItems();

        protected virtual void DrawDiffLines(DrawingContext dc, double localStart, double localEnd, double remoteStart, double remoteEnd)
        {
            // local horizontal line
            if (localStart >= 0) DrawHorizontalLine(dc, localStart, 0.0, HORIZONTAL_DIFF_LINE_LENGTH, DiffLinePen);
            if (localEnd >= 0) DrawHorizontalLine(dc, localEnd, 0.0, HORIZONTAL_DIFF_LINE_LENGTH, DiffLinePen);

            // local vertical line
            if (localEnd >= 0) DrawVerticalLine(dc, HORIZONTAL_DIFF_LINE_LENGTH, Math.Max(0, localStart), localEnd, DiffLinePen);

            // Right horizontal line
            if (remoteStart >= 0) DrawHorizontalLine(dc, remoteStart, ActualWidth - HORIZONTAL_DIFF_LINE_LENGTH, ActualWidth, DiffLinePen);
            if (remoteEnd >= 0) DrawHorizontalLine(dc, remoteEnd, ActualWidth - HORIZONTAL_DIFF_LINE_LENGTH, ActualWidth, DiffLinePen);

            // Right vertical line
            if (remoteEnd >= 0) DrawVerticalLine(dc, ActualWidth - HORIZONTAL_DIFF_LINE_LENGTH, Math.Max(0, remoteStart), remoteEnd, DiffLinePen);


            // joining line
            double ly = (localStart + localEnd) / 2;
            double ry = (remoteStart + remoteEnd) / 2;

            if (ly < 0 && ry < 0)
                return;

            double lx = HORIZONTAL_DIFF_LINE_LENGTH;
            double rx = ActualWidth - HORIZONTAL_DIFF_LINE_LENGTH;

            if (Math.Abs(ly - ry) > 0.05)
            {
                double rh = Math.Abs(Math.Min(0, ry));
                double lh = Math.Abs(Math.Min(0, ly));

                rx -= (rx - lx) * rh / Math.Abs(ly - ry);
                lx += (rx - lx) * lh / Math.Abs(ly - ry);
            }


            dc.DrawLine(DiffLinePen, new Point(lx, Math.Max(0, ly)), new Point(rx, Math.Max(0, ry) + 1));
        }

        #endregion
    }
}
