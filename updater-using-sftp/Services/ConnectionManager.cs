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
    public class ConnectionManager
    {
        #region [ Variables ]
        private readonly SftpClient manager;

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

        private List<FileInfoData> sftpFiles;
        public List<FileInfoData> SftpFiles
        {
            get { return sftpFiles; }
        }

        private List<FileInfoData> projectFiles;
        public List<FileInfoData> ProjectFiles
        {
            get { return projectFiles; }
        }

        private readonly string fileDirectory;
        public string FileDirectory { get; }

        private readonly string sftpFileDirectory;
        public string SftpFileDirectory { get; }
        #endregion\

        public ConnectionManager(CustomConnectionInfo info)
        {
            #region [ variables init ]

            sftpFiles = new List<FileInfoData>();
            projectFiles = new List<FileInfoData>();

            #endregion

            manager = new SftpClient(info.Address, info.Port, info.User, info.Password);

            fileDirectory = info.FileDirectory;
            sftpFileDirectory = info.SftpFileDirectory;

            manager.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5); //SshOperationTimeoutException
            manager.OperationTimeout = TimeSpan.FromSeconds(5); //SshOperationTimeoutException
        }

        /// <summary>
        /// Init connection based on given connection info
        /// check connection by manager.IsConnected
        /// </summary>
        public void InitManager()
        {
            try
            {
                manager.Connect();
                isConnected = manager.IsConnected;
            }
            catch (SshOperationTimeoutException e)
            {
                Debug.WriteLine(e.Message);
                isConnected = false;
            }
        }
        /// <summary>
        /// Dispose manager if in use
        /// </summary>
        public void DiposeManager()
        {
            if (manager != null)
            {
                manager.Disconnect();
                manager.Dispose();
            }
        }
        /// <summary>
        /// Returns true if there is a file named exists in the directory
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <param name="isFtpServer"></param>
        /// <returns></returns>
        public bool CheckFileExists(string directory, string fileName, bool isFtpServer)
        {
            bool hasFile = false;
            if (isFtpServer)
            {
                foreach (SftpFile file in manager.ListDirectory(directory))
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
        public List<FileInfoData> GetSftpFilesInfoFromDirectory(string baseDirectory)
        {
            if (manager != null && isConnected)
            {
                foreach (SftpFile file in manager.ListDirectory(baseDirectory))
                {
                    if (file.Name.Equals(".") || file.Name.Equals(".."))
                    {
                        continue;
                    }
                    else if (file.IsDirectory)
                    {
                        GetSftpFilesInfoFromDirectory(file.FullName);
                    }
                    else
                    {
                        SaveSftpFileInfoTotheList(file);
                    }
                }
                return sftpFiles;
            }
            else
            {
                Debug.WriteLine("the manager is not set or the connection has failed");
                return new List<FileInfoData>();
            }
        }
        public List<FileInfoData> GetFilesInfoFromDirectory(string baseDirectory)
        {
            if (manager != null && isConnected)
            {
                // get files on the directory 
                DirectoryInfo directoryInfo = new DirectoryInfo(baseDirectory);
                IEnumerable<FileInfo> files = directoryInfo.EnumerateFiles();
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
                        if(!directoryFolder.Name.Equals("net6.0-windows"))GetFilesInfoFromDirectory(directoryFolder.FullName);
                    }
                }
                return projectFiles;
            }
            else
            {
                Debug.WriteLine("the manager is not set or the connection has failed");
                return new List<FileInfoData>();
            }
        }
        public void ClearFilesInfo()
        {
            sftpFiles.Clear();
            projectFiles.Clear();
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

        public IAsyncResult BeginDownloadFileAsync(string path, Stream output)
        {
            return manager.BeginDownloadFile(path, output);
        }

        public void DownloadFile(string path, Stream output)
        {
            manager.DownloadFile(path, output);
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
            if (sftpFileDirectory == "/") directoryParsed = file.FullName;
            else directoryParsed = file.FullName.Split(sftpFileDirectory)[1];

            sftpFiles.Add(new FileInfoData
            {
                Directory = directoryParsed,
                Name = file.Name,
                LastWrittenTime = file.LastWriteTime
            });
        }
        private void SaveFileInfoToTheList(FileInfo file)
        {
            //string directoryParsed = file.FullName.Split(fileDirectory)[1].Replace('\\', '/');
            string directoryParsed = file.FullName.Replace('\\', '/').Replace('\\', '/').Split(fileDirectory)[1];
            projectFiles.Add(new FileInfoData
            {
                Directory = directoryParsed,
                Name = file.Name,
                LastWrittenTime = file.LastWriteTime
            });
        }

        #endregion
    }
}
