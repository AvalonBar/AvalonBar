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
	public string Name;
	public readonly bool hasflyout;
	public readonly bool hasOptions;
	// New tile info
	public readonly string Developer;
	public readonly string Description;
	public readonly string Version;

	public TileInfo(string name, bool hasflyout, bool hasOptions)
	{
	  this.Name = name;
	  this.hasflyout = hasflyout;
	  this.hasOptions = hasOptions;
	}

	public TileInfo(string name, bool hasflyout, bool hasOptions, string developer, string description, string version)
	{
		this.Name = name;
		this.hasflyout = hasflyout;
		this.hasOptions = hasOptions;
		this.Developer = developer;
		this.Description = description;
		this.Version = version;
	}
  }

	public class ErrorHandler : Attribute
	{
		public readonly bool UseErrorHandler;
		public readonly string DeveloperUrl;

		public ErrorHandler(bool UseErrorHandlerB)
		{
			this.UseErrorHandler = UseErrorHandlerB;
			REH = this.UseErrorHandler;
		}
		public ErrorHandler(bool UseErrorHandlerB, string DevUrl)
		{
			this.UseErrorHandler = UseErrorHandlerB;
			this.DeveloperUrl = DevUrl;
			REH = this.UseErrorHandler;
			DURL = this.DeveloperUrl;
		}

		public static bool REH;
		public static string DURL;
	}

  public abstract class BaseTile
  {
	#region Events
	public delegate void CaptionChangedEventHandler(string value);
	public delegate void IconChangedEventHandler(BitmapImage image);
	public delegate void ShowFlyoutEventHandler();
	public delegate void ShowOptionsEventHandler();
	public delegate void HeightChangedEventHandler(double height);

	public event CaptionChangedEventHandler CaptionChanged;
	public event IconChangedEventHandler IconChanged;
	public event ShowFlyoutEventHandler ShowFlyoutEvent;
	public event ShowOptionsEventHandler ShowOptionsEvent;
	public event HeightChangedEventHandler HeightChangedEvent;
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
	}
  }
}
