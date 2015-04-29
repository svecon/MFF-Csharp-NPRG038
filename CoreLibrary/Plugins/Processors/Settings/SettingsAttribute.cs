using System;

namespace CoreLibrary.Plugins.Processors.Settings
{
    /// <summary>
    /// Attribute for processor settings.
    /// 
    /// The attribute holds tooltip Info and argument triggers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SettingsAttribute : Attribute
    {
        public string Info { get; set; }

        public string Argument { get; set; }

        public string Shortcut { get; set; }

        public SettingsAttribute(string info, string argument, string shortcut = null)
        {
            Info = info;
            Argument = argument;
            Shortcut = shortcut;
        }
    }
}
