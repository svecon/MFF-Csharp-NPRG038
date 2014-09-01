﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;

namespace CoreLibrary.Processors.Postprocessors
{
    class SyncMergeProcessor : AbstractPostProcessor
    {
        public override int Priority { get { return 10000; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay; } }

        enum CompareOnEnum { Size = 1, Modification = 2 }

        CompareOnEnum compareOn = CompareOnEnum.Modification;

        bool createEmptyFolders = false;

        public override void Process(IFilesystemTreeDirNode node)
        {
            // create directory only when file is created

            // this means that empty folders need to be created here

            if (!checkModeAndStatus(node))
                return;

            // processor setting
            if (!createEmptyFolders)
                return;

            // if there are any files, folder will be created implicitly
            if (node.Files.Count > 0)
                return;

            // otherwise create empty folder
            if (node.IsInLocation(LocationEnum.OnLeft))
                checkAndCreateDirectory(node.GetAbsolutePath(LocationEnum.OnRight));

            if (node.IsInLocation(LocationEnum.OnRight))
                checkAndCreateDirectory(node.GetAbsolutePath(LocationEnum.OnLeft));
        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            if (!checkModeAndStatus(node))
                return;

            if (node.Differences == DifferencesStatusEnum.LeftRightSame)
                return;

            FileInfo from = null;
            string to = null;

            // one file is missing
            if (node.Location < (int)LocationCombinationsEnum.OnLeftRight)
            {
                if (node.IsInLocation(LocationEnum.OnLeft))
                {
                    from = (FileInfo)node.InfoLeft;
                    to = node.GetAbsolutePath(LocationEnum.OnRight);

                } else if (node.IsInLocation(LocationEnum.OnRight))
                {
                    from = (FileInfo)node.InfoRight;
                    to = node.GetAbsolutePath(LocationEnum.OnLeft);

                } else
                {
                    throw new InvalidDataException();
                }

                checkAndCreateDirectoryFromFilename(to);
                from.CopyTo(to);
                return;
            }

            // both files are present

            int comparison = 0;
            switch (compareOn)
            {
                case CompareOnEnum.Size:
                    comparison = node.InfoLeft.LastWriteTime.CompareTo(node.InfoRight.LastWriteTime);
                    break;

                case CompareOnEnum.Modification:
                    comparison = node.InfoLeft.LastWriteTime.CompareTo(node.InfoRight.LastWriteTime);
                    break;
            }

            switch (comparison)
            {
                case -1:
                    from = (FileInfo)node.InfoRight;
                    to = node.GetAbsolutePath(LocationEnum.OnLeft); ;
                    break;
                case 0:
                    // files have modification date, skip everything
                    return;
                case 1:
                    from = (FileInfo)node.InfoLeft;
                    to = node.GetAbsolutePath(LocationEnum.OnRight); ;
                    break;
            }

            checkAndCreateDirectoryFromFilename(to);
            from.CopyTo(to, true);
        }

        private void checkAndCreateDirectoryFromFilename(string path)
        {
            checkAndCreateDirectory(Path.GetDirectoryName(path));
        }

        private void checkAndCreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
