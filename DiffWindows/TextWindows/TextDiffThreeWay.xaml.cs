using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CoreLibrary.DiffWindow;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using DiffAlgorithm.ThreeWay;
using DiffIntegration.DiffFilesystemTree;
using DiffWindows.FolderWindows;
using DiffWindows.TextWindows.Controls;
using DiffWindows.TextWindows.Controls.LineMarkers;

namespace DiffWindows.TextWindows
{
    /// <summary>
    /// Interaction logic for TextDiffThreeWay.xaml
    /// </summary>
    [DiffWindow(200)]
    public partial class TextDiffThreeWay : UserControl, IDiffWindow
    {
        private DiffFileNode node;

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


        public TextDiffThreeWay(object diffNode)
        {
            InitializeComponent();
            node = (DiffFileNode)diffNode;

            InitializeComponent();

            var localText = new TextDiff3Area(node, TextDiff3Area.TargetFileEnum.Local);
            var remoteText = new TextDiff3Area(node, TextDiff3Area.TargetFileEnum.Remote);
            var baseText = new TextDiff3Area(node, TextDiff3Area.TargetFileEnum.Base);

            ScrollViewerLocal.Content = localText;
            ScrollViewerRemote.Content = remoteText;
            ScrollViewerBase.Content = baseText;

            localText.OnHorizontalScroll += offset =>
            {
                baseText.SetHorizontalOffset(offset);
            };

            remoteText.OnHorizontalScroll += offset =>
            {
                baseText.SetHorizontalOffset(offset);
            };

            baseText.OnHorizontalScroll += offset =>
            {
                localText.SetHorizontalOffset(offset);
                remoteText.SetHorizontalOffset(offset);
            };

            localText.OnVerticalScrollSynchronization += offset =>
            {
                int differenceToBase = 0;
                int differenceToRemote = 0;
                foreach (Diff3Item diffItem in node.Diff3.Items
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
                foreach (Diff3Item diffItem in node.Diff3.Items
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
                foreach (Diff3Item diffItem in node.Diff3.Items
                    .TakeWhile(diffItem => diffItem.LocalLineStart <= localText.StartsOnLine))
                {
                    differenceToRemote += (diffItem.RemoteAffectedLines - diffItem.BaseAffectedLines);
                    differenceToLocal += (diffItem.LocalAffectedLines - diffItem.BaseAffectedLines);
                }

                remoteText.SetVerticalOffsetWithoutSynchornizing(offset, differenceToRemote);
                localText.SetVerticalOffsetWithoutSynchornizing(offset, differenceToLocal);
            };

            LineMarkersPanelLocal.Content = new LineMarkersThreeWayElement(node, localText, baseText, LineMarkersThreeWayElement.MarkerTypeEnum.BaseLeft);
            LineMarkersPanelRemote.Content = new LineMarkersThreeWayElement(node, baseText, remoteText, LineMarkersThreeWayElement.MarkerTypeEnum.BaseRight);
        }

        public static bool CanBeApplied(object instance)
        {
            var diffNode = instance as DiffFileNode;

            if (diffNode == null)
                return false;

            if (diffNode.FileType != FileTypeEnum.Text)
                return false;

            return diffNode.Mode == DiffModeEnum.ThreeWay;
        }

        private void TextDiffThreeWay_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFileLocation = node.IsInLocation(LocationEnum.OnLocal)
                ? PathHelper.TrimPath(node.InfoLocal.FullName, FilePathLabel)
                : DiffWindows.Resources.Diff_No_File_At_Location;
            RemoteFileLocation = node.IsInLocation(LocationEnum.OnRemote)
                ? PathHelper.TrimPath(node.InfoRemote.FullName, FilePathLabel)
                : DiffWindows.Resources.Diff_No_File_At_Location;
            BaseFileLocation = node.IsInLocation(LocationEnum.OnBase)
                ? PathHelper.TrimPath(node.InfoBase.FullName, FilePathLabel)
                : DiffWindows.Resources.Diff_No_File_At_Location;
        }
    }
}
