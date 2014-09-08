using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CoreLibrary.Exceptions
{
    /// <summary>
    /// Exception is thrown when the value for given option could not be parsed.
    /// </summary>
    public class SettingsUnknownValue : Exception
    {
        public SettingsUnknownValue(string option, Exception inner)
            : base(option, inner)
        {
        }
    }
}
