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
using System.Windows.Media.Animation;

namespace Sidebar
{
    /// <summary>
    /// Interaction logic for TileDragWindow.xaml
    /// </summary>
    public partial class TileDragWindow : Window
    {
        private IntPtr handle;
        private StackPanel panel;
        private TileDragSplitter splitter;
        private int currentIndex = -1;
        private Tile content;

        public TileDragWindow(StackPanel panel, Tile content)
        {
            InitializeComponent();

            this.panel = panel;
            this.content = content;
            splitter = new TileDragSplitter(Height);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            handle = new WindowInteropHelper(this).Handle;
            SourceGrid.Children.Add(content);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // hWnd, WM_SYSCOMMAND, SC_SIZE | 0x9 (Drag from anywhere), lParam
                NativeMethods.SendMessage(
                    handle,
                    0x112,
                    new IntPtr(0xF000 | 0x9),
                    IntPtr.Zero);
            }

            if (e.LeftButton == MouseButtonState.Released)
            {
                Window_MouseLeftButtonUp(
                    this,
                    new MouseButtonEventArgs(
                        (MouseDevice)e.Device, e.Timestamp, MouseButton.Left));
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            // Take the element index by coordinate.
            int index = SidebarWindow.GetElementIndexByYCoord(panel, Top);
            //Txt.Text = currentIndex.ToString() + "|" + index.ToString();
            // Move the current tile if it's placed somewhere else.
            if (index != currentIndex)
            {
                if ((index == -1 && currentIndex == 0) ||
                    (index == 100500 && currentIndex == panel.Children.Count - 1) &&
                    panel.Children.Contains(splitter))
                {
                    return;
                }
                if (index > 0 && index < 100500 && panel.Children.IndexOf(splitter) == index - 1)
                {
                    return;
                }
                // Remove splitter if it's already on the panel.
                if (panel.Children.Contains(splitter))
                {
                    panel.Children.Remove(splitter);
                }

                switch (index)
                {
                    // Insert the splitter at the beginning
                    case -1:
                        panel.Children.Insert(0, splitter); 
                        break;
                    // Insert the splitter at the end if the index is "stopitsot".
                    case 100500:
                        panel.Children.Add(splitter); 
                        break;
                    // Insert the splitter before the index.
                    default:
                        panel.Children.Insert(index, splitter);
                        break;
                }
                splitter.Reload();
                currentIndex = panel.Children.IndexOf(splitter);
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (panel.Children.Contains(splitter))
            {
                panel.Children.Remove(splitter);
            }
            SourceGrid.Children.Clear();
            try
            {
                panel.Children.Insert(currentIndex, content);
            }
            catch { }
            Close();
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
        }
    }
}
