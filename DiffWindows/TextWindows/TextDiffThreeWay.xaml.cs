using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CoreLibrary.DiffWindow;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using DiffAlgorithm.ThreeWay;
using DiffIntegration.DiffFilesystemTree;
using DiffWindows.FolderWindows;
using DiffWindows.Menus;
using DiffWindows.TextWindows.Controls;
using DiffWindows.TextWindows.Controls.LineMarkers;

namespace DiffWindows.TextWindows
{
    /// <summary>
    /// Interaction logic for TextDiffThreeWay.xaml
    /// </summary>
    [DiffWindow(200)]
    public partial class TextDiffThreeWay : UserControl, IDiffWindow<DiffFileNode>, IChangesMenu
    {
        public DiffFileNode DiffNode { get; private set; }

        public int CurrentDiff { get; internal set; }

        private TextDiff3Area localText;
        private TextDiff3Area baseText;
        private TextDiff3Area remoteText;

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
            DiffNode = (DiffFileNode)diffNode;
            CurrentDiff = -1;

            InitializeComponent();

            localText = new TextDiff3Area(DiffNode, TextDiff3Area.TargetFileEnum.Local);
            remoteText = new TextDiff3Area(DiffNode, TextDiff3Area.TargetFileEnum.Remote);
            baseText = new TextDiff3Area(DiffNode, TextDiff3Area.TargetFileEnum.Base);

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

                if (DiffNode.Diff3 != null)
                    foreach (Diff3Item diffItem in DiffNode.Diff3.Items
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

                if (DiffNode.Diff3 != null)
                    foreach (Diff3Item diffItem in DiffNode.Diff3.Items
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

                if (DiffNode.Diff3 != null)
                    foreach (Diff3Item diffItem in DiffNode.Diff3.Items
                        .TakeWhile(diffItem => diffItem.LocalLineStart <= localText.StartsOnLine))
                    {
                        differenceToRemote += (diffItem.RemoteAffectedLines - diffItem.BaseAffectedLines);
                        differenceToLocal += (diffItem.LocalAffectedLines - diffItem.BaseAffectedLines);
                    }

                remoteText.SetVerticalOffsetWithoutSynchornizing(offset, differenceToRemote);
                localText.SetVerticalOffsetWithoutSynchornizing(offset, differenceToLocal);
            };

            LineMarkersPanelLocal.Content = new LineMarkersThreeWayElement(DiffNode, localText, baseText, LineMarkersThreeWayElement.MarkerTypeEnum.BaseLeft);
            LineMarkersPanelRemote.Content = new LineMarkersThreeWayElement(DiffNode, baseText, remoteText, LineMarkersThreeWayElement.MarkerTypeEnum.BaseRight);
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
            LocalFileLocation = DiffNode.IsInLocation(LocationEnum.OnLocal)
                ? PathHelper.TrimPath(DiffNode.InfoLocal.FullName, FilePathLabel)
                : DiffWindows.Resources.Diff_No_File_At_Location;
            RemoteFileLocation = DiffNode.IsInLocation(LocationEnum.OnRemote)
                ? PathHelper.TrimPath(DiffNode.InfoRemote.FullName, FilePathLabel)
                : DiffWindows.Resources.Diff_No_File_At_Location;
            BaseFileLocation = DiffNode.IsInLocation(LocationEnum.OnBase)
                ? PathHelper.TrimPath(DiffNode.InfoBase.FullName, FilePathLabel)
                : DiffWindows.Resources.Diff_No_File_At_Location;
        }

        #region Custom menu commands

        private void ScrollToCurrentDiff()
        {
            localText.ScrollToLine(DiffNode.Diff3.Items[CurrentDiff].LocalLineStart - 1);
            baseText.ScrollToLine(DiffNode.Diff3.Items[CurrentDiff].BaseLineStart - 1);
            remoteText.ScrollToLine(DiffNode.Diff3.Items[CurrentDiff].RemoteLineStart - 1);
        }

        public RoutedUICommand PreviousCommand()
        {
            return Previous;
        }

        public CommandBinding PreviousCommandBinding()
        {
            return new CommandBinding(Previous,
                (sender, args) =>
                {
                    CurrentDiff--;
                    ScrollToCurrentDiff();
                },
                (sender, args) => { args.CanExecute = DiffNode.Diff3 != null && CurrentDiff > 0; });
        }

        public static RoutedUICommand Previous = new RoutedUICommand("PreviousCommand", "PreviousCommand", typeof(TextDiff3Area),
                new InputGestureCollection() { new KeyGesture(Key.F7) }
            );

        public RoutedUICommand NextCommand()
        {
            return Next;
        }

        public CommandBinding NextCommandBinding()
        {
            return new CommandBinding(Next,
                (sender, args) =>
                {
                    CurrentDiff++;
                    ScrollToCurrentDiff();
                },
                (sender, args) => { args.CanExecute = DiffNode.Diff3 != null && CurrentDiff < DiffNode.Diff3.Items.Length - 1; });
        }

        public static RoutedUICommand Next = new RoutedUICommand("NextCommand", "NextCommand", typeof(TextDiff3Area),
                new InputGestureCollection() { new KeyGesture(Key.F8) }
            );

        #endregion
    }
}
