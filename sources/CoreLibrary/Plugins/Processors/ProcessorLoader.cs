using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CoreLibrary.Exceptions;
using CoreLibrary.Plugins.Processors.Settings;

namespace CoreLibrary.Plugins.Processors
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
        /// <summary>
        /// A dictionary that contains list of processors by the processor type.
        /// </summary>
        protected Dictionary<ProcessorTypeEnum, SortedList<int, IProcessor>> ProcessorsDictionary;

        /// <summary>
        /// Dictionary for searching lists of settings by a processor name.
        /// </summary>
        protected readonly Dictionary<string, List<ISettings>> SettingsByProcessor;

        /// <summary>
        /// Dictionary for searching processors by name.
        /// </summary>
        protected readonly Dictionary<string, IProcessor> ProcessorByName;

        /// <summary>
        /// Structure that holds available settings types (for different variable types).
        /// </summary>
        Dictionary<Type, Type> availableSettings;

        /// <summary>
        /// Signature (array of types) for settings constructor.
        /// </summary>
        private readonly Type[] settingsConstructorSignature = { typeof(object), typeof(FieldInfo), typeof(SettingsAttribute) };

        /// <summary>
        /// Initializes new instance of the <see cref="ProcessorLoader"/>
        /// </summary>
        public ProcessorLoader()
        {
            ProcessorsDictionary = new Dictionary<ProcessorTypeEnum, SortedList<int, IProcessor>>();
            SettingsByProcessor = new Dictionary<string, List<ISettings>>();
            ProcessorByName = new Dictionary<string, IProcessor>();
        }

        /// <inheritdoc />
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
            foreach (Type type in PluginsLoader.AssemblyTypes()
                .Where(p => !p.IsAbstract)
                .Where(p => !p.IsInterface)
                .Where(p => typeof(IProcessor).IsAssignableFrom(p))
                )
            {

                try
                {
                    ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { });

                    if (constructorInfo == null) continue;

                    var instance = (IProcessor)constructorInfo.Invoke(new object[] { });
                    AddProcessor(instance);
                    RetrieveSettings(instance);

                } catch (Exception)
                {
                    // ignores plugin errors in release version
#if DEBUG
                    // rethrow in Debug mode; ignore in Production if faulty 
                    throw;
#endif
                }
            }

        }

        /// <summary>
        /// Traverses currently loaded assemblies and load all classes with ISettings interface,
        /// which are used as a base for every settings that instance can have.
        /// </summary>
        public void LoadAllAvailableSettings()
        {
            availableSettings = new Dictionary<Type, Type>();

            foreach (Type item in PluginsLoader.AssemblyTypes()
                .Where(p => !p.IsAbstract)
                .Where(p => !p.IsInterface)
                .Where(p => typeof(ISettings).IsAssignableFrom(p))
                )
            {
                try
                {
                    PropertyInfo property = item.GetProperty("ForType", typeof(Type));
                    if (property == null)
                        throw new NotImplementedException(
                            "All Setting types have to implement 'Type ForType' static property.");

                    availableSettings.Add((Type)property.GetValue(null), item);

                } catch (Exception)
                {
                    // ignores plugin errors in release version
#if DEBUG
                    // rethrow in Debug mode; ignore in Production if faulty 
                    throw;
#endif
                }
            }
        }

        /// <inheritdoc />
        public void RetrieveSettings(object instance, bool isStatic = false)
        {
            var settingsByProcessorList = new List<ISettings>();

            if (isStatic && !(instance is Type))
                throw new ArgumentException("Instance must be type of Type when isStatic");

            foreach (FieldInfo field in isStatic ? ((Type)instance).GetFields() : instance.GetType().GetFields())
            {
                try
                {
                    object[] attributes = field.GetCustomAttributes(typeof(SettingsAttribute), false);

                    if (attributes.Length == 0)
                        continue;

                    var annotation = (SettingsAttribute)attributes[0];

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

                    var settingsInstance =
                        (ISettings)constructorInfo.Invoke(new object[] { isStatic ? null : instance, field, annotation });

                    settingsByProcessorList.Add(settingsInstance);

                } catch (Exception)
                {
                    // ignores plugin errors in release version
#if DEBUG
                    // rethrow in Debug mode; ignore in Production if faulty 
                    throw;
#endif
                }
            }

            string instanceType = instance is IProcessor && !isStatic ? instance.GetType().ToString() : typeof(Object).ToString();
            SettingsByProcessor.Add(instanceType, settingsByProcessorList);
        }

        /// <inheritdoc />
        public void AddProcessor(IProcessor processor)
        {
            var attr = (ProcessorAttribute)processor.GetType().GetCustomAttribute(typeof(ProcessorAttribute));

            if (attr == null)
            {
#if DEBUG
                // rethrow in Debug mode; ignore in Production if faulty 
                throw new ConstraintException("Every processor must implement ProcessorAttribute.");
#endif
#pragma warning disable 162
                return;
#pragma warning restore 162
            }

            SortedList<int, IProcessor> list;
            if (!ProcessorsDictionary.TryGetValue(attr.ProcessorType, out list))
            {
                list = new SortedList<int, IProcessor>();
                ProcessorsDictionary.Add(attr.ProcessorType, list);
            }

            try
            {
                list.Add(attr.Priority, processor);
                ProcessorByName.Add(processor.GetType().ToString(), processor);

#pragma warning disable 0168
            } catch (ArgumentException e)
#pragma warning restore 0168
            {
#if DEBUG
                throw new ProcessorPriorityColissionException(processor.ToString(), e);
                // TODO: load processors anyway in undefined order, print out warning for the user?
#endif
            } catch (Exception)
            {
                // ignores plugin errors in release version
#if DEBUG
                // rethrow in Debug mode; ignore in Production if faulty 
                throw;
#endif
            }
        }

        /// <inheritdoc />
        public IProcessorLoader SplitLoaderByType(ProcessorTypeEnum processorType)
        {
            var newLoader = new ProcessorLoader();

            foreach (IProcessor processor in GetProcessors(processorType))
            {
                newLoader.AddProcessor(processor);
            }

            return newLoader;
        }

        /// <inheritdoc />
        public IEnumerable<IProcessor> GetProcessors(ProcessorTypeEnum processorType)
        {
            return !ProcessorsDictionary.ContainsKey(processorType)
                ? Enumerable.Empty<IProcessor>()
                : ProcessorsDictionary[processorType].Select(valuePair => valuePair.Value);
        }

        /// <inheritdoc />
        public IEnumerable<ISettings> GetSettings()
        {
            return SettingsByProcessor.Values.SelectMany(settings => settings);
        }

        /// <inheritdoc />
        public IEnumerable<ISettings> GetSettingsByProcessor(IProcessor processor)
        {
            return SettingsByProcessor[processor.GetType().ToString()];
        }

        /// <inheritdoc />
        public IEnumerable<ISettings> GetSettingsByProcessor(string processor)
        {
            return SettingsByProcessor[processor];
        }
    }
}
