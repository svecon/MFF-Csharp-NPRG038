﻿using System;
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
        private Dictionary<int, DiffItem> diffItemsByLine;


        public enum TargetFileEnum { Local, Remote }
        private readonly TargetFileEnum target;

        public TextDiffArea(DiffFileNode fileNode, TargetFileEnum targetFile)
            : base(fileNode)
        {
            target = targetFile;
            info = (FileInfo)(target == TargetFileEnum.Local ? fileNode.InfoLocal : fileNode.InfoRemote);
        }

        protected override bool IsDiffAvailable()
        {
            return node.Diff != null;
        }

        protected override void PreloadFileToMemory()
        {
            if (!IsDiffAvailable())
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
        }

        #endregion

        private IEnumerable<DiffItem> VisibleDiffItems()
        {
            if (node.Diff == null)
            {
                return Enumerable.Empty<DiffItem>();
            }

            switch (target)
            {
                case TargetFileEnum.Local:
                    return node.Diff.Items
                        .SkipWhile(diffItem => diffItem.LocalLineStart + diffItem.DeletedInOld < StartsOnLine)
                        .TakeWhile(diffItem => diffItem.LocalLineStart <= EndsOnLine);

                case TargetFileEnum.Remote:
                    return node.Diff.Items
                        .SkipWhile(diffItem => diffItem.RemoteLineStart + diffItem.InsertedInNew < StartsOnLine)
                        .TakeWhile(diffItem => diffItem.RemoteLineStart <= EndsOnLine);
            }

            return Enumerable.Empty<DiffItem>();
        }
    }
}
