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

            ScrollViewerLocal.Content = new TextDiffArea(node, TextDiffArea.TargetFileEnum.Local);
            ScrollViewerRemote.Content = new TextDiffArea(node, TextDiffArea.TargetFileEnum.Remote);
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
