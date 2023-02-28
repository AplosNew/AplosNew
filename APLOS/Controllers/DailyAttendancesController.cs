using Aplos.Helpers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Logs;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Securites;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Aplos.Controllers
{
	public class DailyAttendancesController : BaseController
	{
		private readonly IEmployeeMobileAppsAuthorizationService _empAuthService;
		private readonly IPasswordChangeService _passwordChangeService;
		private readonly IAccessLogService _accessLogService;
		private readonly ISqlRepository _sqlRepository;

		public DailyAttendancesController(
			IEmployeeMobileAppsAuthorizationService empAuthService
			, IPasswordChangeService passwordChangeService
			, IAccessLogService accessLogService
			, ISqlRepository R)
		{
			_passwordChangeService = passwordChangeService;
			_empAuthService = empAuthService;
			_accessLogService = accessLogService;
			_sqlRepository = R;
		}

		[HttpGet, Authorize]
		public ActionResult Calendar()
		{
			return View();
		}

		[HttpGet]
		public ActionResult Aplos()
		{
			return View();
		}

		[HttpGet, Authorize]
		public ActionResult DALayout()
		{
#if DEBUG
			ViewBag.BasePath = "/dapanel";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (string.IsNullOrEmpty(appName))
                ViewBag.BasePath = "/dapanel";
            else
                ViewBag.BasePath = "/" + appName + "/dapanel";
#endif
			try
			{

				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				ViewBag.EmployeeId = identity.EmployeeId;
				ViewBag.FullName = identity.FullName;
				var accessLog = _accessLogService.GetEmployeeLastLogin(identity.EmployeeId, PanelEnum.Portal.ToString());
				if (accessLog == null) return View();
				var offset = int.Parse(Session["timezoneoffset"].ToString());
				var requestDateTime = accessLog.AccessTime.AddMinutes(-1 * offset);
				var lastLoginTime = requestDateTime.ToString("dd-MMM-yyyy hh:mm tt");
				if (!string.IsNullOrEmpty(accessLog.CountryName))
					lastLoginTime += " (" + accessLog.CountryName + ")";
				ViewBag.LastLoginTime = lastLoginTime;

				Dictionary<string, object> empProfile = new Dictionary<string, object>();
				empProfile = _sqlRepository.GetDataCollection("select * from EmployeeInformation where EmployeeCode='" + ViewBag.EmployeeId + "'")[0];

				//ViewBag.EmpPicPath = empProfile["EmpPicPath"].ToString();

				return View();
			}
			catch (Exception)
			{
				return RedirectToAction("Portal");
			}
		}

		[HttpGet, Authorize]
		public ActionResult Dashboard()
		{
			return View();
		}

		[HttpGet, AllowAnonymous]
		public ActionResult Login()
		{
			return View();
		}

		[HttpPost, AllowAnonymous]
		public ActionResult Login(string timezoneoffset, string employeeId, string remember)
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
			var _result = ParentsLogin(employeeId);
			if (_result["Status"].ToString() == "Success")
				SetAuthentication(employeeId, _result, isRemember, ip);
			//_accessLogService.Insert(new AccessLog
			//{
			//	AccessTime = DateTime.UtcNow,
			//	AccessTimeWithCountry = timezoneoffset,
			//	Browser = AccessInfo.GetBrowserName(http),
			//	City = Convert.ToString(location.city),
			//	CompanyGroupId = null,
			//	CountryCode = Convert.ToString(location.country_code),
			//	CountryName = Convert.ToString(location.country_name),
			//	DaylightName = null,
			//	DeviceType = null,
			//	Dstoffset = http.Request.Browser.Platform,
			//	Gmtoffset = null,
			//	IsCookieEnable = http.Request.Browser.Cookies,
			//	IsJavascriptEnable = http.Request.Browser.VBScript,
			//	Latitude = Convert.ToString(location.latitude),
			//	Longitude = Convert.ToString(location.longitude),
			//	OS = AccessInfo.GetOS(http),
			//	Panel = PanelEnum.Portal.ToString(),
			//	RegionCode = Convert.ToString(location.region_code),
			//	RegionName = Convert.ToString(location.region_name),
			//	Resistered = _result["Status"].ToString() == "Success",
			//	ScreenSize = null,
			//	Status = _result["Status"].ToString() == "Success",
			//	TimeZone = Convert.ToString(location.time_zone),
			//	UserAgent = HttpContext.Request.UserAgent,
			//	UserId = null,
			//	EmployeeId = employeeId,
			//	WorkStationIP = ip,
			//	WorkStationName = AccessInfo.GetWorkstationName(ip),
			//	ZipCode = Convert.ToString(location.zip_code)
			//});

			Dictionary<string, object> empProfile = new Dictionary<string, object>();
			empProfile = _sqlRepository.GetDataCollection("select * from EmployeeInformation Where EmployeeCode='" + employeeId + "'")[0];

			return Json(new { result = _result, profile = empProfile }, JsonRequestBehavior.AllowGet);
		}

		public Dictionary<string, object> ParentsLogin(string employeeId)
		{
			var sql = @"Select SystemId EmployeeId,EmployeeName,GroupId CompanyGroupId,''CompanyGroupName,CompanyId,''CompanyName,PlantId,''PlantName from EmployeeInformation Where EmployeeCode='"+ employeeId + "'";
			var result = _sqlRepository.GetData(sql);
			if (result.Count() > 0 && result != null)
			{
				// Retun result is ok.
				result.Add("Status", "Success");
			}
			else
			{
				result.Add("Status", "Fail");
				result.Add("ErrorText", ResourcesCore.LoginFailError);
			}
			// Return final result.
			return result;
		}

		[HttpPost, AllowAnonymous]
		public ActionResult LoginUerInfo(string employeeId)
		{

			Dictionary<string, object> empProfile = new Dictionary<string, object>();
			empProfile = _sqlRepository.GetDataCollection("select * from EmployeeInformation Where EmployeeCode='" + employeeId + "'")[0];

			return Json(new { profile = empProfile }, JsonRequestBehavior.AllowGet);
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

		private void SetAuthentication(string userId, Dictionary<string, object> result, bool isRemember, string ip)
		{
			var basicTicket = CustomIdentity.CreateBasicTicket(
				result["EmployeeId"].ToString(), result["EmployeeId"].ToString(), result["EmployeeName"].ToString()
				, result["CompanyGroupId"].ToString(), result["CompanyGroupName"].ToString()
				, result["CompanyId"].ToString(), result["CompanyName"].ToString()
				, result["PlantId"].ToString(), result["PlantName"].ToString()
				, result["EmployeeId"].ToString()
				, ip, false, false, false, null, null);
			var roleTicket = CustomIdentity.CreateRoleTicket(new[] { "ControlAdmin, SysAdmin, Admin, NoAction" });
			var timeOut = Convert.ToInt32(new AppSettingsReader().GetValue("COOKIE_TIMEOUT", typeof(string)));
			var authTicket = new FormsAuthenticationTicket(1, FormsAuthentication.FormsCookieName, DateTime.Now, DateTime.Now.AddMinutes(timeOut), isRemember, basicTicket);
			var encTicket = FormsAuthentication.Encrypt(authTicket);
			HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
			HttpContext.Response.Cookies.Add(new HttpCookie("ROOT_FOLDRR", ResourcesPathReader.GetROOT_FOLDER()));
			HttpContext.Application["BasicTicket" + userId] = basicTicket;
			HttpContext.Application["RoleTicket" + userId] = roleTicket;
			HttpContext.Session["Panel"] = PanelEnum.Portal.ToString();
		}

		[HttpGet, Authorize]
		public JsonResult GetForPasswordChange(string id)
		{
			var data = _empAuthService.Query(t => t.EmployeeId == id).Select().FirstOrDefault();
			return Json(data, JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public ActionResult PasswordChange()
		{
			return View();
		}

		[HttpPost, Authorize]
		public JsonResult PasswordChange(string id, string password)
		{
			_passwordChangeService.UpdateEmployeePin(id, password);
			return Json(new { Message = AplosMessage.Updated });
		}
	}
}