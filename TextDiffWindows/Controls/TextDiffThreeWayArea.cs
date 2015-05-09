using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree.Enums;
using TextDiffAlgorithm.ThreeWay;

namespace TextDiffWindows.Controls
{
    /// <summary>
    /// An element for showing a text file and highlighting different lines using 3-way diff.
    /// </summary>
    class TextDiffThreeWayArea : TextAreaAbstract, IScrollInfo
    {
        public enum TargetFileEnum { Local, Base, Remote }
        private readonly TargetFileEnum target;

        public delegate void OnDiffChangeDelegate();
        public OnDiffChangeDelegate OnDiffChange;

        public delegate void OnDiffSelectedDelegate(int selected);
        public OnDiffSelectedDelegate OnDiffSelected;

        /// <summary>
        /// Initializes new instance of the <see cref="TextDiffThreeWayArea"/>
        /// </summary>
        /// <param name="node">Node containing files and diffs.</param>
        /// <param name="targetFile">Which file will be diplayed.</param>
        public TextDiffThreeWayArea(FileDiffNode node, TargetFileEnum targetFile)
            : base(node)
        {
            target = targetFile;

            switch (target)
            {
                case TargetFileEnum.Local: Info = (FileInfo)Node.InfoLocal;
                    break;
                case TargetFileEnum.Remote: Info = (FileInfo)Node.InfoRemote;
                    break;
                case TargetFileEnum.Base: Info = (FileInfo)Node.InfoBase;
                    break;
            }
        }

        protected override bool IsDiffAvailable()
        {
            return Node.Diff is Diff3;
        }

        protected override void PreloadFileToMemory()
        {
            if (!IsDiffAvailable())
            {
                Lines = new List<string>();
            } else
            {
                var diff = (Diff3)Node.Diff;

                switch (target)
                {
                    case TargetFileEnum.Local:
                        Lines = new List<string>(diff.FilesLineCount.Local);
                        break;
                    case TargetFileEnum.Remote:
                        Lines = new List<string>(diff.FilesLineCount.Remote);
                        break;
                    case TargetFileEnum.Base:
                        Lines = new List<string>(diff.FilesLineCount.Base);
                        break;
                }
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
            InvalidateMeasure();
        }

        #region Rendering

        protected override void OnRender(DrawingContext dc)
        {
            if (Lines == null && Info != null)
            {
                PreloadFileToMemory();
            }

            try
            {
                DrawBackground(dc);
                DrawDiffs(dc);
                DrawText(dc);
                DrawBorders(dc);
            } catch (Exception)
            {
                // ignore exceptions during rendering
            }
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

            foreach (Diff3Item diffItem in VisibleDiffItems())
            {
                int diffStartLine = -1;
                int diffAffectedLines = 0;
                bool textDiscarded = false;
                bool diffHovered = false;
                Color diffColor = (diffItem.Differeces == DifferencesStatusEnum.AllDifferent) ? Colors.MediumVioletRed : Colors.MediumPurple;

                switch (target)
                {
                    case TargetFileEnum.Local:
                        if (diffItem.BaseAffectedLines == 0 && diffItem.RemoteAffectedLines == 0)
                        {
                            diffColor = Colors.LimeGreen;
                        }
                        if (diffItem.PreferedAction != PreferedActionThreeWayEnum.Default &&
                                   diffItem.PreferedAction != PreferedActionThreeWayEnum.ApplyLocal)
                        {
                            textDiscarded = true;
                        }

                        diffStartLine = diffItem.LocalLineStart;
                        diffAffectedLines = diffItem.LocalAffectedLines;

                        break;
                    case TargetFileEnum.Remote:
                        if (diffItem.BaseAffectedLines == 0 && diffItem.LocalAffectedLines == 0)
                        {
                            diffColor = Colors.LimeGreen;
                        }
                        if (diffItem.PreferedAction != PreferedActionThreeWayEnum.Default &&
                                   diffItem.PreferedAction != PreferedActionThreeWayEnum.ApplyRemote)
                        {
                            textDiscarded = true;
                        }

                        diffStartLine = diffItem.RemoteLineStart;
                        diffAffectedLines = diffItem.RemoteAffectedLines;

                        break;
                    case TargetFileEnum.Base:
                        if (diffItem.LocalAffectedLines == 0 && diffItem.RemoteAffectedLines == 0)
                        {
                            diffColor = Colors.LimeGreen;
                        }
                        if (diffItem.PreferedAction != PreferedActionThreeWayEnum.Default &&
                                    diffItem.PreferedAction != PreferedActionThreeWayEnum.RevertToBase)
                        {
                            textDiscarded = true;
                        }

                        diffStartLine = diffItem.BaseLineStart;
                        diffAffectedLines = diffItem.BaseAffectedLines;

                        break;
                }

                if (textDiscarded)
                {
                    diffColor = Colors.LightSlateGray;
                }

                Brush b = new SolidColorBrush(diffColor) { Opacity = .33 };

                if (diffStartLine <= PositionToLine(MouseArgs) && PositionToLine(MouseArgs) < diffStartLine + diffAffectedLines)
                {
                    diffHovered = true;
                    b.Opacity = 1;
                    Cursor = Cursors.Hand;
                }

                dc.DrawRectangle(b, null, new Rect(new Point(BORDER_SIZE, PositionY(diffStartLine)), new Size(ActualWidth - 2 * BORDER_SIZE, LineHeight * diffAffectedLines)));

                DrawHorizontalLine(dc, PositionY(diffStartLine), 0.0, ActualWidth, DiffLinePen);
                DrawHorizontalLine(dc, PositionY(diffStartLine + diffAffectedLines), 0.0, ActualWidth, DiffLinePen);

                for (; textLineToBePrinted < diffStartLine; textLineToBePrinted++)
                {
                    // print text between diffs
                    FormattedText oneLine = CreateFormattedText(GetLine(textLineToBePrinted));
                    dc.DrawText(oneLine, new Point(PositionX(), PositionY(textLineToBePrinted)));
                }

                for (; textLineToBePrinted < diffStartLine + diffAffectedLines; textLineToBePrinted++)
                {
                    // print text on diff
                    FormattedText oneLine = CreateFormattedText(GetLine(textLineToBePrinted));
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
                FormattedText oneLine = CreateFormattedText(GetLine(textLineToBePrinted));
                dc.DrawText(oneLine, new Point(PositionX(), PositionY(textLineToBePrinted)));
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
                    return ((Diff3)Node.Diff).Items
                        .SkipWhile(diffItem => diffItem.LocalLineStart + diffItem.LocalAffectedLines < StartsOnLine)
                        .TakeWhile(diffItem => diffItem.LocalLineStart <= EndsOnLine);

                case TargetFileEnum.Remote:
                    return ((Diff3)Node.Diff).Items
                        .SkipWhile(diffItem => diffItem.RemoteLineStart + diffItem.RemoteAffectedLines < StartsOnLine)
                        .TakeWhile(diffItem => diffItem.RemoteLineStart <= EndsOnLine);

                case TargetFileEnum.Base:
                    return ((Diff3)Node.Diff).Items
                        .SkipWhile(diffItem => diffItem.BaseLineStart + diffItem.BaseAffectedLines < StartsOnLine)
                        .TakeWhile(diffItem => diffItem.BaseLineStart <= EndsOnLine);
            }

            return Enumerable.Empty<Diff3Item>();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            foreach (Diff3Item diffItem in VisibleDiffItems())
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
                    case TargetFileEnum.Base:
                        diffStartLine = diffItem.BaseLineStart;
                        diffAffectedLines = diffItem.BaseAffectedLines;
                        break;
                }

                if (diffStartLine > PositionToLine(e) || PositionToLine(e) >= diffStartLine + diffAffectedLines)
                    continue;

                switch (target)
                {
                    case TargetFileEnum.Local:
                        diffItem.PreferedAction = PreferedActionThreeWayEnum.ApplyLocal;
                        break;
                    case TargetFileEnum.Remote:
                        diffItem.PreferedAction = PreferedActionThreeWayEnum.ApplyRemote;
                        break;
                    case TargetFileEnum.Base:
                        diffItem.PreferedAction = PreferedActionThreeWayEnum.RevertToBase;
                        break;
                }

                if (OnDiffChange != null)
                    OnDiffChange();

                if (OnDiffSelected != null)
                    OnDiffSelected(Array.FindIndex(((Diff3)Node.Diff).Items, item => item == diffItem));

                break;
            }
        }
    }
}
