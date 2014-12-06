using CoreLibrary.Exceptions;
using CoreLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreLibrary.Settings
{
    /// <summary>
    /// Parses the string arguments and tries to find setting triggers in them.
    /// 
    /// All arguments need to start with a hyphen.
    /// The long ones start with two hyphens and shortcuts start with only one.
    /// 
    /// If the argument is not found then SettingsNotFoundException is thrown.
    /// </summary>
    public class SettingsParser
    {
        readonly Dictionary<string, ISettings> longSettings;

        readonly Dictionary<string, ISettings> shortSettings;

        public SettingsParser(IEnumerable<ISettings> settings)
        {
            longSettings = new Dictionary<string, ISettings>();
            shortSettings = new Dictionary<string, ISettings>();

            foreach (ISettings option in settings)
            {
                if (option.Argument != null)
                    longSettings.Add(option.Argument, option);

                if (option.ArgumentShortcut != null)
                    shortSettings.Add(option.ArgumentShortcut, option);
            }
        }

        /// <summary>
        /// Parses the string array for settings arguments.
        /// </summary>
        /// <param name="arguments">String settings (usually from console).</param>
        /// <returns>Arguments that are not settings.</returns>
        public string[] ParseSettings(params string[] arguments)
        {
            var leftOvers = new List<string>();

            int i = 0;
            while (i < arguments.Length)
            {
                ISettings setting;

                if (arguments[i].StartsWith("--")) // long settings
                {
                    if (longSettings.TryGetValue(arguments[i].Remove(0, 2), out setting))
                    {
                        try
                        {
                            setting.SetValue(arguments.Skip(i + 1).Take(setting.NumberOfParams).ToArray());
                        } catch (ArgumentException e)
                        {
                            throw new SettingsUnknownValue(arguments[i] + " " + string.Join(" ", arguments.Skip(i + 1).Take(setting.NumberOfParams).ToArray()), e);
                        }
                        i += 1 + setting.NumberOfParams;
                    } else
                        throw new SettingsNotFoundException(arguments[i]);

                } else if (arguments[i].StartsWith("-")) // short settings
                {
                    if (shortSettings.TryGetValue(arguments[i].Remove(0, 1), out setting))
                    {
                        try
                        {
                            setting.SetValue(arguments.Skip(i + 1).Take(setting.NumberOfParams).ToArray());
                        } catch (ArgumentException e)
                        {
                            throw new SettingsUnknownValue(arguments[i] + " " + string.Join(" ", arguments.Skip(i + 1).Take(setting.NumberOfParams).ToArray()), e);
                        }

                        i += 1 + setting.NumberOfParams;
                    } else
                        throw new SettingsNotFoundException(arguments[i]);
                } else
                {
                    leftOvers.Add(arguments[i]);
                    i++;
                }
            }

            return leftOvers.ToArray();
        }

    }
}
