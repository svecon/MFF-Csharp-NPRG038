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
    public class ProcessorLoader : IProcessorLoader
    {
        protected readonly SortedList<int, IPreProcessor> PreProcessors;

        protected readonly SortedList<int, IProcessor> Processors;

        protected readonly SortedList<int, IPostProcessor> PostProcessors;

        protected readonly Dictionary<string, List<ISettings>> SettingsByProcessor;

        protected readonly Dictionary<string, IProcessorBase> ProcessorByName;

        /// <summary>
        /// Structure that holds available settings types (for different variable types).
        /// </summary>
        Dictionary<Type, Type> availableSettings;

        private readonly Type[] settingsConstructorSignature = { typeof(object), typeof(FieldInfo), typeof(SettingsAttribute) };

        public ProcessorLoader()
        {
            PreProcessors = new SortedList<int, IPreProcessor>();
            Processors = new SortedList<int, IProcessor>();
            PostProcessors = new SortedList<int, IPostProcessor>();
            SettingsByProcessor = new Dictionary<string, List<ISettings>>();
            ProcessorByName = new Dictionary<string, IProcessorBase>();
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

                    var instance = (IProcessorBase)constructorInfo.Invoke(new object[] { });
                    AddProcessor(instance, item);
                    RetrieveSettings(instance);

                } catch (NullReferenceException) { }
            }
        }

        /// <summary>
        /// Traverses currently loaded assemblies and load all classes with ISettings interface,
        /// which are used as a base for every settings that instance can have.
        /// </summary>
        public void LoadAllAvailableSettings()
        {
            availableSettings = new Dictionary<Type, Type>();

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

        public void RetrieveSettings(object instance, bool isStatic = false)
        {
            var settingsByProcessorList = new List<ISettings>();

            if (isStatic && !(instance is Type))
                throw new ArgumentException("Instance must be type of Type when isStatic");

            foreach (FieldInfo field in isStatic ? ((Type)instance).GetFields() : instance.GetType().GetFields())
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

                var settingsInstance = (ISettings) constructorInfo.Invoke(new object[] { isStatic ? null : instance, field, annotation});

                settingsByProcessorList.Add(settingsInstance);
            }

            string instanceType = instance is IProcessorBase && !isStatic ? instance.GetType().ToString() : typeof (Object).ToString();
            SettingsByProcessor.Add(instanceType, settingsByProcessorList);
        }

        public void AddProcessor(IProcessorBase processor, Type item = null)
        {
            if (item == null)
            {
                item = processor.GetType();
            }

            if (typeof(IPreProcessor).IsAssignableFrom(item))
            {
                AddProcessor((IPreProcessor)processor);
            } else if (typeof(IProcessor).IsAssignableFrom(item))
            {
                AddProcessor((IProcessor)processor);
            } else if (typeof(IPostProcessor).IsAssignableFrom(item))
            {
                AddProcessor((IPostProcessor)processor);
            } else
            {
                throw new NotImplementedException("This instance type is not supported.");
            }
        }

        public void AddProcessor(IPreProcessor processor)
        {
            try
            {
                PreProcessors.Add(processor.Priority, processor);
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
            } catch (ArgumentException e)
            {
                throw new ProcessorPriorityColissionException(processor.ToString(), e);
            }
        }

        public IProcessorLoader SplitLoaderUsing(params string[] processors)
        {
            var newLoader = new ProcessorLoader();

            foreach (string processorName in processors)
            {
                IProcessorBase processor;
                if (!ProcessorByName.TryGetValue(processorName, out processor))
                {
                    throw new KeyNotFoundException(
                        String.Format("Requested instance '{0}' was not found.", processorName)
                    );
                }

                newLoader.AddProcessor(processor);
            }

            return newLoader;
        }

        public IEnumerable<IPreProcessor> GetPreProcessors()
        {
            return PreProcessors.Select(processor => processor.Value);
        }

        public IEnumerable<IProcessor> GetProcessors()
        {
            return Processors.Select(processor => processor.Value);
        }

        public IEnumerable<IPostProcessor> GetPostProcessors()
        {
            return PostProcessors.Select(processor => processor.Value);
        }

        public IEnumerable<ISettings> GetSettings()
        {
            return SettingsByProcessor.Values.SelectMany(settings => settings);
        }

        public IEnumerable<ISettings> GetSettingsByProcessor(string processorName)
        {
            return SettingsByProcessor[processorName];
        }
    }
}
