using Aplos.Helpers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Logs;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Securites;
using System;
using System.Configuration;
using System.Data;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Aplos.Controllers
{
    public class MPanelController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IAccessLogService _accessLogService;
        private readonly IUserAccessPlantService _userAccessPlantService;
        private readonly ISqlRepository _sqlrepository;
        public MPanelController(
            IUserService userService
            , IAccessLogService accessLogService
            , IUserAccessPlantService userAccessPlantService
               , ISqlRepository r
            )
        {
            _userService = userService;
            _accessLogService = accessLogService;
            _userAccessPlantService = userAccessPlantService;
            _sqlrepository = r;
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Aplos(string authToken, string groupId)
        {
            try
            {
                if (!UserValidate(PanelConst.mPanel.ToLower(), authToken, groupId))
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
        public ActionResult MLayout()
        {
#if DEBUG
            ViewBag.BasePath = "/masterpanel";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (string.IsNullOrEmpty(appName))
                ViewBag.BasePath = "/masterpanel";
            else
                ViewBag.BasePath = "/" + appName + "/masterpanel";
#endif
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ViewBag.Name = identity.Name;
                ViewBag.FullName = identity.FullName;
                ViewBag.CompanyGroupName = identity.CompanyGroupName;
                ViewBag.CompanyName = identity.CompanyName;
                ViewBag.PlantName = identity.PlantName;
                var accessLog = _accessLogService.GetLastLogin(identity.Name, PanelEnum.mPanel.ToString());
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
                return RedirectToAction(nameof(Login), "Account", new { servicepanel = "mPanel" });
            }
        }

        [HttpGet, Authorize]
        public ActionResult Dashboard()
        {
            return View();
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
            SetAuthentication(identity.UserId, identity.Name, identity.FullName, false, identity.IPAddress, identity.AuthenticationToken, identity.CompanyGroupId, identity.CompanyGroupName, false, identity.CompanyId, identity.CompanyName, identity.IsSysAdmin, identity.IsPowerUser, identity.EmployeeId, plantId, plantName);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Login(string authToken, string groupId)
        {
            try
            {
                if (!UserValidate(PanelConst.mPanel.ToLower(), authToken, groupId))
                    return Redirect("~/portal?authenticationToken=" + authToken + "&groupId=" + groupId + "&invalidPanel=Access denied!");
                return View();
            }
            catch (Exception ex)
            {
                return Redirect("~/home/Error?message=" + ex.Message);
            }
        }

        [HttpPost, AllowAnonymous]
        public JsonResult Login(string timezoneoffset, string userId, string password, string remember, string authToken, string groupId, string groupName, string companyId, string companyName)
        {
            //var user = _userService.Query(t => t.UserId == userId).Select().FirstOrDefault();
            //if (!user.PowerUser) return null;

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
            var result = _userService.Login(authToken, groupId, companyId, userId, password);
            if (result["Status"].ToString() == "Success")
            {
                var employeeId = result["EmployeeId"]?.ToString();
                SetAuthentication(result["Id"].ToString(), userId, result["UserFullName"].ToString(),
                        isRemember, ip, authToken, groupId, groupName,
                        Convert.ToBoolean(result["ConcurrentUser"]), companyId, companyName,
                        Convert.ToBoolean(result["IsSysAdmin"]), true, employeeId, null, null);
            }

            string sql = @"SELECT * FROM ORg.Company AS c WHERE c.Id='" + companyId + "'";

            DataTable dtCompany = _sqlrepository.GetDataTable(sql);
            result.Add("CompanyFullName", dtCompany.Rows[0]["UserName"].ToString());
            result.Add("CompanyImage", dtCompany.Rows[0]["Image"].ToString());

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
                Panel = PanelEnum.mPanel.ToString(),
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

        private void SetAuthentication(string id, string userId, string fullName, bool isRemember, string ip, string authenticationToken, string companyGroupId, string companyGroupName, bool concurrentuser, string companyId, string companyName, bool isSysAdmin, bool isPowerUser, string employeeId, string plantId, string plantName)
        {
            if (concurrentuser)
            {
                var rdnumber = new Random();
                userId = userId + rdnumber.Next(0, 99999);
            }
            var basicTicket = CustomIdentity.CreateBasicTicket(
                id, userId, fullName, companyGroupId, companyGroupName,
                companyId, companyName, plantId, plantName, employeeId, ip, false, isSysAdmin, isPowerUser,
                authenticationToken, null);
            var roleTicket = CustomIdentity.CreateRoleTicket(new[] { "NoAction" });
            var timeOut = Convert.ToInt32(new AppSettingsReader().GetValue("COOKIE_TIMEOUT", typeof(string)));
            var authTicket = new FormsAuthenticationTicket(1, FormsAuthentication.FormsCookieName, DateTime.Now, DateTime.Now.AddMinutes(timeOut), isRemember, basicTicket);
            var encTicket = FormsAuthentication.Encrypt(authTicket);
            HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
            HttpContext.Response.Cookies.Add(new HttpCookie("ROOT_FOLDRR", ResourcesPathReader.GetROOT_FOLDER()));
            HttpContext.Application["BasicTicket" + userId] = basicTicket;
            HttpContext.Application["RoleTicket" + userId] = roleTicket;
            HttpContext.Session["Panel"] = PanelEnum.mPanel.ToString();
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
    }
}