using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.ManagementChartOfAccounts;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CompanyGroupBudgetCategoryController : BaseController
    {
        #region Constructor

        private readonly ICompanyGroupBudgetCategoryService _companyGroupBudgetCategoryService;

        public CompanyGroupBudgetCategoryController(
            ICompanyGroupBudgetCategoryService companyGroupBudgetCategoryService)
        {
            _companyGroupBudgetCategoryService = companyGroupBudgetCategoryService;
        }

        #endregion Constructor

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupBudgetCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}