using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using CoreLibrary.Plugins.DiffWindow;

namespace BasicMenus.ChangesMenu
{
    [DiffWindowMenu(100)]
    public class ChangesMenu : IDiffWindowMenu
    {
        private readonly IChangesMenu window;

        private readonly ICommand previous;

        private readonly ICommand next;

        private readonly ICommand recalculate;

        public ChangesMenu(object instance)
        {
            window = instance as IChangesMenu;

            if (window == null)
                throw new ArgumentException("Instance must implement menu interface.");

            previous = new RoutedUICommand("Previous", "Previous", window.GetType(),
                new InputGestureCollection() { new KeyGesture(Key.F8) }
            );

            next = new RoutedUICommand("Next", "Next", window.GetType(),
                new InputGestureCollection() { new KeyGesture(Key.F9) }
            );

            recalculate = new RoutedUICommand("Recalculate diff", "Recalculate diff", window.GetType(),
                new InputGestureCollection() { new KeyGesture(Key.F5, ModifierKeys.Control) }
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

            var menuRecalcualte = new MenuItem { Header = Resources.Menu_Changes_Recalculate, Command = recalculate };

            menu.Items.Add(menuPrevious);
            menu.Items.Add(menuNext);

            menu.Items.Add(new Separator());

            menu.Items.Add(menuRecalcualte);

            return menu;
        }

        public IEnumerable<CommandBinding> CommandBindings()
        {
            yield return window.PreviousCommandBinding(previous);
            yield return window.NextCommandBinding(next);
            yield return window.RecalculateCommandBinding(recalculate);
        }
    }
}
