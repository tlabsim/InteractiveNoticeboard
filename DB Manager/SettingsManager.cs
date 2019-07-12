using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLABS.Extensions;

namespace InteractiveNoticeboard.DB_Manager
{
    public class SettingsManager
    {
        public static bool HasSettings(string settings_group, string property)
        {
            string query = string.Format("SELECT COUNT(*) AS c FROM settings WHERE property= '{0}' AND settings_group = '{1}'", property, settings_group);

            return DBClient.ExecuteScalar(query).GetString().ToInt(-1) > 0;
        }

        public static string GetSettings(string settings_group, string property)
        {
            string query = string.Format("SELECT val FROM settings WHERE property= '{0}' AND settings_group = '{1}'", property, settings_group);
            return DBClient.ExecuteScalar(query).GetString();
        }

        public static bool SetSettings(string settings_group, string property, object value)
        {
            string query = string.Format("UPDATE settings SET val = '{0}' WHERE property= '{1}' AND settings_group = '{2}'", value.ToString(), property, settings_group);
            return DBClient.ExecuteNonQuery(query) >= 0;
        }
    }
}
