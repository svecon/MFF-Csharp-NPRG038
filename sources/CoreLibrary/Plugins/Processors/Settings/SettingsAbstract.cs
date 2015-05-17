using System.Reflection;
using System.Text;

namespace CoreLibrary.Plugins.Processors.Settings
{
    /// <summary>
    /// Abstract class for Processor's settings.
    /// 
    /// This class needs to be extended for needed field types.
    /// 
    /// All Settings classes MUST HAVE a static ForType attribute
    /// which defines what field type the class is for.
    /// </summary>
    public abstract class SettingsAbstract : ISettings
    {
        // MUST HAVE for children
        //public abstract Type ForType { get; }

        /// <inheritdoc />
        public string Info { get; protected set; }

        /// <inheritdoc />
        public string Argument { get; protected set; }

        /// <inheritdoc />
        public string ArgumentShortcut { get; protected set; }

        /// <inheritdoc />
        public bool WasSet { get; protected set; }

        /// <inheritdoc />
        public abstract int NumberOfParams { get; }

        /// <inheritdoc />
        public object Instance { get; protected set; }

        /// <inheritdoc />
        public FieldInfo Field { get; protected set; }

        /// <summary>
        /// Initializes new instance of the <see cref="SettingsAbstract"/>
        /// </summary>
        /// <param name="instance">Instance of the class that has the setting.</param>
        /// <param name="field">Field that is associated with the setting.</param>
        /// <param name="attribute">Setting attribute.</param>
        protected SettingsAbstract(object instance, FieldInfo field, SettingsAttribute attribute)
        {
            Instance = instance;
            Field = field;

            Info = attribute.Info;
            Argument = attribute.Switch;
            ArgumentShortcut = attribute.ShortSwitch;

            WasSet = false;
        }

        /// <inheritdoc />
        public abstract void SetValue(params string[] value);

        /// <inheritdoc />
        public object GetValue()
        {
            return Field.GetValue(Instance);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (ArgumentShortcut != null)
            {
                sb.AppendFormat("  -{0, -2}", ArgumentShortcut);
            } else
            {
                sb.AppendFormat("{0,5}", "");
            }

            if (Argument != null)
            {
                sb.AppendFormat(" --{0}", Argument);
            }

            sb.AppendFormat(" [{0}] ", NumberOfParams);

            sb.Append(Info);

            return sb.ToString();
        }
    }
}
