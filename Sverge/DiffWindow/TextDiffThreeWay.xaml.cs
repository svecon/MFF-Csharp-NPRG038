using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoreLibrary.Enums;
using DiffIntegration.DiffFilesystemTree;
using Sverge.Control;
using Sverge.Control.LineMarkers;

namespace Sverge.DiffWindow
{
    /// <summary>
    /// Interaction logic for TextDiffThreeWay.xaml
    /// </summary>
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
