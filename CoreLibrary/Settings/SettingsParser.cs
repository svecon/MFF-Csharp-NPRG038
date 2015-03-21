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
        readonly Dictionary<string, List<ISettings>> longSettings;

        readonly Dictionary<string, List<ISettings>> shortSettings;

        public SettingsParser(IEnumerable<ISettings> settings)
        {
            longSettings = new Dictionary<string, List<ISettings>>();
            shortSettings = new Dictionary<string, List<ISettings>>();

            foreach (ISettings option in settings)
            {
                List<ISettings> settingsList;
                if (option.Argument != null)
                {
                    if (longSettings.TryGetValue(option.Argument, out settingsList))
                    {
                        if (settingsList.First().NumberOfParams != option.NumberOfParams)
                        { // different number of params in the same argument
                            throw new SettingsNumberOfParamsMismatchException
                                (settingsList.First().Field.DeclaringType, option.Field.DeclaringType, "--" + option.Argument);
                        }

                        settingsList.Add(option);
                    } else
                    {
                        longSettings.Add(option.Argument, new List<ISettings>(new[] { option }));
                    }

                }

                if (option.ArgumentShortcut != null)
                {
                    if (shortSettings.TryGetValue(option.ArgumentShortcut, out settingsList))
                    {
                        if (settingsList.First().NumberOfParams != option.NumberOfParams)
                        { // different number of params in the same argument
                            throw new SettingsNumberOfParamsMismatchException
                                (settingsList.First().Field.DeclaringType, option.Field.DeclaringType, "-" + option.ArgumentShortcut);
                        }

                        settingsList.Add(option);
                    } else
                    {
                        shortSettings.Add(option.ArgumentShortcut, new List<ISettings>(new[] { option }));
                    }

                }
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
                List<ISettings> settingsList;

                if (arguments[i].StartsWith("--")) // long settings
                {
                    if (longSettings.TryGetValue(arguments[i].Remove(0, 2), out settingsList))
                    {
                        ApplySettings(settingsList, ref arguments, ref i);
                    } else
                        throw new SettingsNotFoundException(arguments[i]);

                } else if (arguments[i].StartsWith("-")) // short settings
                {
                    if (shortSettings.TryGetValue(arguments[i].Remove(0, 1), out settingsList))
                    {
                        ApplySettings(settingsList, ref arguments, ref i);
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

        private void ApplySettings(IEnumerable<ISettings> settingsList, ref string[] arguments, ref int i)
        {
            foreach (ISettings setting in settingsList)
            {
                try
                {
                    i += 1 + setting.NumberOfParams;
                    setting.SetValue(arguments.Skip(i + 1).Take(setting.NumberOfParams).ToArray());

                } catch (ArgumentException e)
                {
                    throw new SettingsUnknownValue(arguments[i] + " " + string.Join(" ", arguments.Skip(i + 1).Take(setting.NumberOfParams).ToArray()), e);
                }
            }
        }

    }
}
