using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Updater.model;

namespace Updater.services
{
    //public class MainwindowModel : INotifyPropertyChanged
    public class MainwindowModel
    {
        private string updateFileDirectoryPath;
        private string currentFileDirectoryPath;

        private List<FileInfoData> updateFileInfos;
        private List<FileInfoData> projectFileInfos;

        private ConnectionManager manager;

        public MainwindowModel()
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
            // for test
            initConnection(new CustomConnectionInfo
            {
                Address = "195.144.107.198",
                Port = 22,
                User = "demo",
                Password = "password",
                SftpFileDirectory = "/",
                FileDirectory = string.Join("/", Directory.GetCurrentDirectory().Split('\\').SkipLast(1))
            });

            if (manager != null)
            {
                this.UpdateFileToDirectory();
            }
        }

        public void initConnection(CustomConnectionInfo info)
        {
            manager = new(info);
            Debug.WriteLine("#@##");
            Debug.WriteLine(info.FileDirectory);
            Debug.WriteLine("#@##");

            manager.InitManager();

            if (manager.IsConnected)
            {
                currentFileDirectoryPath = info.FileDirectory;
                updateFileDirectoryPath = info.SftpFileDirectory;

                updateFileInfos = manager.GetSftpFilesInfoFromDirectory(updateFileDirectoryPath);
                projectFileInfos = manager.GetFilesInfoFromDirectory(currentFileDirectoryPath);

                Debug.WriteLine("Connection has succeded");
            }
            else
            {
                Debug.WriteLine("Connection has failed");
            }
        }
        private async void UpdateFileToDirectory()
        {
            List<FileInfoData> filesToUpdate = FileManager<FileInfoData>.GetListWithoutDuplicates(updateFileInfos, projectFileInfos);
            List<FileInfoData> filesToDelete = new List<FileInfoData>();
            //get if updated time is newer
            for (int i = 0; i < filesToUpdate.Count; i++)
            {
                for (int j = 0; j < projectFileInfos.Count; j++)
                {
                    if (filesToUpdate[i].Directory.Equals(projectFileInfos[j].Directory) && filesToUpdate[i].LastWrittenTime <= projectFileInfos[j].LastWrittenTime)
                    {
                        filesToDelete.Add(filesToUpdate[i]);
                        Debug.WriteLine(i);
                    }
                }
            }

            filesToUpdate = filesToUpdate.Except(filesToDelete).ToList();

            //Get Settings Info
            //string[] FolderNamesToExclude = new string[Updater.Properties.Settings.Default.FolderNamesToExclude.Count];
            //string[] FilesToExclude = new string[Updater.Properties.Settings.Default.FilesToExclude.Count];

            StringCollection folderNamesToExlcude = Updater.Properties.Settings.Default.FolderNamesToExclude;
            StringCollection filesToExclude = Updater.Properties.Settings.Default.FilesToExclude;

            Debug.WriteLine("Download sequence has started");
            Debug.WriteLine("Files count to update");
            Debug.WriteLine(filesToUpdate.Count);

            var task = Task.Run(() =>
             {
                 try
                 {
                     if (filesToUpdate.Count == 0)
                     {
                         Debug.WriteLine("There is no file to update!");
                         return;
                     }
                     for (int i = 0; i < filesToUpdate.Count; i++)
                     //foreach(FileInfoData file in filesToUpdate)
                     {
                         string directoryWithoutFileName = manager.GetParentDirectory(filesToUpdate[i].Directory);

                         if (folderNamesToExlcude != null && folderNamesToExlcude.Contains(directoryWithoutFileName.Split("/").LastOrDefault()))
                         {
                             filesToUpdate.RemoveAt(i);
                         }

                         if (filesToExclude != null && filesToExclude.Contains(filesToUpdate[i].Name))
                         {
                             filesToUpdate.RemoveAt(i);
                         }

                         string downloadDirectory = string.Concat(currentFileDirectoryPath, filesToUpdate[i].Directory);

                         FileManager<FileInfoData>.MakeDirectoryIfNotExists(currentFileDirectoryPath, filesToUpdate[i].Directory);

                         using Stream fileStream = File.OpenWrite(downloadDirectory);
                         var fileDownloadResult = manager.BeginDownloadFile(downloadDirectory, fileStream);
                         fileDownloadResult.AsyncWaitHandle.WaitOne();

                         //ClearFilesInfo();
                     }
                 }
                 catch (UnauthorizedAccessException ex)
                 {
                     Task.FromException(ex);
                 }
             });

            await task.ConfigureAwait(false);

            if (task.IsCompletedSuccessfully) Debug.WriteLine("Suceess");
            else Debug.WriteLine("failed");
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
