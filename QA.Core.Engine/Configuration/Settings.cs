using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Engine.Configuration
{
    public class Settings
    {
        internal static int GetInt32ValueForCurrentProperty(int def = 0, [CallerMemberName] string caller = "")
        {
            var str = ConfigurationManager.AppSettings[GetKey(caller)];
            if (str == null)
                return def;

            return Int32.Parse(str);
        }



        internal static bool GetBoolean(bool def = false, [CallerMemberName] string caller = "")
        {
            var str = ConfigurationManager.AppSettings[GetKey(caller)];
            if (str == null)
                return def;

            return Boolean.Parse(str);
        }
        internal static string GetString(string def = "", [CallerMemberName] string caller = "")
        {
            return ConfigurationManager.AppSettings[GetKey(caller)] ?? def;
        }


        private static string GetKey(string caller)
        {
            return string.Format("Engine.{0}", caller);
        }
    }
}
