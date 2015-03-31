using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using DiffAlgorithm.TwoWay;
using DiffIntegration.DiffFilesystemTree;

namespace Sverge.Control
{
    class TextAreaAbstract : FrameworkElement
    {

        protected readonly FormattedText Sample;
        protected Pen DiffLinePen;

        public TextAreaAbstract()
        {
            Sample = CreateFormattedText("M");

            DiffLinePen = new Pen(Brushes.LightGray, DIFF_LINE_SIZE);
        }

        #region Computing paddings and offsets

        protected const int CHAR_PADDING_RIGHT = 5;
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
        protected int StartsOnLine
        {
            get
            {
                int temp = (int)(Offset.Y / LineHeight);
                return temp < 0 ? 0 : temp;
            }
        }
        protected int EndsOnLine
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
        /// <param name="position">Y position (of a mouse)</param>
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

            var set = new GuidelineSet(new[] { x1 + thickness, x2 + thickness }, new[] { y + thickness });
            dc.PushGuidelineSet(set);

            dc.DrawLine(p, new Point(x1, y), new Point(x2, y));

            dc.Pop();
        }

        protected static void DrawVerticalLine(DrawingContext dc, double x, double y1, double y2, Pen p)
        {
            double thickness = p.Thickness / 2;

            var set = new GuidelineSet(new[] { x + thickness }, new[] { y1 + thickness, y2 + thickness });
            dc.PushGuidelineSet(set);

            dc.DrawLine(p, new Point(x, y1), new Point(x, y2));

            dc.Pop();
        }

        #endregion
    }
}
