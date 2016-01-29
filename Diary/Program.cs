using Diary.Controller;
using log4net;
using Microsoft.Win32;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace Diary
{
    static public class Program
    {
        public static ILog Log;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ConfigureLogging();
            ConfigureStartupOnLogon(ConfigurationManager.AppSettings["StartOnLogon"]);
            var autoSaveInMin = Convert.ToInt32(ConfigurationManager.AppSettings["AutoSave"]);
            var dc = new DiaryController(Log, ConfigurationManager.AppSettings["EntryPath"], ConfigurationManager.AppSettings["Sort"]);
            var hideUserColumn = bool.Parse(ConfigurationManager.AppSettings["HideUserNameColumn"]);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(Log, dc, autoSaveInMin, hideUserColumn));
        }

        

        private static void ConfigureStartupOnLogon(string appSetting)
        {
            Log.Info("Configure Startup.");
            bool startOnBoot;

            if (!bool.TryParse(appSetting, out startOnBoot))
            {
                //MessageBox.Show("StartOnLogon application setting is not defined.");
                Log.Error("StartOnLogon application setting is not defined.");
                //Application.Exit();
            }

            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rkApp == null)
            {
                Log.Error("Can't open Registry. HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                return;
            }

            if (startOnBoot)
            {
                // The path to the key where Windows looks for startup applications
                rkApp.SetValue(Assembly.GetExecutingAssembly().GetName().Name, Application.ExecutablePath);
                Log.Info(string.Format("Adding Registry Key - {0} = {1}", Assembly.GetExecutingAssembly().GetName().Name, Application.ExecutablePath));
            }
            else
            {
                rkApp.DeleteValue(Assembly.GetExecutingAssembly().GetName().Name, false);
                Log.Info(string.Format("Removing Registry Key - {0}", Assembly.GetExecutingAssembly().GetName().Name));
            }
        }

        private static void ConfigureLogging()
        {
            // Set logfile name and application name variables
            GlobalContext.Properties["LogName"] = string.Format("{0}{1}", ConfigurationManager.AppSettings["LogPath"], Assembly.GetExecutingAssembly().GetName().Name + ".log");
            GlobalContext.Properties["Application"] = Assembly.GetExecutingAssembly().GetName().Name;
            GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;

            Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType); //Instantiate the logger
            Log.Info("Starting the application..");


        }

    }
}
