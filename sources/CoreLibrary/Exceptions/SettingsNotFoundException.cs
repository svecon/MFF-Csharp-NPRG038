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
        /// <summary>
        /// Initializes new instance of the <see cref="SettingsNotFoundException"/>
        /// </summary>
        /// <param name="msg">Message for the exception</param>
        public SettingsNotFoundException(string msg)
            : base(msg)
        {

        }
    }
}
