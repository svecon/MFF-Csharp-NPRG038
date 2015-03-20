using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Sverge
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
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

    }
}
