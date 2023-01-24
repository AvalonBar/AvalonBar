using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Applications.Sidebar
{
    [Serializable]
    public abstract class Tile
    {
        // Fields
        public bool hasConfigWindow;
        public bool hasFlyout;

        // Events
        public event ConfigurationWindowDelegate ConfigurationWindowClosing;

        public event ConfigurationWindowDelegate ConfigurationWindowOpened;

        public event FlyoutDelegate FlyoutClosing;

        public event FlyoutDelegate FlyoutOpened;

        // Methods
        protected Tile()
        {
        }

        public virtual void OnConfigurationWindowClosing(Window wdw)
        {
            ConfigWindowEventArgs args2 = new ConfigWindowEventArgs();
            args2.ConfigurationWindow = wdw;
            ConfigWindowEventArgs e = args2;
            this.ConfigurationWindowClosing(e);
        }

        public virtual void OnConfigurationWindowOpened(Window wdw)
        {
            ConfigWindowEventArgs args2 = new ConfigWindowEventArgs();
            args2.ConfigurationWindow = wdw;
            ConfigWindowEventArgs e = args2;
            this.ConfigurationWindowOpened(e);
        }

        public virtual void OnFlyoutClosing()
        {
            FlyoutEventArgs args2 = new FlyoutEventArgs();
            args2.FlyoutContent = this.FlyoutContent;
            FlyoutEventArgs e = args2;
            this.FlyoutClosing(e);
        }

        public virtual void OnFlyoutOpened()
        {
            FlyoutEventArgs args2 = new FlyoutEventArgs();
            args2.FlyoutContent = this.FlyoutContent;
            FlyoutEventArgs e = args2;
            this.FlyoutOpened(e);
        }

        // Properties
        public abstract Window ConfigurationWindow { get; }

        public abstract FrameworkElement FlyoutContent { get; }

        public abstract FrameworkElement SidebarContent { get; }

        // Nested Types
        public delegate void ConfigurationWindowDelegate(ConfigWindowEventArgs e);

        public delegate void FlyoutDelegate(FlyoutEventArgs e);
    }
}
