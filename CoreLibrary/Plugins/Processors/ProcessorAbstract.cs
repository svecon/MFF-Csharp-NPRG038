using System.Data;
using System.Reflection;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;

namespace CoreLibrary.Plugins.Processors
{
    /// <summary>
    /// ProcessorAbstract is a base class for all types of processors.
    /// 
    /// Contain some helper methods that are shared for all processors.
    /// </summary>
    public abstract class ProcessorAbstract : IProcessor
    {
        ProcessorAttribute Attribute { get; set; }

        /// <summary>
        /// Initializes new instance of the <see cref="ProcessorAbstract"/>
        /// </summary>
        protected ProcessorAbstract()
        {
            Attribute = (ProcessorAttribute)GetType().GetCustomAttribute(typeof(ProcessorAttribute));

            if (Attribute == null)
                throw new ConstraintException("Every processor must implement ProcessorAttribute.");
        }

        /// <summary>
        /// Virtual method for checking whether the processor should run.
        /// </summary>
        /// <param name="node">Node that should be processed.</param>
        /// <returns>True if the processor should run.</returns>
        protected virtual bool CheckStatus(INodeDirNode node)
        {
            return (node.Mode & Attribute.Mode) != 0;
        }

        /// <summary>
        /// Virtual method for checking whether the processor should run.
        /// </summary>
        /// <param name="node">Node that should be processed.</param>
        /// <returns>True if the processor should run.</returns>
        protected virtual bool CheckStatus(INodeFileNode node)
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

        public virtual void Process(INodeDirNode node)
        {
            if (!CheckStatus(node))
                return;

            ProcessChecked(node);
        }

        public virtual void Process(INodeFileNode node)
        {
            if (!CheckStatus(node))
                return;

            ProcessChecked(node);
        }

        /// <summary>
        /// Logic for processing the node.
        /// </summary>
        /// <param name="node">Node that will be processed.</param>
        protected abstract void ProcessChecked(INodeDirNode node);

        /// <summary>
        /// Logic for processing the node.
        /// </summary>
        /// <param name="node">Node that will be processed.</param>
        protected abstract void ProcessChecked(INodeFileNode node);
    }
}
