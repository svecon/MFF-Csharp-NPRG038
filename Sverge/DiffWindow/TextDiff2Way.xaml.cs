using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CoreLibrary.Enums;
using DiffAlgorithm.TwoWay;
using DiffIntegration.DiffFilesystemTree;
using Sverge.Control;
using Sverge.Control.LineMarkers;

namespace Sverge.DiffWindow
{
    /// <summary>
    /// Interaction logic for TextDiff2Way.xaml
    /// </summary>
    public partial class TextDiff2Way : UserControl, IDiffWindow
    {
        private DiffFileNode node;

        public static readonly DependencyProperty LocalFileLocationProperty
            = DependencyProperty.Register("LocalFileLocation", typeof(string), typeof(TextDiff2Way));

        public string LocalFileLocation
        {
            get { return (string)GetValue(LocalFileLocationProperty); }
            set { SetValue(LocalFileLocationProperty, value); }
        }

        public static readonly DependencyProperty RemoteFileLocationProperty
            = DependencyProperty.Register("RemoteFileLocation", typeof(string), typeof(TextDiff2Way));

        public string RemoteFileLocation
        {
            get { return (string)GetValue(RemoteFileLocationProperty); }
            set { SetValue(RemoteFileLocationProperty, value); }
        }

        public TextDiff2Way(object diffNode)
        {
            node = (DiffFileNode)diffNode;

            InitializeComponent();

            var local = new TextDiffArea(node, TextDiffArea.TargetFileEnum.Local);
            var remote = new TextDiffArea(node, TextDiffArea.TargetFileEnum.Remote);

            ScrollViewerLocal.Content = local;
            ScrollViewerRemote.Content = remote;

            LineMarkersPanel.Content = new LineMarkersTwoWayElement(node, local, remote);
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
            LocalFileLocation = node.IsInLocation(LocationEnum.OnLocal)
                ? PathHelper.TrimPath(node.InfoLocal.FullName, FilePathLabel) 
                : Properties.Resources.Diff_No_File_At_Location;
            RemoteFileLocation = node.IsInLocation(LocationEnum.OnRemote)
                ? PathHelper.TrimPath(node.InfoRemote.FullName, FilePathLabel)
                : Properties.Resources.Diff_No_File_At_Location;
        }
    }
}
