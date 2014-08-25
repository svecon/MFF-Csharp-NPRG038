using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Interfaces;

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
            AddProcessor(new SizeTimeDiffProcessor());
            //AddProcessor(new BinaryProcessor());
        }

        public void AddProcessor(IPreProcessor processor)
        {
            PreProcessors.Add(processor.Priority, processor);
        }

        public void AddProcessor(IProcessor processor)
        {
            Processors.Add(processor.Priority, processor);
        }

        public void AddProcessor(IPostProcessor processor)
        {
            PostProcessors.Add(processor.Priority, processor);
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
