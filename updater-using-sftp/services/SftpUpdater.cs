using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.services
{
    public class SftpUpdater
    {
        private string address;
        private int port;
        private string password;
        private string username;
        public bool IsConnected { get; set; }

        private SftpManager manager;
        public SftpUpdater(string address, int port, string username,string password)
        {
            this.address = address;
            this.password = password;
            this.username = username;
            this.port = port;

            manager = new SftpManager(this.address, this.port, this.username, this.password);
            manager.InitManager();
            IsConnected = manager.IsConnected;
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
