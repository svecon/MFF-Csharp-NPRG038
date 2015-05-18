using System;
using System.Collections.Generic;
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
        /// A list of all available types.
        /// </summary>
        private static List<Type> _assemblyTypes = new List<Type>();

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

            _assemblyTypes = new List<Type>();
        }

        /// <summary>
        /// All types from currently loaded assemblies.
        /// 
        /// TODO exclude system asseblies
        /// </summary>
        /// <returns>An enumerable of all types in loaded assemblies.</returns>
        public static IEnumerable<Type> AssemblyTypes()
        {
            if (_assemblyTypes.Count > 0)
                return _assemblyTypes;


            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    _assemblyTypes.AddRange(assembly.GetTypes());
                } catch (Exception)
                {
                    // ignores plugin errors in release version
#if DEBUG
                    // rethrow in Debug mode; ignore in Production if faulty 
                    throw;
#endif
                }
            }

            return _assemblyTypes;
        }
    }
}
