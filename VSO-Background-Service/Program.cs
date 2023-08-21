using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using VSO_Background_Service.Properties;

namespace VSO_Background_Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MyCustomApplicationContext());
        }
    }


    public class MyCustomApplicationContext : ApplicationContext
    {
        public string vsoFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\valorant-stream-overlay";
        private NotifyIcon trayIcon;

        public MyCustomApplicationContext()
        {
            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Resources.AppIcon,
                ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Exit", exit)
            }),
                Visible = true
            };

            Thread checkForStreamingSoftwareThread = new Thread(new ThreadStart(checkForStreamingSoftware));
            checkForStreamingSoftwareThread.Start();

            if (!File.Exists(vsoFolder + "\\options\\obs_pid")) File.WriteAllText(vsoFolder + "\\options\\obs_pid", "000000");
        }

        int getOBSStudioPID()
        {
            var processes = Process.GetProcessesByName("obs64");
            foreach (var p in processes)
            {
                return p.Id;
            }
            return 0;
        }

        void checkForStreamingSoftware()
        {
            while (true)
            {
                Thread.Sleep(2000);

                if (File.ReadAllText(vsoFolder + "\\options\\autostart_streamingsoftware") != "true")
                    continue;

                if (Process.GetProcessesByName("obs64").Length > 0 && Process.GetProcessesByName("vso").Length == 0 && File.ReadAllText(vsoFolder + "\\options\\obs_pid") != getOBSStudioPID().ToString())
                {
                    Process.Start("vso.exe");
                }


                File.WriteAllText(vsoFolder + "\\options\\obs_pid", getOBSStudioPID().ToString());
            }
        }

        void exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;

            Environment.Exit(0);
        }
    }
}
