using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CoreLibrary.Enums;
using CoreLibrary.Helpers;
using CoreLibrary.Interfaces;
using CoreLibrary.Plugins.DiffWindow;
using DiffAlgorithm.ThreeWay;
using DiffIntegration.DiffFilesystemTree;
using DiffWindows.FolderWindows;
using DiffWindows.Menus;
using DiffWindows.TextWindows.Controls;
using DiffWindows.TextWindows.Controls.LineMarkers;

namespace DiffWindows.TextWindows
{
    /// <summary>
    /// Interaction logic for TextDiffThreeWay.xaml
    /// </summary>
    [DiffWindow(200)]
    public partial class TextDiffThreeWay : UserControl, IDiffWindow<DiffFileNode>, IChangesMenu, IMergeMenu
    {
        private readonly IDiffWindowManager manager;
        public DiffFileNode DiffNode { get; private set; }

        public int CurrentDiff { get; internal set; }

        private readonly TextDiff3Area localText;
        private readonly TextDiff3Area baseText;
        private readonly TextDiff3Area remoteText;
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

        public TextDiffThreeWay(IFilesystemTreeVisitable diffNode, IDiffWindowManager manager)
        {
            InitializeComponent();
            this.manager = manager;
            DiffNode = (DiffFileNode)diffNode;
            CurrentDiff = -1;

            InitializeComponent();

            localText = new TextDiff3Area(DiffNode, TextDiff3Area.TargetFileEnum.Local);
            remoteText = new TextDiff3Area(DiffNode, TextDiff3Area.TargetFileEnum.Remote);
            baseText = new TextDiff3Area(DiffNode, TextDiff3Area.TargetFileEnum.Base);

            ScrollViewerLocal.Content = localText;
            ScrollViewerRemote.Content = remoteText;
            ScrollViewerBase.Content = baseText;

            localText.OnDiffChange += InvalidateAllVisual;
            remoteText.OnDiffChange += InvalidateAllVisual;
            baseText.OnDiffChange += InvalidateAllVisual;

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

                if (DiffNode.Diff3 != null)
                    foreach (Diff3Item diffItem in DiffNode.Diff3.Items
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

                if (DiffNode.Diff3 != null)
                    foreach (Diff3Item diffItem in DiffNode.Diff3.Items
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

                if (DiffNode.Diff3 != null)
                    foreach (Diff3Item diffItem in DiffNode.Diff3.Items
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
            var diffNode = instance as DiffFileNode;

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
            InvalidateAllVisual();
        }

        public void OnMergeComplete(Task t)
        {
            InvalidateAllFileContents();
        }

        private void InvalidateAllFileContents()
        {
            localText.InvalidateFileContents();
            remoteText.InvalidateFileContents();
            baseText.InvalidateFileContents();

            localText.InvalidateVisual();
            remoteText.InvalidateVisual();
            baseText.InvalidateVisual();
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
                ? PathHelper.TrimPath(DiffNode.InfoLocal.FullName, FilePathLabel)
                : DiffWindows.Resources.Diff_No_File_At_Location;
            RemoteFileLocation = DiffNode.IsInLocation(LocationEnum.OnRemote)
                ? PathHelper.TrimPath(DiffNode.InfoRemote.FullName, FilePathLabel)
                : DiffWindows.Resources.Diff_No_File_At_Location;
            BaseFileLocation = DiffNode.IsInLocation(LocationEnum.OnBase)
                ? PathHelper.TrimPath(DiffNode.InfoBase.FullName, FilePathLabel)
                : DiffWindows.Resources.Diff_No_File_At_Location;
        }

        #region Custom ChangesMenu commands

        private void ScrollToLine(int diffIndex)
        {
            localText.ScrollToLine(DiffNode.Diff3.Items[diffIndex].LocalLineStart - 1);
            baseText.ScrollToLine(DiffNode.Diff3.Items[diffIndex].BaseLineStart - 1);
            remoteText.ScrollToLine(DiffNode.Diff3.Items[diffIndex].RemoteLineStart - 1);
        }

        public RoutedUICommand PreviousCommand()
        {
            return Previous;
        }

        public CommandBinding PreviousCommandBinding()
        {
            return new CommandBinding(Previous,
                (sender, args) => { ScrollToLine(--CurrentDiff); },
                (sender, args) => { args.CanExecute = DiffNode.Diff3 != null && CurrentDiff > 0; });
        }

        public static RoutedUICommand Previous = new RoutedUICommand("Previous", "Previous", typeof(TextDiff3Area),
                new InputGestureCollection() { new KeyGesture(Key.F7) }
            );

        public RoutedUICommand NextCommand()
        {
            return Next;
        }

        public CommandBinding NextCommandBinding()
        {
            return new CommandBinding(Next,
                (sender, args) => { ScrollToLine(++CurrentDiff); },
                (sender, args) => { args.CanExecute = DiffNode.Diff3 != null && CurrentDiff < DiffNode.Diff3.Items.Length - 1; });
        }

        public static RoutedUICommand Next = new RoutedUICommand("Next", "Next", typeof(TextDiff3Area),
                new InputGestureCollection() { new KeyGesture(Key.F8) }
            );

        #endregion

        #region Custom MergeMenu commands

        public RoutedUICommand PreviousConflictCommand() { return PreviousConflict; }

        private int FindPreviousConflict()
        {
            for (int i = CurrentDiff - 1; i >= 0; i--)
            {
                if (DiffNode.Diff3.Items[i].Differeces == DifferencesStatusEnum.AllDifferent)
                    return i;
            }

            return -1;
        }

        public CommandBinding PreviousConflictCommandBinding()
        {
            return new CommandBinding(PreviousConflict,
                (sender, args) =>
                {
                    ScrollToLine(CurrentDiff = FindPreviousConflict());
                },
                (sender, args) =>
                {
                    args.CanExecute = DiffNode.Diff3 != null && FindPreviousConflict() != -1;
                });
        }

        public static RoutedUICommand PreviousConflict = new RoutedUICommand("PreviousConflict", "PreviousConflict", typeof(TextDiff3Area),
            new InputGestureCollection() { new KeyGesture(Key.D9, ModifierKeys.Control) }
        );

        public RoutedUICommand NextConflictCommand() { return NextConflict; }

        private int FindNextConflict(int start)
        {
            for (int i = start; i < DiffNode.Diff3.Items.Length; i++)
            {
                if (DiffNode.Diff3.Items[i].Differeces == DifferencesStatusEnum.AllDifferent)
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

                if (DiffNode.Diff3.Items[next].Action == Diff3Item.ActionEnum.Default)
                    return next;
            }

            return -1;
        }

        public CommandBinding NextConflictCommandBinding()
        {
            return new CommandBinding(NextConflict, (sender, args) =>
            {
                ScrollToLine(CurrentDiff = FindNextConflict(CurrentDiff + 1));
            }, (sender, args) =>
            {
                args.CanExecute = DiffNode.Diff3 != null && FindNextConflict(CurrentDiff + 1) != -1;
            });
        }

        public static RoutedUICommand NextConflict = new RoutedUICommand("NextConflict", "NextConflict", typeof(TextDiff3Area),
            new InputGestureCollection() { new KeyGesture(Key.D0, ModifierKeys.Control) }
        );

        public RoutedUICommand MergeCommand() { return Merge; }

        public CommandBinding MergeCommandBinding()
        {
            return new CommandBinding(Merge,
                (sender, args) =>
                {
                    CurrentDiff = FindUnresolvedConflict(0);
                    if (CurrentDiff != -1)
                    {
                        ScrollToLine(CurrentDiff);
                    } else
                    {
                        manager.RequestMerge(this);
                    }
                },
                (sender, args) => { args.CanExecute = DiffNode.Diff3 != null; }
            );
        }

        public static RoutedUICommand Merge = new RoutedUICommand("Merge", "Merge", typeof(TextDiff3Area),
            new InputGestureCollection() { new KeyGesture(Key.M, ModifierKeys.Control) }
        );

        private bool CurrentDiffAvailable()
        {
            return DiffNode.Diff3 != null && 0 <= CurrentDiff && CurrentDiff < DiffNode.Diff3.Items.Length;
        }

        public RoutedUICommand UseLocalCommand() { return UseLocal; }

        public static RoutedUICommand UseLocal = new RoutedUICommand("UseLocal", "UseLocal", typeof(TextDiff3Area),
            new InputGestureCollection() { new KeyGesture(Key.I, ModifierKeys.Control) }
        );

        public CommandBinding UseLocalCommandBinding()
        {
            return new CommandBinding(UseLocal,
                (sender, args) =>
                {
                    DiffNode.Diff3.Items[CurrentDiff].Action = Diff3Item.ActionEnum.ApplyLocal;
                    InvalidateAllVisual();
                },
                (sender, args) => args.CanExecute = CurrentDiffAvailable()
            );
        }

        public RoutedUICommand UseBaseCommand() { return UseBase; }

        public static RoutedUICommand UseBase = new RoutedUICommand("UseBase", "UseBase", typeof(TextDiff3Area),
            new InputGestureCollection() { new KeyGesture(Key.O, ModifierKeys.Control) }
        );

        public CommandBinding UseBaseCommandBinding()
        {
            return new CommandBinding(UseBase,
                (sender, args) =>
                {
                    DiffNode.Diff3.Items[CurrentDiff].Action = Diff3Item.ActionEnum.RevertToBase;
                    InvalidateAllVisual();
                },
                (sender, args) => args.CanExecute = CurrentDiffAvailable()
            );
        }

        public RoutedUICommand UseRemoteCommand() { return UseRemote; }

        public static RoutedUICommand UseRemote = new RoutedUICommand("UseRemote", "UseRemote", typeof(TextDiff3Area),
            new InputGestureCollection() { new KeyGesture(Key.P, ModifierKeys.Control) }
        );

        public CommandBinding UseRemoteCommandBinding()
        {
            return new CommandBinding(UseRemote,
                (sender, args) =>
                {
                    DiffNode.Diff3.Items[CurrentDiff].Action = Diff3Item.ActionEnum.ApplyRemote;
                    InvalidateAllVisual();
                },
                (sender, args) => args.CanExecute = CurrentDiffAvailable()
            );
        }

        #endregion
    }
}
