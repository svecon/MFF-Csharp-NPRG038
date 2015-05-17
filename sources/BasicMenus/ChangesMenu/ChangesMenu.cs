using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using CoreLibrary.Plugins.DiffWindow;

namespace BasicMenus.ChangesMenu
{
    /// <summary>
    /// A menu for iterating thourgh differences
    /// </summary>
    [DiffWindowMenu(100)]
    public class ChangesMenu : IDiffWindowMenu
    {
        /// <summary>
        /// Window that implements this interface.
        /// </summary>
        private readonly IChangesMenu window;

        /// <summary>
        /// Command for previous action.
        /// </summary>
        private readonly ICommand previous;

        /// <summary>
        /// Command for next action.
        /// </summary>
        private readonly ICommand next;

        /// <summary>
        /// Command for recalculate action.
        /// </summary>
        private readonly ICommand recalculate;

        /// <summary>
        /// Initializes new instance of the <see cref="ChangesMenu"/>
        /// </summary>
        /// <param name="instance">Instance of the <see cref="IDiffWindow{TNode}"/> that handles the menu actions.</param>
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

        /// <summary>
        /// Checks whether instance implements correct interface.
        /// </summary>
        /// <param name="instance">Instance that should implement interface.</param>
        /// <returns>True when the instance implements the interface.</returns>
        public static bool CanBeApplied(object instance)
        {
            return instance is IChangesMenu;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public IEnumerable<CommandBinding> CommandBindings()
        {
            yield return window.PreviousCommandBinding(previous);
            yield return window.NextCommandBinding(next);
            yield return window.RecalculateCommandBinding(recalculate);
        }
    }
}
