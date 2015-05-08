using System;

namespace CoreLibrary.Plugins.DiffWindow
{
    /// <summary>
    /// Attribute for <see cref="IDiffWindowMenu"/> plugins
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DiffWindowMenuAttribute : Attribute
    {
        /// <summary>
        /// Priority of the <see cref="IDiffWindowMenu"/> used for ordering the menus.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Initializes new instance of the <see cref="DiffWindowAttribute"/>
        /// </summary>
        /// <param name="priority"><see cref="Priority"/></param>
        public DiffWindowMenuAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
