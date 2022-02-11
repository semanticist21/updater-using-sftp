using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Updater.services;

namespace Updater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SftpUpdater updater = new SftpUpdater("195.144.107.198", 22, "demo", "password");
            if (updater.IsConnected)
            {
                var main = new MainWindow();
                main.Show();
            }
            else
            {
                Environment.Exit(0);
            }
        }
    }
}
