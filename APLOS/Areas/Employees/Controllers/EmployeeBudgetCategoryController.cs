#region Using
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Employees;
using Library.Service.Employees;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeBudgetCategoryController : Controller
    {
        #region Constructor
        private readonly IEmployeeBudgetCategoryService _employeeBudgetCategoryService;
        private readonly ICompanyGroupEmployeeBudgetCategoryService _companyGroupEmployeeBudgetCategoryService;

        public EmployeeBudgetCategoryController(
            IEmployeeBudgetCategoryService employeeBudgetCategoryService,
            ICompanyGroupEmployeeBudgetCategoryService companyGroupEmployeeBudgetCategoryService)
        {
            _employeeBudgetCategoryService = employeeBudgetCategoryService;
            _companyGroupEmployeeBudgetCategoryService = companyGroupEmployeeBudgetCategoryService;
        }
        #endregion
        
        #region dll
        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_employeeBudgetCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_employeeBudgetCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupEmployeeBudgetCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(EmployeeBudgetCategory employeeBudgetCategory)
        {
            _employeeBudgetCategoryService.Insert(employeeBudgetCategory);
            return Json(new { EmployeeBudgetCategory = employeeBudgetCategory, Sequence = _employeeBudgetCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(EmployeeBudgetCategory employeeBudgetCategory)
        {
            _employeeBudgetCategoryService.Update(employeeBudgetCategory);
            return Json(new { EmployeeBudgetCategory = employeeBudgetCategory, Sequence = _employeeBudgetCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }
        [HttpPost]
        public JsonResult Delete(string id)
        {
            _employeeBudgetCategoryService.Archive(id);
            return Json(new { Sequence = _employeeBudgetCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}