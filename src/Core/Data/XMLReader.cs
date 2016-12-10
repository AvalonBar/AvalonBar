using System;
using System.Xml;
using System.IO;
using System.Reflection;

namespace Slate.Data
{
	/// <summary>
	/// Description of XMLReader.
	/// </summary>
	public class XMLReader
	{
		public const string SettingsLoc = "Settings.xml";
		public static string ReadSettings(string block, string key)
		{
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(SettingsLoc);
			XmlNode documentElement = xdoc.DocumentElement;
			if (documentElement[block] == null || documentElement[block][key] == null)
			{
				return string.Format("Requested block/key could not be found: {0}/{1}", block, key);
			}
			return documentElement[block][key].InnerText;
		}
		public static void WriteSettings(string block, string key, string innertext)
		{
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(SettingsLoc);
			XmlNode documentElement = xdoc.DocumentElement;
			if (documentElement[block] == null || documentElement[block][key] == null)
			{
				throw new Exception(string.Format("Requested block/key could not be found: {0}/{1}", block, key));
			}
			documentElement[block][key].InnerText = innertext;
		}
		public static string ReadXML(string block, string key, XmlDocument xdoc)
		{
			XmlNode documentElement = xdoc.DocumentElement;
			if (documentElement[block] == null || documentElement[block][key] == null)
			{
				return string.Format("Requested block/key could not be found: {0}/{1}", block, key);
			}
			return documentElement[block][key].InnerText;
		}
		public static string ReadXML(string block, string key, string filename)
		{
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(filename);
			XmlNode documentElement = xdoc.DocumentElement;
			if (documentElement[block] == null || documentElement[block][key] == null)
			{
				return string.Format("Requested block/key could not be found: {0}/{1}", block, key);
			}
			return documentElement[block][key].InnerText;
		}
		public static void WriteXML(string block, string key, string filename, string innertext)
		{
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(filename);
			XmlNode documentElement = xdoc.DocumentElement;
			if (documentElement[block] == null || documentElement[block][key] == null)
			{
				throw new Exception(string.Format("Requested block/key could not be found: {0}/{1}", block, key));
			}
			documentElement[block][key].InnerText = innertext;
		}
		public static void WriteXML(string block, string key, XmlDocument xdoc, string innertext)
		{
			XmlNode documentElement = xdoc.DocumentElement;
			if (documentElement[block] == null || documentElement[block][key] == null)
			{
				throw new Exception(string.Format("Requested block/key could not be found: {0}/{1}", block, key));
			}
			documentElement[block][key].InnerText = innertext;
		}
	}
}
