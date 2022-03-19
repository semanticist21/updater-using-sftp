﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Updater.Popup;

namespace Updater.Pages.General
{
    /// <summary>
    /// Interaction logic for Connection.xaml
    /// </summary>
    public partial class DirectorySettings : Page
    {
        public DirectorySettings()
        {
            this.DataContext = PopupWindowModel.Instance();
            InitializeComponent();
        }
    }
}
