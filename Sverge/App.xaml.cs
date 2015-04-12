using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using CoreLibrary.Exceptions;
using CoreLibrary.Interfaces;
using CoreLibrary.Settings;
using DiffIntegration;

//"C:\Program Files\KDiff3\bin\d0.txt" "C:\Program Files\KDiff3\bin\d1.txt" "C:\Program Files\KDiff3\bin\d2.txt"
//"C:\Users\svecon\Downloads\DiffAlgorithm.cs" "C:\Users\svecon\Downloads\DiffAlgorithmB.cs" "C:\Users\svecon\Downloads\DiffAlgorithmR.cs"
//"C:\Users\svecon\Downloads\WpfApplication1" "C:\Users\svecon\Downloads\WpfApplication1 - Copy" "C:\Users\svecon\Downloads\WpfApplication1 - Copy - Copy"
//"C:\Users\svecon\Downloads\WpfApplication1" "C:\Users\svecon\Downloads\WpfApplication1 - Copy" -C#

namespace Sverge
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool ShowHelp = false;

        private IProcessorLoader loader;

        private MainWindow mainWindow;

        const int ERROR_PROCESSOR_COLLISION = 9;
        const int ERROR_SETTINGS_NOT_FOUND = 10;
        const int ERROR_SETTINGS_UNKNOWN = 11;

        /// <summary>
        /// Return assembly version for current program
        /// </summary>
        /// <returns>Current assembly version</returns>
        private static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        private void LoadAssemblies()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "plugins");

            if (!Directory.Exists(path))
                return;

            foreach (string file in Directory.GetFiles(path, "*.dll"))
            {
                Assembly.LoadFile(file);
            }
        }

        private void App_OnStartup(object sender, StartupEventArgs eventArgs)
        {
            #region DEBUG: Print arguments
#if DEBUG
            //string[] args = Environment.GetCommandLineArgs();
            using (TextWriter tw = File.CreateText("C:/Users/svecon/Downloads/git-arguments.txt"))
            {
                foreach (string s in eventArgs.Args)
                {
                    tw.WriteLine(s);
                }

                tw.WriteLine(">>>");
                tw.WriteLine(Environment.CurrentDirectory);
            }
#endif
            #endregion

            #region Load all available processors and their settings
            try
            {
                LoadAssemblies();
                loader = new DiffProcessorLoader();
                // Load available processors and their settings
                loader.LoadAll();
            } catch (ProcessorPriorityColissionException e)
            {
                MessageBox.Show(
                    string.Format(Sverge.Properties.Resources.App_OnStartup_ProcessorCollision, e.Message),
                    Sverge.Properties.Resources.App_OnStartup_ErrorParsingSettings
                );
                Current.Shutdown(ERROR_PROCESSOR_COLLISION);
                return;
            }
            #endregion

            #region Parse arguments as settings

            string[] args = eventArgs.Args;
            try
            {
                // pass the arguments and parse them
                var parser = new SettingsParser(loader.GetSettings());
                args = parser.ParseSettings(args);
            } catch (SettingsNotFoundException e)
            {
                MessageBox.Show(
                    string.Format(Sverge.Properties.Resources.App_OnStartup_InvalidSettingsValue, e.Message),
                    Sverge.Properties.Resources.App_OnStartup_ErrorParsingSettings
                );
                Current.Shutdown(ERROR_SETTINGS_NOT_FOUND);
                return;
            } catch (SettingsUnknownValue e)
            {
                MessageBox.Show(
                    string.Format(Sverge.Properties.Resources.App_OnStartup_UnknownSettingsValue, e.Message),
                    Sverge.Properties.Resources.App_OnStartup_ErrorParsingSettings
                );
                Current.Shutdown(ERROR_SETTINGS_UNKNOWN);
                return;
            }
            #endregion

            mainWindow = new MainWindow(loader);
            mainWindow.Show();

            if (args.Length > 0)
            {
                mainWindow.AddNewTab(args);
            }
            else
            {
                mainWindow.OpenWindowDialog();
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
        }
    }
}
