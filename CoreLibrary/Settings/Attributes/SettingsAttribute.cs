using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Settings.Attributes
{
    /// <summary>
    /// Attribute for processor settings.
    /// 
    /// The attribute holds tooltip Info and argument triggers.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SettingsAttribute : System.Attribute
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
