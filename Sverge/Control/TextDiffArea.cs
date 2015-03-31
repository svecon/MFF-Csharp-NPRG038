using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using DiffAlgorithm.TwoWay;
using DiffIntegration.DiffFilesystemTree;

namespace Sverge.Control
{
    class TextDiffArea : TextAreaAbstract, IScrollInfo
    {
        private readonly DiffFileNode node;
        private readonly FileInfo info;

        private Size extent;
        private Size viewport;

        private Dictionary<int, DiffItem> diffItemsByLine;

        private List<string> lines;
        private int longestLine;

        private bool drawRightBorder;
        private bool drawBottomBorder;

        private bool ShowLineNumbers = true;

        private readonly Pen borderLinePen;

        public delegate void OnVerticalScrollDelegate(double yOffset);

        public OnVerticalScrollDelegate OnVerticalScroll;

        public enum TargetFileEnum { Local, Remote }
        private readonly TargetFileEnum target;

        public TextDiffArea(DiffFileNode fileNode, TargetFileEnum targetFile)
        {
            node = fileNode;
            target = targetFile;
            info = (FileInfo)(target == TargetFileEnum.Local ? fileNode.InfoLocal : fileNode.InfoRemote);

            borderLinePen = new Pen(Brushes.LightGray, BORDER_SIZE);
        }

        private void PreloadFileToMemory()
        {
            if (node.Diff == null)
            {
                lines = new List<string>();
            } else
            {
                lines = (target == TargetFileEnum.Local)
                    ? new List<string>(node.Diff.FilesLineCount.Local)
                    : new List<string>(node.Diff.FilesLineCount.Remote);
            }

            using (StreamReader reader = info.OpenText())
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);

                    if (line.Length > longestLine)
                    {
                        longestLine = line.Length;
                    }
                }
            }

            LinesCount = lines.Count;
        }
        private void PrepareDiffItemsByLines()
        {
            if (diffItemsByLine == null)
            {
                diffItemsByLine = (node.Diff == null)
                ? new Dictionary<int, DiffItem>()
                : new Dictionary<int, DiffItem>(node.Diff.Items.Length);
            }

            if (node.Diff == null)
                return;

            foreach (DiffItem diffItem in node.Diff.Items)
            {
                switch (target)
                {
                    case TargetFileEnum.Local:
                        diffItemsByLine.Add(diffItem.LocalLineStart, diffItem);
                        break;
                    case TargetFileEnum.Remote:
                        diffItemsByLine.Add(diffItem.RemoteLineStart, diffItem);
                        break;
                }

            }
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

        protected override void OnRender(DrawingContext dc)
        {
            //VisualEdgeMode = EdgeMode.Aliased;
            // background
            dc.DrawRectangle(Brushes.White, null, new Rect(new Point(0.0, 0.0), new Size(ActualWidth, ActualHeight)));

            foreach (DiffItem diffItem in VisibleDiffItems())
            {
                int diffStartLine = -1;
                int diffAffectedLines = 0;
                Brush b = Brushes.Black;

                switch (target)
                {
                    case TargetFileEnum.Local:
                        b = new SolidColorBrush(Colors.MediumPurple) { Opacity = .2 };

                        diffStartLine = diffItem.LocalLineStart;
                        diffAffectedLines = diffItem.DeletedInOld;

                        break;
                    case TargetFileEnum.Remote:
                        b = new SolidColorBrush(Colors.MediumVioletRed) { Opacity = .2 };

                        diffStartLine = diffItem.RemoteLineStart;
                        diffAffectedLines = diffItem.InsertedInNew;

                        break;
                }

                if (diffStartLine <= PositionToLine(mouse) && PositionToLine(mouse) < diffStartLine + diffAffectedLines)
                {
                    b.Opacity = 1;
                }

                dc.DrawRectangle(b, null, new Rect(new Point(BORDER_SIZE, PositionY(diffStartLine)), new Size(ActualWidth, LineHeight * diffAffectedLines)));

                DrawHorizontalLine(dc, PositionY(diffStartLine), 0.0, ActualWidth, DiffLinePen);
                DrawHorizontalLine(dc, PositionY(diffStartLine + diffAffectedLines), 0.0, ActualWidth, DiffLinePen);
            }

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

            #region Borders
            DrawHorizontalLine(dc, BORDER_SIZE, 0.0, ActualWidth, borderLinePen); // top
            DrawVerticalLine(dc, 0.0, 0.0, ActualHeight, borderLinePen); // left
            if (drawBottomBorder) DrawHorizontalLine(dc, ActualHeight - BORDER_SIZE, 0.0, ActualWidth, borderLinePen); // bottom
            if (drawRightBorder) DrawVerticalLine(dc, ActualWidth - BORDER_SIZE, 0.0, ActualHeight, borderLinePen); // right

            // border between linenumbers and textarea
            if (ShowLineNumbers) DrawVerticalLine(dc, PositionX(false) + LineNumbersSize + LineNumbersPaddingRight / 2, 0.0, ActualHeight, DiffLinePen);
            #endregion
        }

        protected IEnumerable<DiffItem> VisibleDiffItems()
        {
            if (node.Diff == null)
            {
                return Enumerable.Empty<DiffItem>();
            }

            switch (target)
            {
                case TargetFileEnum.Local:
                    return node.Diff.Items
                            .Where(diffItem => diffItem.LocalLineStart <= EndsOnLine)
                            .Where(diffItem => diffItem.LocalLineStart + diffItem.DeletedInOld >= StartsOnLine);

                case TargetFileEnum.Remote:
                    return node.Diff.Items
                            .Where(diffItem => diffItem.RemoteLineStart <= EndsOnLine)
                            .Where(diffItem => diffItem.RemoteLineStart + diffItem.InsertedInNew >= StartsOnLine);
            }

            return Enumerable.Empty<DiffItem>();
        }
    }
}
