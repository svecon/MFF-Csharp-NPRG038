using System;
using System.Collections.Generic;
using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.Settings;

namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// ProcessorLoader tries to load all available Processors and can iterate over them.
    /// </summary>
    public interface IProcessorLoader
    {
        /// <summary>
        /// Loads all processors.
        /// 
        /// May use Reflection to find new ones.
        /// </summary>
        void LoadAll();

        IEnumerable<IPreProcessor> GetPreProcessors();

        IEnumerable<IProcessor> GetProcessors();

        IEnumerable<IPostProcessor> GetPostProcessors();

        void AddProcessor(IPreProcessor processor);

        void AddProcessor(IProcessor processor);

        void AddProcessor(IPostProcessor processor);

        IEnumerable<ISettings> GetSettings();
    }
}
