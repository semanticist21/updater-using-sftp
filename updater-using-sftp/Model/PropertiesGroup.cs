using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Model
{
    public class PropertiesGroup
    {
        public int Key { get; set; }
        public string GroupName { get; set; }
        public ObservableCollection<PropertiesItem> GroupItems { get; set; }
        public PropertiesGroup()
        {
            GroupItems = new ObservableCollection<PropertiesItem>();
        }
    }
}
