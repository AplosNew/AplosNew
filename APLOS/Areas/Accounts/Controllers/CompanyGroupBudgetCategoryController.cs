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
        public ActionResult GetList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(_companyGroupBudgetCategoryService.Query(column, value, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
    }
}