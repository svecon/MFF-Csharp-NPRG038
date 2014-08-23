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
    class SizeAndTimeDifferProcessor : IPreProcessor
    {
        public bool Process(IFilesystemTreeDirNode node)
        {
            return false;
        }

        public bool Process(IFilesystemTreeFileNode node)
        {
            const int BASE_LEFT = 0x1;
            const int BASE_RIGHT = 0x2;
            const int LEFT_RIGHT = 0x4;

            const int BASE = 0x1;
            const int LEFT = 0x2;
            const int RIGHT = 0x4;

            int possibleCombinations = 0x0;
            int filesToBeChecked = 0x0;

            // check for existence
            if (node.IsInLocation(LocationEnum.OnBase))
                filesToBeChecked |= BASE;

            if (node.IsInLocation(LocationEnum.OnLeft))
                filesToBeChecked |= LEFT;

            if (node.IsInLocation(LocationEnum.OnRight))
                filesToBeChecked |= RIGHT;

            // create combinations
            if ((filesToBeChecked & (BASE | LEFT)) == (BASE | LEFT))
                possibleCombinations |= BASE_LEFT;

            if ((filesToBeChecked & (BASE | RIGHT)) == (BASE | RIGHT))
                possibleCombinations |= BASE_RIGHT;

            if ((filesToBeChecked & (LEFT | RIGHT)) == (LEFT | RIGHT))
                possibleCombinations |= LEFT_RIGHT;

            try
            {
                FileInfo InfoBase = (FileInfo)node.InfoBase;
                FileInfo InfoLeft = (FileInfo)node.InfoLeft;
                FileInfo InfoRight = (FileInfo)node.InfoRight;

                // check for sizes
                if ((possibleCombinations & BASE_LEFT) > 0 && InfoBase.Length != InfoLeft.Length)
                    possibleCombinations &= ~BASE_LEFT;

                if ((possibleCombinations & BASE_RIGHT) > 0 && InfoBase.Length != InfoRight.Length)
                    possibleCombinations &= ~BASE_RIGHT;

                if ((possibleCombinations & LEFT_RIGHT) > 0 && InfoLeft.Length != InfoRight.Length)
                    possibleCombinations &= ~LEFT_RIGHT;

                // check for modifications
                if ((possibleCombinations & BASE_LEFT) > 0 && InfoBase.LastWriteTime != InfoLeft.LastWriteTime)
                    possibleCombinations &= ~BASE_LEFT;

                if ((possibleCombinations & BASE_RIGHT) > 0 && InfoBase.LastWriteTime != InfoRight.LastWriteTime)
                    possibleCombinations &= ~BASE_RIGHT;

                if ((possibleCombinations & LEFT_RIGHT) > 0 && InfoLeft.LastWriteTime != InfoRight.LastWriteTime)
                    possibleCombinations &= ~LEFT_RIGHT;

                // recheck filesToBeChecked
                if ((possibleCombinations & (BASE_LEFT | BASE_RIGHT)) == 0)
                    filesToBeChecked &= ~BASE;

                if ((possibleCombinations & (BASE_LEFT | LEFT_RIGHT)) == 0)
                    filesToBeChecked &= ~LEFT;

                if ((possibleCombinations & (BASE_RIGHT | LEFT_RIGHT)) == 0)
                    filesToBeChecked &= ~RIGHT;

                node.Differences = (DifferencesStatus)filesToBeChecked;
                node.Status = NodeStatus.WasDiffed;

            } catch (Exception e)
            {
                Console.WriteLine(e);
                node.Status = NodeStatus.HasError;
            }

            return true;
        }

        public int Priority { get { return 10; } }

        public DiffModeEnum Mode { get { return DiffModeEnum.ThreeWay; } }
    }
}
