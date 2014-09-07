using System;
using System.Collections.Generic;
using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.Settings;

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
        /// Adds a custom PreProcessor that was not loaded automatically.
        /// </summary>
        void AddProcessor(IPreProcessor processor);

        /// <summary>
        /// Adds a custom Processor that was not loaded automatically.
        /// </summary>
        void AddProcessor(IProcessor processor);

        /// <summary>
        /// Adds a custom PostProcessor that was not loaded automatically.
        /// </summary>
        void AddProcessor(IPostProcessor processor);

        /// <summary>
        /// Iterates over all available Settings on the loaded Processors.
        /// </summary>
        /// <returns>IEnumerable</returns>
        IEnumerable<ISettings> GetSettings();
    }
}
