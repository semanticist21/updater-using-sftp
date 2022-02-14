using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Updater.model;

namespace Updater.services
{
    public class SftpManager
    {
        SftpClient manager;

        private string newProjectDir;
        private string currentProjectDir;

        private int i;
        private int el;

        public List<FileInfoData> NewFileInfos;
        public List<FileInfoData> currentFileInfos;

        private string[] parentFolderNamesToSearch;
        private string[] fileNamesToExclude;

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
            // connect when initiated
            // contains get all items
            // check whether XML file exists to refer
            // write XML into file function
            // 
            manager = new SftpClient(address, port, user, password);
            //i = 0;
            //el = 0;
            //NewFileInfos = new List<FileInfoData>();
            //currentFileInfos = new List<FileInfoData>();
            //fileNamesToExclude = new string[1];
            //parentFolderNamesToSearch = new string[3];

            //currentProjectDir = GetCurrentProjectDir(Directory.GetCurrentDirectory());
            //GetCurrnetProjectFileInfo();
            newProjectDir = "/";
        }

        public async void InitManager()
        {
            // manager setting zone
            manager.KeepAliveInterval = TimeSpan.FromMilliseconds(-1);
            manager.ConnectionInfo.Timeout = TimeSpan.FromSeconds(3); //오류 시 SshOperationTimeoutException
            manager.OperationTimeout = TimeSpan.FromSeconds(0.5); //SshOperationTimeoutException
            // connection and return true if connection is succesful
            try
            {
                manager.Connect();
                #region [ old code ]
                //Timer b = new Timer();
                //b.Interval = 100;
                //b.Elapsed += ElapsedTimeHandler2;
                //b.Start();
                //manager.Connect();
                //isConnected = manager.IsConnected;
                //b.Stop();
                //Console.WriteLine("elapsed time to connect :{0} ", el);
                //if (isConnected)
                //{
                //    Timer a = new Timer();
                //    a.Interval = 100;
                //    a.Elapsed += ElapsedTimeHandler;
                //    a.Start();
                //    await FindAndGetAllDirs(newProjectDir);
                //    a.Stop();
                //    Console.WriteLine("elapsed time : {0}", i);

                //    manager.Dispose();
                //}
                //else
                //{
                //    manager.Dispose();
                //    Environment.Exit(0);
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        //private Task FindAndGetAllDirs(string fileDirFullPath)
        //{
        //    IEnumerable<SftpFile> files = manager.ListDirectory(fileDirFullPath);
        //    foreach (SftpFile file in files)
        //    {
        //        if (file.Name.Equals(".") || file.Name.Equals(".."))
        //        {
        //            continue;
        //        }
        //        else if (file.IsDirectory)
        //        {
        //            Console.WriteLine(file.FullName);
        //            FindAndGetAllDirs(file.FullName);
        //            Console.WriteLine("elapsed time: {0}", i);
        //        }
        //        else
        //        {
        //            foreach (string fileNameToExclude in fileNamesToExclude)
        //            {
        //                if (!file.Name.Equals(fileNameToExclude))
        //                {
        //                    string fullPathSplit;
        //                    if (NewProjectDir.Equals("/")) fullPathSplit = file.FullName;
        //                    else fullPathSplit = file.FullName.Split(NewProjectDir)[1];

        //                    NewFileInfos.Add(new FileInfoData
        //                    {
        //                        FullName = fullPathSplit,
        //                        Name = file.Name,
        //                        lastWrittenTime = file.LastWriteTime
        //                    });
        //                }
        //                else continue;
        //            }
        //        }
        //    }

        //    return Task.FromResult(0);
        //}
        //private string GetCurrentProjectDir(string dir)
        //{
        //    return Directory.GetParent(dir).ToString();
        //}
        //private void GetCurrnetProjectFileInfo()
        //{
        //    parentFolderNamesToSearch[0] = "net6.0-windows";
        //    string[] words = { currentProjectDir, parentFolderNamesToSearch[0] };
        //    string targetFolderDir = string.Join("\\", words);
        //    string[] a;

        //}

        public void ElapsedTimeHandler(object? sender, ElapsedEventArgs e)
        {
            i++;
        }

        public void ElapsedTimeHandler2(object? sender, ElapsedEventArgs e)
        {
            this.el++;
        }
    }
}
