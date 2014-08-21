using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesystemCrawler.Interfaces
{
    public interface IFilesystemTreeVisitor
    {
        void Visit(FilesystemTree.DirDiffNode node);
        void Visit(FilesystemTree.FileDiffNode node);
    }
}
