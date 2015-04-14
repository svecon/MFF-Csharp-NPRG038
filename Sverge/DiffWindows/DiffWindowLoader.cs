using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreLibrary.DiffWindow;
using CoreLibrary.Interfaces;
using DiffIntegration.DiffFilesystemTree;

namespace Sverge.DiffWindows
{
    class DiffWindowLoader
    {
        private readonly SortedList<int, Type> availableWindows;
        private readonly List<Type> availableWindowMenus;
        private MainWindow window;

        public DiffWindowLoader(MainWindow mainWindow)
        {
            window = mainWindow;
            availableWindows = new SortedList<int, Type>();
            availableWindowMenus = new List<Type>();
        }

        public void LoadWindows()
        {
            Type type = typeof(IDiffWindow<object>);
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
                    availableWindowMenus.Add(item);
                } catch (Exception)
                {
#if DEBUG
                    throw;
#endif
                }
        }

        public IEnumerable<IDiffWindowMenu> CreateDiffWindowMenus(object diffWindow)
        {
            foreach (Type availableWindowMenu in availableWindowMenus)
            {
                bool canBeApplied = false;

                try
                {
                    canBeApplied = (bool)availableWindowMenu.GetMethod("CanBeApplied").Invoke(null, new[] { diffWindow });
                } catch (Exception)
                {
#if DEBUG
                    throw;
#endif
                }

                if (!canBeApplied)
                    continue;

                ConstructorInfo constructorInfo = availableWindowMenu.GetConstructor(new Type[] { typeof(object) });

                if (constructorInfo == null)
                    continue;

                yield return (IDiffWindowMenu)constructorInfo.Invoke(new[] { diffWindow });
            }
        }

        public IDiffWindow<object> CreateWindowFor(object structure)
        {
            foreach (KeyValuePair<int, Type> valuePair in availableWindows)
            {
                try
                {
                    bool canBeApplied = (bool)valuePair.Value.GetMethod("CanBeApplied").Invoke(null, new[] { structure });

                    if (!canBeApplied)
                        continue;

                    bool usingWindowParam = false;
                    ConstructorInfo constructorInfo = valuePair.Value.GetConstructor(new Type[] { typeof(object) });

                    if (constructorInfo == null)
                    {
                        constructorInfo = valuePair.Value.GetConstructor(new Type[] { typeof(object), typeof(IWindow) });
                        usingWindowParam = true;
                    }

                    if (constructorInfo == null)
                        throw new InvalidOperationException(string.Format("DiffWindow of type {0} does not have correct constructor.", valuePair.Value));

                    return (IDiffWindow<object>)constructorInfo.Invoke(usingWindowParam ? new[] { structure, window } : new[] { structure });

                } catch (Exception)
                {
#if DEBUG
                    throw;
#endif
                }
            }

            throw new ArgumentException("This instance does not have a diff window associated.");
        }
    }
}
