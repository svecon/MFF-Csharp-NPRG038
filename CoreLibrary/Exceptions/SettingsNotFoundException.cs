using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Exceptions
{
    /// <summary>
    /// Specified setting was not found.
    /// 
    /// Maybe the processor with this setting was not loaded.
    /// </summary>
    public class SettingsNotFoundException : Exception
    {
        public SettingsNotFoundException(string msg)
            : base(msg)
        {

        }
    }
}
