using CoreLibrary.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// Interface for a FilesystemTree structure.
    /// 
    /// Hold information about filesystem trees from all locations.
    /// </summary>
    public interface IFilesystemTree : IFilesystemTreeVisitable
    {
        /// <summary>
        /// Root of the filesystem tree.
        /// </summary>
        IFilesystemTreeDirNode Root { get; }

        /// <summary>
        /// Adds new Directory path to Root.
        /// </summary>
        /// <param name="root">DirectoryInfo for given directory.</param>
        /// <param name="location">Behave like folder from this location.</param>
        void AddDirToRoot(DirectoryInfo root, LocationEnum location);
    }
}
