using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeGroupController : Controller
    {
        #region Constructor
        private readonly IEmployeeGroupService _employeeGroupService;
        public EmployeeGroupController(IEmployeeGroupService employeeGroupService)
        {
            _employeeGroupService = employeeGroupService;
        }
        #endregion

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_employeeGroupService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            return Json(_employeeGroupService.GetCboByCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_employeeGroupService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult EmployeeGroupSearch(GridParameter parameters)
        {
            return Json(_employeeGroupService.EmployeeGroupSearch(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_employeeGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(EmployeeGroup product)
        {
            _employeeGroupService.Insert(product);
            return Json(new { EmployeeGroup = product, Sequence = _employeeGroupService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(EmployeeGroup product)
        {
            _employeeGroupService.Update(product);
            return Json(new { Sequence = _employeeGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _employeeGroupService.Delete(id);
            return Json(new { Sequence = _employeeGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}