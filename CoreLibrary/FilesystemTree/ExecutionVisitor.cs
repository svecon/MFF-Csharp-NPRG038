using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Interfaces;

namespace CoreLibrary.FilesystemTree
{
    class ExecutionVisitor : IFilesystemTreeVisitor
    {

        IProcessorLoader loader;

        public ExecutionVisitor(IProcessorLoader loader)
        {
            this.loader = loader;
        }

        public void Visit(IFilesystemTreeDirNode node)
        {
            foreach (var processor in loader.GetPreProcessors())
                processor.Process(node);
            foreach (var processor in loader.GetProcessors())
                processor.Process(node);
            foreach (var processor in loader.GetPostProcessors())
                processor.Process(node);

            foreach (var file in node.Files)
                file.Accept(this);

            foreach (var dir in node.Directories)
                dir.Accept(this);
        }

        public void Visit(IFilesystemTreeFileNode node)
        {
            foreach (var processor in loader.GetPreProcessors())
                processor.Process(node);
            foreach (var processor in loader.GetProcessors())
                processor.Process(node);
            foreach (var processor in loader.GetPostProcessors())
                processor.Process(node);
        }
    }
}
