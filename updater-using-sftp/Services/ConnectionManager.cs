using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Updater.model;

namespace Updater.services
{
    /// <summary>
    /// Filemanager which handles downloading, updating files from sftp server and directories.
    /// It only supports donwloading from sftp server to local directory path.
    /// </summary>
    public sealed class ConnectionManager
    {
        #region [ Variables ]

        private static SftpClient sftpClient;

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

        private static List<FileInfoData> serverFiles;
        public List<FileInfoData> ServerFiles
        {
            get { return serverFiles; }
        }

        private static List<FileInfoData> localFiles;
        public List<FileInfoData> LocalFiles
        {
            get { return localFiles; }
        }

        private string localFileDirectory;
        public string LocalFileDirectory { get; }

        private string serverFileDirectory;
        public string ServerFileDirectory { get; }
        #endregion\

        private static ConnectionManager singletonManager;
        private ConnectionManager()
        {
            serverFiles = new List<FileInfoData>();
            localFiles = new List<FileInfoData>();
        }
        public static ConnectionManager Instance()
        {
            if (singletonManager == null) singletonManager = new ConnectionManager();
            return singletonManager;
        }

        public void Init(CustomConnectionInfo info)
        {
            if (singletonManager != null)
            {
                if (sftpClient == null)
                {
                    sftpClient = new SftpClient(info.Address, info.Port, info.User, info.Password);
                }

                localFileDirectory = info.LocalFileDirectory;
                serverFileDirectory = info.SftpFileBaseDirectory;

                sftpClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(15); //SshOperationTimeoutException
                sftpClient.OperationTimeout = TimeSpan.FromSeconds(15); //SshOperationTimeoutException
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Init connection based on given connection info
        /// check connection by manager.IsConnected
        /// </summary>
        public bool InitConnection()
        {
            if (sftpClient != null)
            {
                sftpClient.Connect();
                IsConnected = sftpClient.IsConnected;
                return true;
            }
            else
            {
                Debug.WriteLine("Please execute 'Init' method first");
                return false;
            }
        }
        /// <summary>
        /// Dispose manager if in use
        /// </summary>
        public void DiposeManager()
        {
            if (sftpClient != null)
            {
                IsConnected = sftpClient.IsConnected;
                sftpClient.Disconnect();
                sftpClient.Dispose();
            }
        }
        /// <summary>
        /// Disconnect if it is connected.
        /// </summary>
        public void DisconnectManager()
        {
            if(sftpClient != null && sftpClient.IsConnected)
            {
                sftpClient.Disconnect();
            }
        }
        /// <summary>
        /// Returns true if there is a file named exists in the directory
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <param name="isSFtpServer"></param>
        /// <returns></returns>
        public bool CheckFileExists(string directory, string fileName, bool isSFtpServer)
        {
            bool hasFile = false;
            if (isSFtpServer)
            {
                foreach (SftpFile file in sftpClient.ListDirectory(directory))
                {
                    if (file.IsDirectory) continue;
                    else if (file.Name.Equals(fileName))
                    {
                        hasFile = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (string file in Directory.GetFiles(directory))
                {
                    string? fileNameParsed = file.Split('\\').LastOrDefault();
                    if (fileNameParsed != null && fileNameParsed.Equals(fileName)) hasFile = true;
                    else continue;
                }
            }

            return hasFile;
        }
        /// <summary>
        /// returns Ienumerable file info from the server. Please execute ClearFilesInfo method before using it.
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <returns></returns>
        public IEnumerable<FileInfoData> GetSftpFilesInfoFromTargetDirectory(string baseDirectory, string[]? targetFolderNames = null, bool isInitial = false)
        {
            if (sftpClient != null && isConnected)
            {
                if (isInitial && targetFolderNames != null)
                {
                    var files = sftpClient.ListDirectory(baseDirectory);
                    if (files.Where(x => x.IsDirectory).Select(x => x).Count() >= 1)
                    {
                        foreach (SftpFile file in sftpClient.ListDirectory(baseDirectory))
                        {
                            if (file.IsDirectory && targetFolderNames.Contains(file.Name))
                            {
                                GetSftpFilesInfoFromTargetDirectory(file.FullName, null, false);
                            }
                        }
                    }
                    else
                    {
                        //pass;
                    }
                }
                else
                {
                    foreach (SftpFile file in sftpClient.ListDirectory(baseDirectory))
                    {
                        if (file.Name.Equals(".") || file.Name.Equals(".."))
                        {
                            continue;
                        }
                        else if (file.IsDirectory)
                        {
                            GetSftpFilesInfoFromTargetDirectory(file.FullName, null, false);
                        }
                        else
                        {
                            SaveSftpFileInfoTotheList(file);
                        }
                    }
                }
                return serverFiles;
            }
            else
            {
                Debug.WriteLine("the manager is not set or the connection has failed");
                return new List<FileInfoData>();
            }
        }
        /// <summary>
        /// returns Ienumerable file info from the local directory. Please execute ClearFilesInfo method before using it.
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <returns></returns>
        public IEnumerable<FileInfoData> GetFilesInfoFromTargetDirectory(string baseDirectory, string[]? targetFolderNames = null, bool isInitial = false)
        {
            if (sftpClient != null && isConnected)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(baseDirectory);
                IEnumerable<FileInfo> files = directoryInfo.EnumerateFiles();
                if (isInitial && targetFolderNames != null)
                {
                    if (!directoryInfo.GetDirectories().Any())
                    {
                        //pass
                    }
                    else if (directoryInfo.GetDirectories().Any())
                    {
                        foreach (DirectoryInfo directoryFolder in directoryInfo.GetDirectories())
                        {
                            if (targetFolderNames.Contains(directoryFolder.Name)) GetFilesInfoFromTargetDirectory(directoryFolder.FullName);
                        }
                    }
                }
                else
                {
                    // get files on the directory 
                    if (!directoryInfo.GetDirectories().Any())
                    {
                        foreach (FileInfo file in files)
                        {
                            SaveFileInfoToTheList(file);
                        }
                    }
                    else if (directoryInfo.GetDirectories().Any())
                    {
                        foreach (FileInfo file in files)
                        {
                            SaveFileInfoToTheList(file);
                        }

                        foreach (DirectoryInfo directoryFolder in directoryInfo.GetDirectories())
                        {
                            GetFilesInfoFromTargetDirectory(directoryFolder.FullName);
                        }
                    }
                }
                return localFiles;
            }
            else
            {
                Debug.WriteLine("the manager is not set or the connection has failed");
                return new List<FileInfoData>();
            }
        }

        public void ClearFilesInfo()
        {
            serverFiles.Clear();
            localFiles.Clear();
        }
        /// <summary>
        /// Returns parent directory. if it contains file name within, returns a directory without a file name.
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <returns></returns>
        public string GetParentDirectory(string baseDirectory)
        {
            string[] ParsedString = baseDirectory.Split('/');
            List<string> LastSkipped = ParsedString.SkipLast(1).Skip(1).ToList();

            string parentDirectory = string.Join('/', LastSkipped);

            return parentDirectory;
        }

        public void DownloadFile(string path, Stream output)
        {
            sftpClient.DownloadFile(path, output);
        }

        /// <summary>
        /// Returns last file/directory name from the given directory.
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <returns></returns>
        public string GetFileNameFromDirectory(string baseDirectory)
        {
            return baseDirectory.Split('/').LastOrDefault() ?? string.Empty;
        }

        #region [ Private Methods ]

        private void SaveSftpFileInfoTotheList(SftpFile file)
        {
            string directoryParsed;
            if (serverFileDirectory == "/") directoryParsed = file.FullName;
            else directoryParsed = file.FullName.Split(serverFileDirectory)[1];

            serverFiles.Add(new FileInfoData
            {
                Directory = directoryParsed,
                Name = file.Name,
                LastWrittenTime = file.LastWriteTime
            });
        }
        private void SaveFileInfoToTheList(FileInfo file)
        {
            //string directoryParsed = file.FullName.Split(fileDirectory)[1].Replace('\\', '/');
            string directoryParsed = file.FullName.Replace('\\', '/').Replace('\\', '/').Split(localFileDirectory)[1];
            localFiles.Add(new FileInfoData
            {
                Directory = directoryParsed,
                Name = file.Name,
                LastWrittenTime = file.LastWriteTime
            });
        }

        #endregion
    }
}
