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

        ObservableCollection<PropertiesGroup> propertiesGroups;

        #endregion

        #region [ Public Variabels ]

        public ObservableCollection<PropertiesGroup> PropertiesGroups
        {
            get { return propertiesGroups; }
            private set
            {
                propertiesGroups = value;
                RaisePropertyChanged("PropertiesGroups");
            }
        }

        #endregion
        internal PopupWindowModel()
        {
            propertiesGroups = new ObservableCollection<PropertiesGroup>();
            InitHierarchicalItems();
        }

        private void InitHierarchicalItems()
        {
            ObservableCollection<PropertiesItem> groupItems = new();

            groupItems.Add(MakePropertiesItem(1, "Connection"));
            groupItems.Add(MakePropertiesItem(2, "Directories"));
            groupItems.Add(MakePropertiesItem(3, "Customs"));
            PropertiesGroups.Add(MakePropertiesGroup(1, "General Configs", groupItems));
            groupItems.Clear();

            groupItems.Add(MakePropertiesItem(4, "General"));
            PropertiesGroups.Add(MakePropertiesGroup(2, "Excludes", groupItems));
            groupItems.Clear();

            groupItems.Add(MakePropertiesItem(5, "General"));
            PropertiesGroups.Add(MakePropertiesGroup(3, "Run Configs", groupItems));
            groupItems.Clear();
        }

        private static PropertiesItem MakePropertiesItem(int key, string name)
        {
            return new PropertiesItem() { Key = key, Name = name };
        }

        private static PropertiesGroup MakePropertiesGroup(int key, string groupName, ObservableCollection<PropertiesItem> groupItems)
        {
            return new PropertiesGroup() { Key = key, GroupName = groupName, GroupItems = new ObservableCollection<PropertiesItem>(groupItems) };
        }

    }
}
