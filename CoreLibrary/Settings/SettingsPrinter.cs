using CoreLibrary.Interfaces;
using CoreLibrary.Settings.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Settings
{
    /// <summary>
    /// Prints all available settings on a Console into a well-arranged column layout.
    /// </summary>
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

                if (option.Argument != null && longestOption < option.Argument.Length)
                    longestOption = option.Argument.Length;
            }
        }

        /// <summary>
        /// Prints the layout.
        /// </summary>
        public void Print()
        {
            sortByOptionParameters();

            Console.WriteLine("Listing of all possible options:");

            foreach (var option in settings)
            {
                if (option.ArgumentShortcut != null)
                {
                    Console.CursorLeft = 2;
                    Console.Write("-" + option.ArgumentShortcut);
                }

                if (option.Argument != null)
                {
                    Console.CursorLeft = 6;
                    Console.Write("--" + option.Argument);
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

        /// <summary>
        /// Compares the two settings by their ArgumentShortcuts alphabetically.
        /// </summary>
        private class SettingsComparer : IComparer<ISettings>
        {
            public int Compare(ISettings x, ISettings y)
            {
                string left = x.ArgumentShortcut != null ? x.ArgumentShortcut : x.Argument;
                string right = y.ArgumentShortcut != null ? y.ArgumentShortcut : y.Argument;

                return left.CompareTo(right);
            }
        }

    }
}
