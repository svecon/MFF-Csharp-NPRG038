using System;

namespace CoreLibrary.Exceptions
{
    /// <summary>
    /// The same processor priority already exists for giver processor type.
    /// 
    /// You must use a different processor priority.
    /// 
    /// //TODO: no exception - just load them ordered by name
    /// </summary>
    public class ProcessorPriorityColissionException : TypeLoadException
    {
        public ProcessorPriorityColissionException(string msg, Exception inner)
            : base(msg, inner)
        {

        }
    }
}
