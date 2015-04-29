using System;
using System.Reflection;

namespace CoreLibrary.Plugins.Processors.Settings.Types
{
    public class BooleanSettings : SettingsAbstract
    {
        public static Type ForType { get { return typeof(bool); } }

        public override int NumberOfParams { get { return 0; } }

        public BooleanSettings(object instance, FieldInfo field, SettingsAttribute attribute)
            : base(instance, field, attribute)
        {
        }

        public override void SetValue(params string[] value)
        {
            if (value.Length == 0)
            {
                Field.SetValue(Instance, !(bool)Field.GetValue(Instance));
            } else
            {
                Field.SetValue(Instance, bool.Parse(value[0]));
            }

            WasSet = true;
        }
    }
}
