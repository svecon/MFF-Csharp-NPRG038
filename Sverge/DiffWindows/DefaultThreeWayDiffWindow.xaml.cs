using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Helpers;
using CoreLibrary.Plugins.DiffWindow;

namespace Sverge.DiffWindows
{
    using DW = IDiffWindow<INodeVisitable>;

    /// <summary>
    /// Default plugin DiffWindow used as a fallback for 3-way when no better visualisation plugin is found.
    /// </summary>
    [DiffWindow(int.MaxValue)]
    public partial class DefaultThreeWayDiffWindow : UserControl, IDiffWindow<INodeAbstractNode>
    {
        private readonly IDiffWindowManager manager;
        public INodeAbstractNode DiffNode { get; private set; }

        #region Dependency properties

        /// <summary>
        /// Dependency property for <see cref="LocalFileLocation"/> 
        /// </summary>
        public static readonly DependencyProperty LocalFileLocationProperty
            = DependencyProperty.Register("LocalFileLocation", typeof(string), typeof(DefaultThreeWayDiffWindow));

        /// <summary>
        /// String path to the local file.
        /// </summary>
        public string LocalFileLocation
        {
            get { return (string)GetValue(LocalFileLocationProperty); }
            set { SetValue(LocalFileLocationProperty, value); }
        }

        /// <summary>
        /// Dependency property for <see cref="RemoteFileLocation"/> 
        /// </summary>
        public static readonly DependencyProperty RemoteFileLocationProperty
            = DependencyProperty.Register("RemoteFileLocation", typeof(string), typeof(DefaultThreeWayDiffWindow));

        /// <summary>
        /// String path to the remote file.
        /// </summary>
        public string RemoteFileLocation
        {
            get { return (string)GetValue(RemoteFileLocationProperty); }
            set { SetValue(RemoteFileLocationProperty, value); }
        }

        /// <summary>
        /// Dependency property for <see cref="BaseFileLocation"/> 
        /// </summary>
        public static readonly DependencyProperty BaseFileLocationProperty
            = DependencyProperty.Register("BaseFileLocation", typeof(string), typeof(DefaultThreeWayDiffWindow));

        /// <summary>
        /// String path to the base file.
        /// </summary>
        public string BaseFileLocation
        {
            get { return (string)GetValue(BaseFileLocationProperty); }
            set { SetValue(BaseFileLocationProperty, value); }
        }

        #endregion

        /// <summary>
        /// Initializes new node of the <see cref="DefaultThreeWayDiffWindow"/>
        /// </summary>
        /// <param name="node">Diff node holding the calculated diff.</param>
        /// <param name="manager">Manager for this window.</param>
        public DefaultThreeWayDiffWindow(INodeVisitable node, IDiffWindowManager manager)
        {
            this.manager = manager;
            DiffNode = (INodeAbstractNode)node;

            InitializeComponent();

            // only for files
            if (!(DiffNode is INodeFileNode))
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

        /// <summary>
        /// Can this <see cref="IDiffWindow{TNode}"/> be applied to given instance?
        /// </summary>
        /// <param name="instance">Instance holding the calculated diff.</param>
        /// <returns>True if this plugin can be applied.</returns>
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
