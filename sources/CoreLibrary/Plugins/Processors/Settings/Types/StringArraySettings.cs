using System;
using System.Reflection;

namespace CoreLibrary.Plugins.Processors.Settings.Types
{
    /// <summary>
    /// Setting for array of strings field type.
    /// </summary>
    public class StringArraySettings : SettingsAbstract
    {
        /// <summary>
        /// What type is this setting for.
        /// </summary>
        public static Type ForType { get { return typeof(string[]); } }

        /// <inheritdoc />
        public override int NumberOfParams { get { return 1; } }

        /// <summary>
        /// Initializes new instance of the <see cref="StringArraySettings"/>
        /// </summary>
        /// <param name="instance">Instance that contains the setting.</param>
        /// <param name="field">Field assoiated with the setting.</param>
        /// <param name="attribute">Setting's attribute.</param>
        public StringArraySettings(object instance, FieldInfo field, SettingsAttribute attribute)
            : base(instance, field, attribute)
        {
        }

        /// <inheritdoc />
        public override void SetValue(params string[] value)
        {
            Field.SetValue(Instance, value[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            WasSet = true;
        }
    }
}
