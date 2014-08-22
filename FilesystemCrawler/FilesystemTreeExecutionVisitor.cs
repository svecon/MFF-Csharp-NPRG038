using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilesystemCrawler.Interfaces;

namespace FilesystemCrawler
{
    class FilesystemTreeExecutionVisitor : IFilesystemTreeVisitor
    {

        public FilesystemTreeExecutionVisitor() {
        

        
        }

        public void Visit(FilesystemTree.DirNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(FilesystemTree.FileNode node)
        {
            throw new NotImplementedException();
        }
    }
}
