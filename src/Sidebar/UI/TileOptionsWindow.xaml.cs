﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sidebar
{
    /// <summary>
    /// Interaction logic for TileOptionsWindow.xaml
    /// </summary>
    public partial class TileOptionsWindow : Window
    {
        public TileOptionsWindow()
        {
            InitializeComponent();
            this.OKbutton.Click += new RoutedEventHandler(OKbutton_Click);
            this.Closing += new System.ComponentModel.CancelEventHandler(TileOptions_Closing);
        }

        private void OKbutton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TileOptions_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ContentGrid.Children.Clear();
        }
    }
}
