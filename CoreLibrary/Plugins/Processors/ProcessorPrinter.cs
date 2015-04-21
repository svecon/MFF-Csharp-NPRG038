using System;
using CoreLibrary.Interfaces;
using CoreLibrary.Settings;

namespace CoreLibrary.Processors
{
    /// <summary>
    /// Class for printing loaded processors and their corresponding settings.
    /// </summary>
    public class ProcessorPrinter : IPrinter
    {
        private readonly IProcessorLoader loader;
        private readonly bool printSettings;

        /// <summary>
        /// Constructs printer with giver Loader.
        /// </summary>
        /// <param name="processorLoader">ProcessorLoader that will have the processors print out.</param>
        /// <param name="printSettings">True for printing out corresponding settings.</param>
        public ProcessorPrinter(IProcessorLoader processorLoader, bool printSettings = false)
        {
            loader = processorLoader;
            this.printSettings = printSettings;
        }

        private void PrintProcessorInfo(IProcessor processor)
        {
            Console.WriteLine("{0,10} {1} in {2}", processor.Attribute.Priority, processor.GetType().Name,
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

            foreach (object value in Enum.GetValues(typeof(ProcessorTypeEnum)))
            {
                Console.WriteLine("\n=== {0}", value);

                foreach (IProcessor processor in loader.GetProcessors((ProcessorTypeEnum)value))
                {
                    PrintProcessorInfo(processor);
                }
            }
        }

    }
}
