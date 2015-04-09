using System;

namespace CoreLibrary.Exceptions
{
    /// <summary>
    /// Exception thrown when some Settings have same Argument, but different number of parameters.
    /// </summary>
    class SettingsNumberOfParamsMismatchException : FormatException
    {
        public SettingsNumberOfParamsMismatchException(Type first, Type second, string argument) : 
            base(String.Format("{0} and {1} have same option '{2}' but different number of parameters.", first, second, argument))
        {
        }
    }
}
