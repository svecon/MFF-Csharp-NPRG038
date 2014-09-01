using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Exceptions
{
    class ProcessorPriorityColissionException : TypeLoadException
    {
        public ProcessorPriorityColissionException(string msg, Exception inner)
            : base(msg, inner)
        {

        }
    }
}
