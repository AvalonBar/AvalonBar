using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Applications.Sidebar
{
    public class UsefulMethods
    {
        // Methods
        public static string getTileDllPath()
        {
            return Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
        }
    }
}
