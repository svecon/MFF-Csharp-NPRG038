using System;
using System.Reflection;
using CoreLibrary.Plugins.Processors;

namespace SvergeConsole.Printers
{
    /// <summary>
    /// Class for printing loaded processors and their corresponding settings.
    /// </summary>
    public class ProcessorPrinter : IPrinter
    {
        /// <summary>
        /// Loader that contains all processors to be printed.
        /// </summary>
        private readonly IProcessorLoader loader;

        /// <summary>
        /// True for printing out corresponding processors' settings.
        /// </summary>
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

        /// <summary>
        /// Prints processor info.
        /// </summary>
        /// <param name="processor">Processor to be printed out.</param>
        private void PrintProcessorInfo(IProcessor processor)
        {
            var attr = (ProcessorAttribute)processor.GetType().GetCustomAttribute(typeof(ProcessorAttribute));

            Console.WriteLine("{0,10} {1} in {2}",
                attr == null ? string.Empty : attr.Priority.ToString(),
                processor.GetType().Name,
                processor.GetType().Namespace);

            if (!printSettings)
                return;

            var settingsPrinter = new SettingsPrinter(loader.GetSettingsByProcessor(processor.GetType().ToString()), 11);
            settingsPrinter.Print();

            //foreach (ISettings procSettings in loader.GetSettingsByProcessor(processor.GetType().ToString()))
            //    Console.WriteLine("{0,10} {1}", "", procSettings.ToString());
        }

        /// <inheritdoc />
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
