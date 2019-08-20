using System;
using System.Collections.Generic;
using System.Text;

namespace Applications.Sidebar
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class SidebarTileInfo : Attribute
    {
        // Fields
        public string Author;
        public string Copyright;
        public string Description;
        public bool IsOuterTile;
        public string Title;
        public double Version;

        // Methods
        public SidebarTileInfo(string title, string author, string copyright, string description, double version, bool outertile)
        {
            this.Title = title;
            this.Author = author;
            this.Copyright = copyright;
            this.Description = description;
            this.Version = version;
            this.IsOuterTile = outertile;
        }
    }
}
