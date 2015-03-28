using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Sverge
{
    class TextDiffArea : FrameworkElement, IScrollInfo
    {
        private FileInfo info;
        private string[] lines;
        private int longestLine;

        private bool ShowLineNumbers = true;

        #region Computing paddings and offsets

        private readonly FormattedText sample;
        private const int CHAR_PADDING_RIGHT = 5;
        private const double BORDER_SIZE = 1.0;
        private const double DIFF_LINE_SIZE = 1.0;

        double LineHeight { get { return sample.Height + DIFF_LINE_SIZE; } }

        double PaddingTop { get { return BORDER_SIZE + sample.Height / 4; } }
        double PaddingBottom { get { return BORDER_SIZE + sample.Height / 4; } }
        double PaddingRight { get { return CHAR_PADDING_RIGHT * sample.Width; } }
        double PaddingLeft { get { return BORDER_SIZE + sample.Width / 1; } }
        double LineNumbersPaddingRight { get { return sample.Width * 2; } }

        double LineNumbersSize { get { return lines.Length.ToString().Length * sample.Width; } }

        public TextDiffArea()
        {
            info = new FileInfo("C:/csharp/Merge/Sverge/TextDiffArea.cs");

            sample = CreateFormattedText("M");

            var linesList = new List<string>();
            using (StreamReader reader = info.OpenText())
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    linesList.Add(line);

                    if (line.Length > longestLine)
                    {
                        longestLine = line.Length;
                    }
                }
            }
            lines = linesList.ToArray();
        }

        double PositionY(int lineNumber)
        {
            return lineNumber * LineHeight - offset.Y + PaddingTop;
        }

        double PositionX(bool includeLineNumbers = true)
        {
            double result = PaddingLeft - offset.X;

            if (includeLineNumbers && ShowLineNumbers)
                result += LineNumbersSize + LineNumbersPaddingRight;

            return result;
        }

        #endregion

        #region Helper methods
        private static double AlightRight(double size, double maxSize, double paddingLeft)
        {
            return paddingLeft + maxSize - size;
        }

        private FormattedText CreateFormattedText(string text)
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

        #endregion

        #region IScrollInfo

        private double WheelSize { get { return 3 * LineHeight; } }

        private Vector offset;
        private Size extent;
        private Size viewport;

        public ScrollViewer ScrollOwner { get; set; }
        public bool CanHorizontallyScroll { get; set; }
        public bool CanVerticallyScroll { get; set; }

        public double ExtentHeight
        { get { return extent.Height; } }

        public double ExtentWidth
        { get { return extent.Width; } }

        public double HorizontalOffset
        { get { return offset.X; } }

        public double VerticalOffset
        { get { return offset.Y; } }

        public double ViewportHeight
        { get { return viewport.Height; } }

        public double ViewportWidth
        { get { return viewport.Width; } }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            // We do not have any children in this Element.
            return new Rect();
        }

        #region Movement Methods
        public void LineDown()
        { SetVerticalOffset(VerticalOffset + LineHeight); }

        public void LineUp()
        { SetVerticalOffset(VerticalOffset - LineHeight); }

        public void LineLeft()
        { SetHorizontalOffset(HorizontalOffset - LineHeight); }

        public void LineRight()
        { SetHorizontalOffset(HorizontalOffset + LineHeight); }

        public void MouseWheelDown()
        { SetVerticalOffset(VerticalOffset + WheelSize); }

        public void MouseWheelUp()
        { SetVerticalOffset(VerticalOffset - WheelSize); }

        public void MouseWheelLeft()
        { SetHorizontalOffset(HorizontalOffset - WheelSize); }

        public void MouseWheelRight()
        { SetHorizontalOffset(HorizontalOffset + WheelSize); }

        public void PageDown()
        { SetVerticalOffset(VerticalOffset + ViewportHeight); }

        public void PageUp()
        { SetVerticalOffset(VerticalOffset - ViewportHeight); }

        public void PageLeft()
        { SetHorizontalOffset(HorizontalOffset - ViewportWidth); }

        public void PageRight()
        { SetHorizontalOffset(HorizontalOffset + ViewportWidth); }
        #endregion

        private void CheckOffeset(ref double newOffset, double viewportSize, double extentSize)
        {
            if (newOffset < 0 || viewportSize >= extentSize)
            {
                newOffset = 0;
            } else if (newOffset + viewportSize >= extentSize)
            {
                newOffset = extentSize - viewportSize;
            }

            TryInvalidateScrollInfo();
        }

        public void SetHorizontalOffset(double newOffset)
        {
            CheckOffeset(ref newOffset, viewport.Width, extent.Width);

            if (!(Math.Abs(newOffset - offset.X) > 0)) return;

            offset.X = newOffset;
            //InvalidateArrange();
            InvalidateVisual();
        }

        public void SetVerticalOffset(double newOffset)
        {
            CheckOffeset(ref newOffset, viewport.Height, extent.Height);

            if (!(Math.Abs(newOffset - offset.Y) > 0)) return;

            offset.Y = newOffset;
            //InvalidateArrange();
            InvalidateVisual();
        }

        private void TryInvalidateScrollInfo()
        {
            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        #endregion

        #region Mouse events

        private MouseEventArgs mouse;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            mouse = e;
            InvalidateVisual();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            mouse = null;
            InvalidateVisual();
        }

        #endregion

        #region Measure & Arrange Override

        private Size CalculateSize()
        {
            double width = sample.Width * longestLine + PaddingLeft + PaddingRight;
            double height = lines.Length * LineHeight + PaddingTop + PaddingBottom;

            if (ShowLineNumbers)
                width += LineNumbersSize + LineNumbersPaddingRight;

            return new Size(width, height);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size newExtent = CalculateSize();

            if (extent != newExtent)
            {
                extent = newExtent;
                TryInvalidateScrollInfo();
            }

            if (double.IsPositiveInfinity(availableSize.Width))
            {
                availableSize.Width = extent.Width;
            }
            if (double.IsPositiveInfinity(availableSize.Height))
            {
                availableSize.Height = extent.Height;
            }

            if (availableSize != viewport)
            {
                viewport = availableSize;
                TryInvalidateScrollInfo();
            }

            return viewport;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // basically do nothing -- we are inside a scrollviewer
            // let MeasureOverride do the job
            Size newExtent = extent;

            if (newExtent != extent)
            {
                extent = newExtent;
                TryInvalidateScrollInfo();
            }

            if (finalSize != viewport)
            {
                viewport = finalSize;
                TryInvalidateScrollInfo();
            }

            return finalSize;
        }

        #endregion

        protected override void OnRender(DrawingContext dc)
        {
            int startsOnLine = (int)(offset.Y / LineHeight);
            int endsOnLine = startsOnLine + (int)(viewport.Height / LineHeight) + 1;

            if (startsOnLine < 0) { startsOnLine = 0; }
            if (endsOnLine > lines.Length) { endsOnLine = lines.Length; }

            // background
            dc.DrawRectangle(Brushes.White, null, new Rect(new Point(BORDER_SIZE, BORDER_SIZE), new Size(extent.Width, extent.Height)));

            for (int i = startsOnLine; i < endsOnLine; i++)
            {
                if (mouse != null && mouse.GetPosition(this).Y > PositionY(i)
                    && mouse.GetPosition(this).Y < PositionY(i + 1))
                {
                    dc.DrawRectangle(Brushes.SeaShell, null, new Rect(new Point(BORDER_SIZE, PositionY(i)), new Size(extent.Width, LineHeight)));
                }

                // print line numbers
                if (ShowLineNumbers)
                {
                    // actual line numbers text
                    FormattedText lineNumber = CreateFormattedText((i + 1).ToString());
                    dc.DrawText(lineNumber, new Point(AlightRight(lineNumber.Width, LineNumbersSize, PositionX(false)), PositionY(i)));
                }

                // print text
                FormattedText oneLine = CreateFormattedText(lines[i]);
                dc.DrawText(oneLine, new Point(PositionX(), PositionY(i)));
            }

            // border
            dc.DrawRectangle(Brushes.LightGray, null, new Rect(new Point(0.0, 0.0), new Size(extent.Width, BORDER_SIZE)));
            dc.DrawRectangle(Brushes.LightGray, null, new Rect(new Point(0.0, 0.0), new Size(BORDER_SIZE, extent.Height)));

            if (ShowLineNumbers)
            { // border between linenumbers and textarea
                dc.DrawRectangle(Brushes.LightGray, null, new Rect(new Point(PositionX(false) + LineNumbersSize + LineNumbersPaddingRight / 2, 0.0), new Size(BORDER_SIZE, extent.Height)));
            }
        }
    }
}
