using System;
using System.Collections.Generic;
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
        private bool isTopMost;
        public bool IsTopMost
        {
            get
            {
                return isTopMost;
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
    }
}
