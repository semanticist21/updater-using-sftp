using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Updater.model;
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
            //SftpUpdater updater = new SftpUpdater
            //(new CustomConnectionInfo
            //{
            //    Address = "195.144.107.198",
            //    Port = 22,
            //    User = "demo",
            //    Password = "password",
            //    SftpFileDirectory = "/",
            //    FileDirectory = Directory.GetCurrentDirectory()
            //}
            //);
            //if (updater.IsConnected)
            //{
            var main = new MainWindow();
            main.Show();
            //}
            //else
            //{
            //    Environment.Exit(0);
            //}
        }
    }
}
