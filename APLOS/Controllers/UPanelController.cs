using Aplos.Helpers;
using Aplos.Properties;
using ConnectionManager;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Logs;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Securites;
using Library.Service.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Management;
using System.IO;
using System.Net.NetworkInformation;

namespace Aplos.Controllers
{
    public class UPanelController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IAccessLogService _accessLogService;
        private readonly ISqlRepository _sqlrepository;
        public UPanelController(
            IUserService userService
            , IAccessLogService accessLogService
            , ISqlRepository r
            )
        {
            _userService = userService;
            _accessLogService = accessLogService;
            _sqlrepository = r;
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Aplos(string authToken, string groupId)
        {
            try
            {

                if (!UserValidate(PanelConst.uPanel.ToLower(), authToken, groupId))
                    return Redirect("~/portal?authToken=" + authToken + "&groupId=" + groupId + "&invalidPanel=Access denied!");
                ViewBag.AuthToken = authToken;
                ViewBag.GroupId = groupId;
                return View();
            }
            catch (Exception ex)
            {
                return Redirect("~/home/Error?message=" + ex.Message);
            }
        }
        [HttpGet, Authorize]
        public ActionResult NotificationURL()
        {

            try
            {
                var indentity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var x = _sqlrepository.GetData("select * from NotificationURL");
                if (x == null)
                    throw new Exception();

                if (string.IsNullOrEmpty(x["URL"].ToString()) == true)
                    throw new Exception();
                //x["URL"] = @"http://118.179.203.179/aglpop/Notification/signalr";//will be deleted later
                x.Add("PlantId", indentity.PlantId);
                x.Add("UserId", indentity.UserId);


                return Json(x, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpGet, Authorize]
        public ActionResult ULayout()
        {
#if DEBUG
            ViewBag.BasePath = "/applicationpanel";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (string.IsNullOrEmpty(appName))
                ViewBag.BasePath = "/applicationpanel";
            else
                ViewBag.BasePath = "/" + appName + "/applicationpanel";
#endif
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ViewBag.Name = identity.Name;
                ViewBag.FullName = identity.FullName;
                ViewBag.CompanyGroupName = identity.CompanyGroupName;
                ViewBag.CompanyName = identity.CompanyName;
                var accessLog = _accessLogService.GetLastLogin(identity.Name, PanelEnum.uPanel.ToString());
                if (accessLog == null) return View();
                var offset = int.Parse(Session["timezoneoffset"].ToString());
                var requestDateTime = accessLog.AccessTime.AddMinutes(-1 * offset);
                var lastLoginTime = requestDateTime.ToString("dd-MMM-yyyy hh:mm tt");
                if (!string.IsNullOrEmpty(accessLog.CountryName))
                    lastLoginTime += " (" + accessLog.CountryName + ")";
                ViewBag.LastLoginTime = lastLoginTime;
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Login), "Account", new { servicepanel = "uPanel" });
            }
        }

        [HttpGet, Authorize]
        public async Task<ActionResult> Dashboard()
        {
            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    var _identitySignal = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    clsMobileNotification.SendMessage(_identitySignal.CompanyGroupId, _identitySignal.PlantId, _identitySignal.UserId, "Status: Ready To Process");

                }
                catch (Exception ex)
                {

                }
             
                return View();
            });
        }

        [HttpGet, Authorize]
        public ActionResult PlantSelection()
        {
            return View();
        }

        [HttpPost, Authorize]
        public JsonResult PlantSelection(string plantId, string plantName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            SetAuthentication(identity.UserId, identity.Name, identity.FullName, false, identity.IPAddress, identity.AuthenticationToken, identity.CompanyGroupId, identity.CompanyGroupName, false, identity.CompanyId, identity.CompanyName, identity.IsSysAdmin, identity.IsPowerUser, identity.EmployeeId, plantId, plantName, new Dictionary<string, object>());


            return Json(new
            {

                Message = AplosMessage.Success
            });
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Login(string authToken, string groupId)
        {
            try
            {
                if (!UserValidate(PanelConst.uPanel.ToLower(), authToken, groupId))
                    return Redirect("~/portal?authToken=" + authToken + "&groupId=" + groupId + "&invalidPanel=Access denied!");
                return View();
            }
            catch (Exception ex)
            {
                return Redirect("~/home/Error?message=" + ex.Message);
            }
        }

        

        [HttpPost, AllowAnonymous]
        public JsonResult Login(string timezoneoffset, string userId, string password, string remember, string authToken, string groupId, string groupName, string companyId, string companyName, string plantId)
        {
            //var user = _userService.Query(t => t.UserId == userId).Select().FirstOrDefault();
            //if (user.SysAdmin||user.PowerUser) return null;
           //var macId= GetLocalMacAddress();
         //   PhysicalAddress macId = GetMacAddress();
            var http = System.Web.HttpContext.Current;
            var isRemember = !string.IsNullOrWhiteSpace(remember) && remember == "on";
            var ip = AccessInfo.GetWorkstationIP(http);
            dynamic location = AccessInfo.GetLocation(ip);
            if (null == location)
            {
                location = new
                {
                    country_code = " ",
                    country_name = " ",
                    region_code = " ",
                    region_name = " ",
                    city = " ",
                    latitude = " ",
                    longitude = " ",
                    time_zone = " ",
                    zip_code = " "
                };
            }
            if (!string.IsNullOrEmpty(timezoneoffset))
            {
                HttpContext.Session[nameof(timezoneoffset)] = timezoneoffset;
                var offset = int.Parse(timezoneoffset);
                var requestDateTime = DateTime.UtcNow.AddMinutes(-1 * offset);
                timezoneoffset = requestDateTime.DayOfWeek + " " + requestDateTime + " " + location.country_name + " Standard Time";
            }


            var result = _userService.ApplicationPanelLogin(authToken, groupId, companyId, userId, password);
            if (result["Status"].ToString() == "Success")
            {
                var employeeId = result["EmployeeId"]?.ToString();
                SetAuthentication(result["Id"].ToString(), userId, result["UserFullName"].ToString(),
                        isRemember, ip, authToken, groupId, groupName,
                        Convert.ToBoolean(result["ConcurrentUser"]), companyId, companyName,
                        Convert.ToBoolean(result["IsSysAdmin"]), true, employeeId, null, null, result);
            }
            string sql = @"SELECT * FROM ORg.Company AS c WHERE c.Id='" + companyId + "'";

            DataTable dtCompany = _sqlrepository.GetDataTable(sql);
            result.Add("CompanyFullName", dtCompany.Rows[0]["UserName"].ToString());
            result.Add("CompanyImage", dtCompany.Rows[0]["Image"].ToString());

            AccessLog al = new AccessLog();
            al.Id = userId + System.DateTime.Now.Ticks.ToString();
            al.AccessTime = DateTime.UtcNow;
            al.AccessTimeWithCountry = timezoneoffset;
            al.Browser = AccessInfo.GetBrowserName(http);
            al.City = Convert.ToString(location.city);
            al.CompanyGroupId = null;
            al.CountryCode = Convert.ToString(location.country_code);
            al.CountryName = Convert.ToString(location.country_name);
            al.DaylightName = null;
            al.DeviceType = null;
            al.Dstoffset = http.Request.Browser.Platform;
            al.Gmtoffset = null;
            al.IsCookieEnable = http.Request.Browser.Cookies;
            al.IsJavascriptEnable = http.Request.Browser.VBScript;
            al.Latitude = Convert.ToString(location.latitude);
            al.Longitude = Convert.ToString(location.longitude);
            al.OS = AccessInfo.GetOS(http);
            al.Panel = PanelEnum.uPanel.ToString();
            al.RegionCode = Convert.ToString(location.region_code);
            al.RegionName = Convert.ToString(location.region_name);
            al.Resistered = result["Status"].ToString() == "Success";
            al.ScreenSize = null;
            al.Status = result["Status"].ToString() == "Success";
            al.TimeZone = Convert.ToString(location.time_zone);
            al.UserAgent = HttpContext.Request.UserAgent;
            al.UserId = userId;
            al.WorkStationIP = ip;
            al.WorkStationName = AccessInfo.GetWorkstationName(ip);
            al.ZipCode = Convert.ToString(location.zip_code);
            // _accessLogService.Insert(al);

            try
            {
                clsConnectionManager clsConnection = new clsConnectionManager();
                clsConnection.getDataSet("SELECT * FROM ACS.AccessLog WHERE 1=2", out DataSet dsAccessLog);
                AddNewRow(dsAccessLog.Tables[0], al);
                SaveDataSets(dsAccessLog);
            }
            catch (Exception ex)
            {
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, AllowAnonymous]
        public JsonResult _Login(
    string timezoneoffset,
    string userId,
    string password,
    string remember,
    string authToken,
    string groupId,
    string groupName,
    string companyId,
    string companyName,
    string plantId)
        {
            var http = System.Web.HttpContext.Current;
            bool isRemember = !string.IsNullOrWhiteSpace(remember) && remember == "on";

            string ip = AccessInfo.GetWorkstationIP(http);
            dynamic location = AccessInfo.GetLocation(ip);

            // Fallback location if API fails
            if (location == null)
            {
                location = new
                {
                    country_code = "",
                    country_name = "",
                    region_code = "",
                    region_name = "",
                    city = "",
                    latitude = "",
                    longitude = "",
                    time_zone = "",
                    zip_code = ""
                };
            }

            // ==============================
            // 🌍 TIMEZONE FIX (IMPORTANT)
            // ==============================
            DateTime utcNow = DateTime.UtcNow;
            DateTime userLocalTime = utcNow;
            int offsetMinutes = 0;

            if (!string.IsNullOrEmpty(timezoneoffset))
            {
                int.TryParse(timezoneoffset, out offsetMinutes);

                // JS offset is reversed sign
                // Bangladesh sends -360 → we add +360
                userLocalTime = utcNow.AddMinutes(-offsetMinutes);

                // Save in session for future use
                HttpContext.Session["timezoneoffset"] = offsetMinutes;
            }

            // ==============================
            // 🔐 LOGIN PROCESS
            // ==============================
            var result = _userService.ApplicationPanelLogin(authToken, groupId, companyId, userId, password);

            if (result["Status"].ToString() == "Success")
            {
                var employeeId = result["EmployeeId"]?.ToString();

                SetAuthentication(
                    result["Id"].ToString(),
                    userId,
                    result["UserFullName"].ToString(),
                    isRemember,
                    ip,
                    authToken,
                    groupId,
                    groupName,
                    Convert.ToBoolean(result["ConcurrentUser"]),
                    companyId,
                    companyName,
                    Convert.ToBoolean(result["IsSysAdmin"]),
                    true,
                    employeeId,
                    null,
                    null,
                    result);
            }

            // ==============================
            // 🏢 COMPANY INFO
            // ==============================
            string sql = $"SELECT * FROM ORg.Company WHERE Id='{companyId}'";
            DataTable dtCompany = _sqlrepository.GetDataTable(sql);

            result.Add("CompanyFullName", dtCompany.Rows[0]["UserName"].ToString());
            result.Add("CompanyImage", dtCompany.Rows[0]["Image"].ToString());

            // ==============================
            // 📝 ACCESS LOG (FIXED)
            // ==============================
            try
            {
                AccessLog al = new AccessLog();
                al.Id = userId + DateTime.UtcNow.Ticks.ToString();

                // ✔ Always store UTC
                al.AccessTime = utcNow;

                // ✔ Store user's real local time
                //al.LocalAccessTime = userLocalTime;

                // ✔ Store offset separately
                //al.TimeZoneOffsetMinutes = offsetMinutes;

                al.Browser = AccessInfo.GetBrowserName(http);
                al.OS = AccessInfo.GetOS(http);
                al.UserAgent = HttpContext.Request.UserAgent;
                al.UserId = userId;
                al.WorkStationIP = ip;
                al.WorkStationName = AccessInfo.GetWorkstationName(ip);

                al.CountryCode = Convert.ToString(location.country_code);
                al.CountryName = Convert.ToString(location.country_name);
                al.RegionCode = Convert.ToString(location.region_code);
                al.RegionName = Convert.ToString(location.region_name);
                al.City = Convert.ToString(location.city);
                al.Latitude = Convert.ToString(location.latitude);
                al.Longitude = Convert.ToString(location.longitude);
                al.TimeZone = Convert.ToString(location.time_zone);
                al.ZipCode = Convert.ToString(location.zip_code);

                al.Panel = PanelEnum.uPanel.ToString();
                al.Status = result["Status"].ToString() == "Success";
                al.Resistered = al.Status;
                al.IsCookieEnable = http.Request.Browser.Cookies;
                al.IsJavascriptEnable = http.Request.Browser.VBScript;
                al.Dstoffset = http.Request.Browser.Platform;

                clsConnectionManager clsConnection = new clsConnectionManager();
                clsConnection.getDataSet("SELECT * FROM ACS.AccessLog WHERE 1=2", out DataSet dsAccessLog);

                AddNewRow(dsAccessLog.Tables[0], al);
                SaveDataSets(dsAccessLog);
            }
            catch { }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public static PhysicalAddress GetMacAddress()
        {
            var myInterfaceAddress = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .OrderByDescending(n => n.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .Select(n => n.GetPhysicalAddress()).ElementAt(1);

            return myInterfaceAddress;
        }

        public string Get_MACAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            String sMacAddress = string.Empty;
            foreach (NetworkInterface adapter in nics)
            {
                if (sMacAddress == String.Empty)// only return MAC Address from first card  
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
            }
            return sMacAddress;
        }

        static string Get_LocalMacAddress()
        {
            string mac_src = "";
            try
            {
                
                // Get the network interface for the local machine
                var localInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .OrderByDescending(n => n.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                     .Select(n => n.GetPhysicalAddress()).ElementAt(1);
                mac_src = BitConverter.ToString(localInterface.GetAddressBytes()).Replace('-', '-');
                
                

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return mac_src;

        }
        public static string GetLocalMacAddress()
        {
            try
            {
                // Get all network interfaces on the machine
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface nic in networkInterfaces)
                {
                    // Make sure the network interface is up and not a loopback or tunnel interface
                    if (nic.OperationalStatus == OperationalStatus.Up &&
                        nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    {
                        // Get the physical address (MAC address) of the network interface
                        PhysicalAddress physicalAddress = nic.GetPhysicalAddress();
                        byte[] bytes = physicalAddress.GetAddressBytes();

                        // Format the MAC address as a string (e.g., "00-1A-2B-3C-4D-5E")
                        string macAddress = string.Join("-", bytes.Select(b => b.ToString("X2")));

                        return macAddress;
                    }
                }

                // No valid network interface found
                return "No valid network interface found.";
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the process
                return $"Error: {ex.Message}";
            }
        }
        static string XGetLocalMacAddress()
        {
            try
            {
                // Get the network interface for the local machine
                NetworkInterface localInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(nic => nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

                if (localInterface != null)
                {
                    return localInterface.GetPhysicalAddress().ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting MAC address: " + ex.Message);
            }

            return null;
        }

        static string GetWifiMacAddress()
        {
            try
            {
                // Get the network interface for Wi-Fi
                NetworkInterface wifiInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211);

                if (wifiInterface != null)
                {
                    return wifiInterface.GetPhysicalAddress().ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting Wi-Fi MAC address: " + ex.Message);
            }

            return null;
        }

        public string GetMACAddress()
        {
            string mac_src = "";
            string macAddress = "";

            foreach (System.Net.NetworkInformation.NetworkInterface nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                //if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet) continue;
                if (nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                {
                    mac_src += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }

            while (mac_src.Length < 12)
            {
                mac_src = mac_src.Insert(0, "0");
            }

            for (int i = 0; i < 11; i++)
            {
                if (0 == (i % 2))
                {
                    if (i == 10)
                    {
                        macAddress = macAddress.Insert(macAddress.Length, mac_src.Substring(i, 2));
                    }
                    else
                    {
                        macAddress = macAddress.Insert(macAddress.Length, mac_src.Substring(i, 2)) + "-";
                    }
                }
            }
            return macAddress;
        }

        public void AddNewRow<T>(DataTable dt, T Data)
        {
            Dictionary<string, object> sourceData = Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(Data, null));
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dt.Rows.Add(dr);
        }
        public void SaveDataSets(params System.Data.DataSet[] dsRef)
        {
            clsConnection objCon = null;
            try
            {
                objCon = new clsConnection();
                objCon.BeginTransaction();
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveData(ref dsRef[i]);
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }

        } // End Function
        private void SetAuthentication(string id, string userId, string fullName, bool isRemember, string ip, string authenticationToken, string companyGroupId, string companyGroupName, bool concurrentuser, string companyId, string companyName, bool isSysAdmin, bool isPowerUser, string employeeId, string plantId, string plantName, Dictionary<string, object> result)
        {
            if (concurrentuser)
            {
                var rdnumber = new Random();
                userId = userId + rdnumber.Next(0, 99999);
            }
            var basicTicket = CustomIdentity.CreateBasicTicket(
                id, userId, fullName, companyGroupId, companyGroupName,
                companyId, companyName, plantId, plantName, employeeId, ip, false, isSysAdmin,
                isPowerUser, authenticationToken, null);
            var roleTicket = CustomIdentity.CreateRoleTicket(new[] { "NoAction" });
            var timeOut = Convert.ToInt32(new AppSettingsReader().GetValue("COOKIE_TIMEOUT", typeof(string)));
            var authTicket = new FormsAuthenticationTicket(1, FormsAuthentication.FormsCookieName, DateTime.Now, DateTime.Now.AddMinutes(timeOut), isRemember, basicTicket);
            var encTicket = FormsAuthentication.Encrypt(authTicket);
            HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
            HttpContext.Response.Cookies.Add(new HttpCookie("Identity", basicTicket));
            HttpContext.Response.Cookies.Add(new HttpCookie("ROOT_FOLDRR", ResourcesPathReader.GetROOT_FOLDER()));
            HttpContext.Application["BasicTicket" + userId] = basicTicket;
            HttpContext.Application["RoleTicket" + userId] = roleTicket;
            HttpContext.Session["Panel"] = PanelEnum.uPanel.ToString();

            result.Add("Cookie", ".ASPXAUTH=" + encTicket);
            result.Add("Identity", basicTicket);
        }

        private bool UserValidate(string servicepanel, string authenticationToken, string groupId)
        {
            if (groupId == null) throw new ArgumentNullException(nameof(groupId));
            var userData = _userService.CheckUserAuthenticationToken(authenticationToken);
            if (Convert.ToBoolean(userData[5]) &&
                servicepanel == PanelConst.cPanel) return false;
            if (Convert.ToBoolean(userData[6]) && (servicepanel == PanelConst.cPanel ||
                servicepanel == PanelConst.aPanel))
                return false;
            if (!Convert.ToBoolean(userData[5]) && !Convert.ToBoolean(userData[6]) && (servicepanel == PanelConst.cPanel || servicepanel == PanelConst.aPanel || servicepanel == PanelConst.mPanel))
                return false;
            if (Convert.ToBoolean(userData[3]))
                throw new Exception("Your Authentication Token is locked!");
            if (Convert.ToBoolean(userData[4]))
                throw new Exception("Your Account is locked!");
            if (userData[0] != groupId)
                throw new Exception("Invalid Group!");
            ViewBag.CompanyGroupId = userData[0];
            ViewBag.CompanyGroupName = userData[1];
            ViewBag.AuthenticationToken = authenticationToken;
            return true;
        }
    }
}