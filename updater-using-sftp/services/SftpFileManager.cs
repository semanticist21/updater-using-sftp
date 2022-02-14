using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Updater.model;

namespace Updater.services
{
    public class SftpFileManager
    {
        #region [ Variables ]
        SftpClient manager;

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

        private string fileDirectory;
        private string sftpFileDirectory;
        #endregion\

        public SftpFileManager(CustomConnectionInfo info)
        {
            #region [ variables init ]
            sftpFiles = new List<FileInfoData>();
            projectFiles = new List<FileInfoData>();
            #endregion

            manager = new SftpClient(info.Address, info.Port, info.User, info.Password);

            fileDirectory = info.FileDirectory;
            sftpFileDirectory = info.SftpFileDirectory;

            //manager connection setting
            manager.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5); //오류 시 SshOperationTimeoutException
            manager.OperationTimeout = TimeSpan.FromSeconds(5); //SshOperationTimeoutException

            try
            {
                manager.Connect();
                isConnected = manager.IsConnected;
            }
            catch (SshOperationTimeoutException e)
            {
                Console.WriteLine(e.Message);
                isConnected = false;
            }
        }

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
                Console.WriteLine("the manager is not set or the connection has failed");
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

                    foreach (DirectoryInfo directories in directoryInfo.GetDirectories())
                    {
                        GetFilesInfoFromDirectory(directories.FullName);
                    }
                }
                return projectFiles;
            }
            else
            {
                Console.WriteLine("the manager is not set or the connection has failed");
                return new List<FileInfoData>();
            }
        }
        public static string GetParentDirectory(string baseDirectory)
        {
            string[] ParsedString = baseDirectory.Split('/');
            List<string> LastSkipped = ParsedString.SkipLast(ParsedString.Length).ToList();

            string parentDirectory = string.Join("/", LastSkipped);

            return parentDirectory;
        }
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
            string directoryParsed = file.FullName.Split(fileDirectory)[1].Replace('\\', '/');
            projectFiles.Add(new FileInfoData
            {
                Directory = directoryParsed,
                Name = file.Name,
                LastWrittenTime = file.LastWriteTime
            });
        }

    }
}
