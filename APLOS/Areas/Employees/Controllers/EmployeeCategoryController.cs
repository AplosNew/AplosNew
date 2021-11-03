using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Employees;
using Library.Model.Setups;
using Library.Service.Employees;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeCategoryController : Controller
    {
        #region Constructor

        private readonly IEmployeeCategoryService _employeeCategoryService;
        private readonly ICompanyGroupEmployeeCategoryService _companyGroupEmployeeCategoryService;

        public EmployeeCategoryController(
            IEmployeeCategoryService employeeCategoryService,
            ICompanyGroupEmployeeCategoryService companyGroupEmployeeCategoryService
            )
        {
            _employeeCategoryService = employeeCategoryService;
            _companyGroupEmployeeCategoryService = companyGroupEmployeeCategoryService;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_employeeCategoryService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_employeeCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupEmployeeCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(EmployeeCategory EmployeeCategory, IEnumerable<LocalLanguage> localLanguages)
        {
            _employeeCategoryService.Insert(EmployeeCategory, localLanguages);
            return Json(new { EmployeeCategory, Sequence = _employeeCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(EmployeeCategory EmployeeCategory, IEnumerable<LocalLanguage> localLanguages)
        {
            _employeeCategoryService.Update(EmployeeCategory, localLanguages);
            return Json(new { EmployeeCategory, Sequence = _employeeCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _employeeCategoryService.Archive(id);
            return Json(new { Sequence = _employeeCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}