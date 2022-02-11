using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Updater.services
{
    public class SftpManager
    {
        SftpClient manager;

        private string newProjectDir;
        private string currentProjectDir;

        private int i;
        private int e;

        public string NewProjectDir
        {
            get
            {
                return newProjectDir;
            }
            set
            {
                newProjectDir = value;
            }
        }
        public string CurrentProjectDir
        {
            get
            {
                return currentProjectDir;
            }
        }

        private bool isConnected;
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
            set
            {
                isConnected = value;
            }
        }
        public SftpManager(string address, int port, string user, string password)
        {
            manager = new SftpClient(address, port, user, password);
            i = 0;
            e = 0;

            currentProjectDir = Directory.GetCurrentDirectory();
            newProjectDir = "/pub";
        }

        public async void InitManager()
        {
            // manager setting zone
            manager.KeepAliveInterval = TimeSpan.FromMilliseconds(-1);
            manager.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
            manager.OperationTimeout = TimeSpan.FromSeconds(10);
            try
            {
                Timer b = new Timer();
                b.Interval = 100;
                b.Elapsed += ElapsedTimeHandler2;
                b.Start();
                manager.Connect();
                isConnected = manager.IsConnected;
                b.Stop();
                Console.Write(IsConnected);
                Console.Write(e);
                if (isConnected)
                {
                    Timer a = new Timer();
                    a.Interval = 100;
                    a.Elapsed += ElapsedTimeHandler;
                    a.Start();
                    await FindAndGetAllDirs(newProjectDir);
                    a.Stop();
                    Console.Write(i);
                    
                    manager.Dispose();
                }
                else
                {
                    manager.Dispose();
                    Environment.Exit(0);    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private Task FindAndGetAllDirs(string fileDirFullPath)
        {
            IEnumerable<SftpFile> files = manager.ListDirectory(fileDirFullPath);
            foreach(SftpFile file in files)
            {
                if(file.Name.Equals(".") || file.Name.Equals(".."))
                {
                    continue;
                }
                else if (file.IsDirectory)
                {
                    Console.WriteLine(file.FullName);
                    FindAndGetAllDirs(file.FullName);
                    Console.WriteLine(i);
                }
                else
                {

                    string fileFullPath = file.FullName;
                    string fuleFullPathSplit = file.FullName.Split(NewProjectDir)[1];
                    Console.WriteLine(fuleFullPathSplit);
                }
            }

            return Task.FromResult(0);
        }

        public void ElapsedTimeHandler(object? sender, ElapsedEventArgs e)
        {
            i++;
        }

        public void ElapsedTimeHandler2(object? sender, ElapsedEventArgs e)
        {
            this.e++;
        }
    }
}
