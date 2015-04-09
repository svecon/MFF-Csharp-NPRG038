using System.Windows.Controls;
using CoreLibrary.DiffWindow;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using DiffIntegration.DiffFilesystemTree;
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
    }
}
