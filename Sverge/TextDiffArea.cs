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
        private const int CHAR_PADDING_RIGHT = 5;
        private FormattedText sample;
        private MouseEventArgs mouse;

        public TextDiffArea()
        {
            info = new FileInfo("C:/csharp/Merge/Sverge/TextDiffArea.cs");

            sample = createFormattedText("M");
            lineSize = sample.Height;

            var linesList = new List<string>();
            using (StreamReader reader = info.OpenText())
            {
                string line;
                while((line = reader.ReadLine()) != null)
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

        #region IScrollInfo

        private readonly double lineSize;
        private double WheelSize { get { return 3 * lineSize; } }

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
        { SetVerticalOffset(VerticalOffset + lineSize); }

        public void LineUp()
        { SetVerticalOffset(VerticalOffset - lineSize); }

        public void LineLeft()
        { SetHorizontalOffset(HorizontalOffset - lineSize); }

        public void LineRight()
        { SetHorizontalOffset(HorizontalOffset + lineSize); }

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

        protected override Size MeasureOverride(Size availableSize)
        {
            var newExtent = new Size(sample.Width * (longestLine + CHAR_PADDING_RIGHT), lines.Length * sample.Height);

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

        protected override void OnRender(DrawingContext dc)
        {
            int startsOnLine = (int) (offset.Y/sample.Height);
            int endsOnLine = startsOnLine + (int) (viewport.Height / sample.Height) + 1;

            if (startsOnLine < 0)
            {
                startsOnLine = 0;
            }

            if (endsOnLine > lines.Length)
            {
                endsOnLine = lines.Length;
            }

            double paddingTop = 1.0;
            double paddingLeft = 1.0;

            // background
            dc.DrawRectangle(Brushes.Gray, null, new Rect(new Point(0.0, 0.0), new Size(extent.Width, extent.Height)));
            dc.DrawRectangle(Brushes.White, null, new Rect(new Point(paddingTop, paddingLeft), new Size(extent.Width, extent.Height)));

            for (int i = startsOnLine; i < endsOnLine; i++)
            {
                paddingTop = 1.0;
                paddingLeft = 1.0;

                if (mouse != null && mouse.GetPosition(this).Y > i * sample.Height - offset.Y
                    && mouse.GetPosition(this).Y < (i+1) * sample.Height - offset.Y)
                {
                    dc.DrawRectangle(Brushes.SeaShell, null, new Rect(new Point(0 - offset.X + paddingLeft, i * sample.Height - offset.Y + paddingTop), new Size(extent.Width, sample.Height)));
                }

                paddingLeft = 1 + (1) * sample.Width;

                // print line numbers
                var lineNumber = createFormattedText((i + 1).ToString());
                dc.DrawText(lineNumber, new Point(alightRight(lineNumber.Width, lines.Length.ToString().Length * sample.Width, 0 - offset.X + paddingLeft), i * sample.Height - offset.Y + paddingTop));

                paddingLeft = (1 + lines.Length.ToString().Length + 1) * sample.Width;

                // print text
                FormattedText oneLine = createFormattedText(lines[i]);
                dc.DrawText(oneLine, new Point(0 - offset.X + paddingLeft, i * oneLine.Height - offset.Y + paddingTop));
            }
        }

        private double alightRight(double size, double maxSize, double paddingLeft)
        {
            return paddingLeft + maxSize - size;
        }

        private FormattedText createFormattedText(string text)
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
    }
}
