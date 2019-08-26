using System;
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
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using Sidebar.Core;

namespace Sidebar
{
  /// <summary>
  /// Interaction logic for Flyout.xaml
  /// </summary>
  public partial class Flyout : Window
  {
    private IntPtr handle;
    private ContextMenu m;

    private bool loaded = false;

    public Flyout(string tileName)
    {
      InitializeComponent();
      this.Deactivated += new EventHandler(Flyout_Deactivated);
      this.Loaded += new RoutedEventHandler(Flyout_Loaded);
      this.MouseMove += new MouseEventHandler(Flyout_MouseMove);
      this.SourceInitialized += new EventHandler(Flyout_SourceInitialized);

      m = (ContextMenu)this.Resources["Menu"];
    }

    private void Flyout_Deactivated(object sender, EventArgs e)
    {
        if (!((MenuItem)m.Items[0]).IsChecked && loaded)
        {
            this.ContentGrid.Children.Clear();
            this.Close();
            Application.Current.Windows[0].Activate();
        }
    }

    private void Flyout_Loaded(object sender, RoutedEventArgs e)
    {
        //DwmManager.DisableGlass(ref handle);
        //this.Width = ((FrameworkElement)this.ContentGrid.Children[0]).Width + ContentGrid.Margin.Left + ContentGrid.Margin.Right;
        //this.Height = ((FrameworkElement)this.ContentGrid.Children[0]).Height + ContentGrid.Margin.Top + ContentGrid.Margin.Bottom;
        //DwmManager.EnableGlass(ref handle, 0);
        switch (LongBarMain.sett.side)
        {
            case AppBarSide.Left:
                ((DoubleAnimation)TryFindResource("LoadAnimLeft")).To = this.Left + LongBarMain.sett.width;
                break;
            case AppBarSide.Right:
                ((DoubleAnimation)TryFindResource("LoadAnimLeft")).To = this.Left - this.Width;
                break;
        }
        this.BeginAnimation(LeftProperty, (DoubleAnimation)TryFindResource("LoadAnimLeft"));
        this.BeginAnimation(OpacityProperty, (DoubleAnimation)TryFindResource("LoadAnimOpacity"));
        Application.Current.Windows[0].Activate();
    }

    private void Flyout_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed)
        NativeMethods.SendMessageW(handle, 274, 61449, IntPtr.Zero);
    }

    private void Flyout_SourceInitialized(object sender, EventArgs e)
    {
      handle = new WindowInteropHelper(this).Handle;
      this.Width = ((FrameworkElement)this.ContentGrid.Children[0]).Width + ContentGrid.Margin.Left + ContentGrid.Margin.Right;
      this.Height = ((FrameworkElement)this.ContentGrid.Children[0]).Height + ContentGrid.Margin.Top + ContentGrid.Margin.Bottom;
      //DwmManager.EnableGlass(ref handle, 0);
    }

    private void DropDownMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      m.IsOpen = true;
    }

    private void CloseItem_Click(object sender, RoutedEventArgs e)
    {
      ((MenuItem)m.Items[0]).IsChecked = false;
      Application.Current.Windows[0].Activate();
    }

    private void DoubleAnimation_Completed(object sender, EventArgs e)
    {
        loaded = true;
        if (Sidebar.LongBarMain.sett.enableGlass)
          DwmManager.EnableGlass(ref handle, IntPtr.Zero);
    }
  }
}
