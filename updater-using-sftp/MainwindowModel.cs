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
        #region [ Variabels ]

        private string serverFileDirectoryPath;
        private string currentFileDirectoryPath;

        private int progressValue;
        private int progressMaxValue;

        StringCollection folderNamesToExlcude;
        StringCollection filesToExclude;

        private List<FileInfoData> updateFileInfos;
        private List<FileInfoData> projectFileInfos;

        private ConnectionManager manager;



        #endregion

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


            // for test
            CustomConnectionInfo test = new CustomConnectionInfo
            {
                Address = "195.144.107.198",
                Port = 22,
                User = "demo",
                Password = "password",
                SftpFileDirectory = "/",
                FileDirectory = GetCurrentFileDirectory()
            };
            InitConnection(test);

            if (manager != null)
            {
                this.UpdateFilesToFileDirectory();
            }

        }

        public void InitConnection(CustomConnectionInfo info)
        {
            manager = new(info);

            manager.InitManager();

            if (manager.IsConnected)
            {
                InitializeVariables(info);

                Debug.WriteLine("Connection has succeded.");
            }
            else
            {
                Debug.WriteLine("Connection has failed.");
            }
        }
        private void InitializeVariables(CustomConnectionInfo info)
        {
            progressValue = 0;
            folderNamesToExlcude = Updater.Properties.Settings.Default.FolderNamesToExclude;
            filesToExclude = Updater.Properties.Settings.Default.FilesToExclude;

            currentFileDirectoryPath = info.FileDirectory;
            serverFileDirectoryPath = info.SftpFileDirectory;

            updateFileInfos = manager.GetSftpFilesInfoFromDirectory(serverFileDirectoryPath);
            projectFileInfos = manager.GetFilesInfoFromDirectory(currentFileDirectoryPath);
        }
        private string GetCurrentFileDirectory()
        {
            return string.Join("/", Directory.GetCurrentDirectory().Split('\\').SkipLast(1));
        }
        private async void UpdateFilesToFileDirectory()
        {
            List<FileInfoData> filesToUpdate = RemoveDuplicatesFromProjectFiles(updateFileInfos, projectFileInfos);
            updateFileInfos.ForEach(file => Debug.WriteLine(file.Directory));
            projectFileInfos.ForEach(file => Debug.WriteLine(file.Directory));
            List<FileInfoData> filesToUpdateFiltered = FilterOlderFiles(filesToUpdate);

            Debug.WriteLine("Download sequence has started...");
            Debug.WriteLine("Files count to update");
            Debug.WriteLine(filesToUpdate.Count);

            bool hasUpdatedCompleted = false;

            if (filesToUpdate.Count > 0) hasUpdatedCompleted = await ExecuteUpdateAsync(filesToUpdateFiltered);
            else Debug.WriteLine("There is no file to update!");

            #region [ old code ]

            //var task = Task.Run(() =>
            // {
            //     try
            //     {
            //         if (filesToUpdate.Count == 0)
            //         {
            //             Debug.WriteLine("There is no file to update!");
            //             return;
            //         }

            //         for (int i = 0; i < filesToUpdate.Count; i++)
            //         //foreach(FileInfoData file in filesToUpdate)
            //         {
            //             string directoryWithoutFileName = manager.GetParentDirectory(filesToUpdate[i].Directory);

            //             if (folderNamesToExlcude != null && folderNamesToExlcude.Contains(directoryWithoutFileName.Split("/").LastOrDefault()))
            //             {
            //                 filesToUpdate.RemoveAt(i);
            //             }

            //             if (filesToExclude != null && filesToExclude.Contains(filesToUpdate[i].Name))
            //             {
            //                 filesToUpdate.RemoveAt(i);
            //             }

            //             string downloadDirectory = string.Concat(currentFileDirectoryPath, filesToUpdate[i].Directory);

            //             FileManager<FileInfoData>.MakeDirectoryIfNotExists(currentFileDirectoryPath, filesToUpdate[i].Directory);

            //             using Stream fileStream = File.OpenWrite(downloadDirectory);
            //             var fileDownloadResult = manager.BeginDownloadFileAsync(downloadDirectory, fileStream);
            //             fileDownloadResult.AsyncWaitHandle.WaitOne();
            //             //ClearFilesInfo();
            //         }
            //     }
            //     catch (UnauthorizedAccessException ex)
            //     {
            //         Task.FromException(ex);
            //     }
            // });

            //await task.ConfigureAwait(false);

            //if (task.IsCompletedSuccessfully) Debug.WriteLine("Suceess");
            //else Debug.WriteLine("failed");

            #endregion
        }

        private static List<FileInfoData> RemoveDuplicatesFromProjectFiles(List<FileInfoData> updateFileInfos, List<FileInfoData> projectFileInfos)
        {
            return FileManager<FileInfoData>.GetListWithoutDuplicates(updateFileInfos, projectFileInfos);
        }
        private List<FileInfoData> FilterOlderFiles(List<FileInfoData> filesToUpdate)
        {
            List<FileInfoData> filesToDelete = new List<FileInfoData>();

            for (int i = 0; i < filesToUpdate.Count; i++)
            {
                for (int j = 0; j < projectFileInfos.Count; j++)
                {
                    if (filesToUpdate[i].Directory.Equals(projectFileInfos[j].Directory) && filesToUpdate[i].LastWrittenTime <= projectFileInfos[j].LastWrittenTime)
                    {
                        filesToDelete.Add(filesToUpdate[i]);
                    }
                }
            }

            return filesToUpdate.Except(filesToDelete).ToList();
        }
        private Task<bool> ExecuteUpdateAsync(List<FileInfoData> filesToUpdateFiltered)
        {
            TaskCompletionSource<bool> taskSource = new();

            List<FileInfoData> finalList = filesToUpdateFiltered.Where(x => !IsInExclusion(x)).ToList();

            if (finalList.Count == 0) taskSource.SetResult(false);
            else
            {
                try
                {
                    SetInitialProgressValue(finalList);
                    progressValue = finalList.Count;

                    finalList.ForEach(x =>
                    {
                        FileDownload(x);
                        Debug.WriteLine(x.Directory);
                    });
                    taskSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    taskSource.SetResult(false);
                    taskSource.TrySetException(ex);
                }
            }

            return taskSource.Task;
        }
        private bool IsInExclusion(FileInfoData x)
        {
            bool result = false;

            if (filesToExclude != null && filesToExclude.Contains(x.Name)) result = true;

            string directoryWithoutFileName = manager.GetParentDirectory(x.Directory);
            string[] folders = directoryWithoutFileName.Split("/");

            if (folderNamesToExlcude != null)
            {
                int numOfFolderIncluded = folders.Where(x => folderNamesToExlcude.Contains(x)).Select(x => x).Count();
                if (numOfFolderIncluded > 0) result = true;
            }

            return result;
        }
        private void FileDownload(FileInfoData file)
        {
            MakeEmptyDirectories(file);
            FileDownloadBegin(file);
            progressValue++;
        }
        private void MakeEmptyDirectories(FileInfoData file)
        {
            FileManager<FileInfoData>.MakeDirectoryIfNotExists(currentFileDirectoryPath, file.Directory);
        }
        private void FileDownloadBegin(FileInfoData file)
        {
            string downloadDirectory = GetDownloadDirectory(file.Directory);
            string serverDownloadDirectory = GetServerDownloadDirectory(file.Directory);

            using Stream stream = File.OpenWrite(downloadDirectory);

            manager.DownloadFile(serverDownloadDirectory, stream);
        }
        private string GetDownloadDirectory(string fileDirectory)
        {
            return string.Concat(currentFileDirectoryPath, fileDirectory);
        }
        private string GetServerDownloadDirectory(string fileDirectory)
        {
            return string.Concat(serverFileDirectoryPath, fileDirectory);
        }
        private void SetInitialProgressValue(List<FileInfoData> finalList)
        {
            progressMaxValue = finalList.Count;
            progressValue = 0;
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
