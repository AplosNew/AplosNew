using Library.Model.External;
using Library.Service.External;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Controllers
{
	public class EmployeeProfileFromExcelController : BaseController
	{
		private readonly IEmployeeProfileFromExcelService _employeeProfileFromExcelService;
		public EmployeeProfileFromExcelController(IEmployeeProfileFromExcelService employeeProfileFromExcelService)
		{
			_employeeProfileFromExcelService = employeeProfileFromExcelService;
		}
		public ActionResult Aplos()
		{
			ViewBag.ControllerName = "employeeProfileFromExcelController";
			return View();
		}
		
		public JsonResult Save(List<EmployeeProfileFromExcel> employeeList)
		{

			_employeeProfileFromExcelService.Insert(employeeList);
			return Json(new { Message = "Data Uploaded Successfully" });
		}

	}
}