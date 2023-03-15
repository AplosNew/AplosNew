#region Using

using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Logs;
using Library.Model.Setups;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

#endregion Using

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class EditControlController : BaseController
    {
		#region Constructor
		private readonly IEmployeeMobileAppsAuthorizationService _empAuthService;
		private readonly IAccessLogService _accessLogService;
		private readonly ISqlRepository _sqlRepository;
        public EditControlController(IEmployeeMobileAppsAuthorizationService empAuthService, IAccessLogService accessLogService, ISqlRepository R)
        {
			_empAuthService = empAuthService;
			_accessLogService = accessLogService;
			_sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult EditCtrl()
        {
            return View();
        }

		[HttpPost, AllowAnonymous]
		public ActionResult Login(string timezoneoffset, string employeeId, string password, string remember)
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
			var _result = _empAuthService.Login(employeeId, password);
			if (_result["Status"].ToString() == "Success")
				SetAuthentication(employeeId, _result, isRemember, ip);
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
				Panel = PanelEnum.Portal.ToString(),
				RegionCode = Convert.ToString(location.region_code),
				RegionName = Convert.ToString(location.region_name),
				Resistered = _result["Status"].ToString() == "Success",
				ScreenSize = null,
				Status = _result["Status"].ToString() == "Success",
				TimeZone = Convert.ToString(location.time_zone),
				UserAgent = HttpContext.Request.UserAgent,
				UserId = null,
				EmployeeId = employeeId,
				WorkStationIP = ip,
				WorkStationName = AccessInfo.GetWorkstationName(ip),
				ZipCode = Convert.ToString(location.zip_code)
			});

			Dictionary<string, object> empProfile = new Dictionary<string, object>();
			empProfile = _sqlRepository.GetDataCollection("select * from employeeinformation where systemid='" + employeeId + "'")[0];

			return Json(new { result = _result, profile = empProfile }, JsonRequestBehavior.AllowGet);
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

	}
}