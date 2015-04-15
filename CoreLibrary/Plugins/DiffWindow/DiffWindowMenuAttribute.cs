using System;

namespace CoreLibrary.Plugins.DiffWindow
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DiffWindowMenuAttribute : Attribute
    {
        public int Priority { get; set; }

        public DiffWindowMenuAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
