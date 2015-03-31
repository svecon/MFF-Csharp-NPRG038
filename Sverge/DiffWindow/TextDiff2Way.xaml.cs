using System.IO;
using System.Windows;
using System.Windows.Controls;
using CoreLibrary.Enums;
using DiffAlgorithm.TwoWay;
using DiffIntegration.DiffFilesystemTree;
using Sverge.Control;

namespace Sverge.DiffWindow
{
    /// <summary>
    /// Interaction logic for TextDiff2Way.xaml
    /// </summary>
    public partial class TextDiff2Way : UserControl, IDiffWindow
    {
        private DiffFileNode node;

        public TextDiff2Way(object diffNode)
        {
            node = (DiffFileNode)diffNode;

            InitializeComponent();

            var local = new TextDiffArea(node, TextDiffArea.TargetFileEnum.Local);
            var remote = new TextDiffArea(node, TextDiffArea.TargetFileEnum.Remote);
            ScrollViewerLocal.Content = local;
            ScrollViewerRemote.Content = remote;
            LineMarkersPanel.Content = new LineMarkersElement(node, local, remote);
        }

        public static bool CanBeApplied(object instance)
        {
            var diffNode = instance as DiffFileNode;

            if (diffNode == null)
                return false;

            return diffNode.Mode == DiffModeEnum.TwoWay;
        }
    }
}
