using System;

namespace CoreLibrary.Exceptions
{
    /// <summary>
    /// Exception thrown when some Settings have same Switch, but different number of parameters.
    /// </summary>
    public class SettingsNumberOfParamsMismatchException : FormatException
    {
        /// <summary>
        /// Initializes new instance of the <see cref="SettingsNumberOfParamsMismatchException"/>
        /// </summary>
        /// <param name="first">First Settings type</param>
        /// <param name="second">Second Settings type</param>
        /// <param name="argument">Shared switch for the two settings.</param>
        public SettingsNumberOfParamsMismatchException(Type first, Type second, string argument) :
            base(String.Format("{0} and {1} have same option '{2}' but different number of parameters.", first, second, argument))
        {
        }
    }
}
