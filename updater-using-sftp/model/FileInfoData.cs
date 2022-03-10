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
        private string directory;
        /// <summary>
        /// folder/folder/.../fileName, Except base directory
        /// </summary>
        public string Directory
        {
            get { return directory; }
            set
            {
                if (directory != value)
                {
                    if (!value.First().Equals("\\")) directory = value;
                    else directory = string.Concat(value.Skip(1));
                }

            }
        }
        public DateTime LastWrittenTime { get; set; }
    }
}
