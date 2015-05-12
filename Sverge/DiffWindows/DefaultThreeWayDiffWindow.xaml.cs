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
    public partial class DefaultThreeWayDiffWindow : UserControl, IDiffWindow<INodeVisitable>
    {
        private readonly IDiffWindowManager manager;
        public INodeVisitable DiffNode { get; private set; }

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
            DiffNode = node;

            InitializeComponent();

            // only for files
            var abstractNode = DiffNode as INodeAbstractNode;
            if (abstractNode == null)
                return;

            if (abstractNode.IsInLocation(LocationCombinationsEnum.OnLocal))
            {
                LocalFileSize.Content = string.Format("{0:0.#}kB", ((FileInfo)abstractNode.InfoLocal).Length / 1024.0);
                LocalFileDate.Content = ((FileInfo)abstractNode.InfoLocal).LastWriteTime;
            }

            if (abstractNode.IsInLocation(LocationCombinationsEnum.OnBase))
            {
                BaseFileSize.Content = string.Format("{0:0.#}kB", ((FileInfo)abstractNode.InfoBase).Length / 1024.0);
                BaseFileDate.Content = ((FileInfo)abstractNode.InfoBase).LastWriteTime;
            }

            if (abstractNode.IsInLocation(LocationCombinationsEnum.OnRemote))
            {
                RemoteFileSize.Content = string.Format("{0:0.#}kB", ((FileInfo)abstractNode.InfoRemote).Length / 1024.0);
                RemoteFileDate.Content = ((FileInfo)abstractNode.InfoRemote).LastWriteTime;
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
            var abstractNode = DiffNode as INodeAbstractNode;
            if (abstractNode != null)
            {
                LocalFileLocation = abstractNode.IsInLocation(LocationEnum.OnLocal)
                    ? PathShortener.TrimPath(abstractNode.InfoLocal.FullName, FilePathLabel)
                    : Properties.Resources.Diff_No_File_At_Location;
                RemoteFileLocation = abstractNode.IsInLocation(LocationEnum.OnRemote)
                    ? PathShortener.TrimPath(abstractNode.InfoRemote.FullName, FilePathLabel)
                    : Properties.Resources.Diff_No_File_At_Location;
                BaseFileLocation = abstractNode.IsInLocation(LocationEnum.OnBase)
                    ? PathShortener.TrimPath(abstractNode.InfoBase.FullName, FilePathLabel)
                    : Properties.Resources.Diff_No_File_At_Location;
            }

            var abstractTree = DiffNode as IFilesystemTree;
            if (abstractTree != null)
            {
                LocalFileLocation = abstractTree.Root.IsInLocation(LocationEnum.OnLocal)
                    ? PathShortener.TrimPath(abstractTree.Root.InfoLocal.FullName, FilePathLabel)
                    : Properties.Resources.Diff_No_File_At_Location;
                RemoteFileLocation = abstractTree.Root.IsInLocation(LocationEnum.OnRemote)
                    ? PathShortener.TrimPath(abstractTree.Root.InfoRemote.FullName, FilePathLabel)
                    : Properties.Resources.Diff_No_File_At_Location;
                BaseFileLocation = abstractTree.Root.IsInLocation(LocationEnum.OnBase)
                    ? PathShortener.TrimPath(abstractTree.Root.InfoBase.FullName, FilePathLabel)
                    : Properties.Resources.Diff_No_File_At_Location;
            }

        }
    }
}
