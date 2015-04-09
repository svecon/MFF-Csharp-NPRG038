using System.Windows.Controls;
using CoreLibrary.DiffWindow;
using CoreLibrary.Interfaces;

namespace Sverge.DiffWindows
{
    /// <summary>
    /// Interaction logic for DefaultDiffWindow.xaml
    /// </summary>
    [DiffWindow(int.MaxValue)]
    public partial class DefaultDiffWindow : UserControl, IDiffWindow
    {
        public DefaultDiffWindow(object instance)
        {
            InitializeComponent();
        }

        public static bool CanBeApplied(object instance)
        {
            return true;
        }
    }
}
