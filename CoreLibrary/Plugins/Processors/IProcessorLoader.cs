using System.Collections.Generic;
using CoreLibrary.Plugins.Processors.Settings;

namespace CoreLibrary.Plugins.Processors
{
    /// <summary>
    /// ProcessorLoader tries to load all available Processors in loaded assemblies and can iterate over them.
    /// 
    /// Processors are loaded into three different categories depending on which order they are executed
    /// (PreProcessors, Processors, PostProcessors).
    /// </summary>
    public interface IProcessorLoader
    {
        /// <summary>
        /// Loads all processors.
        /// 
        /// May use Reflection to find new ones.
        /// </summary>
        void LoadAll();

        IEnumerable<IProcessor> GetProcessors(ProcessorTypeEnum processorType);

        /// <summary>
        /// Traverses through instance of an object and creates instances of annotated fields with ISettings annotation.
        /// </summary>
        /// <param name="instance">Instance to be searched for ISettings.</param>
        /// <param name="isStatic">If isStatic is true then instance must be type of Type.</param>
        void RetrieveSettings(object instance, bool isStatic = false);

        /// <summary>
        /// Adds a custom Processor that was not loaded automatically.
        /// </summary>
        /// <param name="processor">IProcessor to be added.</param>
        void AddProcessor(IProcessor processor);

        /// <summary>
        /// Iterates over all available Settings on the loaded Processors.
        /// </summary>
        /// <returns>IEnumerable</returns>
        IEnumerable<ISettings> GetSettings();

        /// <summary>
        /// Iterator over ISettings for given processor name (GetType()).
        /// </summary>
        /// <param name="processor">Processor to get the settings from.</param>
        /// <returns>IEnumerable</returns>
        IEnumerable<ISettings> GetSettingsByProcessor(IProcessor processor);

        IEnumerable<ISettings> GetSettingsByProcessor(string processor);

        IProcessorLoader SplitLoaderByType(ProcessorTypeEnum processorType);
    }
}
