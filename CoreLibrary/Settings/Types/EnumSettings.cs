using CoreLibrary.Settings.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
    }
}
