using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Interfaces;
using System.Threading;

namespace CoreLibrary.FilesystemTree.Visitors
{
    public class ExecutionVisitor : IExecutionVisitor
    {

        IProcessorLoader loader;

        public List<Task> tasks = new List<Task>();

        CancellationTokenSource tokenSource;

        public ExecutionVisitor(IProcessorLoader loader)
        {
            this.loader = loader;
            tokenSource = new CancellationTokenSource();
        }

        public void Visit(IFilesystemTreeDirNode node)
        {
            // create a completed task
            Task t = Task.FromResult(false);

            foreach (var processor in loader.GetPreProcessors())
                t = t.ContinueWith(_ => processor.Process(node), tokenSource.Token);
            foreach (var processor in loader.GetProcessors())
                t = t.ContinueWith(_ => processor.Process(node), tokenSource.Token);
            foreach (var processor in loader.GetPostProcessors())
                t = t.ContinueWith(_ => processor.Process(node), tokenSource.Token);

            foreach (var file in node.Files)
                file.Accept(this);

            foreach (var dir in node.Directories)
                dir.Accept(this);
        }

        public void Visit(IFilesystemTreeFileNode node)
        {
            // create a completed task
            Task t = Task.FromResult(false);

            foreach (var processor in loader.GetPreProcessors())
                t = t.ContinueWith(_ => processor.Process(node), tokenSource.Token);
            foreach (var processor in loader.GetProcessors())
                t = t.ContinueWith(_ => processor.Process(node), tokenSource.Token);
            foreach (var processor in loader.GetPostProcessors())
                t = t.ContinueWith(_ => processor.Process(node), tokenSource.Token);

            tasks.Add(t);
        }

        public void Wait()
        {
            if (tokenSource.IsCancellationRequested)
                return;

            Task.WaitAll(tasks.ToArray());
        }

        public void Cancel()
        {
            tokenSource.Cancel();
        }
    }
}
