using System;
using System.IO;
using System.Reflection;

namespace CoreLibrary.Plugins
{
    public class PluginsLoader
    {
        public static void LoadAssemblies()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

            if (!Directory.Exists(path))
                return;

            foreach (string file in Directory.GetFiles(path, "*.dll"))
            {
                Assembly.LoadFile(file);
            }
        }
    }
}
