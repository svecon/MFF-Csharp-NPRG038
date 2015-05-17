using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BasicMenus.ChangesMenu;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Helpers;
using CoreLibrary.Plugins.DiffWindow;
using TextDiffAlgorithm.TwoWay;
using TextDiffWindows.Controls;
using TextDiffWindows.Controls.LineMarkers;

namespace TextDiffWindows
{
    /// <summary>
    /// Plugin for visualising differences between two text files.
    /// </summary>
    [DiffWindow(100)]
    public partial class TextDiffTwoWay : UserControl, IDiffWindow<FileDiffNode>, IChangesMenu
    {
        /// <summary>
        /// Window manager for current visualisation.
        /// </summary>
        private readonly IDiffWindowManager manager;

        /// <inheritdoc />
        public FileDiffNode DiffNode { get; private set; }

        /// <summary>
        /// Control for visualising local text.
        /// </summary>
        private readonly TextDiffTwoWayArea localText;

        /// <summary>
        /// Control for visualising remote text.
        /// </summary>
        private readonly TextDiffTwoWayArea remoteText;

        /// <summary>
        /// Control for connecting related diffs between local and remote control.
        /// </summary>
        private readonly LineMarkersTwoWayElement lineMarkers;

        /// <summary>
        /// A pointer to currently selected diff.
        /// </summary>
        public int CurrentDiff { get; internal set; }

        #region Dependency properties

        /// <summary>
        /// Dependency property for <see cref="LocalFileLocation"/> 
        /// </summary>
        public static readonly DependencyProperty LocalFileLocationProperty
            = DependencyProperty.Register("LocalFileLocation", typeof(string), typeof(TextDiffTwoWay));

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
            = DependencyProperty.Register("RemoteFileLocation", typeof(string), typeof(TextDiffTwoWay));

        /// <summary>
        /// String path to the remote file.
        /// </summary>
        public string RemoteFileLocation
        {
            get { return (string)GetValue(RemoteFileLocationProperty); }
            set { SetValue(RemoteFileLocationProperty, value); }
        }

        #endregion

        /// <summary>
        /// Initializes new instance of the <see cref="TextDiffTwoWay"/>
        /// </summary>
        /// <param name="diffNode">Diff node holding the calculated diff.</param>
        /// <param name="manager">Manager for this window.</param>
        public TextDiffTwoWay(INodeVisitable diffNode, IDiffWindowManager manager)
        {
            this.manager = manager;
            DiffNode = (FileDiffNode)diffNode;
            CurrentDiff = -1;

            InitializeComponent();

            localText = new TextDiffTwoWayArea(DiffNode, TextDiffTwoWayArea.TargetFileEnum.Local);
            remoteText = new TextDiffTwoWayArea(DiffNode, TextDiffTwoWayArea.TargetFileEnum.Remote);

            LineMarkersPanel.Content = lineMarkers = new LineMarkersTwoWayElement(DiffNode, localText, remoteText);

            ScrollViewerLocal.Content = localText;
            ScrollViewerRemote.Content = remoteText;

            localText.OnDiffSelected += selected => CurrentDiff = selected;
            remoteText.OnDiffSelected += selected => CurrentDiff = selected;

            localText.OnDiffChange += InvalidateAllVisual;
            remoteText.OnDiffChange += InvalidateAllVisual;

            localText.OnHorizontalScroll += offset => remoteText.SetHorizontalOffset(offset);
            remoteText.OnHorizontalScroll += offset => localText.SetHorizontalOffset(offset);

            localText.OnVerticalScrollSynchronization += offset =>
            {
                int difference = DiffNode.Diff == null ? 0 : ((Diff)DiffNode.Diff).Items
                    .TakeWhile(diffItem => diffItem.LocalLineStart <= localText.StartsOnLine)
                    .Sum(diffItem => diffItem.RemoteAffectedLines - diffItem.LocalAffectedLines);

                remoteText.SetVerticalOffsetWithoutSynchornizing(offset, difference);
            };

            remoteText.OnVerticalScrollSynchronization += offset =>
            {
                int difference = DiffNode.Diff == null ? 0 : ((Diff)DiffNode.Diff).Items
                    .TakeWhile(diffItem => diffItem.RemoteLineStart <= remoteText.StartsOnLine)
                    .Sum(diffItem => diffItem.LocalAffectedLines - diffItem.RemoteAffectedLines);

                localText.SetVerticalOffsetWithoutSynchornizing(offset, difference);
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

            return diffNode.Mode == DiffModeEnum.TwoWay;
        }

        /// <summary>
        /// Invalidates visuals of all controls.
        /// </summary>
        private void InvalidateAllVisual()
        {
            localText.InvalidateVisual();
            remoteText.InvalidateVisual();
            lineMarkers.InvalidateVisual();
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
        /// Invalidates file contents and forces them to redraw.
        /// </summary>
        private void InvalidateAllFileContents()
        {
            localText.InvalidateFileContents();
            remoteText.InvalidateFileContents();

            InvalidateAllVisual();
        }

        /// <summary>
        /// Size changed event handler for shortening file paths.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void TextDiff2Way_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFileLocation = DiffNode.IsInLocation(LocationEnum.OnLocal)
                ? PathShortener.TrimPath(DiffNode.InfoLocal.FullName, FilePathLabel)
                : TextDiffWindows.Resources.Diff_No_File_At_Location;
            RemoteFileLocation = DiffNode.IsInLocation(LocationEnum.OnRemote)
                ? PathShortener.TrimPath(DiffNode.InfoRemote.FullName, FilePathLabel)
                : TextDiffWindows.Resources.Diff_No_File_At_Location;
        }

        /// <summary>
        /// Scrolls to a given line in all three controls.
        /// </summary>
        /// <param name="diffIndex">Index of the diff to scroll to.</param>
        private void ScrollToLine(int diffIndex)
        {
            localText.ScrollToLine(((Diff)DiffNode.Diff).Items[diffIndex].LocalLineStart - 1);
            remoteText.ScrollToLine(((Diff)DiffNode.Diff).Items[diffIndex].RemoteLineStart - 1);
        }

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
                (sender, args) => { args.CanExecute = DiffNode.Diff != null && CurrentDiff < ((Diff)DiffNode.Diff).Items.Length - 1; });
        }

        /// <inheritdoc />
        public CommandBinding RecalculateCommandBinding(ICommand command)
        {
            return new CommandBinding(command,
                (sender, args) =>
                {
                    manager.RequestDiff(this);
                },
                (sender, args) => args.CanExecute = true);
        }

        #endregion
    }
}
