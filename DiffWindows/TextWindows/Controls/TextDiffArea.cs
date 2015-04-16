using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DiffAlgorithm.TwoWay;
using DiffIntegration.DiffFilesystemTree;

namespace DiffWindows.TextWindows.Controls
{
    class TextDiffArea : TextAreaAbstract, IScrollInfo
    {
        private Dictionary<int, DiffItem> diffItemsByLine;


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

        private void PrepareDiffItemsByLines()
        {
            if (diffItemsByLine == null)
            {
                diffItemsByLine = (Node.Diff == null)
                ? new Dictionary<int, DiffItem>()
                : new Dictionary<int, DiffItem>(Node.Diff.Items.Length);
            }

            if (Node.Diff == null)
                return;

            foreach (DiffItem diffItem in Node.Diff.Items)
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

        private void DrawDiffs(DrawingContext dc)
        {
            foreach (DiffItem diffItem in VisibleDiffItems())
            {
                int diffStartLine = -1;
                int diffAffectedLines = 0;
                Brush b = Brushes.Black;

                switch (target)
                {
                    case TargetFileEnum.Local:
                        b = new SolidColorBrush(diffItem.RemoteAffectedLines == 0 ? Colors.MediumVioletRed : Colors.MediumPurple) { Opacity = .2 };

                        diffStartLine = diffItem.LocalLineStart;
                        diffAffectedLines = diffItem.LocalAffectedLines;

                        break;
                    case TargetFileEnum.Remote:
                        b = new SolidColorBrush(diffItem.LocalAffectedLines == 0 ? Colors.LimeGreen : Colors.MediumPurple) { Opacity = .2 };

                        diffStartLine = diffItem.RemoteLineStart;
                        diffAffectedLines = diffItem.RemoteAffectedLines;

                        break;
                }

                if (diffStartLine <= PositionToLine(MouseArgs) && PositionToLine(MouseArgs) < diffStartLine + diffAffectedLines)
                {
                    b.Opacity = 1;
                }

                dc.DrawRectangle(b, null, new Rect(new Point(BORDER_SIZE, PositionY(diffStartLine)), new Size(ActualWidth, LineHeight * diffAffectedLines)));

                DrawHorizontalLine(dc, PositionY(diffStartLine), 0.0, ActualWidth, DiffLinePen);
                DrawHorizontalLine(dc, PositionY(diffStartLine + diffAffectedLines), 0.0, ActualWidth, DiffLinePen);
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
    }
}
