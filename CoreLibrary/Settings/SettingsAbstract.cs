using CoreLibrary.Settings.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Settings
{
    public abstract class SettingsAbstract
    {
        public string Info { get; protected set; }

        public string Option { get; protected set; }

        public string OptionShortcut { get; protected set; }

        public bool WasSet { get; protected set; }

        public abstract int NumberOfParams { get; }

        protected object instance;

        protected FieldInfo field;

        public SettingsAbstract(object instance, FieldInfo field, SettingsAttribute attribute)
        {
            this.instance = instance;
            this.field = field;

            Info = attribute.Info;
            Option = attribute.Option;
            OptionShortcut = attribute.Shortcut;

            WasSet = false;
        }

        public abstract void SetValue(params string[] value);
    }
}
