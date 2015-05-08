using System;

namespace CoreLibrary.Plugins.DiffWindow
{
    /// <summary>
    /// Attribute for <see cref="IDiffWindow{TNode}"/> plugins
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DiffWindowAttribute : Attribute
    {
        /// <summary>
        /// Priority of the <see cref="IDiffWindow{TNode}"/> used for creating more specific visualisations.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Initializes new instance of the <see cref="DiffWindowAttribute"/>
        /// </summary>
        /// <param name="priority"><see cref="Priority"/></param>
        public DiffWindowAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
