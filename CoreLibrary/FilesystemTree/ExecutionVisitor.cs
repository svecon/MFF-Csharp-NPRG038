using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Interfaces;

namespace CoreLibrary.FilesystemTree
{
    public class ExecutionVisitor : IFilesystemTreeVisitor
    {

        IProcessorLoader loader;

        public List<Task> tasks = new List<Task>();

        public ExecutionVisitor(IProcessorLoader loader)
        {
            this.loader = loader;
        }

        public void Visit(IFilesystemTreeDirNode node)
        {
            Task t = Task.FromResult(false);

            foreach (var processor in loader.GetPreProcessors())
                t = t.ContinueWith(_ => processor.Process(node));
            foreach (var processor in loader.GetProcessors())
                t = t.ContinueWith(_ => processor.Process(node));
            foreach (var processor in loader.GetPostProcessors())
                t = t.ContinueWith(_ => processor.Process(node));

            foreach (var file in node.Files)
                file.Accept(this);

            foreach (var dir in node.Directories)
                dir.Accept(this);
        }

        public void Visit(IFilesystemTreeFileNode node)
        {
            Task t = Task.FromResult(false);

            foreach (var processor in loader.GetPreProcessors())
                t = t.ContinueWith(_ => processor.Process(node));
            foreach (var processor in loader.GetProcessors())
                t = t.ContinueWith(_ => processor.Process(node));
            foreach (var processor in loader.GetPostProcessors())
                t = t.ContinueWith(_ => processor.Process(node));

            tasks.Add(t);
        }

        public void Wait()
        {
            Task.WaitAll(tasks.ToArray());
        }
    }
}
