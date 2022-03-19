using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Updater.model;

namespace Updater.services
{
    public class SftpUpdater
    {
        private readonly string updateFileDirectoryPath;
        private readonly string currentFileDirectoryPath;

        private List<FileInfoData> updateFileInfos;
        private List<FileInfoData> projectFileInfos;

        private SftpManager manager;
        public bool IsConnected;

        private string updateFileName;
        public SftpUpdater(CustomConnectionInfo info)
        {
            manager = new SftpManager(info);
            currentFileDirectoryPath = info.FileDirectory;
            if (manager.IsConnected)
            {
                this.IsConnected = manager.IsConnected;
                updateFileName = "updateInfo.json";

                updateFileDirectoryPath = info.SftpFileDirectory;
                updateFileInfos = manager.GetSftpFilesInfoFromDirectory(updateFileDirectoryPath);
                projectFileInfos = manager.GetFilesInfoFromDirectory(currentFileDirectoryPath);
                if (!manager.CheckFileExists(currentFileDirectoryPath, updateFileName, false))
                {
                    JsonWriter<FileInfoData>.WriteListToJson(projectFileInfos, currentFileDirectoryPath, updateFileName);
                    Task<List<FileInfoData>> task = JsonWriter<FileInfoData>.ReadFromJson(currentFileDirectoryPath, updateFileName);
                    List<FileInfoData> list = task.Result;  
                }
                else
                {

                }
            }
            else
            {
                Console.WriteLine("Connection has failed");
            }

            manager.DiposeManager();
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
