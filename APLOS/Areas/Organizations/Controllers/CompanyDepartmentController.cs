#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class CompanyDepartmentController : BaseController
    {
        #region Constructor

        private readonly ICompanyDepartmentService _companyDepartmentService;

        public CompanyDepartmentController(ICompanyDepartmentService companyDepartmentService)
        {
            _companyDepartmentService = companyDepartmentService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCboList(string companyId)
        {
            return Json(new SelectList(_companyDepartmentService.GetCboByCompany(companyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CompanyDepartment> companyDepartment)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            foreach (var item in companyDepartment)
            {
                item.CompanyGroupId = identity.CompanyGroupId;
            }
            _companyDepartmentService.InsertRange(companyDepartment);
            return Json(new { CompanyDepartment = companyDepartment, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _companyDepartmentService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCompany(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyDepartmentService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }
    }
}