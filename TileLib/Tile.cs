using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows;
using System.IO;

namespace TileLib
{
  public class TileInfo : Attribute
  {
    public readonly string Name;
    public readonly bool hasflyout;
    public readonly bool hasOptions;

    public TileInfo(string name, bool hasflyout, bool hasOptions)
    {
      this.Name = name;
      this.hasflyout = hasflyout;
      this.hasOptions = hasOptions;
    }
  }

  public abstract class BaseTile
  {
    #region Events
    public delegate void CaptionChangedEventHandler(string value);
    public delegate void IconChangedEventHandler(BitmapImage image);
    public delegate void ShowFlyoutEventHandler();
    public delegate void ShowOptionsEventHandler();
    public delegate void HeightChangedEventHandler(double height);
    public delegate void ShowNotificationEventHandler(string header, string message);

    public event CaptionChangedEventHandler CaptionChanged;
    public event IconChangedEventHandler IconChanged;
    public event ShowFlyoutEventHandler ShowFlyoutEvent;
    public event ShowOptionsEventHandler ShowOptionsEvent;
    public event HeightChangedEventHandler HeightChangedEvent;
    public event ShowNotificationEventHandler ShowNotificationEvent;
    #endregion

    private string _caption;
    private BitmapImage _icon;
    private bool _isMinimized;
    private bool _isPinned;
    private UserControl _flyoutContent;
    private UserControl _optionsContent;
    private double _height;
    public string _path;

    public string Caption
    {
      get { return _caption;}
      set
      {
        _caption = value;
        CaptionChanged(value);
      }
    }

    public bool IsMinimized
    {
      get { return _isMinimized; }
      set { _isMinimized = value; }
    }

    public bool IsPinned
    {
        get { return _isPinned; }
        set { _isPinned = value; }
    }

    public BitmapImage Icon
    {
      get { return _icon; }
      set
      {
        _icon = value;
        IconChanged(value);
      }
    }

    public double Height
    {
        get { return _height; }
        set
        {
            _height = value;
            if (HeightChangedEvent != null)
                HeightChangedEvent(value);
        }
    }

    public UserControl FlyoutContent
    {
      get { return _flyoutContent; }
      set { _flyoutContent = value; }
    }

    public UserControl OptionsContent
    {
      get { return _optionsContent; }
      set { _optionsContent = value; }
    }

    public abstract UserControl Load();

    public virtual void Unload() { }
    public virtual void ChangeSide(int side) { }
    public virtual void ChangeLocale(string locale) { }
    public virtual void ChangeTheme(string theme) { }
    public virtual void Minimized() { }
    public virtual void Unminimized() { }
    
    public virtual void ShowFlyout()
    {
      ShowFlyoutEvent();
    }

    public virtual void ShowOptions()
    {
      ShowOptionsEvent();
    }

    public virtual void ShowNotification(string header, string message)
    {
        ShowNotificationEvent(header, message);
    }

    public void WriteSetting(string tileName, string setting, string value)
    {
        bool exit = false;
        if (File.Exists(_path + "\\Library\\" + tileName + "\\" + "Settings.ini"))
        {
            string[] lines = File.ReadAllLines(_path + "\\Library\\" + tileName + "\\" + "Settings.ini");
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(setting))
                {
                    lines[i] = setting + "=" + value;
                    exit = true;
                }
            }
            File.WriteAllLines(_path + "\\Library\\" + tileName + "\\" + "Settings.ini", lines);
            if (exit)
                return;
        }
            StreamWriter writer = File.AppendText(_path + "\\Library\\" + tileName + "\\" + "Settings.ini");
            writer.WriteLine(setting + "=" + value);
            writer.Flush();
            writer.Close();
        /*RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree)
            .OpenSubKey("LongBar", RegistryKeyPermissionCheck.ReadWriteSubTree);
        key = key.OpenSubKey(tileName, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (key == null)
            key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree)
            .OpenSubKey("LongBar", RegistryKeyPermissionCheck.ReadWriteSubTree).CreateSubKey(tileName,RegistryKeyPermissionCheck.ReadWriteSubTree);
        key.SetValue(setting, value, RegistryValueKind.String);
        key.Close();*/
    }

    public void WriteSetting(string tileName, string setting, int value)
    {
        bool exit = false;
        if (File.Exists(_path + "\\Library\\" + tileName + "\\" + "Settings.ini"))
        {
            string[] lines = File.ReadAllLines(_path + "\\Library\\" + tileName + "\\" + "Settings.ini");
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(setting))
                {
                    lines[i] = setting + "=" + value.ToString();
                    exit = true;
                }
            }
            File.WriteAllLines(_path + "\\Library\\" + tileName + "\\" + "Settings.ini", lines);
            if (exit)
                return;
        }
        StreamWriter writer = File.AppendText(_path + "\\Library\\" + tileName + "\\" + "Settings.ini");
        writer.WriteLine(setting + "=" + value.ToString());
        writer.Flush();
        writer.Close();
        /*RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree)
            .OpenSubKey("LongBar", RegistryKeyPermissionCheck.ReadWriteSubTree);
        key = key.OpenSubKey(tileName, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (key == null)
            key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree)
            .OpenSubKey("LongBar", RegistryKeyPermissionCheck.ReadWriteSubTree).CreateSubKey(tileName, RegistryKeyPermissionCheck.ReadWriteSubTree);
        key.SetValue(setting, value, RegistryValueKind.DWord);
        key.Close();*/
    }

    public void WriteSetting(string tileName, string setting, string[] value)
    {
        bool exit = false;
        if (File.Exists(_path + "\\Library\\" + tileName + "\\" + "Settings.ini"))
        {
            string[] lines = File.ReadAllLines(_path + "\\Library\\" + tileName + "\\" + "Settings.ini");
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(setting + ":"))
                {
                    lines[i] = setting + ": ";
                    for (int n = 0; i < value.Length; i++)
                    {
                        lines[i] += value[n] + ";";
                    }
                    exit = true;
                }
            }
            File.WriteAllLines(_path + "\\Library\\" + tileName + "\\" + "Settings.ini", lines);
            if (exit)
                return;
        }
        StreamWriter writer = File.AppendText(_path + "\\Library\\" + tileName + "\\" + "Settings.ini");
        writer.Write(setting + ": ");
        for (int i = 0; i < value.Length; i++)
        {
            writer.Write(value[i] + ";");
        }
        writer.WriteLine();
        writer.Flush();
        writer.Close();
        /*RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree)
            .OpenSubKey("LongBar", RegistryKeyPermissionCheck.ReadWriteSubTree);
        key = key.OpenSubKey(tileName, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (key == null)
            key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree)
            .OpenSubKey("LongBar", RegistryKeyPermissionCheck.ReadWriteSubTree).CreateSubKey(tileName, RegistryKeyPermissionCheck.ReadWriteSubTree);
        key.SetValue(setting, value, RegistryValueKind.MultiString);
        key.Close();*/
    }

    public object ReadSetting(string tileName, string setting)
    {
        if (File.Exists(_path + "\\Library\\" + tileName + "\\" + "Settings.ini"))
        {
          string line = "";
          StreamReader reader = System.IO.File.OpenText(_path + "\\Library\\" + tileName + "\\" + "Settings.ini");
          while (!reader.EndOfStream)
          {
              line = reader.ReadLine();
              if (line.StartsWith(setting + ":"))
              {
                  string s = line.Substring(line.IndexOf(":") + 2, line.Length - line.IndexOf(":") - 2);
                  return (object)s.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
              }
              else if (line.StartsWith(setting))
              {
                  return (object)line.Split('=')[1];
              }
          }
          return null;
        }
        else
            return null;
        /*RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree)
            .OpenSubKey("LongBar", RegistryKeyPermissionCheck.ReadWriteSubTree);
        key = key.OpenSubKey(tileName, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (key == null)
        {
            return null;
        }
        else
        {
            return key.GetValue(setting);
        }*/
    }
  }
}
