using System;

namespace CoreLibrary.Exceptions
{
    /// <summary>
    /// The same processor priority already exists for giver processor type.
    /// 
    /// You must use a different processor priority.
    /// </summary>
    public class ProcessorPriorityColissionException : TypeLoadException
    {
        /// <summary>
        /// Initializes new instance of the <see cref="ProcessorPriorityColissionException"/>
        /// </summary>
        /// <param name="msg">Message for the exception</param>
        /// <param name="inner">Inner exception</param>
        public ProcessorPriorityColissionException(string msg, Exception inner)
            : base(msg, inner)
        {

        }
    }
}
