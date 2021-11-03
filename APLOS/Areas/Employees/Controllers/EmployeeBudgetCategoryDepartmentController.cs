#region Using
using Library.Core;
using Aplos.Properties;
using Aplos.Controllers;
using System.Web.Mvc;
using System.Collections.Generic;
using Library.Service.Employees;
using Library.Model.Employees;
#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeBudgetCategoryDepartmentController : BaseController
    {
        #region Constructor
        private readonly IEmployeeBudgetCategoryDepartmentService _employeeBudgetCategoryDepartmentService;
        public EmployeeBudgetCategoryDepartmentController(IEmployeeBudgetCategoryDepartmentService employeeBudgetCategoryDepartmentService)
        {
            _employeeBudgetCategoryDepartmentService = employeeBudgetCategoryDepartmentService;
        }
        #endregion

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<EmployeeBudgetCategoryDepartment> employeeBudgetCategoryDepartment)
        {
            _employeeBudgetCategoryDepartmentService.InsertOrUpdateGraph(employeeBudgetCategoryDepartment);
            return Json(new { EmployeeBudgetCategoryDepartment = employeeBudgetCategoryDepartment, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _employeeBudgetCategoryDepartmentService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithDepartment(GridParameter parameters,string departmentId)
        {
            return Json(_employeeBudgetCategoryDepartmentService.QueryWithDepartment(parameters, departmentId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetDepartmentWithCompanyGroupList(GridParameter parameters)
        {
            return Json(_employeeBudgetCategoryDepartmentService.QueryDepartmentWithCompany(parameters), JsonRequestBehavior.AllowGet);
        }
    }
}