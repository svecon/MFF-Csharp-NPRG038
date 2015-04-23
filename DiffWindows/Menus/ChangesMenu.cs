using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using CoreLibrary.Interfaces;
using CoreLibrary.Plugins.DiffWindow;

namespace DiffWindows.Menus
{
    [DiffWindowMenu(100)]
    public class ChangesMenu : IDiffWindowMenu
    {
        private readonly IChangesMenu window;

        private readonly ICommand previous;

        private readonly ICommand next;

        public ChangesMenu(object instance)
        {
            window = instance as IChangesMenu;

            if (window == null)
                throw new ArgumentException("Instance must implement menu interface.");

            previous = new RoutedUICommand("Previous", "Previous", window.GetType(),
                new InputGestureCollection() { new KeyGesture(Key.F7) }
            );

            next = new RoutedUICommand("Next", "Next", window.GetType(),
                new InputGestureCollection() { new KeyGesture(Key.F8) }
            );
        }

        public static bool CanBeApplied(object instance)
        {
            return instance is IChangesMenu;
        }

        public MenuItem CreateMenuItem()
        {
            var menu = new MenuItem { Header = Resources.Menu_Changes };

            var menuPrevious = new MenuItem { Header = Resources.Menu_Changes_Previous, Command = previous };

            var menuNext = new MenuItem { Header = Resources.Menu_Changes_Next, Command = next };

            menu.Items.Add(menuPrevious);
            menu.Items.Add(menuNext);

            return menu;
        }

        public IEnumerable<CommandBinding> CommandBindings()
        {
            yield return window.PreviousCommandBinding(previous);
            yield return window.NextCommandBinding(next);
        }
    }
}
