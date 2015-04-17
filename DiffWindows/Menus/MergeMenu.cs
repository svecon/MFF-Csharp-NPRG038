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
            var menu = new MenuItem { Header = Resources.Menu_Merge };

            var previous = new MenuItem { Header = Resources.Menu_Merge_Previous, Command = window.PreviousConflictCommand() };
            var next = new MenuItem { Header = Resources.Menu_Merge_Next, Command = window.NextConflictCommand() };

            var useLocal = new MenuItem { Header = Resources.Menu_Merge_UseLocal, Command = window.UseLocalCommand() };
            var useBase = new MenuItem { Header = Resources.Menu_Merge_UseBase, Command = window.UseBaseCommand() };
            var useRemote = new MenuItem { Header = Resources.Menu_Merge_UseRemote, Command = window.UseRemoteCommand() };

            var merge = new MenuItem { Header = Resources.Menu_Merge_Merge, Command = window.MergeCommand() };

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
