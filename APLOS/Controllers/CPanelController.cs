using Aplos.Helpers;
using Aplos.Properties;
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

namespace Aplos.Controllers
{
    public class CPanelController : BaseController
    {
        private readonly IControlAdminService _controlAdminService;
        private readonly IAccessLogService _accessLogService;

        public CPanelController(
            IControlAdminService controlAdminService
            , IAccessLogService accessLogService
            )
        {
            _controlAdminService = controlAdminService;
            _accessLogService = accessLogService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public ActionResult CLayout()
        {
#if DEBUG
            ViewBag.BasePath = "/controlpanel";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (string.IsNullOrEmpty(appName))
                ViewBag.BasePath = "/controlpanel";
            else
                ViewBag.BasePath = "/" + appName + "/controlpanel";
#endif
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ViewBag.Name = identity.Name;
                ViewBag.FullName = identity.FullName;
                var accessLog = _accessLogService.GetLastLogin(identity.UserId, PanelEnum.cPanel.ToString());
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
                return RedirectToAction(nameof(Login), "Account", new { servicepanel = "cpanel" });
            }
        }

        [HttpGet]
        public ActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public ActionResult QueryEditor()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost, AllowAnonymous]
        public ActionResult Login(string timezoneoffset, string userId, string password, string remember)
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
                HttpContext.Session[nameof(timezoneoffset)] = timezoneoffset;
                var offset = int.Parse(timezoneoffset);
                var requestDateTime = DateTime.UtcNow.AddMinutes(-1 * offset);
                timezoneoffset = requestDateTime.DayOfWeek + " " + requestDateTime + " " + location.country_name + " Standard Time";
            }
            var result = _controlAdminService.Login(userId, password);
            if (result["Status"].ToString() == "Success")
                SetAuthentication(userId, result["UserFullName"].ToString(), isRemember, ip);
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
                Panel = PanelEnum.cPanel.ToString(),
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
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult Logout()
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null && authCookie.Value != "")
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    var identity = new CustomIdentity(ticket.UserData);
                    authCookie.Expires = DateTime.Now.AddDays(-1);
                    HttpContext.Response.Cookies.Add(authCookie);
                    HttpContext.Application["BasicTicket" + identity.Name] = null;
                    HttpContext.Application["RoleTicket" + identity.Name] = null;
                }
            }
            var basePath = "";
#if DEBUG
            basePath = "";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (!string.IsNullOrEmpty(appName))
                basePath = "/" + appName + "";
#endif
            return Json(new { BasePath = basePath, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        private void SetAuthentication(string userId, string fullName, bool isRemember, string ip)
        {
            var basicTicket = CustomIdentity.CreateBasicTicket(
                userId, userId, fullName,
                null, null, null, null, null, null,
                null, ip, true, true, true, null, null);
            var roleTicket = CustomIdentity.CreateRoleTicket(new[] { "ControlAdmin, SysAdmin, Admin, NoAction" });
            var timeOut = Convert.ToInt32(new AppSettingsReader().GetValue("COOKIE_TIMEOUT", typeof(string)));
            var authTicket = new FormsAuthenticationTicket(1, FormsAuthentication.FormsCookieName, DateTime.Now, DateTime.Now.AddMinutes(timeOut), isRemember, basicTicket);
            var encTicket = FormsAuthentication.Encrypt(authTicket);
            HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
            HttpContext.Response.Cookies.Add(new HttpCookie("ROOT_FOLDRR", ResourcesPathReader.GetROOT_FOLDER()));
            HttpContext.Application["BasicTicket" + userId] = basicTicket;
            HttpContext.Application["RoleTicket" + userId] = roleTicket;
            HttpContext.Session["Panel"] = PanelEnum.cPanel.ToString();
        }
    }
}