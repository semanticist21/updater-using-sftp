using GalaSoft.MvvmLight.Command;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Updater.Constants;
using Updater.Model;
using Updater.Popup;

namespace Updater.services
{
    public class MainwindowModel : INotifyPropertyChanged
    {
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

        #region [ Private Variabels ]

        private int progressValue;
        private int progressMaxValue;

        private string[] folderNamesNotToUpdate;
        private string[] filesNotToUpdate;
        private string[] targetFolderNames;

        private string connectionStatus;
        private string log;

        private bool isAutoUpdateEnabled;
        private bool isProcessOn;
        private bool isUpdatedCompleted;
        private bool isSettingRefreshedFromLastConnection;

        private List<FileInfoData> serverFileInfos;
        private List<FileInfoData> localFileInfos;
        private ObservableCollection<FileInfoData> finalList;
        private ObservableCollection<RunFileModel> runFileModels;
        private int selectedFileModelIndex;

        private ConnectionManager manager;
        private CustomConnectionInfo info;
        private readonly JoinableTaskFactory jtFactory;
        private readonly JoinableTaskContext mainThreadContext;

        private MainWindow mainWindowInstance;

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
            get { return runFileModels; }
            set
            {
                runFileModels = value;
                RaisePropertyChanged("RunFileModels");
            }
        }

        public ObservableCollection<FileInfoData> FinalList
        {
            get { return finalList; }
            set
            {
                finalList = value;
                RaisePropertyChanged("FinalList");
            }
        }

        public int SelectedFileModelIndex
        {
            get { return selectedFileModelIndex; }
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
        public ICommand OpenLocalFolderCommand { get; }

        #endregion

        #region [ ICommands Methods ]
        private async void AutoCommandExecute(object? param)
        {
            IsProcessOn = true;

            if (manager == null || !manager.IsConnected)
            {
                await InitConnectionAsync(info, true);
            }

            if (manager.IsConnected && !isUpdatedCompleted)
            {
                await UpdateFilesToFileDirectoryAsync();

                if (isUpdatedCompleted)
                {
                    Logger(ErrorLevel.Info, "All update target files will be updated.");
                    RunCommandExecute(null);
                }
                else
                {
                    Logger(ErrorLevel.Error, "Something went wrong during the process.");
                }
            }
            else if (manager.IsConnected && isUpdatedCompleted)
            {
                if (isUpdatedCompleted)
                {
                    await Task.Delay(1000);
                    RunCommandExecute(null);
                }
                else
                {
                    Logger(ErrorLevel.Error, "Something went wrong during the process.");
                }
            }
            else
            {
                Logger(ErrorLevel.Error, "Failed to connect the server");
            }

            IsProcessOn = false;
        }

        private async void ConnectCommandExecute(object? param)
        {
            await InitConnectionAsync(info);
            isSettingRefreshedFromLastConnection = false;
        }

        private async void UpdateCommandExecute(object param)
        {
            IsProcessOn = true;

            await UpdateFilesToFileDirectoryAsync();

            IsProcessOn = false;
        }

        private async void RunCommandExecute(object? param)
        {
            IsProcessOn = true;

            DisposeConnection();
            Logger(ErrorLevel.Info, "Run command is executing.");
            try
            {
                if (
                    RunFileModels != null
                    && RunFileModels.Count >= 1
                    && RunFileModels[selectedFileModelIndex] != null
                )
                {
                    try
                    {
                        ProcessStartInfo info =
                            new(RunFileModels[selectedFileModelIndex].RunFilesDirectory);
                        Process.Start(info);

                        await Task.Delay(1000);

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
                else
                    Logger(ErrorLevel.Warning, "Warning with run file info. It is canceled.");
            }
            catch (ArgumentException e)
            {
                await mainWindowInstance.ShowMessageConfirmAsync(
                    "Error",
                    "Please select the project to start."
                );
            }
            finally
            {
                IsProcessOn = false;
            }
        }

        private async void OptionsCommandExecute(object param)
        {
            IsProcessOn = true;

            PopupWindow popupWindow = new PopupWindow();
            popupWindow.IsTopMost = true;
            popupWindow.Owner = mainWindowInstance;
            bool? result = popupWindow.ShowDialog();

            if (result == true)
            {
                PopupWindowModel popupModel = PopupWindowModel.Instance();

                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                config.AppSettings.Settings["isAutoUpdateEnabled"].Value =
                    popupModel.IsAutoUpdateOn;

                config.AppSettings.Settings["ipAddress"].Value = popupModel.IpAddress;
                config.AppSettings.Settings["port"].Value = popupModel.Port;
                config.AppSettings.Settings["user"].Value = popupModel.User;
                config.AppSettings.Settings["password"].Value = popupModel.Password;

                config.AppSettings.Settings["sftpBaseDirectory"].Value = popupModel.ServerBaseDir;
                config.AppSettings.Settings["targetFolderNames"].Value = popupModel.TargetFolders;

                config.AppSettings.Settings["folderNamesNotToUpdate"].Value =
                    popupModel.FolderNamesNotToUpdate;
                config.AppSettings.Settings["filesNotToUpdate"].Value = popupModel.FilesNotToUpdate;

                config.AppSettings.Settings["executeFileDirectory"].Value =
                    popupModel.ExecuteFileDir;
                config.AppSettings.Settings["selectedFileModelIndex"].Value =
                    popupModel.SelectedFileModeIndex;

                config.AppSettings.CurrentConfiguration.Save(ConfigurationSaveMode.Modified);

                getCustomInfoFromSetting();
                if (manager != null && manager.IsConnected)
                    ConnectionStatus = "Reconnect";
                isSettingRefreshedFromLastConnection = true;
                await mainWindowInstance.ShowMessageConfirmAsync("Info", "Changes were saved.");
            }

            //bool? result = popupWindow.ShowDialog();
            IsProcessOn = false;
        }

        private async void ExitCommandExecute(object param)
        {
            IsProcessOn = true;

            Logger(ErrorLevel.Info, "Please wait until disposing manager...");
            var task = Task.Run(() => DisposeConnection());
            await task.ConfigureAwait(true);

            if (task.IsCompletedSuccessfully)
                Logger(ErrorLevel.Info, "Sucessfully disposed. Trying exit the program...");
            await Task.Delay(200);

            Environment.Exit(0);

            IsProcessOn = false;
        }

        private async void OpenLocalFolderCommandExecute(object param)
        {
            string dir = this.GetCurrentFileDirectory();
            dir = dir.Replace("/", "\\");
            Process.Start("explorer.exe", dir);
        }

        private bool CanExecute(object param)
        {
            if (!isProcessOn)
                return true;
            else
                return false;
        }

        private bool CanExecuteTrue(object param)
        {
            return true;
        }

        private bool CanExecuteUpdate(object param)
        {
            if (!isProcessOn && manager != null && manager.IsConnected)
                return true;
            else
                return false;
        }

        private bool CanExecuteConnect(object param)
        {
            if (manager != null)
            {
                if (!isProcessOn && !manager.IsConnected)
                    return true;
                else if (!isProcessOn && manager != null && isSettingRefreshedFromLastConnection)
                    return true;
                else
                    return false;
            }
            else if (!isProcessOn)
                return true;
            else
                return false;
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
            ConnectCommand = new DelegateCommand(
                ConnectCommandExecute,
                CanExecuteConnect,
                jtFactory
            );
            UpdateCommand = new DelegateCommand(UpdateCommandExecute, CanExecuteUpdate, jtFactory);
            RunCommand = new DelegateCommand(RunCommandExecute, CanExecute, jtFactory);
            OptionsCommand = new DelegateCommand(OptionsCommandExecute, CanExecute, jtFactory);
            ExitCommand = new DelegateCommand(ExitCommandExecute, CanExecute, jtFactory);
            OpenLocalFolderCommand = new DelegateCommand(
                OpenLocalFolderCommandExecute,
                CanExecuteTrue,
                jtFactory
            );

            mainWindowInstance = Application.Current.MainWindow as MainWindow;

            if (isAutoUpdateEnabled)
                AutoCommandExecute(null);
        }

        #region [ Init Variables Methods ]

        private void getCustomInfoFromSetting()
        {
            if (ConfigurationManager.AppSettings != null)
            {
                var config =
                    ConfigurationManager.OpenExeConfiguration(
                        ConfigurationUserLevel.None
                    ).AppSettings;

                bool parseBool = bool.TryParse(
                    config.Settings["isAutoUpdateEnabled"].Value,
                    out bool boolResult
                );
                isAutoUpdateEnabled = parseBool ? boolResult : false;

                info.Address = config.Settings["ipAddress"].Value ?? string.Empty;
                bool IsParsed = int.TryParse(config.Settings["port"].Value, out int resultIntOne);
                info.Port = IsParsed ? resultIntOne : 22;
                info.User = config.Settings["user"].Value ?? string.Empty;
                info.Password = config.Settings["password"].Value ?? string.Empty;
                info.SftpFileBaseDirectory = config.Settings["sftpBaseDirectory"].Value;
                if (info.SftpFileBaseDirectory == string.Empty)
                    info.SftpFileBaseDirectory = "/";
                info.LocalFileDirectory = GetCurrentFileDirectory();

                folderNamesNotToUpdate = config.Settings["folderNamesNotToUpdate"].Value.Split(";");
                filesNotToUpdate = config.Settings["filesNotToUpdate"].Value.Split(";");
                targetFolderNames = config.Settings["targetFolderNames"].Value.Split(";");

                List<string> FileLists = config.Settings["executeFileDirectory"].Value
                    .Split(';')
                    .ToList();
                IsParsed = int.TryParse(
                    config.Settings["selectedFileModelIndex"].Value,
                    out int resultIntTwo
                );

                runFileModels.Clear();
                FileLists.ForEach(x => AddRunFileModels(x));
                SetCheckBoxInitialIndex(resultIntTwo, runFileModels.Count(), IsParsed);

                Logger(ErrorLevel.Info, "Succesfully fetched app configuration info.");
                Logger(ErrorLevel.Info, $"Target base directory :: {info.LocalFileDirectory}");

                if (info.User.Equals(string.Empty))
                    Logger(ErrorLevel.Error, "There is no user info!!");
                if (info.Password.Equals(string.Empty))
                    Logger(ErrorLevel.Error, "There is no password info!!");

                //bool parseBool = bool.TryParse(ConfigurationManager.AppSettings["isAutoUpdateEnabled"], out bool boolResult);
                //isAutoUpdateEnabled = parseBool ? boolResult : false;

                //info.Address = ConfigurationManager.AppSettings["ipAddress"] ?? string.Empty;
                //bool IsParsed = int.TryParse(ConfigurationManager.AppSettings["port"], out int resultIntOne);
                //info.Port = IsParsed ? resultIntOne : 22;
                //info.User = ConfigurationManager.AppSettings["user"] ?? string.Empty;
                //info.Password = ConfigurationManager.AppSettings["password"] ?? string.Empty;
                //info.SftpFileBaseDirectory = ConfigurationManager.AppSettings["sftpBaseDirectory"];
                //if (info.SftpFileBaseDirectory == string.Empty) info.SftpFileBaseDirectory = "/";
                //info.LocalFileDirectory = GetCurrentFileDirectory();

                //folderNamesNotToUpdate = ConfigurationManager.AppSettings["folderNamesNotToUpdate"].Split(";");
                //filesNotToUpdate = ConfigurationManager.AppSettings["filesNotToUpdate"].Split(";");
                //targetFolderNames = ConfigurationManager.AppSettings["targetFolderNames"].Split(";");

                //List<string> FileLists = ConfigurationManager.AppSettings["executeFileDirectory"].Split(';').ToList();
                //IsParsed = int.TryParse(ConfigurationManager.AppSettings["selectedFileModelIndex"], out int resultIntTwo);

                //runFileModels.Clear();
                //FileLists.ForEach(x => AddRunFileModels(x));
                //SetCheckBoxInitialIndex(resultIntTwo, runFileModels.Count(), IsParsed);

                //Logger(ErrorLevel.Info, "Succesfully fetched app configuration info.");
                //Logger(ErrorLevel.Info, $"Target base directory :: {info.LocalFileDirectory}");

                //if (info.User.Equals(string.Empty)) Logger(ErrorLevel.Error, "There is no user info!!");
                //if (info.Password.Equals(string.Empty)) Logger(ErrorLevel.Error, "There is no password info!!");
            }
            else
            {
                Logger(ErrorLevel.Error, "Failed to fetch app configuration info.");
            }
        }

        private void AddRunFileModels(string x)
        {
            if (string.IsNullOrEmpty(x) || string.IsNullOrWhiteSpace(x))
                return;
            runFileModels.Add(new RunFileModel { RunFilesDirectory = x });
        }

        private string GetCurrentFileDirectory()
        {
            return string.Join("/", Directory.GetCurrentDirectory().Split('\\').SkipLast(1));
        }

        private void SetCheckBoxInitialIndex(int resultIntTwo, int fileListsCount, bool IsParsed)
        {
            if (IsParsed)
            {
                if (resultIntTwo <= fileListsCount - 1)
                {
                    SelectedFileModelIndex = resultIntTwo;
                }
                else
                {
                    SelectedFileModelIndex = 0;
                }
            }
            else
            {
                SelectedFileModelIndex = 0;
            }
        }

        #endregion

        #region [ Connection Methods ]
        public async Task InitConnectionAsync(
            CustomConnectionInfo info,
            bool isFromAutoCommand = false
        )
        {
            IsProcessOn = true;
            ConnectionStatus = "Connecting...";

            try
            {
                await StartConnection(info);
                if (manager.IsConnected)
                {
                    ConnectionStatus = "Connected";
                    Logger(ErrorLevel.Info, $"Connection has been successful!!");
                    Logger(ErrorLevel.Info, $"Address :: {info.Address}");
                    Logger(ErrorLevel.Info, $"Port :: {info.Port}");
                    Logger(ErrorLevel.Info, $"User :: {info.User}");

                    var task = Task.Run(
                        () =>
                        {
                            GetFilesInfo();
                        }
                    );
                    await task.ConfigureAwait(true);

                    if (serverFileInfos != null && localFileInfos != null)
                    {
                        Logger(ErrorLevel.Info, $"Succesfully fetched.");
                        Logger(ErrorLevel.Info, $"Server file count :: {serverFileInfos.Count}");
                        Logger(ErrorLevel.Info, $"Local file count :: {localFileInfos.Count}");

                        Logger(ErrorLevel.Info, $"Sorting update files..");
                        await SetFinalListAsync();
                        Logger(ErrorLevel.Info, $"Sorted final file count :: {FinalList.Count}");
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
                if (!isFromAutoCommand)
                    IsProcessOn = false;
            }
        }

        private async Task StartConnection(CustomConnectionInfo info)
        {
            await TaskScheduler.Default;
            manager = ConnectionManager.Instance();
            //if already connected, disconnect the connecdtion

            if (manager.IsConnected)
                manager.DisconnectManager();
            manager.Init(info);
            bool isInstanced = manager.InitConnection();
            if (isInstanced)
                Logger(ErrorLevel.Info, "SFTP client instance was created.");
        }

        private void GetFilesInfo()
        {
            if (manager != null)
            {
                manager.ClearFilesInfo();
                serverFileInfos = manager
                    .GetSftpFilesInfoFromTargetDirectory(
                        info.SftpFileBaseDirectory,
                        targetFolderNames,
                        true
                    )
                    .ToList();
                localFileInfos = manager
                    .GetFilesInfoFromTargetDirectory(
                        info.LocalFileDirectory,
                        targetFolderNames,
                        true
                    )
                    .ToList();
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

        private async Task SetFinalListAsync()
        {
            List<FileInfoData> filesToUpdate = RemoveDuplicatesFromProjectFiles(
                    serverFileInfos,
                    localFileInfos
                )
                .ToList();
            List<FileInfoData> filesToUpdateFiltered = FilterOlderFiles(filesToUpdate).ToList();

            var final = filesToUpdateFiltered
                .Where(x => !IsInExclusion(x))
                .Select(x => x)
                .GetEnumerator();
            await jtFactory.SwitchToMainThreadAsync();
            while (final.MoveNext())
            {
                var item = final.Current;
                item.IsUpdateTarget = true;
                FinalList.Add(item);
            }
        }

        private static IEnumerable<FileInfoData> RemoveDuplicatesFromProjectFiles(
            List<FileInfoData> serverFileInfos,
            List<FileInfoData> localFileInfos
        )
        {
            return FileManager<FileInfoData>.GetListWithoutDuplicates(
                serverFileInfos,
                localFileInfos
            );
        }

        private IEnumerable<FileInfoData> FilterOlderFiles(List<FileInfoData> filesToUpdate)
        {
            List<FileInfoData> filesToUpdateFiltered = new();
            if (localFileInfos.Count >= 1)
            {
                for (int i = 0; i < filesToUpdate.Count; i++)
                {
                    for (int j = 0; j < localFileInfos.Count; j++)
                    {
                        if (filesToUpdate[i].Directory.Equals(localFileInfos[j].Directory))
                        {
                            if (filesToUpdate[i].LastWrittenTime <= localFileInfos[j].LastWrittenTime)
                            {
                                continue;
                            }
                            else
                            {
                                filesToUpdateFiltered.Add(filesToUpdate[i]);
                            }
                        }
                        else
                        {
                            if (!localFileInfos.Select(x=>x.Directory).ToList().Contains(filesToUpdate[i].Directory))
                            {
                                filesToUpdateFiltered.Add(filesToUpdate[i]);
                            }
                        }
                    }
                }
            }
            else
            {
                return filesToUpdate;
            }
            return filesToUpdateFiltered.Distinct();
        }

        private bool IsInExclusion(FileInfoData x)
        {
            bool result = false;

            if (filesNotToUpdate != null && filesNotToUpdate.Contains(x.Name))
                result = true;

            string directoryWithoutFileName = manager.GetParentDirectory(x.Directory);
            string[] folders = directoryWithoutFileName.Split("/");
            if (folderNamesNotToUpdate != null)
            {
                int numOfFolderIncluded = folders
                    .Where(x => folderNamesNotToUpdate.Contains(x))
                    .Select(x => x)
                    .Count();
                if (numOfFolderIncluded > 0)
                    result = true;
            }

            return result;
        }

        #endregion

        private async Task UpdateFilesToFileDirectoryAsync()
        {
            List<FileInfoData> output = new();

            foreach (var item in finalList)
            {
                if (item.IsUpdateTarget)
                    output.Add(item);
            }

            FinalList = new ObservableCollection<FileInfoData>(output);

            Logger(ErrorLevel.Info, "Download sequence has started...");
            Logger(ErrorLevel.Info, $"Files count to update :: {FinalList.Count}");

            if (finalList.Count > 0)
                isUpdatedCompleted = await ExecuteUpdateAsync(FinalList).ConfigureAwait(true);
            else
            {
                string errorMsg = "There is no file to update!";
                await mainWindowInstance.ShowMessageConfirmAsync("Update Canceled", errorMsg);
                Logger(ErrorLevel.Warning, errorMsg);
            }

            if (isUpdatedCompleted)
                Logger(ErrorLevel.Info, "Download was sucessful!!");

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

            await Task.Run(
                () =>
                {
                    if (finalList.Count() == 0)
                        taskSource.SetResult(false);
                    else
                    {
                        try
                        {
                            SetInitialProgressValue(finalList);

                            var listed = finalList.ToList();
                            List<FileInfoData> downloadedFiles = new();

                            var tasks = listed.Select(
                                file =>
                                    Task.Run(
                                        () =>
                                        {
                                            FileDownload(file);
                                            downloadedFiles.Add(file);
                                        }
                                    )
                            );

                            Task.WaitAll(tasks.ToArray());

                            jtFactory.Run(
                                async () =>
                                {
                                    await jtFactory.SwitchToMainThreadAsync();
                                    downloadedFiles.ForEach(x => FinalList.Remove(x));
                                }
                            );

                            taskSource.TrySetResult(true);
                        }
                        catch (Exception ex)
                        {
                            Logger(ErrorLevel.Error, ex.Message);
                            taskSource.SetResult(false);
                            taskSource.TrySetException(ex);
                        }
                    }
                }
            );

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
            FileManager<FileInfoData>.MakeDirectoryIfNotExists(
                info.LocalFileDirectory,
                file.Directory
            );
        }

        private void FileDownloadBegin(FileInfoData file)
        {
            string localDownloadDirectory = GetDownloadDirectory(file.Directory);
            string serverDownloadDirectory = GetServerDownloadDirectory(file.Directory);

            try
            {
                using Stream stream = File.OpenWrite(localDownloadDirectory);
                manager.DownloadFile(serverDownloadDirectory, stream);
                Logger(ErrorLevel.Info, $"Download Sequence Completed :: {localDownloadDirectory}");
            }
            catch (IOException e)
            {
                Logger(ErrorLevel.Error, e.Message);
                Logger(ErrorLevel.Error, localDownloadDirectory);
            }
        }

        private string GetDownloadDirectory(string fileDirectory)
        {
            return string.Concat(info.LocalFileDirectory, fileDirectory);
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

        /// <summary>
        /// It casts text on the front
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        private void Logger(Constants.ErrorLevel level, string message)
        {
            switch (level)
            {
                case Constants.ErrorLevel.Debug:
                    Log = $"{log}\nDebug - {message}";
                    Debug.WriteLine(Log);
                    break;
                case Constants.ErrorLevel.Info:
                    Log = $"{log}\nInfo - {message}";
                    Debug.WriteLine(Log);
                    break;
                case Constants.ErrorLevel.Warning:
                    Log = $"{log}\nWarning - {message}";
                    Debug.WriteLine(Log);
                    break;
                case Constants.ErrorLevel.Error:
                    Log = $"{log}\nError - {message}";
                    Debug.WriteLine(Log);
                    break;
            }
        }
    }
}
