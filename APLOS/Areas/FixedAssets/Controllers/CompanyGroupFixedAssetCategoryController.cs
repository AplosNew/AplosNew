using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.FixedAssets;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class CompanyGroupFixedAssetCategoryController : BaseController
    {
        private readonly ICompanyGroupFixedAssetCategoryService _companyGroupFixedAssetCategoryService;

        public CompanyGroupFixedAssetCategoryController(
            ICompanyGroupFixedAssetCategoryService companyGroupFixedAssetCategoryService)
        {
            _companyGroupFixedAssetCategoryService = companyGroupFixedAssetCategoryService;
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupFixedAssetCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}