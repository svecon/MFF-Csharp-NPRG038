using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CoreLibrary.Enums;

namespace CoreLibrary.Interfaces
{
    public interface IFilesystemTreeDirNode : IFilesystemTreeAbstractNode
    {
        List<IFilesystemTreeDirNode> Directories { get; }

        List<IFilesystemTreeFileNode> Files { get; }

        IFilesystemTreeDirNode SearchForDir(DirectoryInfo info);

        IFilesystemTreeDirNode AddDir(DirectoryInfo info, LocationEnum location);

        IFilesystemTreeFileNode SearchForFile(FileInfo info);

        void AddFile(FileInfo info, LocationEnum location);
    }
}
