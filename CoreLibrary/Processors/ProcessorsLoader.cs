using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Processors;
using CoreLibrary.Processors.Postprocessors;
using CoreLibrary.Processors.Preprocessors;
using CoreLibrary.Exceptions;
using CoreLibrary.Settings;
using CoreLibrary.Settings.Attributes;
using CoreLibrary.Settings.Types;
using System.Reflection;

namespace CoreLibrary.Processors
{
    /// <summary>
    /// ProcessorLoader is a container for all processors.
    /// 
    /// All avaialble assemblies are scanned for Processors and those are then loaded into a correct queue.
    /// 
    /// Also all the loaded processors are scanned for their Settings.
    /// </summary>
    public class ProcessorsLoader : IProcessorLoader
    {

        SortedList<int, IPreProcessor> PreProcessors;

        SortedList<int, IProcessor> Processors;

        SortedList<int, IPostProcessor> PostProcessors;

        /// <summary>
        /// List of all settings for loaded processors.
        /// </summary>
        List<ISettings> settings;

        /// <summary>
        /// Structure that holds available settings types (for different variable types).
        /// </summary>
        Dictionary<Type, Type> availableSettings;

        private Type[] settingsConstructorSignature = new Type[] { typeof(object), typeof(FieldInfo), typeof(SettingsAttribute) };

        public ProcessorsLoader()
        {
        }

        public void LoadAll()
        {
            loadAllAvailableSettings();

            loadAllAvailableProcessors();
        }

        protected void loadAllAvailableProcessors()
        {
            PreProcessors = new SortedList<int, IPreProcessor>();
            Processors = new SortedList<int, IProcessor>();
            PostProcessors = new SortedList<int, IPostProcessor>();

            var type = typeof(IProcessorBase);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsAbstract)
                .Where(p => !p.IsInterface)
                .Where(p => type.IsAssignableFrom(p));

            foreach (var item in types)
            {
                var instance = item
                    .GetConstructor(new Type[] { })
                    .Invoke(new object[] { });

                if (typeof(IPreProcessor).IsAssignableFrom(item))
                {
                    AddProcessor((IPreProcessor)instance);
                } else if (typeof(IProcessor).IsAssignableFrom(item))
                {
                    AddProcessor((IProcessor)instance);
                } else if (typeof(IPostProcessor).IsAssignableFrom(item))
                {
                    AddProcessor((IPostProcessor)instance);
                } else
                {
                    throw new NotImplementedException("This processor type is not supported.");
                }
            }
        }

        protected void loadAllAvailableSettings()
        {
            availableSettings = new Dictionary<Type, Type>();
            settings = new List<ISettings>();

            var type = typeof(ISettings);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsAbstract)
                .Where(p => !p.IsInterface)
                .Where(p => type.IsAssignableFrom(p));

            foreach (var item in types)
            {
                var property = item.GetProperty("ForType", typeof(Type));
                if (property == null)
                    throw new NotImplementedException("All Setting types have to implement 'Type ForType' static property.");

                availableSettings.Add((Type)property.GetValue(null), item);
            }
        }

        protected void retrieveSettingsFromProcessor(IProcessorBase processor)
        {
            foreach (var field in processor.GetType().GetFields())
            {
                SettingsAttribute annotation = (SettingsAttribute)field.GetCustomAttributes(typeof(SettingsAttribute), false)[0];

                ISettings setting = null;

                Type matchedSettings = null;

                if (availableSettings.ContainsKey(field.FieldType))
                {
                    matchedSettings = availableSettings[field.FieldType];
                } else if (availableSettings.ContainsKey(field.FieldType.BaseType))
                {
                    matchedSettings = availableSettings[field.FieldType.BaseType];
                }

                if (matchedSettings == null)
                    throw new NotImplementedException("Setting class for this type has not been implemented yet.");

                setting = (ISettings)matchedSettings
                    .GetConstructor(settingsConstructorSignature)
                    .Invoke(new object[] { processor, field, annotation });

                settings.Add(setting);
            }
        }

        public void AddProcessor(IPreProcessor processor)
        {
            try
            {
                PreProcessors.Add(processor.Priority, processor);
                retrieveSettingsFromProcessor(processor);
            } catch (ArgumentException e)
            {
                throw new ProcessorPriorityColissionException(processor.ToString(), e);
            }
        }

        public void AddProcessor(IProcessor processor)
        {
            try
            {
                Processors.Add(processor.Priority, processor);
                retrieveSettingsFromProcessor(processor);
            } catch (ArgumentException e)
            {
                throw new ProcessorPriorityColissionException(processor.ToString(), e);
            }
        }

        public void AddProcessor(IPostProcessor processor)
        {
            try
            {
                PostProcessors.Add(processor.Priority, processor);
                retrieveSettingsFromProcessor(processor);
            } catch (ArgumentException e)
            {
                throw new ProcessorPriorityColissionException(processor.ToString(), e);
            }
        }

        public IEnumerable<IPreProcessor> GetPreProcessors()
        {
            foreach (var processor in PreProcessors)
            {
                yield return processor.Value;
            }
        }

        public IEnumerable<IProcessor> GetProcessors()
        {
            foreach (var processor in Processors)
            {
                yield return processor.Value;
            }
        }

        public IEnumerable<IPostProcessor> GetPostProcessors()
        {
            foreach (var processor in PostProcessors)
            {
                yield return processor.Value;
            }
        }

        public IEnumerable<ISettings> GetSettings()
        {
            foreach (var option in settings)
            {
                yield return option;
            }
        }

    }
}
