using System;
using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;

namespace CoreLibrary.FilesystemTree
{
    public abstract class AbstractNode : IFilesystemTreeAbstractNode
    {
        public FileSystemInfo InfoBase { get; protected set; }

        public FileSystemInfo InfoLeft { get; protected set; }

        public FileSystemInfo InfoRight { get; protected set; }

        public NodeStatusEnum Status { get; set; }

        public DifferencesStatusEnum Differences { get; set; }

        /// <summary>
        /// Returns first (out of Base, Left or RightInfo) FileSystemInfo that is not null.
        /// </summary>
        public FileSystemInfo Info
        {
            get
            {
                if (InfoBase != null)
                {
                    return InfoBase;
                } else if (InfoLeft != null)
                {
                    return InfoLeft;
                } else // if (InfoRight != null)
                {
                    return InfoRight;
                }
            }
        }

        public int Location { get; protected set; }

        public DiffModeEnum Mode { get; protected set; }

        public abstract string GetAbsolutePath(LocationEnum location);

        /// <summary>
        /// Constructor for AbstractNode.
        /// </summary>
        /// <param name="info">FileSystemInfo for the node.</param>
        /// <param name="location">Where the node came from.</param>
        /// <param name="mode">Default mode.</param>
        protected AbstractNode(FileSystemInfo info, LocationEnum location, DiffModeEnum mode)
        {
            Differences = DifferencesStatusEnum.Initial;
            Mode = mode;
            AddInfoFromLocation(info, location);
        }

        public bool IsInLocation(LocationEnum location)
        {
            return ((int)location & Location) > 0;
        }

        /// <summary>
        /// Marks that the node has been found in another location.
        /// </summary>
        /// <param name="location">New location where node has been found.</param>
        protected void MarkFound(LocationEnum location)
        {
            Location = Location | (int)location;
        }

        public void AddInfoFromLocation(FileSystemInfo info, LocationEnum location, bool markIsFound = true)
        {
            if (markIsFound)
                MarkFound(location);

            switch (location)
            {
                case LocationEnum.OnBase:
                    InfoBase = info;
                    break;
                case LocationEnum.OnLeft:
                    InfoLeft = info;
                    break;
                case LocationEnum.OnRight:
                    InfoRight = info;
                    break;
                default:
                    if (((LocationCombinationsEnum)location == LocationCombinationsEnum.OnAll3)
                        || ((LocationCombinationsEnum)location == LocationCombinationsEnum.OnLeftRight))
                    {
                        // ok ... manual insert
                        break;
                    }

                    throw new ArgumentException("Cannot add Info from this location.");
            }
        }

        public abstract void Accept(IFilesystemTreeVisitor visitor);

    }
}