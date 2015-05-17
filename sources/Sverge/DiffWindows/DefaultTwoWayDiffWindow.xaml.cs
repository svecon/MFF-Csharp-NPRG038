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
    /// <summary>
    /// Interaction logic for DefaultTwoWayDiffWindow.xaml
    /// </summary>
    [DiffWindow(int.MaxValue - 1)]
    public partial class DefaultTwoWayDiffWindow : UserControl, IDiffWindow<INodeVisitable>
    {
        /// <inheritdoc />
        public INodeVisitable DiffNode { get; private set; }

        #region Dependency properties

        /// <summary>
        /// Dependency property for <see cref="LocalFileLocation"/> 
        /// </summary>
        public static readonly DependencyProperty LocalFileLocationProperty
            = DependencyProperty.Register("LocalFileLocation", typeof(string), typeof(DefaultTwoWayDiffWindow));

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
            = DependencyProperty.Register("RemoteFileLocation", typeof(string), typeof(DefaultTwoWayDiffWindow));

        /// <summary>
        /// String path to the remote file.
        /// </summary>
        public string RemoteFileLocation
        {
            get { return (string)GetValue(RemoteFileLocationProperty); }
            set { SetValue(RemoteFileLocationProperty, value); }
        }

        #endregion

        /// <summary>
        /// Initializes new node of the <see cref="DefaultTwoWayDiffWindow"/>
        /// </summary>
        /// <param name="node">Diff node holding the calculated diff.</param>
        /// <param name="manager">Manager for this window.</param>
        public DefaultTwoWayDiffWindow(INodeVisitable node, IDiffWindowManager manager)
        {
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
            return instance is INodeAbstractNode
                && ((INodeAbstractNode)instance).Mode == DiffModeEnum.TwoWay;
        }

        /// <summary>
        /// Event hadler fired upon resizing window. Shortens the file paths.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void DefaultThreeWayDiffWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
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
            }
        }

        /// <inheritdoc />
        public void OnDiffComplete(Task task)
        {
        }

        /// <inheritdoc />
        public void OnMergeComplete(Task task)
        {
        }
    }
}
