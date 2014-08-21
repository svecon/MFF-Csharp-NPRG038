using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesystemCrawler.Interfaces
{
    public interface IDiffStructureVisitor
    {
        void Visit(DiffStructure.DirDiffNode node);
        void Visit(DiffStructure.FileDiffNode node);
    }
}
