#region Using

using Aplos.Helpers;
using Library.Crosscutting.Security;
using Library.Model.Logs;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Securites;
using System;
using System.Configuration;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

#endregion Using

namespace Aplos.Controllers
{
    public class APanelController : BaseController
    {
        #region Constructor

        private readonly IUserService _userService;
        private readonly IAccessLogService _accessLogService;

        public APanelController(
            IUserService userService
            , IAccessLogService accessLogService
            )
        {
            _userService = userService;
            _accessLogService = accessLogService;
        }

        #endregion Constructor

        [HttpGet]
        public ActionResult Aplos(string authToken, string groupId)
        {
            try
            {
                if (!UserValidate(PanelConst.aPanel.ToLower(), authToken, groupId))
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

        [HttpGet]
        public ActionResult ALayout()
        {
#if DEBUG
            ViewBag.BasePath = "/administrationpanel";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (string.IsNullOrEmpty(appName))
                ViewBag.BasePath = "/administrationpanel";
            else
                ViewBag.BasePath = "/" + appName + "/administrationpanel";
#endif
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ViewBag.Name = identity.Name;
                ViewBag.FullName = identity.FullName;
                ViewBag.CompanyGroupName = identity.CompanyGroupName;
                var accessLog = _accessLogService.GetLastLogin(identity.Name, PanelEnum.aPanel.ToString());
                if (accessLog != null)
                {
                    var offset = int.Parse(Session["timezoneoffset"].ToString());
                    var requestDateTime = accessLog.AccessTime.AddMinutes(-1 * offset);
                    var lastLoginTime = requestDateTime.ToString("dd-MMM-yyyy hh:mm tt");
                    if (!string.IsNullOrEmpty(accessLog.CountryName))
                        lastLoginTime += " (" + accessLog.CountryName + ")";
                    ViewBag.LastLoginTime = lastLoginTime;
                }
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Account", new { servicepanel = "aPanel" });
            }
        }

        [HttpGet]
        public ActionResult Dashboard()
        {
            return View();
        }

        #region Login/Logout

        [HttpGet, AllowAnonymous]
        public ActionResult Login(string authToken, string groupId)
        {
            try
            {
                if (!UserValidate(PanelConst.aPanel.ToLower(), authToken, groupId))
                    return Redirect("~/portal?authToken=" + authToken + "&groupId=" + groupId + "&invalidPanel=Access denied!");
                return View();
            }
            catch (Exception ex)
            {
                return Redirect("~/home/Error?message=" + ex.Message);
            }
        }

        [HttpPost, AllowAnonymous]
        public JsonResult Login(string timezoneoffset, string userId, string password, string remember, string authToken, string groupId, string groupName)
        {
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
                HttpContext.Session["timezoneoffset"] = timezoneoffset;
                var offset = int.Parse(timezoneoffset);
                var requestDateTime = DateTime.UtcNow.AddMinutes(-1 * offset);
                timezoneoffset = requestDateTime.DayOfWeek + " " + requestDateTime + " " + location.country_name + " Standard Time";
            }
            var result = _userService.Login(authToken, groupId, userId, password);
            if (result["Status"].ToString() == "Success")
            {
                var employeeId = result["EmployeeId"]?.ToString();
                SetAuthentication(result["Id"].ToString(), userId, result["UserFullName"].ToString(), isRemember, ip,
                    authToken, groupId, groupName, employeeId);
            }
            try
            {


                _accessLogService.Insert(new AccessLog
                {
                    AccessTime = DateTime.UtcNow,
                    AccessTimeWithCountry = timezoneoffset,
                    Browser = AccessInfo.GetBrowserName(http),
                    City = Convert.ToString(location.city),
                    CompanyGroupId = null,
                    CountryCode = Convert.ToString(location.country_code),
                    CountryName = Convert.ToString(location.country_name),
                    DaylightName = null,
                    DeviceType = null,
                    Dstoffset = http.Request.Browser.Platform,
                    Gmtoffset = null,
                    IsCookieEnable = http.Request.Browser.Cookies,
                    IsJavascriptEnable = http.Request.Browser.VBScript,
                    Latitude = Convert.ToString(location.latitude),
                    Longitude = Convert.ToString(location.longitude),
                    OS = AccessInfo.GetOS(http),
                    Panel = PanelEnum.aPanel.ToString(),
                    RegionCode = Convert.ToString(location.region_code),
                    RegionName = Convert.ToString(location.region_name),
                    Resistered = result["Status"].ToString() == "Success",
                    ScreenSize = null,
                    Status = result["Status"].ToString() == "Success",
                    TimeZone = Convert.ToString(location.time_zone),
                    UserAgent = HttpContext.Request.UserAgent,
                    UserId = userId,
                    WorkStationIP = ip,
                    WorkStationName = AccessInfo.GetWorkstationName(ip),
                    ZipCode = Convert.ToString(location.zip_code)
                });
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
    string groupName)
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
            // 🌍 TIMEZONE FIX
            // ==============================
            DateTime utcNow = DateTime.UtcNow;
            DateTime userLocalTime = utcNow;
            int offsetMinutes = 0;

            if (!string.IsNullOrEmpty(timezoneoffset))
            {
                int.TryParse(timezoneoffset, out offsetMinutes);

                // JS offset is reversed sign
                userLocalTime = utcNow.AddMinutes(-offsetMinutes);

                HttpContext.Session["timezoneoffset"] = offsetMinutes;
            }

            // ==============================
            // 🔐 LOGIN
            // ==============================
            var result = _userService.Login(authToken, groupId, userId, password);

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
                    employeeId);
            }

            // ==============================
            // 📝 ACCESS LOG (FIXED)
            // ==============================
            try
            {
                _accessLogService.Insert(new AccessLog
                {
                    // ✔ Always store UTC
                    AccessTime = utcNow,

                    // ✔ Store user real local time (NEW COLUMN)
                    //LocalAccessTime = userLocalTime,

                    // ✔ Store offset (NEW COLUMN)
                    //TimeZoneOffsetMinutes = offsetMinutes,

                    Browser = AccessInfo.GetBrowserName(http),
                    OS = AccessInfo.GetOS(http),
                    UserAgent = HttpContext.Request.UserAgent,
                    UserId = userId,
                    WorkStationIP = ip,
                    WorkStationName = AccessInfo.GetWorkstationName(ip),

                    CountryCode = Convert.ToString(location.country_code),
                    CountryName = Convert.ToString(location.country_name),
                    RegionCode = Convert.ToString(location.region_code),
                    RegionName = Convert.ToString(location.region_name),
                    City = Convert.ToString(location.city),
                    Latitude = Convert.ToString(location.latitude),
                    Longitude = Convert.ToString(location.longitude),
                    TimeZone = Convert.ToString(location.time_zone),
                    ZipCode = Convert.ToString(location.zip_code),

                    Panel = PanelEnum.aPanel.ToString(),
                    Status = result["Status"].ToString() == "Success",
                    Resistered = result["Status"].ToString() == "Success",

                    IsCookieEnable = http.Request.Browser.Cookies,
                    IsJavascriptEnable = http.Request.Browser.VBScript,
                    Dstoffset = http.Request.Browser.Platform,

                    CompanyGroupId = null,
                    DaylightName = null,
                    DeviceType = null,
                    Gmtoffset = null,
                    ScreenSize = null
                });
            }
            catch { }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private void SetAuthentication(string id, string userId, string fullName, bool isRemember, string ip, string authenticationToken, string companyGroupId, string companyGroupName, string employeeId)
        {
            var basicTicket = CustomIdentity.CreateBasicTicket(
                id, userId, fullName, companyGroupId, companyGroupName,
                null, null, null, null,
                employeeId, ip, false, true, true,
                authenticationToken, null);
            var roleTicket = CustomIdentity.CreateRoleTicket(new[] { "SysAdmin, Admin, NoAction" });
            var timeOut = Convert.ToInt32(new AppSettingsReader().GetValue("COOKIE_TIMEOUT", typeof(string)));
            var authTicket = new FormsAuthenticationTicket(1, FormsAuthentication.FormsCookieName, DateTime.Now, DateTime.Now.AddMinutes(timeOut), isRemember, basicTicket);
            var encTicket = FormsAuthentication.Encrypt(authTicket);
            HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
            HttpContext.Response.Cookies.Add(new HttpCookie("ROOT_FOLDRR", ResourcesPathReader.GetROOT_FOLDER()));
            HttpContext.Application["BasicTicket" + userId] = basicTicket;
            HttpContext.Application["RoleTicket" + userId] = roleTicket;
            HttpContext.Session["Panel"] = PanelEnum.aPanel.ToString();
        }

        private bool UserValidate(string servicepanel, string authenticationToken, string groupId)
        {
            if (groupId == null) throw new ArgumentNullException(nameof(groupId));
            var userData = _userService.CheckUserAuthenticationToken(authenticationToken);
            if (Convert.ToBoolean(userData[5]) && servicepanel == PanelConst.cPanel)
                return false;
            if (Convert.ToBoolean(userData[6]) && (servicepanel == PanelConst.cPanel || servicepanel == PanelConst.aPanel))
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

        #endregion Login/Logout
    }
}