using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.ManagementChartOfAccounts;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CompanyGroupBudgetController : BaseController
    {
        #region Constructor

        private readonly ICompanyGroupBudgetService _companyGroupBudgetService;

        public CompanyGroupBudgetController(
            ICompanyGroupBudgetService companyGroupBudgetService)
        {
            _companyGroupBudgetService = companyGroupBudgetService;
        }

        #endregion Constructor

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupBudgetService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}