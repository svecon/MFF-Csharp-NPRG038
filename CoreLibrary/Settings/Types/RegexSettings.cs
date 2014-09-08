using CoreLibrary.Settings.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoreLibrary.Settings.Types
{
    public class RegexSettings : SettingsAbstract
    {
        public static Type ForType { get { return typeof(Regex); } }

        public override int NumberOfParams { get { return 1; } }

        public RegexSettings(object instance, FieldInfo field, SettingsAttribute attribute)
            : base(instance, field, attribute)
        {
        }

        public override void SetValue(params string[] value)
        {
            Field.SetValue(Instance, new Regex(value[0], RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant));
            WasSet = true;
        }
    }
}
