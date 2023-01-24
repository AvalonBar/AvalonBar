using System;
using System.Collections.Generic;
using System.Text;

namespace TileLib
{
    public class TileInfo : Attribute
    {
        public string Name;
        public readonly bool hasflyout;
        public readonly bool hasOptions;

        public TileInfo(string name, bool hasflyout, bool hasOptions)
        {
            this.Name = name;
            this.hasflyout = hasflyout;
            this.hasOptions = hasOptions;
        }
    }
}
