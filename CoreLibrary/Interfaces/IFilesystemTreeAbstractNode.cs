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

        FileSystemInfo Info { get; }

        LocationEnum Location { get; }

        void AddInfoFromLocation(FileSystemInfo info, LocationEnum location);

        void Accept(IFilesystemTreeVisitor visitor);
    }
}
