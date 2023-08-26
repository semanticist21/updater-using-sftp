using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Updater.Model;
using Updater.Pages.General;
using Updater.Pages.Run;

namespace Updater.Popup
{
    internal class PopupWindowModel : INotifyPropertyChanged
    {
        #region [ PropertyChanged Handler ]

        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged(string name)
        {
            PropertyChangedEventArgs propertyChangedEventArgs = new PropertyChangedEventArgs(name);
            PropertyChanged(this, propertyChangedEventArgs);
        }

        #endregion

        #region [ Private Variables ]

        private ObservableCollection<PropertiesGroup> propertiesGroups;

        private string ipAddress;
        private string port;
        private string user;
        private string password;

        private string serverBaseDir;
        private string localDir;
        private string targetFolders;

        private string isAutoUpdateOn;

        private string folderNamesNotToUpdate;
        private string filesNotToUpdate;

        private string executeFileDir;
        private string selectedFileModeIndex;

        #endregion

        #region [ Public Variabels ]

        public ObservableCollection<PropertiesGroup> PropertiesGroups
        {
            get { return propertiesGroups; }
            private set
            {
                propertiesGroups = value;
                RaisePropertyChanged("PropertiesGroups");
            }
        }

        public string IpAddress
        {
            get { return ipAddress; }
            set
            {
                bool result = IPAddress.TryParse(ipAddress, out _);
                if (result) ipAddress = value;
                else ipAddress = String.Empty;
                RaisePropertyChanged("IpAddress");
            }
        }
        public string Port
        {
            get { return port; }
            set
            {
                bool result = int.TryParse(value, out _);
                if (result) port = value;
                else port = "22";
                RaisePropertyChanged("Port");
            }
        }
        public string User
        {
            get { return user; }
            set
            {
                user = value;
                RaisePropertyChanged("User");
            }
        }
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                RaisePropertyChanged("Password");
            }
        }

        public string ServerBaseDir
        {
            get { return serverBaseDir; }
            set
            {
                if (value.Contains("\\")) value = value.Replace("\\", "/");
                serverBaseDir = value;
                RaisePropertyChanged("ServerBaseDir");
            }
        }
        public string LocalDir
        {
            get { return localDir; }
            set
            {
                if (value.Contains("\\")) value = value.Replace("\\", "/");
                localDir = value;
                RaisePropertyChanged("LocalDir");
            }
        }
        public string TargetFolders
        {
            get { return targetFolders; }
            set
            {
                if (value.Contains("\\")) value = value.Replace("\\", "/");
                targetFolders = value;
                RaisePropertyChanged("TargetFolders");
            }
        }

        public string IsAutoUpdateOn
        {
            get { return isAutoUpdateOn; }
            set
            {
                isAutoUpdateOn = value;
                RaisePropertyChanged("IsAutoUpdateOn");
            }
        }
        public string FolderNamesNotToUpdate
        {
            get { return folderNamesNotToUpdate; }
            set
            {
                folderNamesNotToUpdate = value;
                RaisePropertyChanged("FolderNamesNotToUpdate");
            }
        }
        public string FilesNotToUpdate
        {
            get { return filesNotToUpdate; }
            set
            {
                filesNotToUpdate = value;
                RaisePropertyChanged("FilesNotToUpdate");
            }
        }

        public string ExecuteFileDir
        {
            get { return executeFileDir; }
            set
            {
                if (value.Contains("\\")) value = value.Replace("\\", "/");
                executeFileDir = value;
                RaisePropertyChanged("ExecuteFileDir");
            }
        }
        public string SelectedFileModeIndex
        {
            get { return selectedFileModeIndex; }
            set
            {
                selectedFileModeIndex = value;
                RaisePropertyChanged("SelectedFileModeIndex");
            }
        }

        #endregion

        #region [ Icommands ]

        public ICommand ConfirmCommand { get; }
        public ICommand CloseCommand { get; }

        #endregion

        #region [ Icommands Methods ]
        private bool CanExecute(object param)
        {
            return true;
        }
        private async void ConfirmCommandExecuteAsync(object param)
        {
            if (param is PopupWindow popupWindow)
            {
                await Task.Yield();
                popupWindow.DialogResult = true;
                popupWindow.Close();
            }
        }

        private async void CloseCommandExecuteAsync(object param)
        {
            if (param is PopupWindow popupWindow)
            {
                await Task.Yield();
                popupWindow.DialogResult = false;
                InitConfigurations();
                popupWindow.Close();
            }
        }

        #endregion

        private static PopupWindowModel popupWindowModel;
        private PopupWindowModel()
        {
            propertiesGroups = new ObservableCollection<PropertiesGroup>();
            InitHierarchicalItems();
            InitConfigurations();

            JoinableTaskContext mainContext = new();
            JoinableTaskFactory jtFactory = new JoinableTaskFactory(mainContext);

            ConfirmCommand = new DelegateCommand(ConfirmCommandExecuteAsync, CanExecute, jtFactory);
            CloseCommand = new DelegateCommand(CloseCommandExecuteAsync, CanExecute, jtFactory);
        }

        public static PopupWindowModel Instance()
        {
            if (popupWindowModel == null)
            {
                popupWindowModel = new PopupWindowModel();
            }
            return popupWindowModel;
        }

        #region [ Make Option Properties Tree Item ] 
        private void InitHierarchicalItems()
        {
            ObservableCollection<PropertiesItem> groupItems = new();

            groupItems.Add(MakePropertiesItem(1, "Connection"));
            groupItems.Add(MakePropertiesItem(2, "Directories"));
            groupItems.Add(MakePropertiesItem(3, "Customs"));
            PropertiesGroups.Add(MakePropertiesGroup(1, "General Configs", groupItems));
            groupItems.Clear();

            groupItems.Add(MakePropertiesItem(4, "General"));
            PropertiesGroups.Add(MakePropertiesGroup(2, "Excludes", groupItems));
            groupItems.Clear();

            groupItems.Add(MakePropertiesItem(5, "General"));
            PropertiesGroups.Add(MakePropertiesGroup(3, "Run Configs", groupItems));
            groupItems.Clear();
        }

        private static PropertiesItem MakePropertiesItem(int key, string name)
        {
            return new PropertiesItem() { Key = key, Name = name };
        }

        private static PropertiesGroup MakePropertiesGroup(int key, string groupName, ObservableCollection<PropertiesItem> groupItems)
        {
            return new PropertiesGroup() { Key = key, GroupName = groupName, GroupItems = new ObservableCollection<PropertiesItem>(groupItems) };
        }

        #endregion

        #region [ init page option variables ] 

        public void InitConfigurations()
        {
            isAutoUpdateOn = ConfigurationManager.AppSettings["isAutoUpdateEnabled"];
            ipAddress = ConfigurationManager.AppSettings["ipAddress"];
            port = ConfigurationManager.AppSettings["port"];
            user = ConfigurationManager.AppSettings["user"];
            password = ConfigurationManager.AppSettings["password"];

            serverBaseDir = ConfigurationManager.AppSettings["sftpBaseDirectory"];

            targetFolders = ConfigurationManager.AppSettings["targetFolderNames"];
            folderNamesNotToUpdate = ConfigurationManager.AppSettings["folderNamesNotToUpdate"];
            filesNotToUpdate = ConfigurationManager.AppSettings["filesNotToUpdate"];

            executeFileDir = ConfigurationManager.AppSettings["executeFileDirectory"];
            selectedFileModeIndex = ConfigurationManager.AppSettings["selectedFileModelIndex"];
        }

        #endregion

    }
}
