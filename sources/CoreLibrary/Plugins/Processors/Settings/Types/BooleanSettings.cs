using System;
using System.Reflection;

namespace CoreLibrary.Plugins.Processors.Settings.Types
{
    /// <summary>
    /// Setting for bool field type.
    /// </summary>
    public class BooleanSettings : SettingsAbstract
    {
        /// <summary>
        /// What type is this setting for.
        /// </summary>
        public static Type ForType { get { return typeof(bool); } }

        public override int NumberOfParams { get { return 0; } }

        /// <summary>
        /// Initializes new instance of the <see cref="BooleanSettings"/>
        /// </summary>
        /// <param name="instance">Instance that contains the setting.</param>
        /// <param name="field">Field assoiated with the setting.</param>
        /// <param name="attribute">Setting's attribute.</param>
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
