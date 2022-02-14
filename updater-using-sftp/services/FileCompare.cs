using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.services
{
    public static class FileCompare<T>
    {
        public static List<T> GetListWithoutDuplicates(List<T> listToGet, List<T> listToCompare)
        {
            List<T> listFiltered = listToGet.Except(listToCompare).ToList();

            return listFiltered;
        }
    }
}
