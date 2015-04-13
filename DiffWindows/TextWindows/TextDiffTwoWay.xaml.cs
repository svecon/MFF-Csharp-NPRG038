using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CoreLibrary.DiffWindow;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using DiffIntegration.DiffFilesystemTree;
using DiffWindows.FolderWindows;
using DiffWindows.TextWindows.Controls;
using DiffWindows.TextWindows.Controls.LineMarkers;

namespace DiffWindows.TextWindows
{
    /// <summary>
    /// Interaction logic for TextDiffTwoWay.xaml
    /// </summary>
    [DiffWindow(100)]
    public partial class TextDiffTwoWay : UserControl, IDiffWindow<DiffFileNode>
    {
        public DiffFileNode DiffNode { get; private set; }

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

        public TextDiffTwoWay(object diffNode)
        {
            DiffNode = (DiffFileNode)diffNode;

            InitializeComponent();

            var local = new TextDiffArea(DiffNode, TextDiffArea.TargetFileEnum.Local);
            var remote = new TextDiffArea(DiffNode, TextDiffArea.TargetFileEnum.Remote);

            ScrollViewerLocal.Content = local;
            ScrollViewerRemote.Content = remote;

            local.OnHorizontalScroll += offset =>
            {
                remote.SetHorizontalOffset(offset);
            };

            remote.OnHorizontalScroll += offset =>
            {
                local.SetHorizontalOffset(offset);
            };

            local.OnVerticalScrollSynchronization += offset =>
            {
                int difference = DiffNode.Diff == null ? 0 : DiffNode.Diff.Items
                    .TakeWhile(diffItem => diffItem.LocalLineStart <= local.StartsOnLine)
                    .Sum(diffItem => diffItem.RemoteAffectedLines - diffItem.LocalAffectedLines);

                remote.SetVerticalOffsetWithoutSynchornizing(offset, difference);
            };

            remote.OnVerticalScrollSynchronization += offset =>
            {
                int difference = DiffNode.Diff == null ? 0 : DiffNode.Diff.Items
                    .TakeWhile(diffItem => diffItem.RemoteLineStart <= remote.StartsOnLine)
                    .Sum(diffItem => diffItem.LocalAffectedLines - diffItem.RemoteAffectedLines);

                local.SetVerticalOffsetWithoutSynchornizing(offset, difference);
            };

            LineMarkersPanel.Content = new LineMarkersTwoWayElement(DiffNode, local, remote);
        }

        public static bool CanBeApplied(object instance)
        {
            var diffNode = instance as DiffFileNode;

            if (diffNode == null)
                return false;

            if (diffNode.FileType != FileTypeEnum.Text)
                return false;

            return diffNode.Mode == DiffModeEnum.TwoWay;
        }

        private void TextDiff2Way_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFileLocation = DiffNode.IsInLocation(LocationEnum.OnLocal)
                ? PathHelper.TrimPath(DiffNode.InfoLocal.FullName, FilePathLabel)
                : DiffWindows.Resources.Diff_No_File_At_Location;
            RemoteFileLocation = DiffNode.IsInLocation(LocationEnum.OnRemote)
                ? PathHelper.TrimPath(DiffNode.InfoRemote.FullName, FilePathLabel)
                : DiffWindows.Resources.Diff_No_File_At_Location;
        }
    }
}
