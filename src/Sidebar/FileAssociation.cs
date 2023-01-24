using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sidebar
{
    public class FileAssociation
    {
        public static void Register()
        {
            RegistryKey key;
            key = Registry.ClassesRoot;
            key = key.CreateSubKey(".tile", RegistryKeyPermissionCheck.ReadWriteSubTree);
            key.SetValue(null, "LongBar.Tile", RegistryValueKind.String);
            key = Registry.ClassesRoot;
            key = key.CreateSubKey("LongBar.Tile", RegistryKeyPermissionCheck.ReadWriteSubTree);
            key.SetValue(null, "LongBar Tile", RegistryValueKind.String);
            key = key.CreateSubKey("DefaultIcon", RegistryKeyPermissionCheck.ReadWriteSubTree);
            key.SetValue(null, Settings.Current.path + @"\Slate.dll,0", RegistryValueKind.ExpandString);
            key = Registry.ClassesRoot;
            key = key.OpenSubKey("LongBar.Tile", RegistryKeyPermissionCheck.ReadWriteSubTree);
            key = key.CreateSubKey("shell", RegistryKeyPermissionCheck.ReadWriteSubTree);
            key = key.CreateSubKey("Install", RegistryKeyPermissionCheck.ReadWriteSubTree);
            key = key.CreateSubKey("command", RegistryKeyPermissionCheck.ReadWriteSubTree);
            key.SetValue(null, Settings.Current.path + @"\" + Assembly.GetExecutingAssembly().GetName().Name + @".exe %1", RegistryValueKind.String);
            key.Close();
        }

        public static void Unregister()
        {
            RegistryKey key;
            key = Registry.ClassesRoot.OpenSubKey(".tile", RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (key != null)
            {
                Registry.ClassesRoot.DeleteSubKeyTree(".tile");
            }
            key = Registry.ClassesRoot.OpenSubKey("LongBar.Tile", RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (key != null)
            {
                Registry.ClassesRoot.DeleteSubKeyTree("LongBar.Tile");
            }
            key.Close();
        }
    }
}
