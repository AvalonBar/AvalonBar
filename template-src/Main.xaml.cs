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
using TileLib;

[assembly: TileInfo("$safeprojectname$", true, true)]

namespace $rootnamespace$
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class $safeprojectname$ : UserControl
    {
        public $safeprojectname$()
        {
            InitializeComponent();
        }
    }

    public class Tile : BaseTile
    {
        private $safeprojectname$ Content;

        public override UserControl Load()
        {
            Content = new $safeprojectname$();
            this.Caption = "$safeprojectname$";
 	        return Content;
        }

        public override void  ShowFlyout()
        {
            //this.FlyoutContent = Content.Flyout;
 	        base.ShowFlyout(); // All Flyout initializations must be BEFORE calling this method
        }

        public override void  ShowOptions()
        {
            //this.OptionsContent = Content.Options;
 	        base.ShowOptions(); // All Options initializations must be BEFORE calling this method
        }

        public override void  ChangeSide(int side)
        {
 	        base.ChangeSide(side);
            //Add some code here
        }
        public override void  Unload()
        {
 	        base.Unload();
            //Add some code here
        }
    }
}
