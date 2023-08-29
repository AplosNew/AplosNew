using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.ManagementChartOfAccounts;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CompanyGroupBudgetGroupController : BaseController
    {
        #region Constructor

        private readonly ICompanyGroupBudgetGroupService _companyGroupBudgetGroupService;

        public CompanyGroupBudgetGroupController(
            ICompanyGroupBudgetGroupService companyGroupBudgetGroupService)
        {
            _companyGroupBudgetGroupService = companyGroupBudgetGroupService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo(string companyGroupId)
        {
            return Json(new SelectList(_companyGroupBudgetGroupService.GetCbo(companyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public ActionResult GetList(GridParameter parameters)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_companyGroupBudgetGroupService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public ActionResult GetList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(_companyGroupBudgetGroupService.Query(column, value, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

    }
}