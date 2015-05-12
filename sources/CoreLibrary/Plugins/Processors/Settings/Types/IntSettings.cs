using System;
using System.Reflection;

namespace CoreLibrary.Plugins.Processors.Settings.Types
{
    /// <summary>
    /// Setting for int field type.
    /// </summary>
    public class IntSettings : SettingsAbstract
    {
        /// <summary>
        /// What type is this setting for.
        /// </summary>
        public static Type ForType { get { return typeof(int); } }

        public override int NumberOfParams { get { return 1; } }

        /// <summary>
        /// Initializes new instance of the <see cref="IntSettings"/>
        /// </summary>
        /// <param name="instance">Instance that contains the setting.</param>
        /// <param name="field">Field assoiated with the setting.</param>
        /// <param name="attribute">Setting's attribute.</param>
        public IntSettings(object instance, FieldInfo field, SettingsAttribute attribute)
            : base(instance, field, attribute)
        {
        }

        public override void SetValue(params string[] value)
        {
            Field.SetValue(Instance, int.Parse(value[0]));
            WasSet = true;
        }
    }
}
