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

namespace CoreLibrary.Processors
{
    public class ProcessorsLoader : IProcessorLoader
    {

        SortedList<int, IPreProcessor> PreProcessors;

        SortedList<int, IProcessor> Processors;

        SortedList<int, IPostProcessor> PostProcessors;

        public ProcessorsLoader()
        {
            PreProcessors = new SortedList<int, IPreProcessor>();
            Processors = new SortedList<int, IProcessor>();
            PostProcessors = new SortedList<int, IPostProcessor>();
        }

        public void Load()
        {
            AddProcessor(new ExtensionFilterProcessor());
            AddProcessor(new SizeTimeDiffProcessor());
            AddProcessor(new BinaryDiffProcessor());
            AddProcessor(new SyncMergeProcessor());
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

    }
}
