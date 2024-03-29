﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Model
{
    public struct CustomConnectionInfo
    {
        /// <summary>
        /// IP address used for the connection.
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// sftp port. Usually 22.
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// User name used for the connection.
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// password used for the connection.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// It represents base file directory where update files will be put into.
        /// example : C:/dir1/dir2
        /// </summary>
        private string localFileDirectory;

        public string LocalFileDirectory
        {
            get
            {
                return localFileDirectory;
            }
            set
            {
                if (localFileDirectory != value)
                {
                    if (!value.Contains('\\')) localFileDirectory = value;
                    else
                    {
                        localFileDirectory = value.Replace("\\", "/").Replace("\\", "/");
                    }
                }
            }
        }
        /// <summary>
        /// It represents server base directory.
        /// example : /
        /// </summary>
        public string SftpFileBaseDirectory { get; set; }
    }
}
