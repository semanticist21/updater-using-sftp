using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.services
{
    public static class FileManager<T>
    {
        /// <summary>
        /// Exclude files which exists in "ListToCompare"
        /// </summary>
        /// <param name="listToGet"></param>
        /// <param name="listToCompare"></param>
        /// <returns>List without files in second list</returns>
        public static IEnumerable<T> GetListWithoutDuplicates(List<T> listToGet, List<T> listToCompare)
        {
            // listToCompare => directoryFiles
            // listToGet => sftp files
            List<T> listFiltered = listToGet.Except(listToCompare).ToList();

            return listFiltered;
        }
        /// <summary>
        /// if path has non-exist folder Directories, it creates necessary folders.
        /// last path should includes file name.
        /// </summary>
        /// <param name="path"></param>
        public static void MakeDirectoryIfNotExists(string currentFileDirectoryPath, string path)
        {
            string[] splittedNames = path.Split('/');

            if (splittedNames == null)
            {
                Debug.WriteLine($"failed to parse {path}");
                return;
            }

            if (splittedNames.Length == 0 && splittedNames.Length == 1) return;

            else if (splittedNames.Length >= 2)
            {
                string[] splittedFolderNames = skipFileName(splittedNames);
                string newPath = string.Join("/", splittedFolderNames.Prepend(currentFileDirectoryPath));

                if (Directory.Exists(newPath)) return;
                else
                {
                    Directory.CreateDirectory(newPath);
                }
            }
        }
        /// <summary>
        /// return file name from directory
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static string GetFileNameFromDirectory(string directory)
        {
            string[] directoryParsed = directory.Replace("\\", "/").Replace("\\", "/").Split("/");
            if (directoryParsed.Length >= 2) return directoryParsed.LastOrDefault() ?? string.Empty;
            else return string.Empty;
        }
        private static string[] skipFileName(string[] path)
        {
            return path.SkipLast(1).Where(x => !x.Equals("")).ToArray();
        }
    }
}
