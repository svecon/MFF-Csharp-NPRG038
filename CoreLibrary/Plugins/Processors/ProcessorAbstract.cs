using System.Data;
using System.Reflection;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.Plugins.Processors
{
    /// <summary>
    /// ProcessorAbstract is a base class for all types of processors.
    /// 
    /// Contain some helper methods that are shared for all processors.
    /// </summary>
    public abstract class ProcessorAbstract : IProcessor
    {
        public ProcessorAttribute Attribute { get; private set; }

        protected ProcessorAbstract()
        {
            Attribute = (ProcessorAttribute)GetType().GetCustomAttribute(typeof(ProcessorAttribute));

            if (Attribute == null)
                throw new ConstraintException("Every processor must implement ProcessorAttribute.");
        }

        protected virtual bool CheckStatus(IFilesystemTreeDirNode node)
        {
            return (node.Mode & Attribute.Mode) != 0;
        }

        protected virtual bool CheckStatus(IFilesystemTreeFileNode node)
        {
            if ((node.Mode & Attribute.Mode) == 0)
                return false;

            switch (node.Status)
            {
                case NodeStatusEnum.HasError:
                    return false;
                case NodeStatusEnum.IsIgnored:
                    return false;
            }

            return true;
        }

        public virtual void Process(IFilesystemTreeDirNode node)
        {
            if (!CheckStatus(node))
                return;

            ProcessChecked(node);
        }

        public virtual void Process(IFilesystemTreeFileNode node)
        {
            if (!CheckStatus(node))
                return;

            ProcessChecked(node);
        }

        protected abstract void ProcessChecked(IFilesystemTreeDirNode node);
        protected abstract void ProcessChecked(IFilesystemTreeFileNode node);
    }
}
