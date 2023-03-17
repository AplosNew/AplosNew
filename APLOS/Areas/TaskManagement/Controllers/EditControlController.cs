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
using Library.Service.Properties;
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
		public ActionResult Login(string userId, string password, string remember)
		{
			var http = System.Web.HttpContext.Current;
			var isRemember = !string.IsNullOrWhiteSpace(remember) && remember == "on";
			var ip = AccessInfo.GetWorkstationIP(http);
			var _result = UserEdilControlLogin(userId, password);
			
			return Json(new { result = _result}, JsonRequestBehavior.AllowGet);
		}

		public Dictionary<string, object> UserEdilControlLogin(string userId, string password)
		{
			var sql = @"SELECT * FROM UserEditControl WHERE UserId='" + userId + "' AND Password='" + password + "'";
			
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
	}
}