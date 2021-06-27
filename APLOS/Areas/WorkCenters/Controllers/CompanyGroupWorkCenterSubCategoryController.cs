using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.WorkCenters;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.WorkCenters.Controllers
{
    public class CompanyGroupWorkCenterSubCategoryController : BaseController
    {
        #region Constructor

        private readonly ICompanyGroupWorkCenterSubCategoryService _companyGroupWorkCenterSubCategoryService;

        public CompanyGroupWorkCenterSubCategoryController(
            ICompanyGroupWorkCenterSubCategoryService companyGroupWorkCenterSubCategoryService)
        {
            _companyGroupWorkCenterSubCategoryService = companyGroupWorkCenterSubCategoryService;
        }

        #endregion Constructor

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupWorkCenterSubCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_companyGroupWorkCenterSubCategoryService.GetCbo(), JsonRequestBehavior.AllowGet);
        }
    }
}