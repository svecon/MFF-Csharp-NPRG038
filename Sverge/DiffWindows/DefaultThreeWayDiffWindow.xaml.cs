using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Helpers;
using CoreLibrary.Plugins.DiffWindow;

namespace Sverge.DiffWindows
{
    using DW = IDiffWindow<IFilesystemTreeVisitable>;

    /// <summary>
    /// Interaction logic for DefaultThreeWayDiffWindow.xaml
    /// </summary>
    [DiffWindow(int.MaxValue)]
    public partial class DefaultThreeWayDiffWindow : UserControl, IDiffWindow<IFilesystemTreeAbstractNode>
    {
        private readonly IDiffWindowManager manager;
        public IFilesystemTreeAbstractNode DiffNode { get; private set; }

        #region Dependency properties

        public static readonly DependencyProperty LocalFileLocationProperty
            = DependencyProperty.Register("LocalFileLocation", typeof(string), typeof(DefaultThreeWayDiffWindow));

        public string LocalFileLocation
        {
            get { return (string)GetValue(LocalFileLocationProperty); }
            set { SetValue(LocalFileLocationProperty, value); }
        }

        public static readonly DependencyProperty RemoteFileLocationProperty
            = DependencyProperty.Register("RemoteFileLocation", typeof(string), typeof(DefaultThreeWayDiffWindow));

        public string RemoteFileLocation
        {
            get { return (string)GetValue(RemoteFileLocationProperty); }
            set { SetValue(RemoteFileLocationProperty, value); }
        }

        public static readonly DependencyProperty BaseFileLocationProperty
            = DependencyProperty.Register("BaseFileLocation", typeof(string), typeof(DefaultThreeWayDiffWindow));

        public string BaseFileLocation
        {
            get { return (string)GetValue(BaseFileLocationProperty); }
            set { SetValue(BaseFileLocationProperty, value); }
        }

        #endregion

        public DefaultThreeWayDiffWindow(IFilesystemTreeVisitable instance, IDiffWindowManager manager)
        {
            this.manager = manager;
            DiffNode = (IFilesystemTreeAbstractNode)instance;

            InitializeComponent();

            // only for files
            if (!(DiffNode is IFilesystemTreeFileNode))
                return;

            if (DiffNode.IsInLocation(LocationCombinationsEnum.OnLocal))
            {
                LocalFileSize.Content = string.Format("{0:0.#}kB", ((FileInfo)DiffNode.InfoLocal).Length / 1024.0);
                LocalFileDate.Content = ((FileInfo)DiffNode.InfoLocal).LastWriteTime;
            }

            if (DiffNode.IsInLocation(LocationCombinationsEnum.OnBase))
            {
                BaseFileSize.Content = string.Format("{0:0.#}kB", ((FileInfo)DiffNode.InfoBase).Length / 1024.0);
                BaseFileDate.Content = ((FileInfo)DiffNode.InfoBase).LastWriteTime;
            }

            if (DiffNode.IsInLocation(LocationCombinationsEnum.OnRemote))
            {
                RemoteFileSize.Content = string.Format("{0:0.#}kB", ((FileInfo)DiffNode.InfoRemote).Length / 1024.0);
                RemoteFileDate.Content = ((FileInfo)DiffNode.InfoRemote).LastWriteTime;
            }

        }

        public static bool CanBeApplied(object instance)
        {
            return true;
        }

        public void OnDiffComplete(Task t)
        {
        }

        public void OnMergeComplete(Task t)
        {
        }

        private void DefaultDiffWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFileLocation = DiffNode.IsInLocation(LocationEnum.OnLocal)
                ? PathShortener.TrimPath(DiffNode.InfoLocal.FullName, FilePathLabel)
                : Properties.Resources.Diff_No_File_At_Location;
            RemoteFileLocation = DiffNode.IsInLocation(LocationEnum.OnRemote)
                ? PathShortener.TrimPath(DiffNode.InfoRemote.FullName, FilePathLabel)
                : Properties.Resources.Diff_No_File_At_Location;
            BaseFileLocation = DiffNode.IsInLocation(LocationEnum.OnBase)
                ? PathShortener.TrimPath(DiffNode.InfoBase.FullName, FilePathLabel)
                : Properties.Resources.Diff_No_File_At_Location;
        }
    }
}
