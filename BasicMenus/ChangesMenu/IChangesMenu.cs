using System.Windows.Input;

namespace BasicMenus.ChangesMenu
{
    public interface IChangesMenu
    {
        CommandBinding PreviousCommandBinding(ICommand command);
        CommandBinding NextCommandBinding(ICommand command);
        CommandBinding RecalculateCommandBinding(ICommand next);
    }
}