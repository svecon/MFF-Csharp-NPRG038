using System;

namespace CoreLibrary.DiffWindow
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DiffWindowAttribute : Attribute
    {
        public int Priority { get; set; }

        public DiffWindowAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
