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
        public ActionResult GetList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(_companyGroupBudgetSubCategoryService.Query(column, value, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
    }
}