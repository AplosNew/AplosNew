using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.ManagementChartOfAccounts;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CompanyGroupBudgetClassController : BaseController
    {
        #region Constructor

        private readonly ICompanyGroupBudgetClassService _companyGroupBudgetClassService;

        public CompanyGroupBudgetClassController(
            ICompanyGroupBudgetClassService companyGroupBudgetClassService)
        {
            _companyGroupBudgetClassService = companyGroupBudgetClassService;
        }

        #endregion Constructor

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupBudgetClassService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}