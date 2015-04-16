using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using CoreLibrary.Settings.Attributes;

namespace DiffIntegration.Processors.Postprocessors
{
    /// <summary>
    /// This processor synchronizes two folders based on chosen method.
    /// 
    /// Default method is Modification time of the file.
    /// </summary>
    [Processor(ProcessorTypeEnum.Merge, 500, DiffModeEnum.TwoWay)]
    public class SyncTwoWayProcessor : ProcessorAbstract
    {
        public enum CompareOnEnum { Size = 1, Modification = 2 }

        [Settings("Sync 2 folders with the newer version of the files.", "sync", "s")]
        public bool IsEnabled = false;

        [Settings("Choose syncing based on different criteria.", "sync-criteria", "S")]
        public CompareOnEnum CompareOn = CompareOnEnum.Modification;

        [Settings("Create empty folders.", "empty-folders", "Ef")]
        public bool CreateEmptyFolders = false;

        protected override bool CheckStatus(IFilesystemTreeDirNode node)
        {
            return base.CheckStatus(node) && IsEnabled && CreateEmptyFolders;
        }

        protected override bool CheckStatus(IFilesystemTreeFileNode node)
        {
            return base.CheckStatus(node) && IsEnabled && node.Differences != DifferencesStatusEnum.AllSame;
        }

        protected override void ProcessChecked(IFilesystemTreeDirNode node)
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

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
        {
            FileInfo from = null;
            string to = null;

            // one file is missing
            if (node.Location < (int)LocationCombinationsEnum.OnLocalRemote)
            {
                if (node.IsInLocation(LocationEnum.OnLocal))
                {
                    from = (FileInfo)node.InfoLocal;
                    to = node.GetAbsolutePath(LocationEnum.OnRemote);

                } else if (node.IsInLocation(LocationEnum.OnRemote))
                {
                    from = (FileInfo)node.InfoRemote;
                    to = node.GetAbsolutePath(LocationEnum.OnLocal);

                } else
                {
                    throw new InvalidDataException();
                }

                CheckAndCreateDirectoryFromFilename(to);
                from.CopyTo(to);
                node.Status = NodeStatusEnum.WasMerged;
                return;
            }

            // both files are present

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
