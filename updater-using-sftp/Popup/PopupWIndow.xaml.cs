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

namespace Updater.Popup
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class PopupWindow : Window
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
            InitializeComponent();
            this.DataContext = new PopupWindowModel();
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeView treeView)
            {
                Debug.WriteLine(treeView.SelectedItem);
            }
        }
    }
}
