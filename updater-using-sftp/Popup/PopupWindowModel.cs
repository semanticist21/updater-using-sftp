using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Updater.Model;

namespace Updater.Popup
{
    internal class PopupWindowModel : INotifyPropertyChanged
    {
        #region [ PropertyChanged Handler ]

        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged(string name)
        {
            PropertyChangedEventArgs propertyChangedEventArgs = new PropertyChangedEventArgs(name);
            PropertyChanged(this, propertyChangedEventArgs);
        }

        #endregion

        #region [ Private Variables ]

        IEnumerable<PropertiesGroup> propertiesGroup;
        IEnumerable<PropertiesItem> propertiesItems;

        #endregion

        #region [ Public Variabels ]

        IEnumerable<PropertiesGroup> PropertiesGroup
        {
            get { return propertiesGroup; }
        }
        IEnumerable<PropertiesItem> PropertiesItems
        {
            get { return propertiesItems; }
        }

        #endregion
        internal PopupWindowModel()
        {
            propertiesGroup = new List<PropertiesGroup>();
            propertiesItems = new List<PropertiesItem>();

            initHierarchicalItems();
        }

        private void initHierarchicalItems()
        {

        }


    }
}
