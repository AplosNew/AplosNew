#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Organizations;
using Library.Model.Setups;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class DepartmentController : BaseController
    {
        #region Constructor

        private readonly IDepartmentService _departmentService;
        private readonly ICompanyGroupDepartmentService _companyGroupDepartmentService;
        private readonly ICompanyDepartmentService _companyDepartmentService;

        public DepartmentController(
            IDepartmentService departmentService
            , ICompanyGroupDepartmentService companyGroupDepartmentService
            , ICompanyDepartmentService companyDepartmentService)
        {
            _departmentService = departmentService;
            _companyDepartmentService = companyDepartmentService;
            _companyGroupDepartmentService = companyGroupDepartmentService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_departmentService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            return Json(_companyGroupDepartmentService.GetCboByCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompany(string companyId)
        {
            return Json(_companyDepartmentService.GetCboByCompany(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupDepartmentService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListDepartmentWithCompnay(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupDepartmentService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Edit(Department department, IEnumerable<LocalLanguage> localLanguages)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _departmentService.Update(department, localLanguages, identity.CompanyGroupId);
            return Json(new { Sequence = _departmentService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _departmentService.Delete(id);
                return Json(new { Sequence = _departmentService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [HttpPost]
        public JsonResult Create(Department department, IEnumerable<LocalLanguage> localLanguages)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _departmentService.Insert(department, localLanguages, identity.CompanyGroupId);
            return Json(new { Department = department, Sequence = _departmentService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult Get(string id)
        {
            return Json(_departmentService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_departmentService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
    }
}