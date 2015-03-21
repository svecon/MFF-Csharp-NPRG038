using System;
using System.Collections.Generic;

namespace CoreLibrary.Interfaces
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

        /// <summary>
        /// Iterates over PreProcessors.
        /// </summary>
        /// <returns>IEnumerable</returns>
        IEnumerable<IPreProcessor> GetPreProcessors();

        /// <summary>
        /// Iterates over Processors.
        /// </summary>
        /// <returns>IEnumerable</returns>
        IEnumerable<IProcessor> GetProcessors();

        /// <summary>
        /// Iterates over PostProcessors.
        /// </summary>
        /// <returns>IEnumerable</returns>
        IEnumerable<IPostProcessor> GetPostProcessors();

        /// <summary>
        /// Traverses through instance of an object and creates instances of annotated fields with ISettings annotation.
        /// </summary>
        /// <param name="instance">Instance to be searched for ISettings.</param>
        void RetrieveSettings(object instance);

        /// <summary>
        /// Add IProcessorBase into a correct Processor type.
        /// </summary>
        /// <param name="processor">IProcessorBase to be added.</param>
        /// <param name="type">Type of IProcessorBase. If null then defaults to GetType.</param>
        void AddProcessor(IProcessorBase processor, Type type = null);

        /// <summary>
        /// Adds a custom PreProcessor that was not loaded automatically.
        /// </summary>
        /// <param name="processor">IPreProcessor to be added.</param>
        void AddProcessor(IPreProcessor processor);

        /// <summary>
        /// Adds a custom Processor that was not loaded automatically.
        /// </summary>
        /// <param name="processor">IProcessor to be added.</param>
        void AddProcessor(IProcessor processor);

        /// <summary>
        /// Adds a custom PostProcessor that was not loaded automatically.
        /// </summary>
        /// <param name="processor">IPostProcessor to be added.</param>
        void AddProcessor(IPostProcessor processor);

        /// <summary>
        /// Iterates over all available Settings on the loaded Processors.
        /// </summary>
        /// <returns>IEnumerable</returns>
        IEnumerable<ISettings> GetSettings();

        /// <summary>
        /// Iterator over ISettings for given processor name (GetType()).
        /// </summary>
        /// <param name="processorName"></param>
        /// <returns>IEnumerable</returns>
        IEnumerable<ISettings> GetSettingsByProcessor(string processorName);
    }
}
