using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.ManagementChartOfAccounts;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CompanyGroupBudgetSubCategoryController : BaseController
    {
        #region Constructor

        private readonly ICompanyGroupBudgetSubCategoryService _companyGroupBudgetSubCategoryService;

        public CompanyGroupBudgetSubCategoryController(
            ICompanyGroupBudgetSubCategoryService companyGroupBudgetSubCategoryService)
        {
            _companyGroupBudgetSubCategoryService = companyGroupBudgetSubCategoryService;
        }

        #endregion Constructor

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupBudgetSubCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}