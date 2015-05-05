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
    /// <summary>
    /// Interaction logic for DefaultTwoWayDiffWindow.xaml
    /// </summary>
    [DiffWindow(int.MaxValue - 1)]
    public partial class DefaultTwoWayDiffWindow : UserControl, IDiffWindow<IFilesystemTreeAbstractNode>
    {
        private readonly IDiffWindowManager manager;
        public IFilesystemTreeAbstractNode DiffNode { get; private set; }

        #region Dependency properties

        public static readonly DependencyProperty LocalFileLocationProperty
            = DependencyProperty.Register("LocalFileLocation", typeof(string), typeof(DefaultTwoWayDiffWindow));

        public string LocalFileLocation
        {
            get { return (string)GetValue(LocalFileLocationProperty); }
            set { SetValue(LocalFileLocationProperty, value); }
        }

        public static readonly DependencyProperty RemoteFileLocationProperty
            = DependencyProperty.Register("RemoteFileLocation", typeof(string), typeof(DefaultTwoWayDiffWindow));

        public string RemoteFileLocation
        {
            get { return (string)GetValue(RemoteFileLocationProperty); }
            set { SetValue(RemoteFileLocationProperty, value); }
        }

        #endregion


        public DefaultTwoWayDiffWindow(IFilesystemTreeVisitable instance, IDiffWindowManager manager)
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

            if (DiffNode.IsInLocation(LocationCombinationsEnum.OnRemote))
            {
                RemoteFileSize.Content = string.Format("{0:0.#}kB", ((FileInfo)DiffNode.InfoRemote).Length / 1024.0);
                RemoteFileDate.Content = ((FileInfo)DiffNode.InfoRemote).LastWriteTime;
            }
        }
        public static bool CanBeApplied(object instance)
        {
            return instance is IFilesystemTreeAbstractNode
                && ((IFilesystemTreeAbstractNode)instance).Mode == DiffModeEnum.TwoWay;
        }

        private void DefaultThreeWayDiffWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocalFileLocation = DiffNode.IsInLocation(LocationEnum.OnLocal)
                ? PathShortener.TrimPath(DiffNode.InfoLocal.FullName, FilePathLabel)
                : Properties.Resources.Diff_No_File_At_Location;
            RemoteFileLocation = DiffNode.IsInLocation(LocationEnum.OnRemote)
                ? PathShortener.TrimPath(DiffNode.InfoRemote.FullName, FilePathLabel)
                : Properties.Resources.Diff_No_File_At_Location;
        }

        public void OnDiffComplete(Task task)
        {
        }

        public void OnMergeComplete(Task task)
        {
        }
    }
}
