using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BasicMenus.ChangesMenu;
using BasicMenus.MergeMenu;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Helpers;
using CoreLibrary.Plugins.DiffWindow;
using TextDiffAlgorithm.ThreeWay;
using TextDiffWindows.Controls;
using TextDiffWindows.Controls.LineMarkers;

namespace TextDiffWindows
{
    /// <summary>
    /// Plugin for visualising differences between three text files.
    /// </summary>
    [DiffWindow(200)]
    public partial class TextDiffThreeWay : UserControl, IDiffWindow<FileDiffNode>, IChangesMenu, IMergeMenu
    {
        /// <summary>
        /// Window manager for current visualisation.
        /// </summary>
        private readonly IDiffWindowManager manager;

        /// <inheritdoc />
        public FileDiffNode DiffNode { get; private set; }

        /// <summary>
        /// A pointer to currently selected diff.
        /// </summary>
        private int CurrentDiff { get; set; }

        /// <summary>
        /// Control for visualising local text.
        /// </summary>
        private readonly TextDiffThreeWayArea localText;

        /// <summary>
        /// Control for visualising base text.
        /// </summary>
        private readonly TextDiffThreeWayArea baseText;

        /// <summary>
        /// Control for visualising remote text.
        /// </summary>
        private readonly TextDiffThreeWayArea remoteText;

        /// <summary>
        /// Control for connecting related diffs between local and base control.
        /// </summary>
        private readonly LineMarkersThreeWayElement lineMarkersLeft;

        /// <summary>
        /// Control for connecting related diffs between local and remote control.
        /// </summary>
        private readonly LineMarkersThreeWayElement lineMarkersRight;

        #region Dependency properties

        /// <summary>
        /// Dependency property for <see cref="LocalFileLocation"/> 
        /// </summary>
        public static readonly DependencyProperty LocalFileLocationProperty
            = DependencyProperty.Register("LocalFileLocation", typeof(string), typeof(TextDiffThreeWay));

        /// <summary>
        /// String path to the local file.
        /// </summary>
        public string LocalFileLocation
        {
            get { return (string)GetValue(LocalFileLocationProperty); }
            set { SetValue(LocalFileLocationProperty, value); }
        }

        /// <summary>
        /// Dependency property for <see cref="RemoteFileLocation"/> 
        /// </summary>
        public static readonly DependencyProperty RemoteFileLocationProperty
            = DependencyProperty.Register("RemoteFileLocation", typeof(string), typeof(TextDiffThreeWay));

        /// <summary>
        /// String path to the remote file.
        /// </summary>
        public string RemoteFileLocation
        {
            get { return (string)GetValue(RemoteFileLocationProperty); }
            set { SetValue(RemoteFileLocationProperty, value); }
        }

        /// <summary>
        /// Dependency property for <see cref="BaseFileLocation"/> 
        /// </summary>
        public static readonly DependencyProperty BaseFileLocationProperty
            = DependencyProperty.Register("BaseFileLocation", typeof(string), typeof(TextDiffThreeWay));

        /// <summary>
        /// String path to the base file.
        /// </summary>
        public string BaseFileLocation
        {
            get { return (string)GetValue(BaseFileLocationProperty); }
            set { SetValue(BaseFileLocationProperty, value); }
        }

        #endregion

        /// <summary>
        /// Initializes new instance of the <see cref="TextDiffThreeWay"/>
        /// </summary>
        /// <param name="diffNode">Diff node holding the calculated diff.</param>
        /// <param name="manager">Manager for this window.</param>
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

            LineMarkersPanelLocal.Content = lineMarkersLeft
                = new LineMarkersThreeWayElement(DiffNode, localText, baseText, LineMarkersThreeWayElement.MarkerTypeEnum.BaseLeft);

            LineMarkersPanelRemote.Content = lineMarkersRight
                = new LineMarkersThreeWayElement(DiffNode, baseText, remoteText, LineMarkersThreeWayElement.MarkerTypeEnum.BaseRight);

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
        }

        /// <summary>
        /// Can this <see cref="IDiffWindow{TNode}"/> be applied to given instance?
        /// </summary>
        /// <param name="instance">Instance holding the calculated diff.</param>
        /// <returns>True if this plugin can be applied.</returns>
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

        /// <inheritdoc />
        public void OnDiffComplete(Task t)
        {
            InvalidateAllFileContents();
        }

        /// <inheritdoc />
        public void OnMergeComplete(Task t)
        {
            InvalidateAllFileContents();
        }

        /// <summary>
        /// Iterates over differences and checks whether there are any conflicts.
        /// </summary>
        private void CheckConflicts()
        {
            if (DiffNode.Diff == null)
                return;

            bool anyCoflicting = ((Diff3)DiffNode.Diff).Items.Any(diff3Item =>
                diff3Item.Differeces == DifferencesStatusEnum.AllDifferent
                && diff3Item.PreferedAction == PreferedActionThreeWayEnum.Default);

            DiffNode.Status = anyCoflicting ? NodeStatusEnum.IsConflicting : NodeStatusEnum.WasDiffed;
        }

        /// <summary>
        /// Invalidates file contents and forces them to redraw.
        /// </summary>
        private void InvalidateAllFileContents()
        {
            localText.InvalidateFileContents();
            remoteText.InvalidateFileContents();
            baseText.InvalidateFileContents();

            InvalidateAllVisual();
        }

        /// <summary>
        /// Invalidates visuals of all controls.
        /// </summary>
        private void InvalidateAllVisual()
        {
            localText.InvalidateVisual();
            remoteText.InvalidateVisual();
            baseText.InvalidateVisual();
            lineMarkersLeft.InvalidateVisual();
            lineMarkersRight.InvalidateVisual();
        }

        /// <summary>
        /// Size changed event handler for shortening file paths.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
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

        /// <summary>
        /// Scrolls to a given line in all three controls.
        /// </summary>
        /// <param name="diffIndex">Index of the diff to scroll to.</param>
        private void ScrollToLine(int diffIndex)
        {
            var diff = ((Diff3)DiffNode.Diff);
            localText.ScrollToLine(diff.Items[diffIndex].LocalLineStart - 1);
            baseText.ScrollToLine(diff.Items[diffIndex].BaseLineStart - 1);
            remoteText.ScrollToLine(diff.Items[diffIndex].RemoteLineStart - 1);
        }

        /// <summary>
        /// Find a previous conflicting diff.
        /// </summary>
        /// <returns>Index of the diff.</returns>
        private int FindPreviousConflict()
        {
            for (int i = CurrentDiff - 1; i >= 0; i--)
            {
                if (((Diff3)DiffNode.Diff).Items[i].Differeces == DifferencesStatusEnum.AllDifferent)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Find a next conflicting diff.
        /// </summary>
        /// <param name="start">Where to start from.</param>
        /// <returns>Index of the diff.</returns>
        private int FindNextConflict(int start)
        {
            for (int i = start; i < ((Diff3)DiffNode.Diff).Items.Length; i++)
            {
                if (((Diff3)DiffNode.Diff).Items[i].Differeces == DifferencesStatusEnum.AllDifferent)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Find next unresolved conflicting diff.
        /// </summary>
        /// <param name="start">Where to start searching.</param>
        /// <returns>Index of the diff.</returns>
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

        /// <summary>
        /// Checks availability of current diff.
        /// </summary>
        /// <returns>True when a current diff is avaialble.</returns>
        private bool CurrentDiffAvailable()
        {
            return DiffNode.Diff != null && 0 <= CurrentDiff && CurrentDiff < ((Diff3)DiffNode.Diff).Items.Length;
        }

        #endregion

        #region Custom ChangesMenu commands

        /// <inheritdoc />
        public CommandBinding PreviousCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) => { ScrollToLine(--CurrentDiff); },
                (sender, args) => { args.CanExecute = DiffNode.Diff != null && CurrentDiff > 0; });
        }

        /// <inheritdoc />
        public CommandBinding NextCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) => { ScrollToLine(++CurrentDiff); },
                (sender, args) => { args.CanExecute = DiffNode.Diff != null && CurrentDiff < ((Diff3)DiffNode.Diff).Items.Length - 1; });
        }

        /// <inheritdoc />
        public CommandBinding RecalculateCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) => manager.RequestDiff(this),
                (sender, args) => args.CanExecute = true);
        }

        #endregion

        #region Custom MergeMenu commands

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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
