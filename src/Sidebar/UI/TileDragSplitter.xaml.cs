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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sidebar
{
    /// <summary>
    /// Interaction logic for TileDragSplitter.xaml
    /// </summary>
    public partial class TileDragSplitter : UserControl
    {
        public TileDragSplitter(double height)
        {
            InitializeComponent();

            //HeightAnim.To = height;
        }

        public void Reload()
        {
            //this.BeginAnimation(HeightProperty, HeightAnim);
        }
    }
}
