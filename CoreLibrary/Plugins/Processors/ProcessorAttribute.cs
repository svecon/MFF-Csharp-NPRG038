using System;
using CoreLibrary.Enums;

namespace CoreLibrary.Plugins.Processors
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProcessorAttribute : Attribute
    {
        public ProcessorTypeEnum ProcessorType { get; private set; }

        public int Priority { get; private set; }

        public DiffModeEnum Mode { get; private set; }

        public ProcessorAttribute(ProcessorTypeEnum processorType, int priority, DiffModeEnum mode)
        {
            Priority = priority;
            Mode = mode;
            ProcessorType = processorType;
        }
    }
}
