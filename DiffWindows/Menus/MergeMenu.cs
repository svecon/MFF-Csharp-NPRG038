using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using CoreLibrary.Interfaces;
using CoreLibrary.Plugins.DiffWindow;

namespace DiffWindows.Menus
{
    public interface IMergeMenu
    {
        RoutedUICommand PreviousConflictCommand();
        CommandBinding PreviousConflictCommandBinding();

        RoutedUICommand NextConflictCommand();
        CommandBinding NextConflictCommandBinding();

        RoutedUICommand MergeCommand();
        CommandBinding MergeCommandBinding();

        RoutedUICommand UseLocalCommand();
        CommandBinding UseLocalCommandBinding();

        RoutedUICommand UseBaseCommand();
        CommandBinding UseBaseCommandBinding();

        RoutedUICommand UseRemoteCommand();
        CommandBinding UseRemoteCommandBinding();
    }

    [DiffWindowMenu(1000)]
    public class MergeMenu : IDiffWindowMenu
    {
        private readonly IMergeMenu window;

        public MergeMenu(object instance)
        {
            window = instance as IMergeMenu;
        }

        public static bool CanBeApplied(object instance)
        {
            return instance is IMergeMenu;
        }

        public MenuItem CreateMenuItem()
        {
            var menu = new MenuItem { Header = "Merge" };

            var previous = new MenuItem {Header = "Previous conflict", Command = window.PreviousConflictCommand()};
            var next = new MenuItem {Header = "Next conflict", Command = window.NextConflictCommand()};

            var useLocal = new MenuItem {Header = "Use local version", Command = window.UseLocalCommand()};
            var useBase = new MenuItem {Header = "Use base version", Command = window.UseBaseCommand()};
            var useRemote = new MenuItem {Header = "Use remote version", Command = window.UseRemoteCommand()};

            var merge = new MenuItem {Header = "Merge to base", Command = window.MergeCommand()};

            menu.Items.Add(previous);
            menu.Items.Add(next);
            menu.Items.Add(new Separator());
            menu.Items.Add(useLocal);
            menu.Items.Add(useBase);
            menu.Items.Add(useRemote);
            menu.Items.Add(new Separator());
            menu.Items.Add(merge);

            return menu;
        }

        public IEnumerable<CommandBinding> CommandBindings()
        {
            yield return window.PreviousConflictCommandBinding();
            yield return window.NextConflictCommandBinding();
            yield return window.MergeCommandBinding();
            yield return window.UseLocalCommandBinding();
            yield return window.UseBaseCommandBinding();
            yield return window.UseRemoteCommandBinding();
        }
    }
}
