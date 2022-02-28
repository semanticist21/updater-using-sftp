using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.model
{
    public struct FileInfoData
    {
        public string Name { get; set; }
        public string Directory { get; set; }
        public DateTime LastWrittenTime { get; set; }
    }
}
