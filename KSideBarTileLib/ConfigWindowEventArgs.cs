using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Applications.Sidebar
{
    public class ConfigWindowEventArgs
    {
        // Fields
        private Window _configWDW;

        // Properties
        public Window ConfigurationWindow
        {
            get
            {
                return this._configWDW;
            }
            set
            {
                this._configWDW = value;
            }
        }
    }
}
