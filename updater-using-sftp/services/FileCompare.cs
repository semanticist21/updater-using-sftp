using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.services
{
    public static class FileCompare<T>
    {
        /// <summary>
        /// Exclude files which exists in "ListToCompare"
        /// </summary>
        /// <param name="listToGet"></param>
        /// <param name="listToCompare"></param>
        /// <returns></returns>
        public static List<T> GetListWithoutDuplicates(List<T> listToGet, List<T> listToCompare)
        {
            List<T> listFiltered = listToGet.Except(listToCompare).ToList();

            return listFiltered;
        }
    }
}
