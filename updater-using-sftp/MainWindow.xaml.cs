using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Updater.model;
using Updater.services;

namespace Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            this.DataContext = new MainwindowModel();
            InitializeComponent();
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is ScrollViewer viewer) viewer.ScrollToBottom();
        }

        private void xCheckedAll_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox)
            {
                bool checkStatus = checkbox.IsChecked ?? false;
                foreach (object item in this.mainDataGrid.Items)
                {
                    if (item is FileInfoData itemFileProperties)
                    {
                        itemFileProperties.IsUpdateTarget = checkStatus;
                    }
                }
                CollectionViewSource.GetDefaultView(this.mainDataGrid.ItemsSource).Refresh();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (ConnectionManager.Instance().IsConnected)
            {
                ConnectionManager manager = ConnectionManager.Instance();
                manager.DiposeManager();
            }
        }

        public async Task ShowMessageConfirmAsync(string title, string message)
        {
            await this.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative);
        }
    }
}
