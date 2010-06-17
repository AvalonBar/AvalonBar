using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Runtime.Serialization.Formatters.Binary;

namespace Applications.Sidebar
{
    public class Settings
    {
        // Methods
        private static Dictionary<string, object> getDictionaryForTile(Guid key)
        {
            RegistryKey key2 = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("LongBar");
            MemoryStream serializationStream = new MemoryStream();
            if (key2 != null)
            {
                try
                {
                    if (key2.GetValue(key.ToString()) != null)
                    {
                        byte[] buffer = key2.GetValue(key.ToString()) as byte[];
                        serializationStream.Write(buffer, 0, buffer.Length);
                        serializationStream.Position = 0L;
                        return (Dictionary<string, object>)new BinaryFormatter().Deserialize(serializationStream);
                    }
                }
                catch (Exception)
                {
                }
            }
            return null;
        }

        public static object GetSetting(Guid key, string name)
        {
            Dictionary<string, object> dictionary = getDictionaryForTile(key);
            if ((dictionary != null) && (dictionary[name] != null))
            {
                return dictionary[name];
            }
            return null;
        }

        private static bool saveDictionaryForTile(Guid key, Dictionary<string, object> dictionary)
        {
            RegistryKey key2 = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("LongBar", true);
            MemoryStream serializationStream = new MemoryStream();
            if (key2 != null)
            {
                try
                {
                    new BinaryFormatter().Serialize(serializationStream, dictionary);
                    key2.SetValue(key.ToString(), serializationStream.ToArray());
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        public static bool StoreSetting(Guid key, string name, object value)
        {
            Dictionary<string, object> dictionary = getDictionaryForTile(key);
            if (dictionary == null)
            {
                dictionary = new Dictionary<string, object>();
            }
            if (dictionary.ContainsKey(name))
            {
                dictionary[name] = value;
                return saveDictionaryForTile(key, dictionary);
            }
            dictionary.Add(name, value);
            return saveDictionaryForTile(key, dictionary);
        }
    }
}
