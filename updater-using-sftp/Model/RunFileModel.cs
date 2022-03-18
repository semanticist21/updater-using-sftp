using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Updater.services;

namespace Updater.Model
{
    public class RunFileModel
    {
        private string runFilesDirectory;
        public string RunFilesDirectory
        {
            get
            {
                return runFilesDirectory;
            }
            set
            {
                if (runFilesDirectory != value)
                {
                    runFilesDirectory = value;
                    SetFileName(value);
                }
            }
        }
        public string RunFileName { get; set; }
        /// <summary>
        /// Save file diretory without file name.
        /// </summary>
        /// <param name="runFilesDirectoryInput"></param>
        private void SetFileName(string runFilesDirectoryInput)
        {
            RunFileName = FileManager<string>.GetFileNameFromDirectory(runFilesDirectoryInput);
        }
    }
}
