using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Sverge
{
    class TextDiffArea : FrameworkElement, IScrollInfo
    {
        #region IScrollInfo

        private double lineSize = 16;
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
            var newExtent = new Size(createFormattedText(DateTime.Now.ToString("ss MMM ddd d HH:mm yyyy")).Width, 400);

            if (extent != newExtent)
            {
                extent = newExtent;
                TryInvalidateScrollInfo();
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
            //UpdateLayout();
            //InvalidateArrange();
            //InvalidateMeasure();
            //InvalidateVisual();

            base.OnRender(dc);

            dc.DrawLine(new Pen(Brushes.Blue, 2.0),
            new Point(0.0 - offset.X, 0.0 - offset.Y),
            new Point(ActualWidth - offset.X, ActualHeight - offset.Y));
            dc.DrawLine(new Pen(Brushes.Green, 2.0),
                new Point(ActualWidth, 0.0),
                new Point(0.0, ActualHeight));

            dc.DrawText(createFormattedText("Hello world"), new Point(0 - offset.X, 0 - offset.Y));
            dc.DrawText(createFormattedText(",m,m,m,m,m,"), new Point(0 - offset.X, 32 - offset.Y));

            string format = "ss MMM ddd d HH:mm yyyy";
            dc.DrawText(createFormattedText(DateTime.Now.ToString(format)), new Point(0 - offset.X, 2*32 - offset.Y));
        }

        private FormattedText createFormattedText(string text)
        {
            return new FormattedText(
                text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface("Courier New"),
                32,
                Brushes.Black
            );
        }

    }
}
