using CoreLibrary.Settings.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Settings.Types
{
    public class StringArraySettings : SettingsAbstract
    {
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
