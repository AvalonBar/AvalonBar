using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Applications.Sidebar
{
    [Serializable]
    public class BaseTile : Tile
    {
        // Methods
        public BaseTile()
        {
            base.hasConfigWindow = false;
            base.hasFlyout = false;
            base.ConfigurationWindowClosing += new Tile.ConfigurationWindowDelegate(this.BaseTile_ConfigurationWindowClosing);
            base.ConfigurationWindowOpened += new Tile.ConfigurationWindowDelegate(this.BaseTile_ConfigurationWindowOpened);
            base.FlyoutOpened += new Tile.FlyoutDelegate(this.BaseTile_FlyoutOpened);
            base.FlyoutClosing += new Tile.FlyoutDelegate(this.BaseTile_FlyoutClosing);
        }

        private void BaseTile_ConfigurationWindowClosing(ConfigWindowEventArgs e)
        {
        }

        private void BaseTile_ConfigurationWindowOpened(ConfigWindowEventArgs e)
        {
        }

        private void BaseTile_FlyoutClosing(FlyoutEventArgs e)
        {
        }

        private void BaseTile_FlyoutOpened(FlyoutEventArgs e)
        {
        }

        // Properties
        public override Window ConfigurationWindow
        {
            get
            {
                return null;
            }
        }

        public override FrameworkElement FlyoutContent
        {
            get
            {
                return null;
            }
        }

        public override FrameworkElement SidebarContent
        {
            get
            {
                return new Button();
            }
        }
    }
}
