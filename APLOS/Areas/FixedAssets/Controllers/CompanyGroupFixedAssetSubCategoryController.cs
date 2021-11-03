using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.FixedAssets;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class CompanyGroupFixedAssetSubCategoryController : BaseController
    {
        private readonly ICompanyGroupFixedAssetSubCategoryService _companyGroupFixedAssetSubCategoryService;

        public CompanyGroupFixedAssetSubCategoryController(
            ICompanyGroupFixedAssetSubCategoryService companyGroupFixedAssetSubCategoryService)
        {
            _companyGroupFixedAssetSubCategoryService = companyGroupFixedAssetSubCategoryService;
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupFixedAssetSubCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}