using System;
using System.Reflection;

namespace CoreLibrary.Plugins.Processors.Settings.Types
{
    /// <summary>
    /// Setting for enum field type.
    /// </summary>
    public class EnumSettings : SettingsAbstract
    {
        /// <summary>
        /// What type is this setting for.
        /// </summary>
        public static Type ForType { get { return typeof(Enum); } }

        /// <inheritdoc />
        public override int NumberOfParams { get { return 1; } }

        /// <summary>
        /// Initializes new instance of the <see cref="EnumSettings"/>
        /// </summary>
        /// <param name="instance">Instance that contains the setting.</param>
        /// <param name="field">Field assoiated with the setting.</param>
        /// <param name="attribute">Setting's attribute.</param>
        public EnumSettings(object instance, FieldInfo field, SettingsAttribute attribute)
            : base(instance, field, attribute)
        {
        }

        /// <inheritdoc />
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
