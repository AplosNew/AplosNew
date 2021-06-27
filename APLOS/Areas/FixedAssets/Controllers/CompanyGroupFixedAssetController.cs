using Aplos.Controllers;
using Aplos.Service.FixedAssets;
using Library.Core;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class CompanyGroupFixedAssetController : BaseController
    {
        #region Constractor
        private readonly ICompanyGroupFixedAssetService _companyGroupFixedAssetService;
        public CompanyGroupFixedAssetController(
            ICompanyGroupFixedAssetService companyGroupFixedAssetService)
        {
            _companyGroupFixedAssetService = companyGroupFixedAssetService;
        }
        #endregion

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupFixedAssetService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}