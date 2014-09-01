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

namespace CoreLibrary.Processors
{
    public class ProcessorsLoader : IProcessorLoader
    {

        SortedList<int, IPreProcessor> PreProcessors;

        SortedList<int, IProcessor> Processors;

        SortedList<int, IPostProcessor> PostProcessors;

        List<SettingsAbstract> settings;

        public ProcessorsLoader()
        {
            PreProcessors = new SortedList<int, IPreProcessor>();
            Processors = new SortedList<int, IProcessor>();
            PostProcessors = new SortedList<int, IPostProcessor>();

            settings = new List<SettingsAbstract>();
        }

        public void Load()
        {
            AddProcessor(new ExtensionFilterProcessor());
            AddProcessor(new CsharpSourcesFilterProcessor());
            AddProcessor(new SizeTimeDiffProcessor());
            AddProcessor(new BinaryDiffProcessor());
            AddProcessor(new SyncMergeProcessor());
        }

        protected void retrieveSettingsFromProcessor(IProcessorBase processor)
        {
            var x = processor.GetType();
            var y = x.GetFields();
            foreach (var field in processor.GetType().GetFields())
            {
                SettingsAttribute annotation = (SettingsAttribute)field.GetCustomAttributes(typeof(SettingsAttribute), false)[0];

                SettingsAbstract setting = null;

                if (field.FieldType == typeof(bool))
                {
                    setting = new BooleanSettings(processor, field, annotation.Info, annotation.Option, annotation.Shortcut);
                }

                if (setting == null)
                    throw new NotImplementedException("Setting class for this type has not been implemented yet.");

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

        public IEnumerable<SettingsAbstract> GetSettings()
        {
            foreach (var option in settings)
            {
                yield return option;
            }
        }

    }
}
