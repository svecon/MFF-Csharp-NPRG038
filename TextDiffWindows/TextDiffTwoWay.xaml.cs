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
    /// Interaction logic for TextDiffTwoWay.xaml
    /// </summary>
    [DiffWindow(100)]
    public partial class TextDiffTwoWay : UserControl, IDiffWindow<FileDiffNode>, IChangesMenu
    {
        private readonly IDiffWindowManager manager;
        public FileDiffNode DiffNode { get; private set; }
        private readonly TextDiffTwoWayArea localText;
        private readonly TextDiffTwoWayArea remoteText;
        private readonly LineMarkersTwoWayElement lineMarkers;
        public int CurrentDiff { get; internal set; }

        public static readonly DependencyProperty LocalFileLocationProperty
            = DependencyProperty.Register("LocalFileLocation", typeof(string), typeof(TextDiffTwoWay));

        public string LocalFileLocation
        {
            get { return (string)GetValue(LocalFileLocationProperty); }
            set { SetValue(LocalFileLocationProperty, value); }
        }

        public static readonly DependencyProperty RemoteFileLocationProperty
            = DependencyProperty.Register("RemoteFileLocation", typeof(string), typeof(TextDiffTwoWay));

        public string RemoteFileLocation
        {
            get { return (string)GetValue(RemoteFileLocationProperty); }
            set { SetValue(RemoteFileLocationProperty, value); }
        }

        public TextDiffTwoWay(INodeVisitable diffNode, IDiffWindowManager manager)
        {
            this.manager = manager;
            DiffNode = (FileDiffNode)diffNode;
            CurrentDiff = -1;

            InitializeComponent();

            localText = new TextDiffTwoWayArea(DiffNode, TextDiffTwoWayArea.TargetFileEnum.Local);
            remoteText = new TextDiffTwoWayArea(DiffNode, TextDiffTwoWayArea.TargetFileEnum.Remote);

            ScrollViewerLocal.Content = localText;
            ScrollViewerRemote.Content = remoteText;

            localText.OnDiffSelected += selected => CurrentDiff = selected;
            remoteText.OnDiffSelected += selected => CurrentDiff = selected;

            localText.OnDiffChange += InvalidateAllVisual;
            remoteText.OnDiffChange += InvalidateAllVisual;

            localText.OnHorizontalScroll += offset =>
            {
                remoteText.SetHorizontalOffset(offset);
            };

            remoteText.OnHorizontalScroll += offset =>
            {
                localText.SetHorizontalOffset(offset);
            };

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

            LineMarkersPanel.Content = lineMarkers = new LineMarkersTwoWayElement(DiffNode, localText, remoteText);
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

            return diffNode.Mode == DiffModeEnum.TwoWay;
        }

        private void InvalidateAllVisual()
        {
            localText.InvalidateVisual();
            remoteText.InvalidateVisual();
            lineMarkers.InvalidateVisual();
        }

        public void OnDiffComplete(Task t)
        {
            InvalidateAllFileContents();
        }

        public void OnMergeComplete(Task t)
        {
            InvalidateAllFileContents();
        }

        private void InvalidateAllFileContents()
        {
            localText.InvalidateFileContents();
            remoteText.InvalidateFileContents();

            InvalidateAllVisual();
        }

        private void TextDiff2Way_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFileLocation = DiffNode.IsInLocation(LocationEnum.OnLocal)
                ? PathShortener.TrimPath(DiffNode.InfoLocal.FullName, FilePathLabel)
                : TextDiffWindows.Resources.Diff_No_File_At_Location;
            RemoteFileLocation = DiffNode.IsInLocation(LocationEnum.OnRemote)
                ? PathShortener.TrimPath(DiffNode.InfoRemote.FullName, FilePathLabel)
                : TextDiffWindows.Resources.Diff_No_File_At_Location;
        }

        private void ScrollToLine(int diffIndex)
        {
            localText.ScrollToLine(((Diff)DiffNode.Diff).Items[diffIndex].LocalLineStart - 1);
            remoteText.ScrollToLine(((Diff)DiffNode.Diff).Items[diffIndex].RemoteLineStart - 1);
        }

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
                (sender, args) => { args.CanExecute = DiffNode.Diff != null && CurrentDiff < ((Diff)DiffNode.Diff).Items.Length - 1; });
        }

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
