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

        string GetTrimmedPath(string path, string ellipsis = "...")
        {
            double width = FilePathLabel.ActualWidth - 30;

            string filename = Path.GetFileName(path);
            string directory = Path.GetDirectoryName(path);

            if (directory == null)
                return filename;

            bool widthOk;
            bool changedWidth = false;

            Typeface tf = FilePathLabel.FontFamily.GetTypefaces().FirstOrDefault() ?? new Typeface("Consolas");

            do
            {
                var formatted = new FormattedText(
                    String.Format("{0}{1}{2}{3}", directory, ellipsis, Path.DirectorySeparatorChar, filename),
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    tf,
                    FilePathLabel.FontSize,
                    Brushes.Black
                    );

                widthOk = formatted.Width < width;

                if (widthOk) continue;

                changedWidth = true;

                directory = directory.Substring(0, directory.Length - 1);
                if (directory.Length == 0)
                    return string.Format("{0}{1}{2}", ellipsis, Path.DirectorySeparatorChar, filename);

            } while (!widthOk);

            return !changedWidth ? path : String.Format("{0}{1}{2}{3}", directory, ellipsis, Path.DirectorySeparatorChar, filename);
        }

        private void TextDiff2Way_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFileLocation = node.IsInLocation(LocationEnum.OnLocal) 
                ? GetTrimmedPath(node.InfoLocal.FullName) 
                : Properties.Resources.Diff_No_File_At_Location;
            RemoteFileLocation = node.IsInLocation(LocationEnum.OnRemote) 
                ? GetTrimmedPath(node.InfoRemote.FullName)
                : Properties.Resources.Diff_No_File_At_Location;
        }
    }
}
