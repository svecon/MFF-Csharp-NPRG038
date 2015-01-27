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

        SortedList<int, IPreProcessor> preProcessors;

        SortedList<int, IProcessor> processors;

        SortedList<int, IPostProcessor> postProcessors;

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
        }

        public void LoadAll()
        {
            LoadAllAvailableSettings();

            LoadAllAvailableProcessors();
        }

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

        protected void retrieveSettingsFromProcessor(IProcessorBase processor)
        {
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

                settings.Add((ISettings)constructorInfo.Invoke(new object[] { processor, field, annotation }));
            }
        }

        public void AddProcessor(IPreProcessor processor)
        {
            try
            {
                preProcessors.Add(processor.Priority, processor);
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
                processors.Add(processor.Priority, processor);
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
                postProcessors.Add(processor.Priority, processor);
                retrieveSettingsFromProcessor(processor);
            } catch (ArgumentException e)
            {
                throw new ProcessorPriorityColissionException(processor.ToString(), e);
            }
        }

        public IEnumerable<IPreProcessor> GetPreProcessors()
        {
            return preProcessors.Select(processor => processor.Value); // LINQ YIELD
        }

        public IEnumerable<IProcessor> GetProcessors()
        {
            return processors.Select(processor => processor.Value); // LINQ YIELD
        }

        public IEnumerable<IPostProcessor> GetPostProcessors()
        {
            return postProcessors.Select(processor => processor.Value); // LINQ YIELD
        }

        public IEnumerable<ISettings> GetSettings()
        {
            return settings; // LINQ YIELD
        }
    }
}
