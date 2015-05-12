using System;

namespace CoreLibrary.Exceptions
{
    /// <summary>
    /// Exception is thrown when the value for given option could not be parsed.
    /// </summary>
    public class SettingsUnknownValue : Exception
    {
        /// <summary>
        /// Initializes new instance of the <see cref="SettingsUnknownValue"/>
        /// </summary>
        /// <param name="option">String switch that was not found.</param>
        /// <param name="inner">Inner exception.</param>
        public SettingsUnknownValue(string option, Exception inner)
            : base(option, inner)
        {
        }
    }
}
