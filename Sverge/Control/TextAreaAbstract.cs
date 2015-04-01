using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DiffIntegration.DiffFilesystemTree;

namespace Sverge.Control
{
    abstract class TextAreaAbstract : PositioningAreaAbstract
    {

        public bool ShowLineNumbers = true;

        protected readonly DiffFileNode node;
        protected FileInfo info;

        protected Size extent;
        protected Size viewport;

        public delegate void OnVerticalScrollDelegate(double yOffset); // TODO routed event (object sender, XXX (potomek) : RoutedEventArgs)
        public OnVerticalScrollDelegate OnVerticalScroll;

        protected bool drawRightBorder;
        protected bool drawBottomBorder;

        protected readonly Pen borderLinePen;

        protected List<string> lines;
        protected int longestLine;

        protected TextAreaAbstract(DiffFileNode fileNode)
        {
            node = fileNode;

            borderLinePen = new Pen(Brushes.LightGray, BORDER_SIZE);
        }

        #region Horizontal calculations
        protected double PositionX(bool includeLineNumbers = true)
        {
            double result = PaddingLeft - Offset.X;

            if (includeLineNumbers && ShowLineNumbers)
                result += LineNumbersSize + LineNumbersPaddingRight;

            return result;
        }
        protected double PaddingRight { get { return CHAR_PADDING_RIGHT * Sample.Width; } }
        protected double PaddingLeft { get { return BORDER_SIZE + Sample.Width / 1; } }
        protected double LineNumbersPaddingRight { get { return Sample.Width * 2; } }
        protected double LineNumbersSize { get { return LinesCount.ToString().Length * Sample.Width; } }
        #endregion


        #region IScrollInfo

        private double WheelSize { get { return 3 * LineHeight; } }

        public ScrollViewer ScrollOwner { get; set; }
        public bool CanHorizontallyScroll { get; set; }
        public bool CanVerticallyScroll { get; set; }

        public double ExtentHeight
        { get { return extent.Height; } }

        public double ExtentWidth
        { get { return extent.Width; } }

        public double HorizontalOffset
        { get { return Offset.X; } }

        public double VerticalOffset
        { get { return Offset.Y; } }

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
            CheckOffeset(ref newOffset, ViewportWidth, ExtentWidth);

            if (!(Math.Abs(newOffset - Offset.X) > 0)) return;

            Offset.X = newOffset;
            InvalidateVisual();
        }

        public void SetVerticalOffset(double newOffset)
        {
            CheckOffeset(ref newOffset, ViewportHeight, ExtentHeight);

            if (!(Math.Abs(newOffset - Offset.Y) > 0)) return;

            Offset.Y = newOffset;

            if (OnVerticalScroll != null) OnVerticalScroll(newOffset);

            InvalidateVisual();
        }

        protected void TryInvalidateScrollInfo()
        {
            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        #endregion

        #region Mouse events

        protected MouseEventArgs mouse;
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


        protected abstract void PreloadFileToMemory();

        protected abstract bool IsDiffAvailable();

        #region Measure & Arrange Override

        private Size CalculateSize()
        {
            double width = Sample.Width * longestLine + PaddingLeft + PaddingRight;
            double height = lines.Count * LineHeight + PaddingTop + PaddingBottom;

            if (ShowLineNumbers)
                width += LineNumbersSize + LineNumbersPaddingRight;

            return new Size(width, height);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (lines == null)
            {
                PreloadFileToMemory();
            }

            Size newExtent = CalculateSize();

            if (newExtent.Height < availableSize.Height)
            {
                drawRightBorder = true; // there is no scrollbar
                SetVerticalOffset(0); // when user is scrolled and then resizes window
                newExtent.Height = availableSize.Height;
            } else
            { drawRightBorder = false; }
            if (newExtent.Width < availableSize.Width)
            {
                drawBottomBorder = true; // there is no scrollbar
                SetHorizontalOffset(0); // when user is scrolled and then resizes window
                newExtent.Width = availableSize.Width;
            } else
            { drawBottomBorder = false; }

            if (extent != newExtent)
            {
                extent = newExtent;
                TryInvalidateScrollInfo();
            }

            if (double.IsPositiveInfinity(availableSize.Width))
            {
                availableSize.Width = ExtentWidth;
            }
            if (double.IsPositiveInfinity(availableSize.Height))
            {
                availableSize.Height = ExtentHeight;
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

        #region Rendering

        protected override void OnRender(DrawingContext dc)
        {
            DrawBackground(dc);
            DrawText(dc);
            DrawBorders(dc);
        }

        protected virtual void DrawBackground(DrawingContext dc)
        {
            // background
            dc.DrawRectangle(Brushes.White, null, new Rect(new Point(0.0, 0.0), new Size(ActualWidth, ActualHeight)));
        }

        protected virtual void DrawText(DrawingContext dc)
        {
            for (int i = StartsOnLine; i < EndsOnLine; i++)
            {
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
        }

        protected virtual void DrawBorders(DrawingContext dc)
        {
            DrawHorizontalLine(dc, BORDER_SIZE, 0.0, ActualWidth, borderLinePen); // top
            DrawVerticalLine(dc, 0.0, 0.0, ActualHeight, borderLinePen); // left
            if (drawBottomBorder) DrawHorizontalLine(dc, ActualHeight - BORDER_SIZE, 0.0, ActualWidth, borderLinePen); // bottom
            if (drawRightBorder) DrawVerticalLine(dc, ActualWidth - BORDER_SIZE, 0.0, ActualHeight, borderLinePen); // right

            // border between linenumbers and textarea
            if (ShowLineNumbers) DrawVerticalLine(dc, PositionX(false) + LineNumbersSize + LineNumbersPaddingRight / 2, 0.0, ActualHeight, DiffLinePen);
        }

        #endregion

    }
}
