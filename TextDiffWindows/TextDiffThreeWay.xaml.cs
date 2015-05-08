using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BasicMenus.ChangesMenu;
using BasicMenus.MergeMenu;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Helpers;
using CoreLibrary.Plugins.DiffWindow;
using TextDiffAlgorithm.ThreeWay;
using TextDiffWindows.Controls;
using TextDiffWindows.Controls.LineMarkers;

namespace TextDiffWindows
{
    /// <summary>
    /// Interaction logic for TextDiffThreeWay.xaml
    /// </summary>
    [DiffWindow(200)]
    public partial class TextDiffThreeWay : UserControl, IDiffWindow<FileDiffNode>, IChangesMenu, IMergeMenu
    {
        private readonly IDiffWindowManager manager;
        public FileDiffNode DiffNode { get; private set; }

        public int CurrentDiff { get; internal set; }

        private readonly TextDiffThreeWayArea localText;
        private readonly TextDiffThreeWayArea baseText;
        private readonly TextDiffThreeWayArea remoteText;
        private readonly LineMarkersThreeWayElement lineMarkersLeft;
        private readonly LineMarkersThreeWayElement lineMarkersRight;

        #region Dependency properties

        public static readonly DependencyProperty LocalFileLocationProperty
            = DependencyProperty.Register("LocalFileLocation", typeof(string), typeof(TextDiffThreeWay));

        public string LocalFileLocation
        {
            get { return (string)GetValue(LocalFileLocationProperty); }
            set { SetValue(LocalFileLocationProperty, value); }
        }

        public static readonly DependencyProperty RemoteFileLocationProperty
            = DependencyProperty.Register("RemoteFileLocation", typeof(string), typeof(TextDiffThreeWay));

        public string RemoteFileLocation
        {
            get { return (string)GetValue(RemoteFileLocationProperty); }
            set { SetValue(RemoteFileLocationProperty, value); }
        }

        public static readonly DependencyProperty BaseFileLocationProperty
            = DependencyProperty.Register("BaseFileLocation", typeof(string), typeof(TextDiffThreeWay));

        public string BaseFileLocation
        {
            get { return (string)GetValue(BaseFileLocationProperty); }
            set { SetValue(BaseFileLocationProperty, value); }
        }

        #endregion

        public TextDiffThreeWay(INodeVisitable diffNode, IDiffWindowManager manager)
        {
            InitializeComponent();
            this.manager = manager;
            DiffNode = (FileDiffNode)diffNode;
            CurrentDiff = -1;

            InitializeComponent();

            localText = new TextDiffThreeWayArea(DiffNode, TextDiffThreeWayArea.TargetFileEnum.Local);
            remoteText = new TextDiffThreeWayArea(DiffNode, TextDiffThreeWayArea.TargetFileEnum.Remote);
            baseText = new TextDiffThreeWayArea(DiffNode, TextDiffThreeWayArea.TargetFileEnum.Base);

            ScrollViewerLocal.Content = localText;
            ScrollViewerRemote.Content = remoteText;
            ScrollViewerBase.Content = baseText;

            localText.OnDiffChange += InvalidateAllVisual;
            remoteText.OnDiffChange += InvalidateAllVisual;
            baseText.OnDiffChange += InvalidateAllVisual;

            localText.OnDiffChange += CheckConflicts;
            remoteText.OnDiffChange += CheckConflicts;
            baseText.OnDiffChange += CheckConflicts;

            localText.OnDiffSelected += selected => CurrentDiff = selected;
            remoteText.OnDiffSelected += selected => CurrentDiff = selected;
            baseText.OnDiffSelected += selected => CurrentDiff = selected;

            localText.OnHorizontalScroll += offset => baseText.SetHorizontalOffset(offset);
            remoteText.OnHorizontalScroll += offset => baseText.SetHorizontalOffset(offset);
            baseText.OnHorizontalScroll += offset =>
            {
                localText.SetHorizontalOffset(offset);
                remoteText.SetHorizontalOffset(offset);
            };

            localText.OnVerticalScrollSynchronization += offset =>
            {
                int differenceToBase = 0;
                int differenceToRemote = 0;

                if (DiffNode.Diff != null)
                    foreach (Diff3Item diffItem in ((Diff3)DiffNode.Diff).Items
                        .TakeWhile(diffItem => diffItem.LocalLineStart <= localText.StartsOnLine))
                    {
                        differenceToBase += (diffItem.BaseAffectedLines - diffItem.LocalAffectedLines);
                        differenceToRemote += (diffItem.RemoteAffectedLines - diffItem.LocalAffectedLines);
                    }

                remoteText.SetVerticalOffsetWithoutSynchornizing(offset, differenceToRemote);
                baseText.SetVerticalOffsetWithoutSynchornizing(offset, differenceToBase);
            };

            remoteText.OnVerticalScrollSynchronization += offset =>
            {
                int differenceToBase = 0;
                int differenceToLocal = 0;

                if (DiffNode.Diff != null)
                    foreach (Diff3Item diffItem in ((Diff3)DiffNode.Diff).Items
                        .TakeWhile(diffItem => diffItem.LocalLineStart <= localText.StartsOnLine))
                    {
                        differenceToBase += (diffItem.BaseAffectedLines - diffItem.RemoteAffectedLines);
                        differenceToLocal += (diffItem.LocalAffectedLines - diffItem.RemoteAffectedLines);
                    }

                baseText.SetVerticalOffsetWithoutSynchornizing(offset, differenceToBase);
                localText.SetVerticalOffsetWithoutSynchornizing(offset, differenceToLocal);
            };

            baseText.OnVerticalScrollSynchronization += offset =>
            {
                int differenceToRemote = 0;
                int differenceToLocal = 0;

                if (DiffNode.Diff != null)
                    foreach (Diff3Item diffItem in ((Diff3)DiffNode.Diff).Items
                        .TakeWhile(diffItem => diffItem.LocalLineStart <= localText.StartsOnLine))
                    {
                        differenceToRemote += (diffItem.RemoteAffectedLines - diffItem.BaseAffectedLines);
                        differenceToLocal += (diffItem.LocalAffectedLines - diffItem.BaseAffectedLines);
                    }

                remoteText.SetVerticalOffsetWithoutSynchornizing(offset, differenceToRemote);
                localText.SetVerticalOffsetWithoutSynchornizing(offset, differenceToLocal);
            };

            LineMarkersPanelLocal.Content = lineMarkersLeft
                = new LineMarkersThreeWayElement(DiffNode, localText, baseText, LineMarkersThreeWayElement.MarkerTypeEnum.BaseLeft);

            LineMarkersPanelRemote.Content = lineMarkersRight
                = new LineMarkersThreeWayElement(DiffNode, baseText, remoteText, LineMarkersThreeWayElement.MarkerTypeEnum.BaseRight);
        }

        public static bool CanBeApplied(object instance)
        {
            var diffNode = instance as FileDiffNode;

            if (diffNode == null)
                return false;

            if (diffNode.FileType == FileTypeEnum.Unknown)
            {
                if (diffNode.Info.FullName.IsTextFile())
                    diffNode.FileType = FileTypeEnum.Text;
                else
                {
                    diffNode.FileType = FileTypeEnum.Binary;
                    return false;
                }
            }

            if (diffNode.FileType != FileTypeEnum.Text)
                return false;

            return diffNode.Mode == DiffModeEnum.ThreeWay;
        }

        public void OnDiffComplete(Task t)
        {
            InvalidateAllFileContents();
        }

        public void OnMergeComplete(Task t)
        {
            InvalidateAllFileContents();
        }

        private void CheckConflicts()
        {
            if (DiffNode.Diff == null)
                return;

            bool anyCoflicting = ((Diff3)DiffNode.Diff).Items.Any(diff3Item =>
                diff3Item.Differeces == DifferencesStatusEnum.AllDifferent
                && diff3Item.PreferedAction == PreferedActionThreeWayEnum.Default);

            DiffNode.Status = anyCoflicting ? NodeStatusEnum.IsConflicting : NodeStatusEnum.WasDiffed;
        }

        private void InvalidateAllFileContents()
        {
            localText.InvalidateFileContents();
            remoteText.InvalidateFileContents();
            baseText.InvalidateFileContents();

            InvalidateAllVisual();
        }

        private void InvalidateAllVisual()
        {
            localText.InvalidateVisual();
            remoteText.InvalidateVisual();
            baseText.InvalidateVisual();
            lineMarkersLeft.InvalidateVisual();
            lineMarkersRight.InvalidateVisual();
        }

        private void TextDiffThreeWay_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFileLocation = DiffNode.IsInLocation(LocationEnum.OnLocal)
                ? PathShortener.TrimPath(DiffNode.InfoLocal.FullName, FilePathLabel)
                : TextDiffWindows.Resources.Diff_No_File_At_Location;
            RemoteFileLocation = DiffNode.IsInLocation(LocationEnum.OnRemote)
                ? PathShortener.TrimPath(DiffNode.InfoRemote.FullName, FilePathLabel)
                : TextDiffWindows.Resources.Diff_No_File_At_Location;
            BaseFileLocation = DiffNode.IsInLocation(LocationEnum.OnBase)
                ? PathShortener.TrimPath(DiffNode.InfoBase.FullName, FilePathLabel)
                : TextDiffWindows.Resources.Diff_No_File_At_Location;
        }

        #region Iterating over diffs and conflicts

        private void ScrollToLine(int diffIndex)
        {
            var diff = ((Diff3)DiffNode.Diff);
            localText.ScrollToLine(diff.Items[diffIndex].LocalLineStart - 1);
            baseText.ScrollToLine(diff.Items[diffIndex].BaseLineStart - 1);
            remoteText.ScrollToLine(diff.Items[diffIndex].RemoteLineStart - 1);
        }

        private int FindPreviousConflict()
        {
            for (int i = CurrentDiff - 1; i >= 0; i--)
            {
                if (((Diff3)DiffNode.Diff).Items[i].Differeces == DifferencesStatusEnum.AllDifferent)
                    return i;
            }

            return -1;
        }

        private int FindNextConflict(int start)
        {
            for (int i = start; i < ((Diff3)DiffNode.Diff).Items.Length; i++)
            {
                if (((Diff3)DiffNode.Diff).Items[i].Differeces == DifferencesStatusEnum.AllDifferent)
                    return i;
            }

            return -1;
        }

        private int FindUnresolvedConflict(int start)
        {
            int next = start - 1;
            while ((next = FindNextConflict(next + 1)) != -1)
            {
                if (next == -1)
                    break;

                if (((Diff3)DiffNode.Diff).Items[next].PreferedAction == PreferedActionThreeWayEnum.Default)
                    return next;
            }

            return -1;
        }

        private bool CurrentDiffAvailable()
        {
            return DiffNode.Diff != null && 0 <= CurrentDiff && CurrentDiff < ((Diff3)DiffNode.Diff).Items.Length;
        }

        #endregion

        #region Custom ChangesMenu commands

        public CommandBinding PreviousCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) => { ScrollToLine(--CurrentDiff); },
                (sender, args) => { args.CanExecute = DiffNode.Diff != null && CurrentDiff > 0; });
        }

        public CommandBinding NextCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) => { ScrollToLine(++CurrentDiff); },
                (sender, args) => { args.CanExecute = DiffNode.Diff != null && CurrentDiff < ((Diff3)DiffNode.Diff).Items.Length - 1; });
        }

        public CommandBinding RecalculateCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) => manager.RequestDiff(this),
                (sender, args) => args.CanExecute = true);
        }

        #endregion

        #region Custom MergeMenu commands

        public CommandBinding PreviousConflictCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    ScrollToLine(CurrentDiff = FindPreviousConflict());
                },
                (sender, args) =>
                {
                    args.CanExecute = DiffNode.Diff != null && FindPreviousConflict() != -1;
                });
        }

        public CommandBinding NextConflictCommandBinding(ICommand command)
        {
            return new CommandBinding(command, (sender, args) =>
            {
                ScrollToLine(CurrentDiff = FindNextConflict(CurrentDiff + 1));
            }, (sender, args) =>
            {
                args.CanExecute = DiffNode.Diff != null && FindNextConflict(CurrentDiff + 1) != -1;
            });
        }

        public CommandBinding MergeCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    CurrentDiff = FindUnresolvedConflict(0);
                    if (CurrentDiff != -1)
                    {
                        ScrollToLine(CurrentDiff);
                        MessageBox.Show(TextDiffWindows.Resources.Popup_Conflicts_Resolve, TextDiffWindows.Resources.Popup_Conflicts, MessageBoxButton.OK);
                    } else
                    {
                        manager.RequestMerge(this);
                    }
                },
                (sender, args) => { args.CanExecute = DiffNode.Diff != null && DiffNode.IsInLocation(LocationCombinationsEnum.OnAll3); }
            );
        }

        public CommandBinding UseLocalCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    ((Diff3)DiffNode.Diff).Items[CurrentDiff].PreferedAction = PreferedActionThreeWayEnum.ApplyLocal;
                    InvalidateAllVisual();
                },
                (sender, args) => args.CanExecute = CurrentDiffAvailable()
            );
        }

        public CommandBinding UseBaseCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    ((Diff3)DiffNode.Diff).Items[CurrentDiff].PreferedAction = PreferedActionThreeWayEnum.RevertToBase;
                    InvalidateAllVisual();
                },
                (sender, args) => args.CanExecute = CurrentDiffAvailable()
            );
        }

        public CommandBinding UseRemoteCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    ((Diff3)DiffNode.Diff).Items[CurrentDiff].PreferedAction = PreferedActionThreeWayEnum.ApplyRemote;
                    InvalidateAllVisual();
                },
                (sender, args) => args.CanExecute = CurrentDiffAvailable()
            );
        }

        #endregion
    }
}
