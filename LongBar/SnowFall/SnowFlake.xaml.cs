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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace LongBar.SnowFall
{
    /// <summary>
    /// Interaction logic for SnowFlake.xaml
    /// </summary>
    public partial class SnowFlake : UserControl
    {
        private double cX = 0;
        private double cY = 0;

        private double radians = 0;
        private double speed;

        private int stageHeight = (int)System.Windows.SystemParameters.WorkArea.Height;

        private static Random ranNum = new Random();

        private bool _enabled = false;

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public SnowFlake()
        {
            this.InitializeComponent();

            Random rnd = new Random(Environment.TickCount);
            Image.Source = new BitmapImage(new Uri(string.Format("/LongBar;component/Resources/snow{0}.png", rnd.Next(6)), UriKind.Relative));
            // Insert code required on object creation below this point.

            speed = (.1 + ranNum.Next(1000)) / 200;

            if (!IsInDesignMode())
            {
                CompositionTarget.Rendering += new EventHandler(MoveSnowflakes);
            }
        }

        void MoveSnowflakes(object sender, EventArgs e)
        {
            if (Enabled)
            {
                radians += .5 * speed;

                cX = Canvas.GetLeft(this) + Math.Cos(.1 * radians);
                cY = Canvas.GetTop(this) + speed;

                Canvas.SetLeft(this, cX);
                Canvas.SetTop(this, cY);

                if (cY > stageHeight)
                {
                    cY = -50;

                    Canvas.SetTop(this, cY);
                }
            }
        }

        private bool IsInDesignMode()
        {
            return (bool)DependencyPropertyDescriptor.FromProperty(
                DesignerProperties.IsInDesignModeProperty,
                typeof(FrameworkElement)).Metadata.DefaultValue;
        } 
        
    }
}
