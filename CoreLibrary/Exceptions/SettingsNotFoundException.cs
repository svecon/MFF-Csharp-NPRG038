using System;

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
