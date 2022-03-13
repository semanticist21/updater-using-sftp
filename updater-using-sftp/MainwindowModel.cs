using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Updater.Constants;
using Updater.model;
using Updater.Model;

namespace Updater.services
{
    //public class MainwindowModel : INotifyPropertyChanged
    public class MainwindowModel : INotifyPropertyChanged
    {
        #region [ Private Variabels ]

        private int progressValue;
        private int progressMaxValue;

        private string[] folderNamesNotToUpdate;
        private string[] filesNotToUpdate;

        private string selectedFilePath;
        private string connectionStatus;
        private string log;

        private bool isProcessOn;

        private List<FileInfoData> updateFileInfos;
        private List<FileInfoData> projectFileInfos;
        private ObservableCollection<RunFileModel> runFileModels;

        private ConnectionManager manager;
        private CustomConnectionInfo info;
        private readonly JoinableTaskFactory jtFactory;
        private readonly JoinableTaskContext context;

        #endregion
        #region [ PropertyChanged Handler ]

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(string name)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        #endregion

        #region [ Public Variabels ]

        public int ProgressValue
        {
            get { return progressValue; }
            set { progressValue = value; }
        }
        public int ProgressMaxValue
        {
            get { return progressMaxValue; }
            set { progressMaxValue = value; }
        }

        public string ConnectionStatus
        {
            get { return connectionStatus; }
            set
            {
                if (connectionStatus != value)
                {
                    connectionStatus = value;
                    RaisePropertyChanged("ConnectionStatus");
                }
            }
        }

        public string Log
        {
            get { return log; }
            set
            {
                if (log != value)
                {
                    log = value;
                    RaisePropertyChanged("Log");
                }
            }
        }

        public bool IsProcessOn
        {
            get { return isProcessOn; }
            set
            {
                isProcessOn = value;
                RaisePropertyChanged("IsProcessOn");
            }
        }

        public ObservableCollection<RunFileModel> RunFileModels
        {
            get
            {
                return runFileModels;
            }
            set
            {
                runFileModels = value;
                RaisePropertyChanged("RunFileModels");
            }
        }

        #endregion

        #region [ ICommands ]

        public ICommand AutoCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand RunCommand { get; }
        public ICommand OptionsCommand { get; }
        public ICommand ExitCommand { get; }

        #endregion

        #region [ ICommands Methods ]
        private bool CanExecute(object param)
        {
            if (isProcessOn == true) return false;
            else return true;
        }

        private void AutoCommandExecute(object param)
        {
            IsProcessOn = true;
        }
        private void ConnectCommandExecute(object param)
        {
        }
        private void UpdateCommandExecute(object param)
        {
        }
        private void RunCommandExecute(object param)
        {
            XLogger(Constants.ErrorLevel.Info, "Run command has been executed.");
            ProcessStartInfo info = new();

        }
        private void OptionsCommandExecute(object param)
        {
            XLogger(Constants.ErrorLevel.Info, "Option command has been executed.");
        }
        private void ExitCommandExecute(object param)
        {
            if (manager != null && manager.IsConnected)
            {
                manager.DiposeManager();
            }
            Environment.Exit(0);
        }

        #endregion

        public MainwindowModel()
        {
            #region [ old code ]

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
            //CustomConnectionInfo test = new CustomConnectionInfo
            //{
            //    Address = ConfigurationManager.AppSettings["IpAddress"],
            //    Port = 22,
            //    User = "demo",
            //    Password = "password",
            //    SftpFileBaseDirectory = "/pub",
            //    FileDirectory = GetCurrentFileDirectory()
            //};

            //getCustomInfoFromSetting();
            //InitConnection(info);

            //if (manager != null)
            //{
            //    this.UpdateFilesToFileDirectory();
            //}

            #endregion

            progressValue = 0;
            progressMaxValue = 1;
            ConnectionStatus = "Connect";
            Log = "Program has been succesfully initated!";
            RunFileModels = new ObservableCollection<RunFileModel>();

            getCustomInfoFromSetting();

            context = new JoinableTaskContext();
            jtFactory = new JoinableTaskFactory(context);

            AutoCommand = new DelegateCommand(AutoCommandExecute, CanExecute, jtFactory);
            ConnectCommand = new DelegateCommand(ConnectCommandExecute, CanExecute, jtFactory);
            UpdateCommand = new DelegateCommand(UpdateCommandExecute, CanExecute, jtFactory);
            RunCommand = new DelegateCommand(RunCommandExecute, CanExecute, jtFactory);
            OptionsCommand = new DelegateCommand(OptionsCommandExecute, CanExecute, jtFactory);
            ExitCommand = new DelegateCommand(ExitCommandExecute, CanExecute, jtFactory);
        }
        private void getCustomInfoFromSetting()
        {

            if (ConfigurationManager.AppSettings != null)
            {
                info.Address = ConfigurationManager.AppSettings["ipAddress"] ?? string.Empty;
                bool parse = int.TryParse(ConfigurationManager.AppSettings["port"], out int result);
                info.Port = parse ? result : 22;
                info.User = ConfigurationManager.AppSettings["user"] ?? string.Empty;
                info.Password = ConfigurationManager.AppSettings["password"] ?? string.Empty;
                info.SftpFileBaseDirectory = ConfigurationManager.AppSettings["sftpBaseDirectory"] ?? String.Empty;
                info.FileDirectory = GetCurrentFileDirectory();

                folderNamesNotToUpdate = ConfigurationManager.AppSettings["folderNamesNotToUpdate"].Split(";");
                filesNotToUpdate = ConfigurationManager.AppSettings["filesNotToUpdate"].Split(";");

                List<string> FileLists = ConfigurationManager.AppSettings["executeFileDirectory"].Split(';').ToList();
                FileLists.ForEach(x => AddRunFileModels(x));

                runFileModels = new ObservableCollection<RunFileModel>(runFileModels.Where(x => !string.IsNullOrWhiteSpace(x.RunFileName)).Cast<RunFileModel>());
            }
            else
            {
                Debug.WriteLine("MainWindowModel - failed to initiate variabels");
            }
        }
        private string AddRunFileModels(string x)
        {
            runFileModels.Add(new RunFileModel { RunFilesDirectory = x});
            return x;
        }

        public void InitConnection(CustomConnectionInfo info)
        {
            manager = new(info);
            manager.InitManager();

            if (manager.IsConnected)
            {
                GetFilesInfo();
                Debug.WriteLine("Connection has succeded.");
            }
            else
            {
                Debug.WriteLine("Connection has failed.");
            }
        }
        private void GetFilesInfo()
        {
            if (manager != null)
            {
                updateFileInfos = manager.GetSftpFilesInfoFromDirectory(info.SftpFileBaseDirectory);
                projectFileInfos = manager.GetFilesInfoFromDirectory(info.FileDirectory);
            }
        }
        private string GetCurrentFileDirectory()
        {
            return string.Join("/", Directory.GetCurrentDirectory().Split('\\').SkipLast(1));
        }
        private async Task UpdateFilesToFileDirectoryAsync()
        {
            List<FileInfoData> filesToUpdate = RemoveDuplicatesFromProjectFiles(updateFileInfos, projectFileInfos);
            List<FileInfoData> filesToUpdateFiltered = FilterOlderFiles(filesToUpdate);
            List<FileInfoData> finalList = filesToUpdateFiltered.Where(x => !IsInExclusion(x)).ToList();

            Debug.WriteLine("Download sequence has started...");
            Debug.WriteLine("Files count to update");
            Debug.WriteLine(finalList.Count);

            bool hasUpdatedCompleted = false;

            if (finalList.Count > 0) hasUpdatedCompleted = await ExecuteUpdateAsync(finalList);
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


        private Task<bool> ExecuteUpdateAsync(List<FileInfoData> finalList)
        {
            TaskCompletionSource<bool> taskSource = new();

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

            if (filesNotToUpdate != null && filesNotToUpdate.Contains(x.Name)) result = true;

            string directoryWithoutFileName = manager.GetParentDirectory(x.Directory);
            string[] folders = directoryWithoutFileName.Split("/");
            if (folderNamesNotToUpdate != null)
            {
                int numOfFolderIncluded = folders.Where(x => folderNamesNotToUpdate.Contains(x)).Select(x => x).Count();
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
            FileManager<FileInfoData>.MakeDirectoryIfNotExists(info.FileDirectory, file.Directory);
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
            return string.Concat(info.FileDirectory, fileDirectory);
        }
        private string GetServerDownloadDirectory(string fileDirectory)
        {
            return string.Concat(info.SftpFileBaseDirectory, fileDirectory);
        }
        private void SetInitialProgressValue(List<FileInfoData> finalList)
        {
            progressMaxValue = finalList.Count;
            progressValue = 0;
        }
        private void XLogger(Constants.ErrorLevel level, string message)
        {
            switch (level)
            {
                case Constants.ErrorLevel.Debug:
                    Log = $"{log}\nDebug - {message}";
                    break;
                case Constants.ErrorLevel.Info:
                    Log = $"{log}\nInfo - {message}";
                    break;
                case Constants.ErrorLevel.Warning:
                    Log = $"{log}\nWarning - {message}";
                    break;
                case Constants.ErrorLevel.Error:
                    Log = $"{log}\nError - {message}";
                    break;
            }
        }
    }
}
