using Library.Model.External;
using Library.Service.External;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class EmployeeLinkController : BaseController
    {
        private readonly IEmployeeLinkService _employeeLinkService;
        public EmployeeLinkController(IEmployeeLinkService employeeLinkService)
        {
            _employeeLinkService = employeeLinkService;
        }
        public ActionResult Aplos()
        {
            ViewBag.ControllerName = "EmployeeLinkController";
            return View();
        }
        [HttpPost]
        public JsonResult EmpEmailSend(EmployeeLink empLink, IEnumerable<Employee> employeeList)
        {
            _employeeLinkService.EmployeeLinkSend(empLink, employeeList);
            return Json(new { Message = "Send" });
        }
    }
}