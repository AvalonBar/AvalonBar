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
            splitter = new TileDragSplitter(this.Height);
            this.content = content;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            handle = new WindowInteropHelper(this).Handle;
            this.SourceGrid.Children.Add(content);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                NativeMethods.SendMessageW(handle, 274, 61449, IntPtr.Zero);
            if (e.LeftButton == MouseButtonState.Released)
                Window_MouseLeftButtonUp(this, new MouseButtonEventArgs((MouseDevice)e.Device, e.Timestamp, MouseButton.Left));
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            int index = LongBarMain.GetElementIndexByYCoord(panel, this.Top); //берем индекс элемента по координате
            //Txt.Text = currentIndex.ToString() + "|" + index.ToString();
            if (index != currentIndex) //и не равен текущему, то суем сплиттер перед элементом с этим индексом
            {
                if (((index == -1 && currentIndex == 0) || (index == 100500 && currentIndex == panel.Children.Count - 1) &&
                    panel.Children.Contains(splitter)))
                    return;
                if (index > 0 && index < 100500 && panel.Children.IndexOf(splitter) == index - 1)
                    return;
                if (panel.Children.Contains(splitter)) //если сплиттер уже на панели,
                    panel.Children.Remove(splitter); //убираем его нафиг
                switch (index)
                {
                    case -1: //если индекс равен -1
                        panel.Children.Insert(0, splitter); //втыкаем сплиттер в начало
                        break;
                    case 100500: //если индекс равен стопицот
                        panel.Children.Add(splitter); //втыкаем сплиттер в конец
                        break;
                    default: //а иначе
                        panel.Children.Insert(index, splitter); //втыкаем сплиттер перед индексом
                        break;
                }
                splitter.Reload();
                currentIndex = panel.Children.IndexOf(splitter);
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (panel.Children.Contains(splitter))
                panel.Children.Remove(splitter);
            SourceGrid.Children.Clear();
            try
            {
                panel.Children.Insert(currentIndex, content);
            }
            catch { }
            this.Close();

        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
        }
    }
}
