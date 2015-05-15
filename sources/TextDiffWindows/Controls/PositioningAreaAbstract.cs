﻿using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TextDiffWindows.Controls
{
    /// <summary>
    /// An element that handles calculating of vertical positioning for text.
    /// </summary>
    abstract class PositioningAreaAbstract : FrameworkElement
    {
        protected readonly FormattedText Sample;
        protected Pen DiffLinePen;

        /// <summary>
        /// Initializes new instance of the <see cref="PositioningAreaAbstract"/>
        /// </summary>
        protected PositioningAreaAbstract()
        {
            Sample = CreateFormattedText("M");

            DiffLinePen = new Pen(Brushes.LightGray, DIFF_LINE_SIZE);
        }

        #region Computing paddings and offsets

        protected const double BORDER_SIZE = 1.0;
        protected const double DIFF_LINE_SIZE = 1.0;
        protected int LinesCount;
        protected Vector Offset;

        protected double LineHeight { get { return Sample.Height + DIFF_LINE_SIZE; } }
        protected double PaddingTop { get { return BORDER_SIZE + Sample.Height / 4; } }
        protected double PaddingBottom { get { return BORDER_SIZE + Sample.Height / 4; } }
        protected double PositionY(int lineNumber)
        {
            return lineNumber * LineHeight - Offset.Y + PaddingTop;
        }
        public int StartsOnLine
        {
            get
            {
                int temp = (int)((Offset.Y - PaddingTop) / LineHeight);
                return temp < 0 ? 0 : temp;
            }
        }
        public int EndsOnLine
        {
            get
            {
                int temp = StartsOnLine + (int)(ActualHeight / LineHeight) + 1;
                return temp > LinesCount ? LinesCount : temp;
            }
        }

        /// <summary>
        /// Returns line number for given Y position.
        /// 
        /// Inverse function to PositionY.
        /// </summary>
        /// <param name="position">Y position (of a MouseArgs)</param>
        /// <returns>Line number (zero based)</returns>
        protected int PositionToLine(double position)
        {
            return (int)((position - PaddingTop + Offset.Y) / LineHeight);
        }

        protected int PositionToLine(MouseEventArgs mouseE)
        {
            if (mouseE == null)
                return -1;

            return PositionToLine(mouseE.GetPosition(this).Y);
        }

        #endregion

        #region Helper methods
        protected static double AlightRight(double size, double maxSize, double paddingLeft)
        {
            return paddingLeft + maxSize - size;
        }

        protected FormattedText CreateFormattedText(string text)
        {
            return new FormattedText(
                text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface("Consolas"),
                13,
                Brushes.Black
            );
        }

        protected static void DrawHorizontalLine(DrawingContext dc, double y, double x1, double x2, Pen p)
        {
            double thickness = p.Thickness / 2;

            var set = new GuidelineSet(new[] { x1 + thickness, x2 + thickness }, new[] { y });
            dc.PushGuidelineSet(set);

            dc.DrawLine(p, new Point(x1, y + thickness), new Point(x2, y + thickness));

            dc.Pop();
        }

        protected static void DrawVerticalLine(DrawingContext dc, double x, double y1, double y2, Pen p)
        {
            double thickness = p.Thickness / 2;

            var set = new GuidelineSet(new[] { x }, new[] { y1 + thickness, y2 + thickness });
            dc.PushGuidelineSet(set);

            dc.DrawLine(p, new Point(x + thickness, y1), new Point(x + thickness, y2));

            dc.Pop();
        }

        #endregion
    }
}