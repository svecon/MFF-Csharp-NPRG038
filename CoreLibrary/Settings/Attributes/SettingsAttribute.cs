using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Settings.Attributes
{
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SettingsAttribute : System.Attribute
    {
        public string Info { get; set; }

        public string Option { get; set; }

        public string Shortcut { get; set; }

        public SettingsAttribute(string info, string option, string shortcut = null)
        {
            Info = info;
            Option = option;
            Shortcut = shortcut;
        }
    }
}
