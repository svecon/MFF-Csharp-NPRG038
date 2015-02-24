using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Interfaces;
using DiffAlgorithm;

namespace DiffIntegration.DiffFilesystemTree
{
    public class DiffFileNode : FileNode
    {
        public Diff Diff { get; private set; }

        public struct NumberOfLinesStruct
        {
            public int Base;
            public int Left;
            public int Right;
        }

        public NumberOfLinesStruct NumberOfLines;

        public DiffFileNode(IFilesystemTreeDirNode parentNode, FileInfo info, LocationEnum location, DiffModeEnum mode)
            : base(parentNode, info, location, mode)
        {
        }

        public DiffFileNode(FileInfo infoLeft, FileInfo infoRight)
            : base(null, null, (LocationEnum)LocationCombinationsEnum.OnLeftRight, DiffModeEnum.TwoWay)
        {
            InfoLeft = infoLeft;
            InfoRight = infoRight;
        }

        public DiffFileNode(FileInfo infoBase, FileInfo infoLeft, FileInfo infoRight)
            : base(null, null, (LocationEnum)LocationCombinationsEnum.OnAll3, DiffModeEnum.ThreeWay)
        {
            InfoBase = infoBase;
            InfoLeft = infoLeft;
            InfoRight = infoRight;
        }

        public void RecalculateDiff()
        {
            var diff = new DiffHelper();

            if (Mode == DiffModeEnum.TwoWay)
            {
                Diff = diff.DiffFiles((FileInfo)InfoLeft, (FileInfo)InfoRight);
                //TODO: load plugin from somewhere --- because of settings    
            } else if (Mode == DiffModeEnum.ThreeWay)
            {
                var x = diff.DiffFiles((FileInfo)InfoBase, (FileInfo)InfoLeft, (FileInfo)InfoRight);
            }

            
        }
    }
}
