using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DiffAlgorithm.TwoWay;
using DiffIntegration.DiffFilesystemTree;
using Sverge.DiffWindow;

namespace Sverge.Control
{
    class LineMarkersElement : PositioningAreaAbstract
    {
        private readonly DiffFileNode node;
        private readonly TextAreaAbstract local;
        private readonly TextAreaAbstract remote;

        private double localOffset;
        private double remoteOffset;

        private const double DIFF_LENGTH = 7;

        public LineMarkersElement(DiffFileNode fileNode, TextAreaAbstract localText, TextAreaAbstract remoteText)
        {
            node = fileNode;
            local = localText;
            remote = remoteText;

            local.OnVerticalScroll += offset =>
            {
                localOffset = offset;
                InvalidateVisual();
            };

            remote.OnVerticalScroll += offset =>
            {
                remoteOffset = offset;
                InvalidateVisual();
            };
        }

        protected double PositionYLocal(int lineNumber)
        {
            return lineNumber * LineHeight - localOffset + PaddingTop;
        }

        protected double PositionYRemote(int lineNumber)
        {
            return lineNumber * LineHeight - remoteOffset + PaddingTop;
        }

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

        protected override void OnRender(DrawingContext dc)
        {
            //dc.DrawRectangle(Brushes.Red, null, new Rect(new Point(0.0, 0.0), new Size(ActualWidth, ActualHeight)));
            
            foreach (DiffItem diffItem in node.Diff.Items)
            {
                if (diffItem.LocalLineStart + diffItem.DeletedInOld < PositionToLine(localOffset)
                    && diffItem.RemoteLineStart + diffItem.InsertedInNew < PositionToLine(remoteOffset))
                {
                    continue;
                }

                if (diffItem.LocalLineStart > PositionToLine(localOffset + ActualHeight)
                    && diffItem.RemoteLineStart > PositionToLine(remoteOffset + ActualHeight))
                {
                    break;
                }


                double localStart = PositionYLocal(diffItem.LocalLineStart);
                double localEnd = PositionYLocal(diffItem.LocalLineStart + diffItem.DeletedInOld);

                double remoteStart = PositionYRemote(diffItem.RemoteLineStart);
                double remoteEnd = PositionYRemote(diffItem.RemoteLineStart + diffItem.InsertedInNew);

                // local horizontal line
                if (localStart >= 0) DrawHorizontalLine(dc, localStart, 0.0, DIFF_LENGTH, DiffLinePen);
                if (localEnd >= 0) DrawHorizontalLine(dc, localEnd, 0.0, DIFF_LENGTH, DiffLinePen);

                // local vertical line
                if (localEnd >= 0) DrawVerticalLine(dc, DIFF_LENGTH, Math.Max(0, localStart), localEnd, DiffLinePen);

                // remote horizontal line
                if (remoteStart >= 0) DrawHorizontalLine(dc, remoteStart, ActualWidth - DIFF_LENGTH, ActualWidth, DiffLinePen);
                if (remoteEnd >= 0) DrawHorizontalLine(dc, remoteEnd, ActualWidth - DIFF_LENGTH, ActualWidth, DiffLinePen);

                // remote vertical line
                if (remoteEnd >= 0) DrawVerticalLine(dc, ActualWidth - DIFF_LENGTH, Math.Max(0, remoteStart), remoteEnd, DiffLinePen);


                // joining line
                double ly = (localStart + localEnd) / 2;
                double ry = (remoteStart + remoteEnd) / 2;

                if (ly < 0 && ry < 0)
                    continue;

                double lx = DIFF_LENGTH;
                double rx = ActualWidth - DIFF_LENGTH;

                double rh = Math.Abs(Math.Min(0, ry));
                double lh = Math.Abs(Math.Min(0, ly));

                rx -= (rx - lx) * rh / Math.Abs(ly - ry);
                lx += (rx - lx) * lh / Math.Abs(ly - ry);

                dc.DrawLine(DiffLinePen, new Point(lx, Math.Max(0, ly)), new Point(rx, Math.Max(0, ry)));
            }
        }
    }
}
