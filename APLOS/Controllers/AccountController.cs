using Aplos.Helpers;
using Library.Crosscutting.Security;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Modules;
using Library.Service.Organizations;
using Library.Service.Securites;
using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace Aplos.Controllers
{
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IPasswordChangeService _passwordChangeService;
        private readonly ICompanyService _companyService;
        private readonly IModuleExtendedService _moduleExtendedService;
        private readonly ICompanyGroupModuleService _companyGroupModuleService;

        public AccountController(
            IUserService userService,
            IPasswordChangeService passwordChangeService,
            ICompanyService companyService,
            IModuleExtendedService moduleExtendedService,
            ICompanyGroupModuleService companyGroupModuleService
            )
        {
            _userService = userService;
            _passwordChangeService = passwordChangeService;
            _companyService = companyService;
            _moduleExtendedService = moduleExtendedService;
            _companyGroupModuleService = companyGroupModuleService;
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Login(string servicepanel, string authToken, string groupId, string moduleId)
        {
            try
            {
                ViewBag.AuthToken = authToken;
                ViewBag.GroupId = groupId;
                ViewBag.servicepanel = servicepanel;
#if DEBUG
                ViewBag.BasePath = "/";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (string.IsNullOrEmpty(appName))
                ViewBag.BasePath = "/";
            else
                ViewBag.BasePath = "/" + appName + "/";
#endif
                switch (servicepanel.ToLower())
                {
                    case PanelConst.uPanel:
                        if (!UserValidate(servicepanel.ToLower(), authToken, groupId))
                            return RedirectToAction("Index", "Home", new { authToken, groupId, invalidPanel = "Access denied!" });
                        ViewBag.CompanyList = new JavaScriptSerializer().Serialize(_companyService.GetCboCompanyByCompanyGroup(groupId));
                        return View("uPanel");

                    case "hrms":
                        if (!UserValidate(servicepanel.ToLower(), authToken, groupId))
                            return Redirect("~/home/portal?authenticationToken=" + authToken + "&groupId=" + groupId + "&invalidPanel=Access denied!");
                        if (!_companyGroupModuleService.Any(r => r.CompanyGroupId == groupId && r.ModuleId == moduleId))
                            return Redirect("~/home/PortalExtended?authenticationToken=" + authToken + "&groupId=" + groupId + "&moduleId=" + moduleId + "&invalidPanel=This module is not eligible for this group!");
                        ViewBag.CompanyList = new JavaScriptSerializer().Serialize(_companyService.GetCboCompanyByCompanyGroup(groupId));
                        return View("hrms");

                    default:
                        return HttpNotFound();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Error");
            }
        }

        [HttpGet, AllowAnonymous]
        public JsonResult AppLogin(string timezoneoffset, string userId, string password, string remember, string authenticationToken, string groupId, string groupName, string companyId, string companyName, string appId)
        {
            bool isRemember = !string.IsNullOrWhiteSpace(remember) && remember == "on";
            var ip = AccessInfo.GetWorkstationIP(System.Web.HttpContext.Current);
            var result = _userService.Login(authenticationToken, groupId, companyId, null, appId, userId, password);
            if (result["Status"].ToString() == "Success")
                SetAuthentication(result["Id"].ToString(), userId, result["UserFullName"].ToString(), isRemember, ip, authenticationToken, groupId, groupName, Convert.ToBoolean(result["ConcurrentUser"]), companyId, companyName, Convert.ToBoolean(result["IsSysAdmin"]), Convert.ToBoolean(result["IsPowerUser"]));
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, AllowAnonymous]
        public JsonResult HRMS(string timezoneoffset, string userId, string password, string authToken, string groupId, string companyId, string plantId)
        {
            ViewBag.AuthToken = authToken;
            ViewBag.GroupId = groupId;
#if DEBUG
            ViewBag.BasePath = "/";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (string.IsNullOrEmpty(appName))
                ViewBag.BasePath = "/";
            else
                ViewBag.BasePath = "/" + appName + "/";
#endif
            HttpContext.Response.Cookies.Add(new HttpCookie("ROOT_FOLDRR", ResourcesPathReader.GetROOT_FOLDER()));

            var http = System.Web.HttpContext.Current;
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
                int offset = int.Parse(timezoneoffset);
                DateTime requestDateTime = DateTime.UtcNow.AddMinutes(-1 * offset);
                timezoneoffset = requestDateTime.DayOfWeek + " " + requestDateTime + " " + location.country_name + " Standard Time";
            }
            var result = _userService.Login(authToken, groupId, companyId, plantId, userId, password);
            if (result["Status"].ToString() == "Success")
                //result["Url"] = _moduleExtendedService.ModuleUrl(groupId, companyId, plantId);
                result["Url"] = _moduleExtendedService.ModuleUrl(groupId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        private void SetAuthentication(string id, string userId, string fullName, bool isRemember, string ip, string authToken, string companyGroupId, string companyGroupName, bool concurrentuser, string companyId, string companyName, bool isSysAdmin, bool isPowerUser)
        {
            if (concurrentuser)
            {
                Random rdnumber = new Random();
                userId = userId + rdnumber.Next(0, 99999);
            }
            var basicTicket = CustomIdentity.CreateBasicTicket(id,
                                                        userId,
                                                        fullName,
                                                        companyGroupId,
                                                        companyGroupName,
                                                        companyId,
                                                        companyName,
                                                        null,
                                                        null,
                                                        null,
                                                        ip,
                                                        false,
                                                        isSysAdmin,
                                                        isPowerUser,
                                                        authToken, null);
            var roleTicket = CustomIdentity.CreateRoleTicket(new[] { "NoAction" });
            int timeOut = Convert.ToInt32(new AppSettingsReader().GetValue("COOKIE_TIMEOUT", typeof(string)));
            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, FormsAuthentication.FormsCookieName, DateTime.Now, DateTime.Now.AddMinutes(timeOut), isRemember, basicTicket);
            string encTicket = FormsAuthentication.Encrypt(authTicket);
            HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
            HttpContext.Application["BasicTicket" + userId] = basicTicket;
            HttpContext.Application["RoleTicket" + userId] = roleTicket;
            HttpContext.Session["Panel"] = "mpanel";
        }

        public ActionResult Logout(string road)
        {
            string token = string.Empty, gid = string.Empty;
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null && authCookie.Value != "")
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                var identity = new CustomIdentity(ticket.UserData);
                token = identity.AuthenticationToken;
                gid = identity.CompanyGroupId;
                authCookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Response.Cookies.Add(authCookie);
                HttpContext.Application["BasicTicket" + identity.Name] = null;
                HttpContext.Application["RoleTicket" + identity.Name] = null;
            }
            switch (road)
            {
                case "cpanel":
                    return Redirect("/#cpanel");

                case "apanel":
                    return Redirect("/#/1/" + token + "/" + gid);

                case "mpanel":
                    return Redirect("/#/2/" + token + "/" + gid);

                case "upanel":
                    return Redirect("/#/3/" + token + "/" + gid);

                default:
                    return Redirect("/#/");
            }
        }

        [HttpGet]
        public ActionResult PasswordChange()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PasswordChange(string id, string password, string url)
        {
            try
            {
                _passwordChangeService.AddAndUpdate(id, password, false);
                return Json(new { Url = url, JsonRequestBehavior.AllowGet });
            }
            catch (Exception ex)
            {
                return Json(new { ex.Message, JsonRequestBehavior.AllowGet });
            }
        }

        private bool UserValidate(string servicepanel, string authenticationToken, string groupId)
        {
            string[] userData = _userService.CheckUserAuthenticationToken(authenticationToken);
            if (Convert.ToBoolean(userData[5]) &&
                servicepanel == PanelConst.cPanel)
                return false;
            if (Convert.ToBoolean(userData[6]) && (servicepanel == PanelConst.cPanel || servicepanel == PanelConst.aPanel))
                return false;
            if (!Convert.ToBoolean(userData[5]) && !Convert.ToBoolean(userData[6]) &&
               (servicepanel == PanelConst.cPanel ||
                servicepanel == PanelConst.aPanel ||
                servicepanel == PanelConst.mPanel))
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