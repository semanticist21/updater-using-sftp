using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.model
{
    public struct CustomConnectionInfo
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string FileDirectory { get; set; }
        public string SftpFileDirectory { get; set; }
    }
}
