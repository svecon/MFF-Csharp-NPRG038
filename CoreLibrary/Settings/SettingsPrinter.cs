using CoreLibrary.Interfaces;
using CoreLibrary.Settings.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Settings
{
    public class SettingsPrinter
    {

        List<ISettings> settings;

        int longestOption = 0;

        public SettingsPrinter(IEnumerable<ISettings> settings)
        {
            this.settings = new List<ISettings>();

            foreach (var option in settings)
            {
                this.settings.Add(option);

                if (option.Option != null && longestOption < option.Option.Length)
                    longestOption = option.Option.Length;
            }
        }

        public void Print()
        {
            sortByOptionParameters();

            Console.WriteLine("Listing of all possible options:");

            foreach (var option in settings)
            {
                if (option.OptionShortcut != null)
                {
                    Console.CursorLeft = 2;
                    Console.Write("-" + option.OptionShortcut);
                }

                if (option.Option != null)
                {
                    Console.CursorLeft = 6;
                    Console.Write("--" + option.Option);
                }

                Console.CursorLeft = 9 + longestOption;
                Console.Write(" [" + option.NumberOfParams + "]");

                Console.CursorLeft = 15 + longestOption;
                Console.Write(option.Info);

                if (option is EnumSettings)
                {
                    Console.WriteLine();
                    Console.CursorLeft = 15 + longestOption;
                    Console.Write("Possible options: ");

                    Console.Write(String.Join(", ", Enum.GetNames(option.Field.FieldType)));
                }

                Console.WriteLine();
            }
        }

        protected void sortByOptionParameters()
        {
            settings.Sort(new SettingsComparer());
        }

        private class SettingsComparer : IComparer<ISettings>
        {
            public int Compare(ISettings x, ISettings y)
            {
                string left = x.OptionShortcut != null ? x.OptionShortcut : x.Option;
                string right = y.OptionShortcut != null ? y.OptionShortcut : y.Option;

                return left.CompareTo(right);
            }
        }

    }
}
