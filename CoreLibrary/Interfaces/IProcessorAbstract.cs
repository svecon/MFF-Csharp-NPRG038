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
        void Process(IFilesystemTreeDirNode node);

        void Process(IFilesystemTreeFileNode node);

        int Priority { get; }

        DiffModeEnum Mode { get; }
    }
}
