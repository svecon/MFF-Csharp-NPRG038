using System;
using System.ComponentModel;
using System.IO;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.FilesystemTree.Visitors;

namespace CoreLibrary.FilesystemTree
{
    /// <summary>
    /// An abstract FilesystemTree for the <see cref="IFilesystemTree"/>
    /// </summary>
    public abstract class AbstractNode : INodeAbstractNode, INotifyPropertyChanged
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

        private PreferedActionThreeWayEnum action;

        public PreferedActionThreeWayEnum Action
        {
            get { return action; }
            set { action = value; OnPropertyChanged("PreferedAction"); }
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
                if (InfoRemote != null)
                {
                    return InfoRemote;
                }
                if (InfoLocal != null)
                {
                    return InfoLocal;
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

        #region INotifyPropertyChanged interface implementation

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// An event handler helper method for the changed properties.
        /// </summary>
        /// <param name="name">Name of the changed property</param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        public abstract string GetAbsolutePath(LocationEnum location);

        /// <summary>
        /// Constructor for AbstractNode.
        /// </summary>
        /// <param name="info">FileSystemInfo for the FilesystemTree.</param>
        /// <param name="location">Where the FilesystemTree came from.</param>
        /// <param name="mode">Default mode.</param>
        protected AbstractNode(FileSystemInfo info, LocationEnum location, DiffModeEnum mode)
        {
            Differences = DifferencesStatusEnum.Initial;
            Mode = mode;
            AddInfoFromLocation(info, location);
        }

        public bool IsInLocation(LocationEnum loc)
        {
            return ((int)loc & Location) == (int)loc;
        }

        public bool IsInLocation(LocationCombinationsEnum loc)
        {
            return ((int)loc & Location) == (int)loc;
        }

        /// <summary>
        /// Marks that the FilesystemTree has been found in another location.
        /// </summary>
        /// <param name="loc">New location where FilesystemTree has been found.</param>
        protected void MarkFound(LocationEnum loc)
        {
            Location = Location | (int)loc;
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

        public void RemoveInfoFromLocation(LocationEnum loc)
        {
            Location &= ~(int)loc;

            switch (loc)
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