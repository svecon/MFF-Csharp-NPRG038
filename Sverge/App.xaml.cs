using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Sverge
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            using (TextWriter tw = File.CreateText("C:/Users/svecon/Downloads/git-arguments.txt"))
            {
                foreach (var s in args)
                {
                    tw.WriteLine(s);
                }

                tw.WriteLine(">>>");
                tw.WriteLine(Environment.CurrentDirectory);
            }
        }

        private void App_OnActivated(object sender, EventArgs e)
        {
            
        }
        
        private void App_OnDeactivated(object sender, EventArgs e)
        {
            
        }

        private void App_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            //((Sverge.MainWindow)MainWindow).Hello();
        }
    }
}
