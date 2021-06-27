using Library.Service.Properties;
using Microsoft.Web.Administration;
using System;
using System.Configuration;
using System.Web.Hosting;

namespace Library.Service.Helpers
{
    /// <summary>
    /// %windir%\system32\inetsrv\config
    /// </summary>
    public class IISManager
    {
        public static string GetVirtualPath(string applicationName, string virtualDirectoryName)
        {
           // return GetSite().Applications["/" + GetApplicationName(applicationName)].VirtualDirectories["/" + virtualDirectoryName].PhysicalPath;
            return System.Web.Hosting.HostingEnvironment.MapPath("/" + GetApplicationName(applicationName) + "/" + virtualDirectoryName);
        }

        public static string GetVirtualDirectoryName(string virtualDirectoryName)
        {
            try
            {
                return new AppSettingsReader().GetValue(virtualDirectoryName, typeof(string)).ToString();
            }
            catch
            {
                throw new Exception(ServiceResources.VirtualDirectoryNameNotFound);
            }
        }

        public static string GetApplicationName(string applicationName)
        {
            try
            {
                return new AppSettingsReader().GetValue(applicationName, typeof(string)).ToString();
            }
            catch
            {
                throw new Exception(ServiceResources.ApplicationNameNotFound);
            }
        }

        private static Site GetSite()
        {
            return new ServerManager().Sites[HostingEnvironment.ApplicationHost.GetSiteName()];
        }
    }
}