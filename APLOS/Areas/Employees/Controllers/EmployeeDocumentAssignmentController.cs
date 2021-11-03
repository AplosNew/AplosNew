#region Using

using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
	public class EmployeeDocumentAssignmentController : BaseController
	{
		#region Constructor

		private readonly IEmployeeDocumentAssignmentService _employeeDocumentAssignmentService;

		public EmployeeDocumentAssignmentController(
			  IEmployeeDocumentAssignmentService employeeDocumentAssignmentService
			)
		{
			_employeeDocumentAssignmentService = employeeDocumentAssignmentService;
		}

		#endregion Constructor

		#region -- Pages

		public ActionResult Aplos()
		{
			return View();
		}

		#endregion -- Pages

		#region -- Operations

		[HttpGet, Authorize]
		public ActionResult GetList(GridParameter parameters, string assign, string plantId)
		{
			return Json(_employeeDocumentAssignmentService.GetEmployeeData(parameters, assign, plantId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public ActionResult GetDocumentDataList(string empId)
		{
			return Json(_employeeDocumentAssignmentService.GetDocumentDataList(empId), JsonRequestBehavior.AllowGet);
		}

		//[HttpPost]
		//public JsonResult Create(IEnumerable<EmployeeInformation> employeeInformation)
		//{
		//	_employeeDocumentAssignmentService.InsertORUpdateMaster(employeeInformation);
		//	return Json(new { Message = AplosMessage.Insert });
		//}
		[HttpPost]
		public JsonResult Create(string employeeInformation)
		{
			var settings = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				MissingMemberHandling = MissingMemberHandling.Ignore
			};
			List<EmployeeInformation> employee = JsonConvert.DeserializeObject<List<EmployeeInformation>>(employeeInformation, settings);

			_employeeDocumentAssignmentService.InsertORUpdateMaster(employee);
			return Json(new { Message = AplosMessage.Insert });
		}

		#endregion -- Operations
	}
}