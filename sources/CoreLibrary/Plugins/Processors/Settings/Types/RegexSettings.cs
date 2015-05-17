using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CoreLibrary.Plugins.Processors.Settings.Types
{
    /// <summary>
    /// Setting for regex field type.
    /// </summary>
    public class RegexSettings : SettingsAbstract
    {
        /// <summary>
        /// What type is this setting for.
        /// </summary>
        public static Type ForType { get { return typeof(Regex); } }

        /// <inheritdoc />
        public override int NumberOfParams { get { return 1; } }

        /// <summary>
        /// Initializes new instance of the <see cref="RegexSettings"/>
        /// </summary>
        /// <param name="instance">Instance that contains the setting.</param>
        /// <param name="field">Field assoiated with the setting.</param>
        /// <param name="attribute">Setting's attribute.</param>
        public RegexSettings(object instance, FieldInfo field, SettingsAttribute attribute)
            : base(instance, field, attribute)
        {
        }

        /// <inheritdoc />
        public override void SetValue(params string[] value)
        {
            Field.SetValue(Instance, new Regex(value[0], RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant));
            WasSet = true;
        }
    }
}
