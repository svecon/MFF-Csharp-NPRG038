using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using CoreLibrary.Plugins.DiffWindow;

namespace BasicMenus.MergeMenu
{
    /// <summary>
    /// Merge menu handles meging and resolving conflicts.
    /// </summary>
    [DiffWindowMenu(1000)]
    public class MergeMenu : IDiffWindowMenu
    {
        private readonly IMergeMenu window;

        private readonly ICommand merge;

        private readonly ICommand previousConflict;

        private readonly ICommand nextConflict;

        private readonly ICommand useLocal;

        private readonly ICommand useBase;

        private readonly ICommand useRemote;

        /// <summary>
        /// Initializes new instance of the <see cref="MergeMenu"/>
        /// </summary>
        /// <param name="instance">Instance of the <see cref="IDiffWindow{TNode}"/> that handles the menu actions.</param>
        public MergeMenu(object instance)
        {
            window = instance as IMergeMenu;

            if (window == null)
                throw new ArgumentException("Instance must implement menu interface.");

            merge = new RoutedUICommand("Merge", "Merge", window.GetType(),
                new InputGestureCollection() { new KeyGesture(Key.M, ModifierKeys.Control) }
            );

            previousConflict = new RoutedUICommand("PreviousConflict", "PreviousConflict", window.GetType(),
                new InputGestureCollection() { new KeyGesture(Key.D9, ModifierKeys.Control) }
            );

            nextConflict = new RoutedUICommand("NextConflict", "NextConflict", window.GetType(),
                new InputGestureCollection() { new KeyGesture(Key.D0, ModifierKeys.Control) }
            );

            useLocal = new RoutedUICommand("UseLocal", "UseLocal", window.GetType(),
                new InputGestureCollection() { new KeyGesture(Key.I, ModifierKeys.Control) }
            );

            useBase = new RoutedUICommand("UseBase", "UseBase", window.GetType(),
                new InputGestureCollection() { new KeyGesture(Key.O, ModifierKeys.Control) }
            );

            useRemote = new RoutedUICommand("UseRemote", "UseRemote", window.GetType(),
                new InputGestureCollection() { new KeyGesture(Key.P, ModifierKeys.Control) }
            );
        }

        /// <summary>
        /// Checks whether instance implements correct interface.
        /// </summary>
        /// <param name="instance">Instance that should implement interface.</param>
        /// <returns>True when the instance implements the interface.</returns>
        public static bool CanBeApplied(object instance)
        {
            return instance is IMergeMenu;
        }

        public MenuItem CreateMenuItem()
        {
            var menu = new MenuItem { Header = Resources.Menu_Merge };

            var previous = new MenuItem { Header = Resources.Menu_Merge_Previous, Command = previousConflict };
            var next = new MenuItem { Header = Resources.Menu_Merge_Next, Command = nextConflict };

            var useLocalMenu = new MenuItem { Header = Resources.Menu_Merge_UseLocal, Command = useLocal };
            var useBaseMenu = new MenuItem { Header = Resources.Menu_Merge_UseBase, Command = useBase };
            var useRemoteMenu = new MenuItem { Header = Resources.Menu_Merge_UseRemote, Command = useRemote };

            var mergeMenu = new MenuItem { Header = Resources.Menu_Merge_Merge, Command = merge };

            menu.Items.Add(previous);
            menu.Items.Add(next);
            menu.Items.Add(new Separator());
            menu.Items.Add(useLocalMenu);
            menu.Items.Add(useBaseMenu);
            menu.Items.Add(useRemoteMenu);
            menu.Items.Add(new Separator());
            menu.Items.Add(mergeMenu);

            return menu;
        }

        public IEnumerable<CommandBinding> CommandBindings()
        {
            yield return window.PreviousConflictCommandBinding(previousConflict);
            yield return window.NextConflictCommandBinding(nextConflict);
            yield return window.MergeCommandBinding(merge);
            yield return window.UseLocalCommandBinding(useLocal);
            yield return window.UseBaseCommandBinding(useBase);
            yield return window.UseRemoteCommandBinding(useRemote);
        }
    }
}
