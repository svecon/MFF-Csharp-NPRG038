using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Interfaces
{
    public interface IFilesystemTreeVisitor
    {
        void Visit(IFilesystemTreeDirNode node);
        void Visit(IFilesystemTreeFileNode node);
    }
}
