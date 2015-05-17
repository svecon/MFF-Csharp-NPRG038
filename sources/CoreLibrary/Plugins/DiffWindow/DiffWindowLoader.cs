using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.Plugins.DiffWindow
{
    using DW = IDiffWindow<INodeVisitable>;

    /// <summary>
    /// A loader for the <see cref="IDiffWindow{TNode}"/> plugins and their menus <see cref="IDiffWindowMenu"/>.
    /// </summary>
    public class DiffWindowLoader
    {
        /// <summary>
        /// A list of all avaialble windows sorted by their priority.
        /// </summary>
        private readonly SortedList<int, Type> availableWindows;

        /// <summary>
        /// A list of all avaialble window menus sorted by their priority.
        /// </summary>
        private readonly SortedList<int, Type> availableWindowMenus;

        /// <summary>
        /// An instance of <see cref="IDiffWindowManager"/> that is required for windows.
        /// </summary>
        private readonly IDiffWindowManager manager;

        /// <summary>
        /// Initializes new instance of the <see cref="DiffWindowLoader"/>
        /// </summary>
        /// <param name="diffManager">Reference to the <see cref="IDiffWindowManager"/></param>
        public DiffWindowLoader(IDiffWindowManager diffManager)
        {
            manager = diffManager;
            availableWindows = new SortedList<int, Type>();
            availableWindowMenus = new SortedList<int, Type>();
        }

        /// <summary>
        /// Finds all <see cref="IDiffWindow{TNode}"/> in the current assemblies.
        /// </summary>
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
                }
                catch (Exception)
                {
                    // ignores plugin errors in release version
#if DEBUG
    // rethrow in Debug mode; ignore in Production if faulty 
                    throw;
#endif
                }
        }

        /// <summary>
        /// Finds all <see cref="IDiffWindowMenu"/> in the current assemblies.
        /// </summary>
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
                    // ignores plugin errors in release version
#if DEBUG
                    // rethrow in Debug mode; ignore in Production if faulty 
                    throw;
#endif
                }
        }

        /// <summary>
        /// Enumerates the menus that are available for the given <see cref="IDiffWindow{TNode}"/>
        /// </summary>
        /// <param name="diffWindow"><see cref="IDiffWindow{TNode}"/></param>
        /// <returns>An enumeration of <see cref="IDiffWindowMenu"/> that are implemented by the given <see cref="IDiffWindow{TNode}"/></returns>
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
                    // ignores plugin errors in release version
#if DEBUG
                    // rethrow in Debug mode; ignore in Production if faulty 
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

        /// <summary>
        /// Creates <see cref="IDiffWindow{TNode}"/> for the given structure.
        /// All <see cref="IDiffWindow{TNode}"/> must have CanBeApplied static method.
        /// </summary>
        /// <param name="structure">Structure that holds some diff.</param>
        /// <returns><see cref="IDiffWindow{TNode}"/> that can handle the given structure.</returns>
        public DW CreateWindowFor(object structure)
        {
            foreach (KeyValuePair<int, Type> valuePair in availableWindows)
            {
                try
                {
                    bool canBeApplied = (bool)valuePair.Value.GetMethod("CanBeApplied").Invoke(null, new[] { structure });

                    if (!canBeApplied)
                        continue;

                    ConstructorInfo constructorInfo = valuePair.Value.GetConstructor(new Type[] { typeof(INodeVisitable), typeof(IDiffWindowManager) });

                    if (constructorInfo == null)
                        throw new InvalidOperationException(string.Format("DiffWindow of type {0} does not have correct constructor.", valuePair.Value));

                    return (DW)constructorInfo.Invoke(new[] { structure, manager });

                } catch (Exception)
                {
                    // ignores plugin errors in release version
#if DEBUG
                    // rethrow in Debug mode; ignore in Production if faulty 
                    throw;
#endif
                }
            }

            throw new ArgumentException("This instance does not have a diff manager associated.");
        }
    }
}
