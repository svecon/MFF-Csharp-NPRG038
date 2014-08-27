using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Processors
{
    class SyncMergeProcessor : IProcessor
    {
        enum CompareOnEnum { Size = 1, Modification = 2 }

        CompareOnEnum compareOn = CompareOnEnum.Modification;

        public void Process(IFilesystemTreeDirNode node)
        {
            if (node.Mode != Enums.DiffModeEnum.TwoWay)
                return;

            if (node.Differences == Enums.DifferencesStatusEnum.LeftRightSame)
                return;

            if (node.Location == (int)Enums.LocationCombinationsEnum.OnLeftRight)
                return;

            string create;

            if (node.IsInLocation(LocationEnum.OnLeft))
            {
                create = node.GetAbsolutePath(LocationEnum.OnRight);
            } else
            {
                create = node.GetAbsolutePath(LocationEnum.OnLeft);
            }

            Directory.CreateDirectory(create);
        }

        public void Process(IFilesystemTreeFileNode node)
        {
            if (node.Mode != Enums.DiffModeEnum.TwoWay)
                return;

            if (node.Status != Enums.NodeStatusEnum.WasDiffed)
                return;

            if (node.Differences == Enums.DifferencesStatusEnum.LeftRightSame)
                return;

            System.Diagnostics.Debug.WriteLine("syncing files ---------");

            FileInfo from = null;
            string to = null;

            // one file is missing
            if (node.Location < (int)Enums.LocationCombinationsEnum.OnLeftRight)
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

            from.CopyTo(to, true);
        }

        protected void sync2Way()
        {

        }

        protected void sync3Way()
        {

        }

        public int Priority { get { return 10000; } }

        public Enums.DiffModeEnum Mode { get { return Enums.DiffModeEnum.ThreeWay; } }
    }
}
