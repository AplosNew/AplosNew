using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.WorkCenters;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.WorkCenters.Controllers
{
    public class CompanyGroupWorkCenterCategoryController : BaseController
    {
        #region Constructor

        private readonly ICompanyGroupWorkCenterCategoryService _companyGroupWorkCenterCategoryService;

        public CompanyGroupWorkCenterCategoryController(
            ICompanyGroupWorkCenterCategoryService companyGroupWorkCenterCategoryService)
        {
            _companyGroupWorkCenterCategoryService = companyGroupWorkCenterCategoryService;
        }

        #endregion Constructor

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupWorkCenterCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_companyGroupWorkCenterCategoryService.GetCbo(), JsonRequestBehavior.AllowGet);
        }
    }
}