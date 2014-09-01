using CoreLibrary.Interfaces;
using CoreLibrary.Settings.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Settings
{
    public abstract class SettingsAbstract : ISettings
    {
        public string Info { get; protected set; }

        public string Option { get; protected set; }

        public string OptionShortcut { get; protected set; }

        public bool WasSet { get; protected set; }

        public abstract int NumberOfParams { get; }

        public object Instance { get; protected set; }

        public FieldInfo Field { get; protected set; }

        public SettingsAbstract(object instance, FieldInfo field, SettingsAttribute attribute)
        {
            Instance = instance;
            Field = field;

            Info = attribute.Info;
            Option = attribute.Option;
            OptionShortcut = attribute.Shortcut;

            WasSet = false;
        }

        public abstract void SetValue(params string[] value);
    }
}
