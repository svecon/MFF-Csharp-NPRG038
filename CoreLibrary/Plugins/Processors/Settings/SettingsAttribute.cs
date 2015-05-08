using System;

namespace CoreLibrary.Plugins.Processors.Settings
{
    /// <summary>
    /// Attribute for processor settings.
    /// 
    /// The attribute holds tooltip Info and Switch triggers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SettingsAttribute : Attribute
    {
        /// <summary>
        /// Information about the setting.
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// Switch that will be used to look up the setting.
        /// </summary>
        public string Switch { get; set; }

        /// <summary>
        /// Short version of a switch, <see cref="Switch"/>
        /// </summary>
        public string ShortSwitch { get; set; }

        /// <summary>
        /// Initializes new instance of the <see cref="SettingsAttribute"/>
        /// </summary>
        /// <param name="info">Information about the setting.</param>
        /// <param name="switch">Switch that will be used to look up the setting.</param>
        /// <param name="shortSwitch">Short version of a switch.</param>
        public SettingsAttribute(string info, string @switch, string shortSwitch = null)
        {
            Info = info;
            Switch = @switch;
            ShortSwitch = shortSwitch;
        }
    }
}
