using System;
using System.ComponentModel;
using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree.Visitors;

namespace CoreLibrary.FilesystemTree
{
    public abstract class AbstractNode : IFilesystemTreeAbstractNode, INotifyPropertyChanged
    {
        private FileSystemInfo infoBase;
        public FileSystemInfo InfoBase
        {
            get { return infoBase; }
            protected set { infoBase = value; OnPropertyChanged("InfoBase"); }
        }

        private FileSystemInfo infoLocal;
        public FileSystemInfo InfoLocal
        {
            get { return infoLocal; }
            protected set { infoLocal = value; OnPropertyChanged("InfoLocal"); }
        }

        private FileSystemInfo infoRemote;

        public FileSystemInfo InfoRemote
        {
            get { return infoRemote; }
            protected set { infoRemote = value; OnPropertyChanged("InfoRemote"); }
        }

        private NodeStatusEnum status;
        public NodeStatusEnum Status
        {
            get { return status; }
            set { status = value; OnPropertyChanged("Status"); }
        }

        public FileTypeEnum FileType { get; set; }

        private DifferencesStatusEnum diff;
        public DifferencesStatusEnum Differences
        {
            get
            {
                return diff;
            }
            set
            {
                // normalize Differences for 2-way mode
                if (Mode == DiffModeEnum.TwoWay && value == DifferencesStatusEnum.LocalRemoteSame)
                {
                    diff = DifferencesStatusEnum.AllSame;
                } else
                {
                    diff = value;
                }
                OnPropertyChanged("Differences");
            }
        }

        public Exception Exception { get; set; }

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
                }
                if (InfoLocal != null)
                {
                    return InfoLocal;
                }
                if (InfoRemote != null)
                {
                    return InfoRemote;
                }

                throw new InvalidDataException("At least one FileSystemInfo can not be null.");
            }
        }

        private int location;

        public int Location
        {
            get { return location; }
            protected set { location = value; OnPropertyChanged("Location"); }
        }

        public DiffModeEnum Mode { get; protected set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

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
            return ((int)location & Location) == (int)location;
        }

        public bool IsInLocation(LocationCombinationsEnum location)
        {
            return ((int)location & Location) == (int)location;
        }

        /// <summary>
        /// Marks that the node has been found in another location.
        /// </summary>
        /// <param name="location">New location where node has been found.</param>
        protected void MarkFound(LocationEnum location)
        {
            Location = Location | (int)location;
        }

        public void AddInfoFromLocation(FileSystemInfo info, LocationEnum location)
        {
            MarkFound(location);

            switch (location)
            {
                case LocationEnum.OnBase:
                    InfoBase = info;
                    break;
                case LocationEnum.OnLocal:
                    InfoLocal = info;
                    break;
                case LocationEnum.OnRemote:
                    InfoRemote = info;
                    break;
                default:
                    if (((LocationCombinationsEnum)location == LocationCombinationsEnum.OnAll3)
                        || ((LocationCombinationsEnum)location == LocationCombinationsEnum.OnLocalRemote))
                    {
                        // ok ... manual insert
                        break;
                    }

                    throw new ArgumentException("Cannot add Info from this location.");
            }
        }

        public void RemoveInfoFromLocation(LocationEnum location)
        {
            Location &= ~(int)location;

            switch (location)
            {
                case LocationEnum.OnBase:
                    InfoBase = null;
                    break;
                case LocationEnum.OnLocal:
                    InfoLocal = null;
                    break;
                case LocationEnum.OnRemote:
                    InfoRemote = null;
                    break;
            }
        }

        public abstract void Accept(IFilesystemTreeVisitor visitor);

        public override string ToString()
        {
            return Info.FullName;
        }
    }
}