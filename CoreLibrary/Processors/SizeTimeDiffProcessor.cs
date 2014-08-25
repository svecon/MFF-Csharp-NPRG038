using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Processors
{
    class SizeTimeDiffProcessor : IPreProcessor
    {
        public void Process(IFilesystemTreeDirNode node)
        {
        }

        public void Process(IFilesystemTreeFileNode node)
        {
            var threeWay = new ThreeWayDiffHelper();

            // check for existence
            if (node.IsInLocation(LocationEnum.OnBase))
                threeWay.AddBaseFilePossibility();

            if (node.IsInLocation(LocationEnum.OnLeft))
                threeWay.AddLeftFilePossibility();

            if (node.IsInLocation(LocationEnum.OnRight))
                threeWay.AddRightFilePossibility();

            // create combinations
            threeWay.RecalculatePossibleCombinations();

            try
            {
                FileInfo InfoBase = (FileInfo)node.InfoBase;
                FileInfo InfoLeft = (FileInfo)node.InfoLeft;
                FileInfo InfoRight = (FileInfo)node.InfoRight;

                // check for sizes
                if (threeWay.CanCombinationBaseLeftBeSame())
                    threeWay.CheckCombinationBaseLeft(InfoBase.Length != InfoLeft.Length);
                if (threeWay.CanCombinationBaseRightBeSame())
                    threeWay.CheckCombinationBaseRight(InfoBase.Length != InfoRight.Length);
                if (threeWay.CanCombinationLeftRightBeSame())
                    threeWay.CheckCombinationLeftRight(InfoLeft.Length != InfoRight.Length);

                // check for modifications
                if (threeWay.CanCombinationBaseLeftBeSame())
                    threeWay.CheckCombinationBaseLeft(InfoBase.LastWriteTime != InfoLeft.LastWriteTime);
                if (threeWay.CanCombinationBaseRightBeSame())
                    threeWay.CheckCombinationBaseRight(InfoBase.LastWriteTime != InfoRight.LastWriteTime);
                if (threeWay.CanCombinationLeftRightBeSame())
                    threeWay.CheckCombinationLeftRight(InfoLeft.LastWriteTime != InfoRight.LastWriteTime);

                node.Differences = (DifferencesStatus)threeWay.GetSameFiles();
                node.Status = NodeStatus.WasDiffed;

            } catch (Exception e)
            {
                Console.WriteLine(e);
                node.Status = NodeStatus.HasError;
            }

            return;
        }

        public int Priority { get { return 10; } }

        public DiffModeEnum Mode { get { return DiffModeEnum.ThreeWay; } }
    }
}
