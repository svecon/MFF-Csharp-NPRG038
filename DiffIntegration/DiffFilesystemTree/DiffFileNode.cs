using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Interfaces;
using DiffAlgorithm;

namespace DiffIntegration.DiffFilesystemTree
{
    /// <summary>
    /// DiffFileNode enhances default FileNode for some features needed in diffing between files.
    /// </summary>
    public class DiffFileNode : FileNode
    {
        /// <summary>
        /// Diff contains all information about a 2-way diff.
        /// </summary>
        public Diff Diff { get; private set; }

        /// <summary>
        /// Diff3 contains all information about a 3-way diff.
        /// </summary>
        public Diff3 Diff3 { get; private set; }

        /// <summary>
        /// Default construtor for DiffFileNode (used in the FilesystemTree).
        /// </summary>
        /// <param name="parentNode">Parent DirNode for this node.</param>
        /// <param name="info">File info for this node.</param>
        /// <param name="location">Location where this node has been found from.</param>
        /// <param name="mode">Default diff mode.</param>
        public DiffFileNode(IFilesystemTreeDirNode parentNode, FileInfo info, LocationEnum location, DiffModeEnum mode)
            : base(parentNode, info, location, mode)
        {
        }

        /// <summary>
        /// Construtor for creating diff between two files (used separately).
        /// </summary>
        /// <param name="infoOld">FileInfo for old file.</param>
        /// <param name="infoNew">FileInfo for new file.</param>
        public DiffFileNode(FileInfo infoOld, FileInfo infoNew)
            : base(null, null, (LocationEnum)LocationCombinationsEnum.OnLeftRight, DiffModeEnum.TwoWay)
        {
            InfoLeft = infoOld;
            InfoRight = infoNew;
        }
        
        /// <summary>
        /// Construtor for creating diff between three files (used separately).
        /// </summary>
        /// <param name="infoOld">FileInfo for old file.</param>
        /// <param name="infoMyNew">FileInfo for my new file.</param>
        /// <param name="infoHisNew">FileInfo for his new file.</param>
        public DiffFileNode(FileInfo infoOld, FileInfo infoMyNew, FileInfo infoHisNew)
            : base(null, null, (LocationEnum)LocationCombinationsEnum.OnAll3, DiffModeEnum.ThreeWay)
        {
            InfoBase = infoOld;
            InfoLeft = infoMyNew;
            InfoRight = infoHisNew;
        }

        /// <summary>
        /// Recalculates diff between files currently in the node.
        /// </summary>
        public void RecalculateDiff()
        {
            var diff = new DiffHelper();

            switch (Mode)
            {
                case DiffModeEnum.TwoWay:
                    Diff = diff.DiffFiles((FileInfo)InfoLeft, (FileInfo)InfoRight);
                    break;
                //TODO: load plugin from somewhere --- because of settings    
                case DiffModeEnum.ThreeWay:
                    Diff3 x = diff.DiffFiles((FileInfo)InfoBase, (FileInfo)InfoLeft, (FileInfo)InfoRight);
                    break;
            }
        }
    }
}
