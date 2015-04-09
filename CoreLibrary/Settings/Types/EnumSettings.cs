using System;
using System.Reflection;
using CoreLibrary.Settings.Attributes;

namespace CoreLibrary.Settings.Types
{
    public class EnumSettings : SettingsAbstract
    {
        public static Type ForType { get { return typeof(Enum); } }

        public override int NumberOfParams { get { return 1; } }

        public EnumSettings(object instance, FieldInfo field, SettingsAttribute attribute)
            : base(instance, field, attribute)
        {
        }

        public override void SetValue(params string[] value)
        {
            Field.SetValue(Instance, Enum.Parse(Field.FieldType, value[0]));
            WasSet = true;
        }

        public override string ToString()
        {
            return base.ToString() + String.Format(" Enum[{0}]", String.Join("/", Enum.GetNames(Field.FieldType)));
        }
    }
}
