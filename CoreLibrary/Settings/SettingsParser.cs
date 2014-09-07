using CoreLibrary.Exceptions;
using CoreLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Dictionary<string, ISettings> longSettings;

        Dictionary<string, ISettings> shortSettings;

        public SettingsParser(IEnumerable<ISettings> settings)
        {
            longSettings = new Dictionary<string, ISettings>();
            shortSettings = new Dictionary<string, ISettings>();

            foreach (var option in settings)
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
            List<string> leftOvers = new List<string>();

            int i = 0;
            while (i < arguments.Length)
            {
                ISettings setting;

                if (arguments[i].StartsWith("--"))
                {
                    if (longSettings.TryGetValue(arguments[i].Remove(0, 2), out setting))
                    {
                        setting.SetValue(arguments.Skip(i + 1).Take(setting.NumberOfParams).ToArray());
                        i += 1 + setting.NumberOfParams;
                    } else
                        throw new SettingsNotFoundException(arguments[i]);

                } else if (arguments[i].StartsWith("-"))
                {
                    if (shortSettings.TryGetValue(arguments[i].Remove(0, 1), out setting))
                    {
                        setting.SetValue(arguments.Skip(i + 1).Take(setting.NumberOfParams).ToArray());
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
