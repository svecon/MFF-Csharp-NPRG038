using System.Collections.Generic;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using CoreLibrary.Interfaces;

namespace DiffWindows.Menus
{
    public interface IChangesMenu
    {
        //RoutedUICommand PreviousCommand { get; }

        RoutedUICommand PreviousCommand();
        CommandBinding PreviousCommandBinding();


        RoutedUICommand NextCommand();
        CommandBinding NextCommandBinding();
    }

    public class ChangesMenu : IDiffWindowMenu
    {
        private readonly IChangesMenu window;

        public ChangesMenu(object instance)
        {
            window = instance as IChangesMenu;
        }

        public static bool CanBeApplied(object instance)
        {
            return instance is IChangesMenu;
        }

        public MenuItem CreateMenuItem()
        {
            var menu = new MenuItem { Header = "Changes" };

            var previous = new MenuItem { Header = "Previous" };
            previous.Command = window.PreviousCommand();

            var next = new MenuItem { Header = "Next" };
            next.Command = window.NextCommand();

            menu.Items.Add(previous);
            menu.Items.Add(next);

            return menu;
        }

        public IEnumerable<CommandBinding> CommandBindings()
        {
            yield return window.PreviousCommandBinding();
            yield return window.NextCommandBinding();
        }
    }
}
