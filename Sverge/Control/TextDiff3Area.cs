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
using DiffAlgorithm.ThreeWay;
using DiffAlgorithm.TwoWay;
using DiffIntegration.DiffFilesystemTree;

namespace Sverge.Control
{
    class TextDiff3Area : TextAreaAbstract, IScrollInfo
    {
        private Dictionary<int, Diff3Item> diffItemsByLine;

        public enum TargetFileEnum { Local, Base, Remote }
        private readonly TargetFileEnum target;

        public TextDiff3Area(DiffFileNode fileNode, TargetFileEnum targetFile)
            : base(fileNode)
        {
            target = targetFile;

            switch (target)
            {
                case TargetFileEnum.Local: info = (FileInfo)node.InfoLocal;
                    break;
                case TargetFileEnum.Remote: info = (FileInfo)node.InfoRemote;
                    break;
                case TargetFileEnum.Base: info = (FileInfo)node.InfoBase;
                    break;
            }
        }

        protected override bool IsDiffAvailable()
        {
            return node.Diff3 != null;
        }

        protected override void PreloadFileToMemory()
        {
            if (!IsDiffAvailable())
            {
                lines = new List<string>();
            } else
            {
                switch (target)
                {
                    case TargetFileEnum.Local:
                        lines = new List<string>(node.Diff3.FilesLineCount.Local);
                        break;
                    case TargetFileEnum.Remote:
                        lines = new List<string>(node.Diff3.FilesLineCount.Remote);
                        break;
                    case TargetFileEnum.Base:
                        lines = new List<string>(node.Diff3.FilesLineCount.Base);
                        break;
                }
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
                diffItemsByLine = (!IsDiffAvailable())
                ? new Dictionary<int, Diff3Item>()
                : new Dictionary<int, Diff3Item>(node.Diff3.Items.Length);
            }

            if (!IsDiffAvailable())
                return;

            foreach (Diff3Item diffItem in node.Diff3.Items)
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
            DrawBackground(dc);
            DrawDiffs(dc);
            DrawText(dc);
            DrawBorders(dc);
        }

        private void DrawDiffs(DrawingContext dc)
        {
            foreach (Diff3Item diffItem in VisibleDiffItems())
            {
                int diffStartLine = -1;
                int diffAffectedLines = 0;
                Brush b = Brushes.Black;

                switch (target)
                {
                    case TargetFileEnum.Local:
                        b = new SolidColorBrush(Colors.MediumPurple) { Opacity = .2 };

                        diffStartLine = diffItem.LocalLineStart;
                        diffAffectedLines = diffItem.LocalAffectedLines;

                        break;
                    case TargetFileEnum.Remote:
                        b = new SolidColorBrush(Colors.MediumVioletRed) { Opacity = .2 };

                        diffStartLine = diffItem.RemoteLineStart;
                        diffAffectedLines = diffItem.RemoteAffectedLines;

                        break;
                    case TargetFileEnum.Base:
                        b = new SolidColorBrush(Colors.MediumAquamarine) { Opacity = .2 };

                        diffStartLine = diffItem.BaseLineStart;
                        diffAffectedLines = diffItem.BaseAffectedLines;

                        break;
                }

                if (diffStartLine <= PositionToLine(mouse) && PositionToLine(mouse) < diffStartLine + diffAffectedLines)
                {
                    b.Opacity = 1;
                }

                dc.DrawRectangle(b, null, new Rect(new Point(BORDER_SIZE, PositionY(diffStartLine)), new Size(ActualWidth - 2 * BORDER_SIZE, LineHeight * diffAffectedLines)));

                DrawHorizontalLine(dc, PositionY(diffStartLine), 0.0, ActualWidth, DiffLinePen);
                DrawHorizontalLine(dc, PositionY(diffStartLine + diffAffectedLines), 0.0, ActualWidth, DiffLinePen);
            }
        }

        #endregion

        private IEnumerable<Diff3Item> VisibleDiffItems()
        {
            if (!IsDiffAvailable())
            {
                return Enumerable.Empty<Diff3Item>();
            }

            switch (target)
            {
                case TargetFileEnum.Local:
                    return node.Diff3.Items
                        .SkipWhile(diffItem => diffItem.LocalLineStart + diffItem.LocalAffectedLines < StartsOnLine)
                        .TakeWhile(diffItem => diffItem.LocalLineStart <= EndsOnLine);

                case TargetFileEnum.Remote:
                    return node.Diff3.Items
                        .SkipWhile(diffItem => diffItem.RemoteLineStart + diffItem.RemoteAffectedLines < StartsOnLine)
                        .TakeWhile(diffItem => diffItem.RemoteLineStart <= EndsOnLine);

                case TargetFileEnum.Base:
                    return node.Diff3.Items
                        .SkipWhile(diffItem => diffItem.BaseLineStart + diffItem.BaseAffectedLines < StartsOnLine)
                        .TakeWhile(diffItem => diffItem.BaseLineStart <= EndsOnLine);
            }

            return Enumerable.Empty<Diff3Item>();
        }
    }
}
