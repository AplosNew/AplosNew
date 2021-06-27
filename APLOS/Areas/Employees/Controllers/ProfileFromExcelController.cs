using Aplos.Controllers;
using Library.Model.Employees;
using Library.Service.Employees;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class ProfileFromExcelController : BaseController
	{
        private readonly IEmployeeInformationService _employeeInformationService;
        public ProfileFromExcelController(
             IEmployeeInformationService employeeInformationService
           )
        {
            _employeeInformationService = employeeInformationService;
        }

        [Authorize]
        public ActionResult Aplos()
		{
			return View();
		}

        public JsonResult Save(List<EmployeeInformation> employeeList)
        {

           // _employeeInformationService.Insert(employeeList);
            return Json(new { Message = "Data Uploaded Successfully" });
        }

    }
}