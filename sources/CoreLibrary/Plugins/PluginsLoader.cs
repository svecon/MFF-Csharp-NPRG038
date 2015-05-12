using System;
using System.IO;
using System.Reflection;

namespace CoreLibrary.Plugins
{
    /// <summary>
    /// Plugins loader loads aseemblies that will be used as plugins.
    /// </summary>
    public class PluginsLoader
    {
        /// <summary>
        /// Loads all assemblies from the plugins folder next to the executable.
        /// 
        /// TODO load assemblies into another domain to prevent running malicious code
        /// </summary>
        public static void LoadAssemblies()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

            if (!Directory.Exists(path))
                return;

            foreach (string file in Directory.GetFiles(path, "*.dll"))
            {
                Assembly.LoadFrom(file);
            }
        }
    }
}
