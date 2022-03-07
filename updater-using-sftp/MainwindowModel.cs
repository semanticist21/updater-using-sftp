using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Updater.model;

namespace Updater.services
{
    //public class MainwindowModel : INotifyPropertyChanged
    public class MainwindowModel
    {
        private readonly string updateFileDirectoryPath;
        private readonly string currentFileDirectoryPath;

        private List<FileInfoData> updateFileInfos;
        private List<FileInfoData> projectFileInfos;

        //private SftpManager manager;
        public bool IsConnected;
        public MainwindowModel(CustomConnectionInfo info)
        {
            ConnectionManager manager = new ConnectionManager(info);
            currentFileDirectoryPath = info.FileDirectory;
            if (manager.IsConnected)
            {
                this.IsConnected = manager.IsConnected;
                updateFileDirectoryPath = info.SftpFileDirectory;
                updateFileInfos = manager.GetSftpFilesInfoFromDirectory(updateFileDirectoryPath);
                projectFileInfos = manager.GetFilesInfoFromDirectory(currentFileDirectoryPath);
            }
            else
            {
                Debug.WriteLine("Connection has failed");
            }

            manager.DiposeManager();
        }

        public void initManager()
        {

        }

        public void MakeConnectionsWithSftpManager()
        {
        }

        private void GetCurrentVersionFiles()
        {

        }

        private void GetFilesInfoFromUpdateSource()
        {

        }

    }
}
