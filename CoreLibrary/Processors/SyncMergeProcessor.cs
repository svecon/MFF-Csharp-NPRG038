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
            //TODO create or delete folders
        }

        public void Process(IFilesystemTreeFileNode node)
        {
            //switch (node.Mode)
            //{
            //    case CoreLibrary.Enums.DiffModeEnum.TwoWay:
            //        sync2Way(node);
            //        break;
            //    case CoreLibrary.Enums.DiffModeEnum.ThreeWay:
            //        sync3Way(node);
            //        break;
            //    default:
            //        throw new NotImplementedException();
            //}

            if (node.Mode != Enums.DiffModeEnum.TwoWay)
                return;

            if (node.Status != Enums.NodeStatusEnum.WasDiffed)
                return;

            if (node.Differences == Enums.DifferencesStatusEnum.LeftRightSame)
                return;

            System.Diagnostics.Debug.WriteLine("syncing files ---------");

            FileInfo from = null;
            FileInfo to = null;

            // one file is missing
            if (node.Location < (int)Enums.LocationCombinationsEnum.OnLeftRight)
            {
                if (node.IsInLocation(Enums.LocationEnum.OnLeft))
                {
                    from = (FileInfo)node.InfoRight;
                    to = (FileInfo)node.InfoLeft;
                } else if (node.IsInLocation(Enums.LocationEnum.OnRight))
                {
                    from = (FileInfo)node.InfoLeft;
                    to = (FileInfo)node.InfoRight;
                } else
                {
                    throw new InvalidDataException();
                }

                from.CopyTo(to.FullName);
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
                    to = (FileInfo)node.InfoLeft;
                    break;
                case 0:
                    // files have modification date, skip everything
                    return;
                case 1:
                    from = (FileInfo)node.InfoLeft;
                    to = (FileInfo)node.InfoRight;
                    break;
            }

            from.CopyTo(to.FullName, true);
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
