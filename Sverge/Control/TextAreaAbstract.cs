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

        protected readonly DiffFileNode Node;
        protected FileInfo Info;

        protected Size Extent;
        protected Size Viewport;

        public delegate void OnVerticalScrollDelegate(double yOffset); // TODO routed event (object sender, XXX (potomek) : RoutedEventArgs)
        public OnVerticalScrollDelegate OnVerticalScroll;

        protected bool DrawRightBorder;
        protected bool DrawBottomBorder;

        protected readonly Pen BackgroundPen;
        protected readonly Pen BorderLinePen;

        protected List<string> Lines;
        protected int LongestLine;

        protected TextAreaAbstract(DiffFileNode fileNode)
        {
            Node = fileNode;

            BackgroundPen = new Pen(Brushes.White, BORDER_SIZE);
            BorderLinePen = new Pen(Brushes.LightGray, BORDER_SIZE);
        }

        #region Horizontal calculations
        protected double PositionX(bool includeLineNumbers = true)
        {
            double result = PaddingLeft - Offset.X;

            if (includeLineNumbers && ShowLineNumbers)
                result += LineNumbersSize + LineNumbersPaddingRight;

            return result;
        }
        protected double PaddingRight { get { return Sample.Width; } }
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
        { get { return Extent.Height; } }

        public double ExtentWidth
        { get { return Extent.Width; } }

        public double HorizontalOffset
        { get { return Offset.X; } }

        public double VerticalOffset
        { get { return Offset.Y; } }

        public double ViewportHeight
        { get { return Viewport.Height; } }

        public double ViewportWidth
        { get { return Viewport.Width; } }

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

        protected MouseEventArgs MouseArgs;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            MouseArgs = e;
            InvalidateVisual();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            MouseArgs = null;
            InvalidateVisual();
        }

        #endregion


        protected abstract void PreloadFileToMemory();

        protected abstract bool IsDiffAvailable();

        #region Measure & Arrange Override

        private Size CalculateSize()
        {
            double width = Sample.Width * LongestLine + PaddingLeft + PaddingRight;
            double height = Lines.Count * LineHeight + PaddingTop + PaddingBottom;

            if (ShowLineNumbers)
                width += LineNumbersSize + LineNumbersPaddingRight;

            return new Size(width, height);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Lines == null)
            {
                PreloadFileToMemory();
            }

            Size newExtent = CalculateSize();

            if (newExtent.Height < availableSize.Height)
            {
                DrawRightBorder = true; // there is no scrollbar
                SetVerticalOffset(0); // when user is scrolled and then resizes window
                newExtent.Height = availableSize.Height;
            } else
            { DrawRightBorder = false; }
            if (newExtent.Width < availableSize.Width)
            {
                DrawBottomBorder = true; // there is no scrollbar
                SetHorizontalOffset(0); // when user is scrolled and then resizes window
                newExtent.Width = availableSize.Width;
            } else
            { DrawBottomBorder = false; }

            if (Extent != newExtent)
            {
                Extent = newExtent;
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

            if (availableSize != Viewport)
            {
                Viewport = availableSize;
                TryInvalidateScrollInfo();
            }

            return Viewport;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // basically do nothing -- we are inside a scrollviewer
            // let MeasureOverride do the job
            Size newExtent = Extent;

            if (newExtent != Extent)
            {
                Extent = newExtent;
                TryInvalidateScrollInfo();
            }

            if (finalSize != Viewport)
            {
                Viewport = finalSize;
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
                FormattedText oneLine = CreateFormattedText(Lines[i]);
                dc.DrawText(oneLine, new Point(PositionX(), PositionY(i)));
            }
        }

        protected virtual void DrawBorders(DrawingContext dc)
        {
            DrawHorizontalLine(dc, BORDER_SIZE, 0.0, ActualWidth, BorderLinePen); // top
            DrawVerticalLine(dc, 0.0, 0.0, ActualHeight, BackgroundPen); // overpaint subpixel text overflow
            DrawVerticalLine(dc, BORDER_SIZE, 0.0, ActualHeight, BorderLinePen); // left

            if (DrawBottomBorder) DrawHorizontalLine(dc, ActualHeight, 0.0, ActualWidth, BorderLinePen); // bottom
            if (DrawRightBorder)
            {
                DrawVerticalLine(dc, ActualWidth, 0.0, ActualHeight, BackgroundPen); // overpaint subpixel text overflow
                DrawVerticalLine(dc, ActualWidth - BORDER_SIZE, 0.0, ActualHeight, BorderLinePen); // right
            }

            // border between linenumbers and textarea
            if (ShowLineNumbers) DrawVerticalLine(dc, PositionX(false) + LineNumbersSize + LineNumbersPaddingRight / 2, 0.0, ActualHeight, DiffLinePen);
        }

        #endregion

    }
}
