using System;
using System.Collections.Generic;
using System.Linq;
using CoreLibrary.Interfaces;
using CoreLibrary.Exceptions;
using CoreLibrary.Settings.Attributes;
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
        readonly SortedList<int, IPreProcessor> preProcessors;

        readonly SortedList<int, IProcessor> processors;

        readonly SortedList<int, IPostProcessor> postProcessors;

        private readonly Dictionary<string, List<ISettings>> settingsByProcessor;

            /// <summary>
        /// List of all settings for loaded processors.
        /// </summary>
        List<ISettings> settings;

        /// <summary>
        /// Structure that holds available settings types (for different variable types).
        /// </summary>
        Dictionary<Type, Type> availableSettings;

        private readonly Type[] settingsConstructorSignature = { typeof(object), typeof(FieldInfo), typeof(SettingsAttribute) };

        public ProcessorsLoader()
        {
            preProcessors = new SortedList<int, IPreProcessor>();
            processors = new SortedList<int, IProcessor>();
            postProcessors = new SortedList<int, IPostProcessor>();
            settingsByProcessor = new Dictionary<string, List<ISettings>>();
        }

        public void LoadAll()
        {
            LoadAllAvailableSettings();

            LoadAllAvailableProcessors();
        }

        /// <summary>
        /// Traverses currently loaded assemblies and loads all classes with Processor interfaces.
        /// </summary>
        public void LoadAllAvailableProcessors()
        {
            Type type = typeof(IProcessorBase);
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsAbstract)
                .Where(p => !p.IsInterface)
                .Where(p => type.IsAssignableFrom(p));

            foreach (Type item in types)
            {
                try
                {
                    ConstructorInfo constructorInfo = item.GetConstructor(new Type[] { });

                    if (constructorInfo == null) continue;

                    object instance = constructorInfo.Invoke(new object[] { });

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
                } catch (NullReferenceException) { }
            }
        }

        /// <summary>
        /// Traverses currently loaded assemblies and load all classes with ISettings interface,
        /// which are used as a base for every settings that processor can have.
        /// </summary>
        public void LoadAllAvailableSettings()
        {
            availableSettings = new Dictionary<Type, Type>();
            settings = new List<ISettings>();

            Type type = typeof(ISettings);
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsAbstract)
                .Where(p => !p.IsInterface)
                .Where(p => type.IsAssignableFrom(p));

            foreach (Type item in types)
            {
                PropertyInfo property = item.GetProperty("ForType", typeof(Type));
                if (property == null)
                    throw new NotImplementedException("All Setting types have to implement 'Type ForType' static property.");

                availableSettings.Add((Type)property.GetValue(null), item);
            }
        }

        /// <summary>
        /// Traverses through found processors and creates instances of annotated fields with ISettings annotation.
        /// </summary>
        /// <param name="processor"></param>
        protected void RetrieveSettingsFromProcessor(IProcessorBase processor)
        {
            var settingsByProcessorList = new List<ISettings>();

            foreach (FieldInfo field in processor.GetType().GetFields())
            {
                var annotation = (SettingsAttribute)field.GetCustomAttributes(typeof(SettingsAttribute), false)[0];

                Type matchedSettings = null;

                if (availableSettings.ContainsKey(field.FieldType))
                {
                    matchedSettings = availableSettings[field.FieldType];
                } else if (field.FieldType.BaseType != null && availableSettings.ContainsKey(field.FieldType.BaseType))
                {
                    matchedSettings = availableSettings[field.FieldType.BaseType];
                }

                if (matchedSettings == null)
                    throw new NotImplementedException("Setting class for this type has not been implemented yet.");

                ConstructorInfo constructorInfo = matchedSettings.GetConstructor(settingsConstructorSignature);
                if (constructorInfo == null) continue;

                var instance = (ISettings) constructorInfo.Invoke(new object[] {processor, field, annotation});

                settings.Add(instance);
                settingsByProcessorList.Add(instance);
            }

            settingsByProcessor.Add(processor.GetType().ToString(), settingsByProcessorList);
        }

        public void AddProcessor(IPreProcessor processor)
        {
            try
            {
                preProcessors.Add(processor.Priority, processor);
                RetrieveSettingsFromProcessor(processor);
            } catch (ArgumentException e)
            {
                throw new ProcessorPriorityColissionException(processor.ToString(), e);
            }
        }

        public void AddProcessor(IProcessor processor)
        {
            try
            {
                processors.Add(processor.Priority, processor);
                RetrieveSettingsFromProcessor(processor);
            } catch (ArgumentException e)
            {
                throw new ProcessorPriorityColissionException(processor.ToString(), e);
            }
        }

        public void AddProcessor(IPostProcessor processor)
        {
            try
            {
                postProcessors.Add(processor.Priority, processor);
                RetrieveSettingsFromProcessor(processor);
            } catch (ArgumentException e)
            {
                throw new ProcessorPriorityColissionException(processor.ToString(), e);
            }
        }

        private void PrintProcessorInfo(IProcessorBase processor)
        {
            Console.WriteLine("\n{0,10} {1} in {2}", processor.Priority, processor.GetType().Name, processor.GetType().Namespace);

            foreach (ISettings procSettings in settingsByProcessor[processor.GetType().ToString()])
                Console.WriteLine("{0,10} {1}", "", procSettings.ToString());
        }

        public void PrintLoadedProcessors()
        {
            Console.Write("PreProcessors ---");
            foreach (IPreProcessor processor in GetPreProcessors())
            {
                PrintProcessorInfo(processor);
            }

            Console.Write("\nProcessors ---");
            foreach (IProcessor processor in GetProcessors())
            {
                PrintProcessorInfo(processor);
            }

            Console.Write("\nPostProcessors ---");
            foreach (IPostProcessor processor in GetPostProcessors())
            {
                PrintProcessorInfo(processor);
            }
        }

        public IEnumerable<IPreProcessor> GetPreProcessors()
        {
            return preProcessors.Select(processor => processor.Value);
        }

        public IEnumerable<IProcessor> GetProcessors()
        {
            return processors.Select(processor => processor.Value);
        }

        public IEnumerable<IPostProcessor> GetPostProcessors()
        {
            return postProcessors.Select(processor => processor.Value);
        }

        public IEnumerable<ISettings> GetSettings()
        {
            return settings;
        }
    }
}
