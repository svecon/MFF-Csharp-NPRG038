using CoreLibrary.Settings.Attributes;
using System;
using System.Reflection;

namespace CoreLibrary.Settings.Types
{
    public class StringArraySettings : SettingsAbstract
    {
        public static Type ForType { get { return typeof(string[]); } }

        public override int NumberOfParams { get { return 1; } }

        public StringArraySettings(object instance, FieldInfo field, SettingsAttribute attribute)
            : base(instance, field, attribute)
        {
        }

        public override void SetValue(params string[] value)
        {
            Field.SetValue(Instance, value[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            WasSet = true;
        }
    }
}
