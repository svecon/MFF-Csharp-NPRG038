using System;
using System.Reflection;

namespace CoreLibrary.Plugins.Processors.Settings.Types
{
    public class StringSettings : SettingsAbstract
    {
        public static Type ForType { get { return typeof(string); } }

        public override int NumberOfParams { get { return 1; } }

        public StringSettings(object instance, FieldInfo field, SettingsAttribute attribute)
            : base(instance, field, attribute)
        {
        }

        public override void SetValue(params string[] value)
        {
            Field.SetValue(Instance, value[0]);
            WasSet = true;
        }
    }
}
