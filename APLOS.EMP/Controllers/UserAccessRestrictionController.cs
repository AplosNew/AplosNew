using Library.Model.External;
using Library.Service.External;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Core;

namespace Aplos.Controllers
{
    public class UserAccessRestrictionController : Controller
    {
        #region Constructor
        private readonly IEmployeeService _employeeService;
        public UserAccessRestrictionController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }
        #endregion

        public ActionResult Aplos()
        {
            ViewBag.ControllerName = "UserAccessRestrictionController";
            return View();
        }
        [HttpGet, AllowAnonymous]
        public ActionResult GetList(GridParameter parameters, string companyGroupId)
        {
            return Json(_employeeService.GetEmployeeDataForRestriction(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Update(IEnumerable<Employee> list)
        {
            _employeeService.UpdateAccessRestriction(list);
            return Json(new { Message = "Save changes successfully." });
        }
    }
}