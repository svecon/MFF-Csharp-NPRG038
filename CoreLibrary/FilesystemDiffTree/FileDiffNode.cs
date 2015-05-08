using System.IO;
using CoreLibrary.Exceptions.NotFound;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;

namespace CoreLibrary.FilesystemDiffTree
{
    /// <summary>
    /// FileDiffNode enhances default FileNode for some features needed in diffing between files.
    /// </summary>
    public class FileDiffNode : FileNode
    {
        private object diff;

        /// <summary>
        /// Diff contains all information about a 3-way diff.
        /// </summary>
        public object Diff
        {
            get { return diff; }
            set { diff = value; OnPropertyChanged("Diff"); }
        }

        /// <summary>
        /// Initializes new instance of the <see cref="FileDiffNode"/>
        /// 
        /// Default construtor for FileDiffNode (used in the Node).
        /// </summary>
        /// <param name="parentNode">Parent DirNode for this node.</param>
        /// <param name="info">File info for this node.</param>
        /// <param name="location">Location where this node has been found from.</param>
        /// <param name="mode">Default diff mode.</param>
        public FileDiffNode(INodeDirNode parentNode, FileInfo info, LocationEnum location, DiffModeEnum mode)
            : base(parentNode, info, location, mode)
        {
        }

        /// <summary>
        /// Initializes new instance of the <see cref="FileDiffNode"/>
        /// 
        /// Construtor for creating diff between two files (used separately).
        /// </summary>
        /// <param name="fileLocal">Path to the local file.</param>
        /// <param name="fileRemote">Path to the remote file.</param>
        public FileDiffNode(string fileLocal, string fileRemote)
            : base(null, null, (LocationEnum)LocationCombinationsEnum.OnLocalRemote, DiffModeEnum.TwoWay)
        {
            InfoLocal = new FileInfo(fileLocal);
            InfoRemote = new FileInfo(fileRemote);

            if (!InfoLocal.Exists)
                throw new LocalFileNotFoundException(InfoLocal);
            if (!InfoRemote.Exists)
                throw new RemoteFileNotFoundException(InfoRemote);
        }

        /// <summary>
        /// Initializes new instance of the <see cref="FileDiffNode"/>
        /// 
        /// Construtor for creating diff between three files (used separately).
        /// </summary>
        /// <param name="fileLocal">Path to the local file.</param>
        /// <param name="fileBase">Path to the base file.</param>
        /// <param name="fileRemote">Path to the remote file.</param>
        public FileDiffNode(string fileLocal, string fileBase, string fileRemote)
            : base(null, null, (LocationEnum)LocationCombinationsEnum.OnAll3, DiffModeEnum.ThreeWay)
        {
            InfoBase = new FileInfo(fileBase);
            InfoLocal = new FileInfo(fileLocal);
            InfoRemote = new FileInfo(fileRemote);

            if (!InfoLocal.Exists)
                throw new LocalFileNotFoundException(InfoLocal);
            if (!InfoBase.Exists)
                throw new BaseFileNotFoundException(InfoBase);
            if (!InfoRemote.Exists)
                throw new RemoteFileNotFoundException(InfoRemote);
        }
    }
}
