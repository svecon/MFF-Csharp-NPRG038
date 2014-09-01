using CoreLibrary.Exceptions;
using CoreLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Settings
{
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
                ISettings setting;

                if (arguments[i].StartsWith("--"))
                {
                    if (longSettings.TryGetValue(arguments[i].Remove(0, 2), out setting))
                    {
                        setting.SetValue(arguments.Skip(i + 1).Take(setting.NumberOfParams).ToArray());
                        i += 1 + setting.NumberOfParams;
                    } else
                        throw new SettingsNotFoundException();

                } else if (arguments[i].StartsWith("-"))
                {
                    if (shortSettings.TryGetValue(arguments[i].Remove(0, 1), out setting))
                    {
                        setting.SetValue(arguments.Skip(i + 1).Take(setting.NumberOfParams).ToArray());
                        i += 1 + setting.NumberOfParams;
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
