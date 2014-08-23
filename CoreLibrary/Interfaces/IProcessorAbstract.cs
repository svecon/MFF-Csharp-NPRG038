using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CoreLibrary.Enums;

namespace CoreLibrary.Interfaces
{
    public interface IProcessorAbstract
    {
        bool Process(IFilesystemTreeDirNode info);

        bool Process(IFilesystemTreeFileNode info);

        int Priority { get; }

        DiffModeEnum Mode { get; }
    }
}
