using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;

namespace Aplos.Helpers
{
    public static class AccessInfo
    {
        public static string GetWorkstationIP(HttpContext http)
        {
            var ips = http.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            return !string.IsNullOrEmpty(ips) ? ips.Split(',')[0] : http.Request.ServerVariables["REMOTE_ADDR"];
        }
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
        public static string GetWorkstationName(string ip)
        {
            try
            {
                var hostEntry = Dns.GetHostEntry(ip);
                if (hostEntry == null) return "default";
                return string.IsNullOrEmpty(hostEntry.HostName) ? "default" : hostEntry.HostName;
            }
            catch (Exception)
            {
                return "default";
            }
        }

        public static string GetBrowserName(HttpContext http)
        {
            return http.Request.Browser.Browser + " " + http.Request.Browser.Version;
        }

        public static string GetOS(HttpContext http)
        {
            var ua = http.Request.UserAgent;
            if (ua.Contains("Android"))
                return $"Android {GetMobileVersion(ua, "Android")}";
            if (ua.Contains("iPad"))
                return $"iPad OS {GetMobileVersion(ua, "OS")}";
            if (ua.Contains("iPhone"))
                return $"iPhone OS {GetMobileVersion(ua, "OS")}";
            if (ua.Contains("Linux") && ua.Contains("KFAPWI"))
                return "Kindle Fire";
            if (ua.Contains("RIM Tablet") || (ua.Contains("BB") && ua.Contains("Mobile")))
                return "Black Berry";
            if (ua.Contains("Windows Phone"))
                return $"Windows Phone {GetMobileVersion(ua, "Windows Phone")}";
            if (ua.Contains("Mac OS"))
                return "Mac OS";
            if (ua.Contains("Windows NT 5.1") || ua.Contains("Windows NT 5.2"))
                return "Windows XP";
            if (ua.Contains("Windows NT 6.0"))
                return "Windows Vista";
            if (ua.Contains("Windows NT 6.1"))
                return "Windows 7";
            if (ua.Contains("Windows NT 6.2"))
                return "Windows 8";
            if (ua.Contains("Windows NT 6.3"))
                return "Windows 8.1";
            if (ua.Contains("Windows NT 10"))
                return "Windows 10";
            return http.Request.Browser.Platform + (ua.Contains("Mobile") ? " Mobile " : "");
        }

        private static string GetMobileVersion(string userAgent, string device)
        {
            var temp = userAgent.Substring(userAgent.IndexOf(device) + device.Length).TrimStart();
            var version = string.Empty;
            foreach (var character in temp)
            {
                var validCharacter = false;
                if (int.TryParse(character.ToString(), out int test))
                {
                    version += character;
                    validCharacter = true;
                }
                if (character == '.' || character == '_')
                {
                    version += '.';
                    validCharacter = true;
                }
                if (!validCharacter)
                    break;
            }
            return version;
        }

        public static object GetLocation(string ip)
        {
            try
            {
                return null;
                //return JsonConvert.DeserializeObject(new WebClient().DownloadString("http://freegeoip.net/json/" + ip));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}