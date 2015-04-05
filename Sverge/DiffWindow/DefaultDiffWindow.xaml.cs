using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sverge.DiffWindow
{
    /// <summary>
    /// Interaction logic for DefaultDiffWindow.xaml
    /// </summary>
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
