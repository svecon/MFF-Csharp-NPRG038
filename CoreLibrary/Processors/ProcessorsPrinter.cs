using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Interfaces;
using CoreLibrary.Settings;

namespace CoreLibrary.Processors
{
    /// <summary>
    /// Class for printing loaded processors and their corresponding settings.
    /// </summary>
    public class ProcessorsPrinter : IPrinter
    {
        private readonly IProcessorLoader loader;
        private readonly bool printSettings;

        /// <summary>
        /// Constructs printer with giver Loader.
        /// </summary>
        /// <param name="processorLoader">ProcessorLoader that will have the processors print out.</param>
        /// <param name="printSettings">True for printing out corresponding settings.</param>
        public ProcessorsPrinter(IProcessorLoader processorLoader, bool printSettings = false)
        {
            loader = processorLoader;
            this.printSettings = printSettings;
        }

        private void PrintProcessorInfo(IProcessorBase processor, bool printSettings)
        {
            Console.WriteLine("{0,10} {1} in {2}", processor.Priority, processor.GetType().Name,
                processor.GetType().Namespace);

            if (!printSettings)
                return;

            var settingsPrinter = new SettingsPrinter(loader.GetSettingsByProcessor(processor.GetType().ToString()), 11);
            settingsPrinter.Print();

            //foreach (ISettings procSettings in loader.GetSettingsByProcessor(processor.GetType().ToString()))
            //    Console.WriteLine("{0,10} {1}", "", procSettings.ToString());
        }

        /// <summary>
        /// Print all loaded processors.
        /// </summary>
        public void Print()
        {
            Console.WriteLine("=== Global settings");
            var settingsPrinter = new SettingsPrinter(loader.GetSettingsByProcessor(typeof(Object).ToString()), 11);
            settingsPrinter.Print();

            Console.WriteLine("\n=== PreProcessors");
            foreach (IPreProcessor processor in loader.GetPreProcessors())
            {
                PrintProcessorInfo(processor, printSettings);
            }

            Console.WriteLine("\n=== Processors");
            foreach (IProcessor processor in loader.GetProcessors())
            {
                PrintProcessorInfo(processor, printSettings);
            }

            Console.WriteLine("\n=== PostProcessors");
            foreach (IPostProcessor processor in loader.GetPostProcessors())
            {
                PrintProcessorInfo(processor, printSettings);
            }
        }

    }
}
