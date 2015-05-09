using System.IO;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;

namespace BasicProcessors.MergeProcessors
{
    /// <summary>
    /// This processor synchronizes two folders based on chosen method.
    /// 
    /// Default method is Modification time of the file.
    /// </summary>
    [Processor(ProcessorTypeEnum.Merge, 5000, DiffModeEnum.TwoWay)]
    public class SyncTwoWayProcessor : ProcessorAbstract
    {
        /// <summary>
        /// Enum for choosing comparison method.
        /// </summary>
        public enum CompareOnEnum
        {
            /// <summary>
            /// Compare based on size (choose bigger).
            /// </summary>
            Size = 1, 
            
            /// <summary>
            /// Compare based on last modification time.
            /// </summary>
            Modification = 2
        }

        /// <summary>
        /// Enables <see cref="SyncTwoWayProcessor"/>
        /// </summary>
        [Settings("Sync 2 folders with the newer version of the files.", "sync", "s")]
        public bool IsEnabled = false;

        /// <summary>
        /// Comparison method.
        /// </summary>
        [Settings("Choose syncing based on different criteria.", "sync-criteria", "S")]
        public CompareOnEnum CompareOn = CompareOnEnum.Modification;

        /// <summary>
        /// Creates empty folders during syncing.
        /// </summary>
        [Settings("Create empty folders.", "empty-folders", "Ef")]
        public bool CreateEmptyFolders = false;

        protected override bool CheckStatus(INodeDirNode node)
        {
            return base.CheckStatus(node) && IsEnabled && CreateEmptyFolders;
        }

        protected override bool CheckStatus(INodeFileNode node)
        {
            return base.CheckStatus(node)
                && IsEnabled
                && node.Differences != DifferencesStatusEnum.AllSame
                && node.Status != NodeStatusEnum.WasMerged;
        }

        protected override void ProcessChecked(INodeDirNode node)
        {
            // create directory only when file is created
            // this means that empty folders need to be created here

            // if there are any files, folder will be created implicitly
            if (node.Files.Count > 0)
                return;

            // otherwise create empty folder
            if (node.IsInLocation(LocationEnum.OnLocal))
                CheckAndCreateDirectory(node.GetAbsolutePath(LocationEnum.OnRemote));

            if (node.IsInLocation(LocationEnum.OnRemote))
                CheckAndCreateDirectory(node.GetAbsolutePath(LocationEnum.OnLocal));
        }

        protected override void ProcessChecked(INodeFileNode node)
        {
            FileInfo from = null;
            string to = null;

            // one file is missing
            if (node.Location < (int)LocationCombinationsEnum.OnLocalRemote)
            {
                LocationEnum toLocationEnum;
                if (node.IsInLocation(LocationEnum.OnLocal))
                {
                    from = (FileInfo)node.InfoLocal;
                    to = node.GetAbsolutePath(LocationEnum.OnRemote);
                    toLocationEnum = LocationEnum.OnRemote;

                } else if (node.IsInLocation(LocationEnum.OnRemote))
                {
                    from = (FileInfo)node.InfoRemote;
                    to = node.GetAbsolutePath(LocationEnum.OnLocal);
                    toLocationEnum = LocationEnum.OnLocal;

                } else
                {
                    throw new InvalidDataException();
                }

                CheckAndCreateDirectoryFromFilename(to);
                from.CopyTo(to);
                node.AddInfoFromLocation(new FileInfo(to), toLocationEnum);
                node.Status = NodeStatusEnum.WasMerged;
                return;
            }

            // both files are present and action is set
            var dnode = node as FileDiffNode;
            if (dnode != null && dnode.Action != PreferedActionThreeWayEnum.Default)
            {
                if (dnode.Action == PreferedActionThreeWayEnum.ApplyLocal)
                {
                    File.Copy(dnode.InfoLocal.FullName, dnode.InfoRemote.FullName, true);
                    node.AddInfoFromLocation(new FileInfo(dnode.InfoRemote.FullName), LocationEnum.OnRemote);
                } else
                {
                    File.Copy(dnode.InfoRemote.FullName, dnode.InfoLocal.FullName, true);
                    node.AddInfoFromLocation(new FileInfo(dnode.InfoLocal.FullName), LocationEnum.OnLocal);
                }
                node.Status = NodeStatusEnum.WasMerged;
                return;
            }

            // decide on size and modification
            int comparison = 0;
            switch (CompareOn)
            {
                case CompareOnEnum.Size:
                    comparison = node.InfoLocal.LastWriteTime.CompareTo(node.InfoRemote.LastWriteTime);
                    break;

                case CompareOnEnum.Modification:
                    comparison = node.InfoLocal.LastWriteTime.CompareTo(node.InfoRemote.LastWriteTime);
                    break;
            }

            switch (comparison)
            {
                case -1:
                    from = (FileInfo)node.InfoRemote;
                    to = node.GetAbsolutePath(LocationEnum.OnLocal); ;
                    break;
                case 0:
                    // files are same, skip everything
                    return;
                case 1:
                    from = (FileInfo)node.InfoLocal;
                    to = node.GetAbsolutePath(LocationEnum.OnRemote); ;
                    break;
            }

            CheckAndCreateDirectoryFromFilename(to);
            from.CopyTo(to, true);
            node.Status = NodeStatusEnum.WasMerged;
        }

        private void CheckAndCreateDirectoryFromFilename(string path)
        {
            CheckAndCreateDirectory(Path.GetDirectoryName(path));
        }

        private void CheckAndCreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
