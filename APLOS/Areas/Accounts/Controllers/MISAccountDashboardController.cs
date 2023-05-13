#region Using

using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Service.Accounts;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Accounts.Controllers
{
    public class MISAccountDashboardController : BaseController
    {
        private readonly IMISAccountDashboardService _misAccountDashboardService;

        public MISAccountDashboardController(IMISAccountDashboardService misAccountDashboardService)
        {
            _misAccountDashboardService = misAccountDashboardService;
        }

        
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetVoucherLatestDate(string dateType, string itemType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetVoucherLatestDate(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, dateType, itemType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult MISBudgetCategoryCbo(string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.MISBudgetCategoryCbo(identity.CompanyGroupId, companyId, plantId, divisionId, subDivisionId, unitId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetWiseAmountListElastic(string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string budgetMasterId, string fromDate, string toDate, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetBudgetWiseAmountListElastic(identity.CompanyGroupId, companyId, plantId, divisionId, subDivisionId, unitId, budgetMasterId, Activity, fromDate, toDate, voucherId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetBudgetWisevarianceElastic(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string fromDate, string toDate, string bType, string dateType)
        {
            return Json(_misAccountDashboardService.GetBudgetWisevarianceElastic(companyGroupId, companyId, plantId, divisionId, subDivisionId, unitId, budgetCategory, budgetSubCategory, budget, Activity, fromDate, toDate, bType, dateType), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult GetBudgetCategoryWisevarianceElastic(string parameterString, string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string fromDate, string toDate, string bType, string dateType)
        {
            return Json(_misAccountDashboardService.GetBudgetCategoryWisevarianceElastic(parameterString, companyGroupId, companyId, plantId, divisionId, subDivisionId, unitId, budgetCategory, budgetSubCategory, budget, Activity, fromDate, toDate, bType, dateType), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetBudgetSubCategoryWisevarianceElastic(string parameterString, string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string fromDate, string toDate, string bType, string dateType)
        {
            return Json(_misAccountDashboardService.GetBudgetSubCategoryWisevarianceElastic(parameterString, companyGroupId, companyId, plantId, divisionId, subDivisionId, unitId, budgetCategory, budgetSubCategory, budget, Activity, fromDate, toDate, bType, dateType), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetBudgetItemWisevarianceElastic(string parameterString, string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string fromDate, string toDate, string bType, string dateType)
        {
            return Json(_misAccountDashboardService.GetBudgetItemWisevarianceElastic(parameterString, companyGroupId, companyId, plantId, divisionId, subDivisionId, unitId, budgetCategory, budgetSubCategory, budget, Activity, fromDate, toDate, bType, dateType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterWiseAmountElastic(string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string budgetMasterId, string fromDate, string toDate, string dayOrPeriod, string dateType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetBudgetMasterWiseAmountElastic(identity.CompanyGroupId, companyId, plantId, divisionId, subDivisionId, unitId, budgetCategory, budgetSubCategory, budget, Activity, budgetMasterId, fromDate, toDate, dayOrPeriod, dateType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetActivityWisevarianceElastic(string divisionId, string subDivisionId, string unitId, string fromDate, string toDate, string bType, string[] budgetMasterId, string budgetCategoryId, string dateType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetActivityWisevarianceElastic(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, divisionId, subDivisionId, unitId, fromDate, toDate, bType, budgetMasterId, budgetCategoryId, dateType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterWiseAmount(string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string budgetMasterId, string fromDate, string toDate, string periodName, string dateType, string dayOrPeriod, string PostingPeriodId, string EntryPeriodId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetBudgetMasterWiseAmount(identity.CompanyGroupId, companyId, plantId, divisionId, subDivisionId, unitId, budgetCategory, budgetSubCategory, budget, Activity, budgetMasterId, fromDate, toDate, periodName, dateType, dayOrPeriod, PostingPeriodId, EntryPeriodId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterWiseExceptionAmount(string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string[] budgetMasterId, string fromDate, string toDate, string periodName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetBudgetMasterWiseExceptionAmount(identity.CompanyGroupId, companyId, plantId, divisionId, subDivisionId, unitId, budgetCategory, budgetSubCategory, budget, Activity, budgetMasterId, fromDate, toDate, periodName), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterWiseExceptionAmountDetail(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string budgetMasterId, string fromDate, string toDate, string periodName)
        {
            return Json(_misAccountDashboardService.GetBudgetMasterWiseExceptionAmountDetail(companyGroupId, companyId, plantId, divisionId, subDivisionId, unitId, budgetCategory, budgetSubCategory, budget, Activity, budgetMasterId, fromDate, toDate, periodName), JsonRequestBehavior.AllowGet);
        }

        #region Independent Entity Combo  and List

        [HttpGet, Authorize]
        public JsonResult GetEntityWisePlantCbo(string compnayGroupId, string companyId, string plantId)
        {
            return Json(_misAccountDashboardService.GetEntityWisePlantCbo(compnayGroupId, companyId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityDetailFromPlantSelection(string plantId)
        {
            return Json(_misAccountDashboardService.GetEntityDetailFromPlantSelection(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityDetailFromCompanySelection(string companyId)
        {
            return Json(_misAccountDashboardService.GetEntityDetailFromCompanySelection(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityWiseEntityCbo(string[] entityList, string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(data: _misAccountDashboardService.GetEntityWiseEntityCbo(entityList, companyGroupId, companyId, plantId, divisionId, subDivisionId, unitId), behavior: JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityDetailFromEntitySelection(string entityId)
        {
            return Json(_misAccountDashboardService.GetEntityDetailFromEntitySelection(entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityWiseDivisionCbo(string compnayGroupId, string companyId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetEntityWiseDivisionCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityDetailFrDivisionCbo(string compnayGroupId, string companyId, string plantId, string entityId, string divisionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetEntityDetailFromDivisionCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, entityId, divisionId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityWiseSubDivisionCbo(string compnayGroupId, string companyId, string plantId, string divisionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetEntityWiseSubDivisionCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, divisionId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityDetailFromSubDivisionCbo(string compnayGroupId, string companyId, string plantId, string entityId, string divisionId, string subDivisionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetEntityDetailFromSubDivisionCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, entityId, divisionId, subDivisionId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityWiseUnitCbo(string entityId, string divisionId, string subDivisionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetEntityWiseUnitCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, entityId, divisionId, subDivisionId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityDetailFromUnitCbo(string entityId, string divisionId, string subDivisionId, string unitId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetEntityDetailFromUnitCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, entityId, divisionId, subDivisionId, unitId), JsonRequestBehavior.AllowGet);
        }

        #endregion Independent Entity Combo  and List

        #region Balance Sheet Tree View
        [HttpPost]
        public JsonResult GetBalanceSheetInfoGLLevel(string parameterString, string date, string GLGeneralInfoId, string BudgetMasterId, string ActivityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetBalanceSheetInfoGLLevel( parameterString, identity.CompanyGroupId, identity.CompanyId, identity.PlantId,  date, GLGeneralInfoId,  BudgetMasterId,  ActivityId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetBalanceSheetInfoBudgetLevel(string parameterString, string date, string GLGeneralInfoId, string BudgetMasterId, string ActivityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetBalanceSheetInfoBudgetLevel(parameterString, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, date, GLGeneralInfoId, BudgetMasterId, ActivityId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetBalanceSheetInfoActivityLevel(string parameterString, string date, string GLGeneralInfoId, string BudgetMasterId, string ActivityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_misAccountDashboardService.GetBalanceSheetInfoActivityLevel(parameterString, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, date, GLGeneralInfoId, BudgetMasterId, ActivityId), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}