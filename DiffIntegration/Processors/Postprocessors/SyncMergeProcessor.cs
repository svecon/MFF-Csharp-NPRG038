using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Postprocessors;
using CoreLibrary.Settings.Attributes;

namespace SyncFolders.Processors.Postprocessors
{
    class SyncMergeProcessor : PostProcessorAbstract
    {
        public override int Priority { get { return 10000; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay; } }

        public enum CompareOnEnum { Size = 1, Modification = 2 }

        [Settings("Choose syncing based on different criteria.", "sync-criteria", "S")]
        public CompareOnEnum CompareOn = CompareOnEnum.Modification;

        [Settings("Create empty folders.", "empty-folders", "E")]
        public bool CreateEmptyFolders = false;

        public override void Process(IFilesystemTreeDirNode node)
        {
            // create directory only when file is created

            // this means that empty folders need to be created here

            if (!CheckModeAndStatus(node))
                return;

            // processor setting
            if (!CreateEmptyFolders)
                return;

            // if there are any files, folder will be created implicitly
            if (node.Files.Count > 0)
                return;

            // otherwise create empty folder
            if (node.IsInLocation(LocationEnum.OnLocal))
                CheckAndCreateDirectory(node.GetAbsolutePath(LocationEnum.OnRemote));

            if (node.IsInLocation(LocationEnum.OnRemote))
                CheckAndCreateDirectory(node.GetAbsolutePath(LocationEnum.OnLocal));
        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            if (!CheckModeAndStatus(node))
                return;

            if (node.Differences == DifferencesStatusEnum.LocalRemoteSame)
                return;

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
