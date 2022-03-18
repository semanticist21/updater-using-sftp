using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Model
{
    public class PropertiesGroup
    {
        public int Key { get; set; }
        public string GroupName { get; set; }
        public IList<PropertiesItem> GroupItems { get; set; }
        public IEnumerable<Object> Items
        {
            get
            {
                return GroupItems.Cast<object>();
            }
        }
    }
}
