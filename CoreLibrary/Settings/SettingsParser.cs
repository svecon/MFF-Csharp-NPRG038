using CoreLibrary.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Settings
{
    public class SettingsParser
    {
        Dictionary<string, SettingsAbstract> longSettings;

        Dictionary<string, SettingsAbstract> shortSettings;

        public SettingsParser(IEnumerable<SettingsAbstract> settings)
        {
            longSettings = new Dictionary<string, SettingsAbstract>();
            shortSettings = new Dictionary<string, SettingsAbstract>();

            foreach (var option in settings)
            {
                if (option.Option != null)
                    longSettings.Add(option.Option, option);

                if (option.OptionShortcut != null)
                    shortSettings.Add(option.OptionShortcut, option);
            }
        }

        public string[] ParseSettings(params string[] arguments)
        {
            List<string> leftOvers = new List<string>();

            int i = 0;
            while (i < arguments.Length)
            {
                SettingsAbstract option;

                if (arguments[i].StartsWith("--"))
                {
                    if (longSettings.TryGetValue(arguments[i].Remove(0, 2), out option))
                    {
                        option.SetValue(arguments.Skip(i).Take(option.NumberOfParams).ToArray());
                        i += 1 + option.NumberOfParams;
                    } else
                        throw new SettingsNotFoundException();

                } else if (arguments[i].StartsWith("-"))
                {
                    if (shortSettings.TryGetValue(arguments[i].Remove(0, 1), out option))
                    {
                        option.SetValue(arguments.Skip(i).Take(option.NumberOfParams).ToArray());
                        i += 1 + option.NumberOfParams;
                    } else
                        throw new SettingsNotFoundException();
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
