#region Using

using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.Employees;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class CompanyGroupEmployeeCategoryController : BaseController
    {
        #region Constructor

        private readonly ICompanyGroupEmployeeCategoryService _companyGroupEmployeeCategoryService;

        public CompanyGroupEmployeeCategoryController(
            ICompanyGroupEmployeeCategoryService companyGroupEmployeeCategoryService)
        {
            _companyGroupEmployeeCategoryService = companyGroupEmployeeCategoryService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupEmployeeCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo(string companyGroupId)
        {
            return Json(new SelectList(_companyGroupEmployeeCategoryService.GetCboList(companyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
    }
}