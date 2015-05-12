using System.Reflection;

namespace CoreLibrary.Plugins.Processors.Settings
{
    /// <summary>
    /// ISettings is a base interface for a Processor's Setting.
    /// 
    /// Each processor may have any number of settings which are usually defined
    /// as attributes over a class fields.
    /// 
    /// Those attributes are then instantiated into a proper setting classes based on their type.
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Information for given setting. Behaves like a tooltip.
        /// </summary>
        string Info { get; }

        /// <summary>
        /// Switch to trigger this setting.
        /// </summary>
        string Argument { get; }

        /// <summary>
        /// ArgumentShortcut to trigger this setting.
        /// </summary>
        string ArgumentShortcut { get; }

        /// <summary>
        /// True if the setting was changed.
        /// </summary>
        bool WasSet { get; }

        /// <summary>
        /// Number of parameters the setting needs after an argument.
        /// </summary>
        int NumberOfParams { get; }

        /// <summary>
        /// Method that tries to parse the NumberOfParam strings and assigns new value based them.
        /// 
        /// Usually each setting type needs to override this method in order to parse the input properly.
        /// </summary>
        /// <param name="value">String arguments to be parsed.</param>
        void SetValue(params string[] value);

        /// <summary>
        /// Gets value of the settings.
        /// </summary>
        /// <returns>Values of the settings as object.</returns>
        object GetValue();

        /// <summary>
        /// An object instance that this setting is for.
        /// </summary>
        object Instance { get; }

        /// <summary>
        /// Field info for the filed that the setting attribute was found on.
        /// </summary>
        FieldInfo Field { get; }
    }
}
