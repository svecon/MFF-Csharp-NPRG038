using System;
using CoreLibrary.FilesystemTree.Enums;

namespace CoreLibrary.Plugins.Processors
{
    /// <summary>
    /// Attribute that is used to add additional information to a processor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProcessorAttribute : Attribute
    {
        /// <summary>
        /// Type of a processor determines when the processor will run and what does it do.
        /// </summary>
        public ProcessorTypeEnum ProcessorType { get; private set; }

        /// <summary>
        /// Priority of a processor determines when the processor will run.
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// Mode of the processor tells in what mode is the processor able to run.
        /// </summary>
        public DiffModeEnum Mode { get; private set; }

        /// <summary>
        /// Initializes new instance of the <see cref="ProcessorAttribute"/>
        /// </summary>
        /// <param name="processorType">Type of the processor, <see cref="ProcessorType"/>.</param>
        /// <param name="priority">Processor's priority, <see cref="Priority"/>.</param>
        /// <param name="mode">Mode that the processor can run in.</param>
        public ProcessorAttribute(ProcessorTypeEnum processorType, int priority, DiffModeEnum mode)
        {
            Priority = priority;
            Mode = mode;
            ProcessorType = processorType;
        }
    }
}
