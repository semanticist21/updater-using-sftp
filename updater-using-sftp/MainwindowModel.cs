using GalaSoft.MvvmLight.Command;
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
using System.Windows;
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

        private string connectionStatus;
        private string log;

        private bool isProcessOn;

        private List<FileInfoData> serverFileInfos;
        private List<FileInfoData> localFileInfos;
        private ObservableCollection<FileInfoData> finalList;
        private ObservableCollection<RunFileModel> runFileModels;
        private int selectedFileModelIndex;

        private ConnectionManager manager;
        private CustomConnectionInfo info;
        private readonly JoinableTaskFactory jtFactory;
        private readonly JoinableTaskContext mainThreadContext;

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
            set
            {
                progressValue = value;
                RaisePropertyChanged("ProgressValue");
            }
        }
        public int ProgressMaxValue
        {
            get { return progressMaxValue; }
            set
            {
                progressMaxValue = value;
                RaisePropertyChanged("ProgressMaxValue");
            }
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
                if (isProcessOn != value)
                {
                    isProcessOn = value;
                    RaisePropertyChanged("IsProcessOn");
                    CommandManager.InvalidateRequerySuggested();
                }
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

        public ObservableCollection<FileInfoData> FinalList
        {
            get
            {
                return finalList;
            }
            set
            {
                finalList = value;
                RaisePropertyChanged("FinalLists");
            }
        }

        public int SelectedFileModelIndex
        {
            get
            {
                return selectedFileModelIndex;
            }
            set
            {
                if (selectedFileModelIndex != value)
                {
                    selectedFileModelIndex = value;
                    RaisePropertyChanged("SelectedFileModelIndex");
                }
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
            if (!isProcessOn) return true;
            else return false;
        }

        private bool CanExecuteUpdate(object param)
        {
            if (!isProcessOn && manager != null && manager.IsConnected) return true;
            else return false;
        }

        private bool CanExecuteConnect(object param)
        {
            if (manager != null)
            {
                if (!isProcessOn && !manager.IsConnected) return true;
                else return false;
            }
            else if (!isProcessOn) return true;
            else return false;
        }

        private void AutoCommandExecute(object param)
        {
            IsProcessOn = true;
        }
        private async void ConnectCommandExecute(object param)
        {
            await InitConnectionAsync(info);
        }
        private async void UpdateCommandExecute(object param)
        {
            IsProcessOn = true;

            await UpdateFilesToFileDirectoryAsync();

            IsProcessOn = false;
        }
        private void RunCommandExecute(object param)
        {
            DisposeConnection();
            Logger(ErrorLevel.Info, "Run command has been executed.");

            if (RunFileModels != null && RunFileModels.Count >= 1 && RunFileModels[selectedFileModelIndex] != null)
            {
                try
                {
                    ProcessStartInfo info = new(RunFileModels[selectedFileModelIndex].RunFilesDirectory);
                    Process.Start(info);

                    Environment.Exit(0);
                }
                catch (ArgumentNullException)
                {
                    Logger(ErrorLevel.Error, "Directory is empty. It is canceled.");
                }
                catch (Exception ex)
                {
                    Logger(ErrorLevel.Error, ex.Message);
                }
            }
            else Logger(ErrorLevel.Warning, "Warning with run file info. It is canceled.");

        }
        private void OptionsCommandExecute(object param)
        {
            Logger(ErrorLevel.Info, "Option command has been executed.");
        }
        private async void ExitCommandExecute(object param)
        {
            IsProcessOn = true;
            Logger(ErrorLevel.Info, "Please wait until disposing manager...");
            var task = Task.Run(() => DisposeConnection());
            await task.ConfigureAwait(true);

            if (task.IsCompletedSuccessfully) Logger(ErrorLevel.Info, "Sucessfully disposed. Trying exit the program...");
            await Task.Delay(500);

            Environment.Exit(0);
            IsProcessOn = false;
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
            finalList = new ObservableCollection<FileInfoData>();

            getCustomInfoFromSetting();

            mainThreadContext = new JoinableTaskContext();
            jtFactory = new JoinableTaskFactory(mainThreadContext);

            AutoCommand = new DelegateCommand(AutoCommandExecute, CanExecute, jtFactory);
            ConnectCommand = new DelegateCommand(ConnectCommandExecute, CanExecuteConnect, jtFactory);
            UpdateCommand = new DelegateCommand(UpdateCommandExecute, CanExecuteUpdate, jtFactory);
            RunCommand = new DelegateCommand(RunCommandExecute, CanExecute, jtFactory);
            OptionsCommand = new DelegateCommand(OptionsCommandExecute, CanExecute, jtFactory);
            ExitCommand = new DelegateCommand(ExitCommandExecute, CanExecute, jtFactory);
        }

        #region [ Init Variables Methods ]

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
                if (runFileModels.Count >= 1) SelectedFileModelIndex = 0;

                Logger(ErrorLevel.Info, "Succesful fetched app configuration info.");
                Logger(ErrorLevel.Info, $"Target base directory :: {info.FileDirectory}.");
            }
            else
            {
                Logger(ErrorLevel.Error, "Failed to fetch app configuration info.");
            }
        }
        private string AddRunFileModels(string x)
        {
            runFileModels.Add(new RunFileModel { RunFilesDirectory = x });
            return x;
        }
        private string GetCurrentFileDirectory()
        {
            return string.Join("/", Directory.GetCurrentDirectory().Split('\\').SkipLast(1));
        }

        #endregion

        #region [ Connection Methods ]
        public async Task InitConnectionAsync(CustomConnectionInfo info)
        {
            IsProcessOn = true;
            ConnectionStatus = "Connecting...";

            try
            {
                await Task.Run(() => StartConnection(info));

                if (manager.IsConnected)
                {

                    ConnectionStatus = "Connected";
                    Logger(ErrorLevel.Info, $"connection has been successful!!");
                    Logger(ErrorLevel.Info, $"Address :: {info.Address}");
                    Logger(ErrorLevel.Info, $"Port :: {info.Port}");
                    Logger(ErrorLevel.Info, $"User :: {info.User}");

                    var task = Task.Run(() => { GetFilesInfo(); });
                    await task.ConfigureAwait(true);

                    if (serverFileInfos != null && localFileInfos != null)
                    {
                        Logger(ErrorLevel.Info, $"Succesfully fetched.");
                        Logger(ErrorLevel.Info, $"Server file count :: {serverFileInfos.Count}");
                        Logger(ErrorLevel.Info, $"Local file count :: {localFileInfos.Count}");

                        Logger(ErrorLevel.Info, $"Sorting update files..");
                        await SetFinalListAsync();
                        Logger(ErrorLevel.Info, $"sorted final file count :: {FinalList.Count}");
                    }


                }
                else
                {
                    ConnectionStatus = "Retry Connect";
                    Debug.WriteLine("Connection has failed.");
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = "Retry Connect";
                Logger(ErrorLevel.Error, ex.Message);
            }
            finally
            {
                IsProcessOn = false;
            }
        }
        private void StartConnection(CustomConnectionInfo info)
        {
            manager = new(info);
            manager.InitManager();
        }
        private void GetFilesInfo()
        {
            if (manager != null)
            {
                manager.ClearFilesInfo();
                serverFileInfos = manager.GetSftpFilesInfoFromDirectory(info.SftpFileBaseDirectory).ToList();
                localFileInfos = manager.GetFilesInfoFromDirectory(info.FileDirectory).ToList();
            }
        }
        private void DisposeConnection()
        {
            if (manager != null && manager.IsConnected)
            {
                manager.DiposeManager();
            }

            Logger(ErrorLevel.Info, "Connection was disposed.");
        }
        private static IEnumerable<FileInfoData> RemoveDuplicatesFromProjectFiles(List<FileInfoData> serverFileInfos, List<FileInfoData> localFileInfos)
        {
            return FileManager<FileInfoData>.GetListWithoutDuplicates(serverFileInfos, localFileInfos);
        }
        private IEnumerable<FileInfoData> FilterOlderFiles(List<FileInfoData> filesToUpdate)
        {
            List<FileInfoData> filesToDelete = new List<FileInfoData>();

            for (int i = 0; i < filesToUpdate.Count; i++)
            {
                for (int j = 0; j < localFileInfos.Count; j++)
                {
                    if (filesToUpdate[i].Directory.Equals(localFileInfos[j].Directory) && filesToUpdate[i].LastWrittenTime <= localFileInfos[j].LastWrittenTime)
                    {
                        filesToDelete.Add(filesToUpdate[i]);
                    }
                }
            }

            return filesToUpdate.Except(filesToDelete).ToList();
        }
        private async Task SetFinalListAsync()
        {
            List<FileInfoData> filesToUpdate = RemoveDuplicatesFromProjectFiles(serverFileInfos, localFileInfos).ToList();
            List<FileInfoData> filesToUpdateFiltered = FilterOlderFiles(filesToUpdate).ToList();

            var final = filesToUpdateFiltered.Where(x => !IsInExclusion(x)).GetEnumerator();
            await jtFactory.SwitchToMainThreadAsync();
            while (final.MoveNext())
            {
                FinalList.Add(final.Current);
            }
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

        #endregion

        private async Task UpdateFilesToFileDirectoryAsync()
        {
            Logger(ErrorLevel.Info, "Download sequence has started...");
            Logger(ErrorLevel.Info, $"Files count to update :: {finalList.Count}");

            bool hasUpdatedCompleted = false;

            if (finalList.Count > 0) hasUpdatedCompleted = await ExecuteUpdateAsync(finalList).ConfigureAwait(true);
            else Logger(ErrorLevel.Warning, "There is no file to update!");

            if (hasUpdatedCompleted) Logger(ErrorLevel.Info, "Download was sucessful!!");

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
        private async Task<bool> ExecuteUpdateAsync(IEnumerable<FileInfoData> finalList)
        {
            TaskCompletionSource<bool> taskSource = new();

            await Task.Run(() =>
            {
                if (finalList.Count() == 0) taskSource.SetResult(false);
                else
                {
                    try
                    {
                        SetInitialProgressValue(finalList);

                        var enumerator = finalList.GetEnumerator();
                        List<FileInfoData> downloadedFiles = new();
                        while (enumerator.MoveNext())
                        {
                            FileDownload(enumerator.Current);
                            downloadedFiles.Add(enumerator.Current);
                        }
                        jtFactory.Run(async () =>
                        {
                            await jtFactory.SwitchToMainThreadAsync();
                            downloadedFiles.ForEach(x => FinalList.Remove(x));
                        });

                        taskSource.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        Logger(ErrorLevel.Error, ex.Message);
                        taskSource.SetResult(false);
                        taskSource.TrySetException(ex);
                    }
                }
            });

            return await taskSource.Task;
        }
        private void FileDownload(FileInfoData file)
        {
            MakeEmptyDirectories(file);
            FileDownloadBegin(file);
            ProgressValue++;
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
            Logger(ErrorLevel.Info, $"Download Completed :: {downloadDirectory}");
        }

        private string GetDownloadDirectory(string fileDirectory)
        {
            return string.Concat(info.FileDirectory, fileDirectory);
        }
        private string GetServerDownloadDirectory(string fileDirectory)
        {
            return string.Concat(info.SftpFileBaseDirectory, fileDirectory);
        }
        private void SetInitialProgressValue(IEnumerable<FileInfoData> finalList)
        {
            ProgressMaxValue = finalList.Count();
            ProgressValue = 0;
        }
        private void Logger(Constants.ErrorLevel level, string message)
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
