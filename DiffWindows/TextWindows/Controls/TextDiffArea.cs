using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CoreLibrary.Enums;
using DiffAlgorithm.ThreeWay;
using DiffAlgorithm.TwoWay;
using DiffIntegration.DiffFilesystemTree;

namespace DiffWindows.TextWindows.Controls
{
    class TextDiffArea : TextAreaAbstract, IScrollInfo
    {
        public delegate void OnDiffChangeDelegate(); // TODO routed event (object sender, XXX (potomek) : RoutedEventArgs)
        public OnDiffChangeDelegate OnDiffChange;

        public delegate void OnDiffSelectedDelegate(int selected); // TODO routed event (object sender, XXX (potomek) : RoutedEventArgs)
        public OnDiffSelectedDelegate OnDiffSelected;

        public enum TargetFileEnum { Local, Remote }
        private readonly TargetFileEnum target;

        public TextDiffArea(DiffFileNode fileNode, TargetFileEnum targetFile)
            : base(fileNode)
        {
            target = targetFile;
            Info = (FileInfo)(target == TargetFileEnum.Local ? fileNode.InfoLocal : fileNode.InfoRemote);
        }

        protected override bool IsDiffAvailable()
        {
            return Node.Diff != null;
        }

        protected override void PreloadFileToMemory()
        {
            if (!IsDiffAvailable())
            {
                Lines = new List<string>();
            } else
            {
                Lines = (target == TargetFileEnum.Local)
                    ? new List<string>(Node.Diff.FilesLineCount.Local)
                    : new List<string>(Node.Diff.FilesLineCount.Remote);
            }

            using (StreamReader reader = Info.OpenText())
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Lines.Add(line);

                    if (line.Length > LongestLine)
                    {
                        LongestLine = line.Length;
                    }
                }
            }

            LinesCount = Lines.Count;
        }

        #region Rendering

        protected override void OnRender(DrawingContext dc)
        {
            if (Lines == null && Info != null)
            {
                PreloadFileToMemory();
            }

            DrawBackground(dc);
            DrawDiffs(dc);
            DrawText(dc);
            DrawBorders(dc);
        }

        protected override void DrawText(DrawingContext dc)
        {
            for (int i = StartsOnLine; i < EndsOnLine; i++)
            {
                if (!ShowLineNumbers) continue;

                // actual line numbers text
                FormattedText lineNumber = CreateFormattedText((i + 1).ToString());
                dc.DrawText(lineNumber, new Point(AlightRight(lineNumber.Width, LineNumbersSize, PositionX(false)), PositionY(i)));
            }
        }

        private void DrawDiffs(DrawingContext dc)
        {
            Cursor = null;

            int textLineToBePrinted = StartsOnLine;

            foreach (DiffItem diffItem in VisibleDiffItems())
            {
                int diffStartLine = -1;
                int diffAffectedLines = 0;
                bool textDiscarded = false;
                bool diffHovered = false;
                Color diffColor = Colors.MediumPurple;
                Brush diffBackgroundBrush = Brushes.Black;

                switch (target)
                {
                    case TargetFileEnum.Local:
                        if (diffItem.RemoteAffectedLines == 0)
                        {
                            diffColor = Colors.LimeGreen;
                        }

                        if (diffItem.PreferedAction != PreferedActionTwoWayEnum.Default &&
                                   diffItem.PreferedAction != PreferedActionTwoWayEnum.ApplyLocal)
                        {
                            textDiscarded = true;
                        }

                        diffStartLine = diffItem.LocalLineStart;
                        diffAffectedLines = diffItem.LocalAffectedLines;

                        break;
                    case TargetFileEnum.Remote:
                        if (diffItem.LocalAffectedLines == 0)
                        {
                            diffColor = Colors.LimeGreen;
                        }

                        if (diffItem.PreferedAction != PreferedActionTwoWayEnum.Default &&
                                   diffItem.PreferedAction != PreferedActionTwoWayEnum.ApplyRemote)
                        {
                            textDiscarded = true;
                        }

                        diffStartLine = diffItem.RemoteLineStart;
                        diffAffectedLines = diffItem.RemoteAffectedLines;

                        break;
                }

                if (textDiscarded)
                {
                    diffColor = Colors.LightSlateGray;
                }

                diffBackgroundBrush = new SolidColorBrush(diffColor) { Opacity = .33 };

                if (diffStartLine <= PositionToLine(MouseArgs) && PositionToLine(MouseArgs) < diffStartLine + diffAffectedLines)
                {
                    diffHovered = true;
                    diffBackgroundBrush.Opacity = 1;
                    Cursor = Cursors.Hand;
                }

                dc.DrawRectangle(diffBackgroundBrush, null, new Rect(new Point(BORDER_SIZE, PositionY(diffStartLine)), new Size(ActualWidth, LineHeight * diffAffectedLines)));

                DrawHorizontalLine(dc, PositionY(diffStartLine), 0.0, ActualWidth, DiffLinePen);
                DrawHorizontalLine(dc, PositionY(diffStartLine + diffAffectedLines), 0.0, ActualWidth, DiffLinePen);

                for (; textLineToBePrinted < diffStartLine; textLineToBePrinted++)
                {
                    // print text between diffs
                    FormattedText oneLine = CreateFormattedText(Lines[textLineToBePrinted]);
                    dc.DrawText(oneLine, new Point(PositionX(), PositionY(textLineToBePrinted)));
                }

                for (; textLineToBePrinted < diffStartLine + diffAffectedLines; textLineToBePrinted++)
                {
                    // print text on diff
                    FormattedText oneLine = CreateFormattedText(Lines[textLineToBePrinted]);
                    if (textDiscarded && diffHovered)
                    {
                        oneLine.SetForegroundBrush(Brushes.White);
                    } else if (textDiscarded)
                    {
                        oneLine.SetForegroundBrush(Brushes.Gray);
                    }
                    dc.DrawText(oneLine, new Point(PositionX(), PositionY(textLineToBePrinted)));
                }
            }

            for (; textLineToBePrinted < EndsOnLine; textLineToBePrinted++)
            {
                // print text
                FormattedText oneLine = CreateFormattedText(Lines[textLineToBePrinted]);
                dc.DrawText(oneLine, new Point(PositionX(), PositionY(textLineToBePrinted)));
            }
        }

        #endregion

        private IEnumerable<DiffItem> VisibleDiffItems()
        {
            if (Node.Diff == null)
            {
                return Enumerable.Empty<DiffItem>();
            }

            switch (target)
            {
                case TargetFileEnum.Local:
                    return Node.Diff.Items
                        .SkipWhile(diffItem => diffItem.LocalLineStart + diffItem.LocalAffectedLines < StartsOnLine)
                        .TakeWhile(diffItem => diffItem.LocalLineStart <= EndsOnLine);

                case TargetFileEnum.Remote:
                    return Node.Diff.Items
                        .SkipWhile(diffItem => diffItem.RemoteLineStart + diffItem.RemoteAffectedLines < StartsOnLine)
                        .TakeWhile(diffItem => diffItem.RemoteLineStart <= EndsOnLine);
            }

            return Enumerable.Empty<DiffItem>();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            foreach (DiffItem diffItem in VisibleDiffItems())
            {
                int diffStartLine = -1;
                int diffAffectedLines = 0;

                switch (target)
                {
                    case TargetFileEnum.Local:
                        diffStartLine = diffItem.LocalLineStart;
                        diffAffectedLines = diffItem.LocalAffectedLines;
                        break;
                    case TargetFileEnum.Remote:
                        diffStartLine = diffItem.RemoteLineStart;
                        diffAffectedLines = diffItem.RemoteAffectedLines;
                        break;
                }

                if (diffStartLine > PositionToLine(e) || PositionToLine(e) >= diffStartLine + diffAffectedLines)
                    continue;

                switch (target)
                {
                    case TargetFileEnum.Local:
                        diffItem.PreferedAction = PreferedActionTwoWayEnum.ApplyLocal;
                        break;
                    case TargetFileEnum.Remote:
                        diffItem.PreferedAction = PreferedActionTwoWayEnum.ApplyRemote;
                        break;
                }

                if (OnDiffChange != null)
                    OnDiffChange();

                if (OnDiffSelected != null)
                    OnDiffSelected(Array.FindIndex(Node.Diff.Items, item => item == diffItem));

                break;
            }
        }
    }
}
