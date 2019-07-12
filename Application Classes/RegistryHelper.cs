using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveNoticeboard
{
    public class RegistryHelper
    {
        public static void AddDefaultRegistryEntries()
        {
            RegistryKey Key;
            Key = Registry.LocalMachine.CreateSubKey("Software\\Interactive Noticeboard\\About");
            Key.SetValue("Application name", "Interactive Noticeboard");
            Key.SetValue("Owner", "Department of CSTE, Noakhali Science and Technology University");
            Key.SetValue("Developed by", "TLABS");
            Key.SetValue("Email", "tlabs.im@gmail.com");
            Key.SetValue("Copyright", "copyright@2019");
        }

        public static void SetSettings(string parameter, object value)
        {
            RegistryKey Key;
            Key = Registry.LocalMachine.CreateSubKey(@"Software\Interactive Noticeboard\Settings");
            Key.SetValue(parameter, value.ToString());
        }

        public static void SetSettings(string parameter, object value, RegistryValueKind value_kind)
        {
            RegistryKey Key;
            Key = Registry.LocalMachine.CreateSubKey(@"Software\Interactive Noticeboard\Settings");
            Key.SetValue(parameter, value, value_kind);
        }

        public static void SetSettings(string key, string parameter, object value)
        {
            RegistryKey Key;
            Key = Registry.LocalMachine.CreateSubKey(@"Software\Interactive Noticeboard\Settings\" + key);
            Key.SetValue(parameter, value.ToString());
        }

        public static void SetSettings(RegistryKey key, string parameter, object value)
        {
            key.SetValue(parameter, value.ToString());
        }

        public static void SetSettings(string key, string parameter, object value, RegistryValueKind value_kind)
        {
            RegistryKey Key;
            Key = Registry.LocalMachine.CreateSubKey(@"Software\Interactive Noticeboard\Settings\" + key);
            Key.SetValue(parameter, value, value_kind);
        }

        public static void SetSettings(RegistryKey key, string parameter, object value, RegistryValueKind value_kind)
        {
            key.SetValue(parameter, value, value_kind);
        }

        public static bool HasSettings(string settings_name)
        {
            try
            {
                RegistryKey Key;
                Key = Registry.LocalMachine.OpenSubKey(@"Software\Interactive Noticeboard\Settings");
                object val = Key.GetValue(settings_name);
                return val != null;
            }
            catch { return false; }
        }

        public static bool HasSettings(string key, string settings_name)
        {
            try
            {
                RegistryKey Key;
                Key = Registry.LocalMachine.OpenSubKey(@"Software\Interactive Noticeboard\Settings\" + key);
                object val = Key.GetValue(settings_name);
                return val != null;
            }
            catch { return false; }
        }

        public static bool HasSettings(RegistryKey key, string settings_name)
        {
            try
            {
                object val = key.GetValue(settings_name);
                return val != null;
            }
            catch { return false; }
        }

        public static string GetSettings(string parameter)
        {
            string value = string.Empty;
            try
            {
                RegistryKey Key;
                Key = Registry.LocalMachine.OpenSubKey(@"Software\Interactive Noticeboard\Settings");
                object val = Key.GetValue(parameter);
                value = (val == null) ? string.Empty : val.ToString();
            }
            catch { }
            return value;
        }

        public static string GetSettings(string key, string parameter)
        {
            string value = string.Empty;
            try
            {
                RegistryKey Key;
                Key = Registry.LocalMachine.OpenSubKey(@"Software\Interactive Noticeboard\Settings\" + key);
                object val = Key.GetValue(parameter);
                value = (val == null) ? string.Empty : val.ToString();
            }
            catch { }
            return value;
        }

        public static string GetSettings(RegistryKey key, string parameter)
        {
            string value = string.Empty;
            try
            {
                object val = key.GetValue(parameter);
                value = (val == null) ? string.Empty : val.ToString();
            }
            catch { }
            return value;
        }

        public static void RemoveSettings(string parameter)
        {
            try
            {
                RegistryKey Key;
                Key = Registry.LocalMachine.OpenSubKey(@"Software\Interactive Noticeboard\Settings");
                Key.SetValue(parameter, null);
                Key.DeleteSubKey(parameter);
            }
            catch { }
        }

        public static void RemoveSettings(string key, string parameter)
        {
            try
            {
                RegistryKey Key;
                Key = Registry.LocalMachine.OpenSubKey(@"Software\Interactive Noticeboard\Settings\" + key);
                Key.SetValue(parameter, null);
                Key.DeleteSubKey(parameter);
            }
            catch { }
        }
    }
}
