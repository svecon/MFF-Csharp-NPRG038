using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CoreLibrary.Enums;

namespace CoreLibrary.Interfaces
{
    public interface IFilesystemTreeAbstractNode
    {
        NodeStatus Status { get; set; }

        DifferencesStatus Differences { get; set; }

        FileSystemInfo Info { get; }

        FileSystemInfo InfoBase { get; }

        FileSystemInfo InfoLeft { get; }

        FileSystemInfo InfoRight { get; }

        LocationEnum Location { get; }

        bool IsInLocation(LocationEnum location);

        void AddInfoFromLocation(FileSystemInfo info, LocationEnum location);

        void Accept(IFilesystemTreeVisitor visitor);
    }
}
