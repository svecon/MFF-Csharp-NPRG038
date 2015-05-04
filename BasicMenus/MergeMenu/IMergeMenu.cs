using System.Windows.Input;

namespace BasicMenus.MergeMenu
{
    public interface IMergeMenu
    {
        CommandBinding PreviousConflictCommandBinding(ICommand command);

        CommandBinding NextConflictCommandBinding(ICommand command);

        CommandBinding MergeCommandBinding(ICommand command);

        CommandBinding UseLocalCommandBinding(ICommand command);

        CommandBinding UseBaseCommandBinding(ICommand command);

        CommandBinding UseRemoteCommandBinding(ICommand command);
    }
}