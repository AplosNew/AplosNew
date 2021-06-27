using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.ManagementChartOfAccounts;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CompanyGroupActivityController : BaseController
    {
        #region Constructor

        private readonly ICompanyGroupActivityService _companyGroupActivityService;

        public CompanyGroupActivityController(
            ICompanyGroupActivityService companyGroupActivityService)
        {
            _companyGroupActivityService = companyGroupActivityService;
        }

        #endregion Constructor

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupActivityService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetForBudgetMasterPopUp(GridParameter parameters, string budgetMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupActivityService.GetForBudgetMasterPopUp(parameters, identity.CompanyGroupId, budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo(string companyGroupId)
        {
            return Json(_companyGroupActivityService.GetCbo(companyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}