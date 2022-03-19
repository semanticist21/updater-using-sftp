using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Updater.Model;
using Updater.Pages.Run;

namespace Updater.Popup
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class PopupWindow : MetroWindow
    {
        public bool IsTopMost
        {
            get
            {
                return this.popupWindow.Topmost;
            }
            set
            {
                this.popupWindow.Topmost = value;
            }
        }
        public PopupWindow()
        {
            this.DataContext = PopupWindowModel.Instance();
            InitializeComponent();
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeView treeView && treeView.SelectedItem is PropertiesItem selectedTreeItem)
            {
                switch (selectedTreeItem.Key)
                {
                    case 1:
                        this.pageViewer.Source = new Uri("/Pages/General/Connection.xaml", UriKind.Relative);
                        break;
                    case 2:
                        this.pageViewer.Source = new Uri("/Pages/General/DirectorySettings.xaml", UriKind.Relative);
                        break;
                    case 3:
                        this.pageViewer.Source = new Uri("/Pages/General/Customs.xaml", UriKind.Relative);
                        break;
                    case 4:
                        this.pageViewer.Source = new Uri("/Pages/Excludes/ExcludesGeneral.xaml", UriKind.Relative);
                        break;
                    case 5:
                        this.pageViewer.Source = new Uri("/Pages/Run/RunGeneral.xaml", UriKind.Relative);
                        break;
                    default: break;
                }
            }
            else return;
        }
    }
}
