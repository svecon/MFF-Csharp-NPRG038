using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.Plugins.DiffWindow
{
    using DW = IDiffWindow<IFilesystemTreeVisitable>;

    public class DiffWindowLoader
    {
        private readonly SortedList<int, Type> availableWindows;
        private readonly SortedList<int, Type> availableWindowMenus;
        private readonly IDiffWindowManager manager;

        public DiffWindowLoader(IDiffWindowManager diffManager)
        {
            manager = diffManager;
            availableWindows = new SortedList<int, Type>();
            availableWindowMenus = new SortedList<int, Type>();
        }

        public void LoadWindows()
        {
            Type type = typeof(DW);
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsAbstract)
                .Where(p => !p.IsInterface)
                .Where(p => type.IsAssignableFrom(p));

            foreach (Type item in types)

                try
                {
                    var attr = (DiffWindowAttribute)item.GetCustomAttributes(typeof(DiffWindowAttribute), false)[0];
                    availableWindows.Add(attr.Priority, item);
                } catch (Exception)
                {
#if DEBUG
                    throw;
#endif
                }
        }

        public void LoadWindowMenus()
        {
            Type type = typeof(IDiffWindowMenu);
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsAbstract)
                .Where(p => !p.IsInterface)
                .Where(p => type.IsAssignableFrom(p));

            foreach (Type item in types)

                try
                {
                    var attr = (DiffWindowMenuAttribute)item.GetCustomAttributes(typeof(DiffWindowMenuAttribute), false)[0];
                    availableWindowMenus.Add(attr.Priority, item);
                } catch (Exception)
                {
#if DEBUG
                    throw;
#endif
                }
        }

        public IEnumerable<IDiffWindowMenu> CreateDiffWindowMenus(object diffWindow)
        {
            foreach (KeyValuePair<int, Type> availableWindowMenu in availableWindowMenus)
            {
                bool canBeApplied = false;

                try
                {
                    canBeApplied = (bool)availableWindowMenu.Value.GetMethod("CanBeApplied").Invoke(null, new[] { diffWindow });
                } catch (Exception)
                {
#if DEBUG
                    throw;
#endif
                }

                if (!canBeApplied)
                    continue;

                ConstructorInfo constructorInfo = availableWindowMenu.Value.GetConstructor(new Type[] { typeof(object) });

                if (constructorInfo == null)
                    continue;

                yield return (IDiffWindowMenu)constructorInfo.Invoke(new[] { diffWindow });
            }
        }

        public DW CreateWindowFor(object structure)
        {
            foreach (KeyValuePair<int, Type> valuePair in availableWindows)
            {
                try
                {
                    bool canBeApplied = (bool)valuePair.Value.GetMethod("CanBeApplied").Invoke(null, new[] { structure });

                    if (!canBeApplied)
                        continue;

                    ConstructorInfo constructorInfo = valuePair.Value.GetConstructor(new Type[] { typeof(IFilesystemTreeVisitable), typeof(IDiffWindowManager) });

                    if (constructorInfo == null)
                        throw new InvalidOperationException(string.Format("DiffWindow of type {0} does not have correct constructor.", valuePair.Value));

                    return (DW)constructorInfo.Invoke(new[] { structure, manager });

                } catch (Exception)
                {
#if DEBUG
                    throw;
#endif
                }
            }

            throw new ArgumentException("This instance does not have a diff manager associated.");
        }
    }
}
