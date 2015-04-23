using System.Windows.Input;

namespace DiffWindows.Menus
{
    public interface IChangesMenu
    {
        CommandBinding PreviousCommandBinding(ICommand command);
        CommandBinding NextCommandBinding(ICommand command);
    }
}