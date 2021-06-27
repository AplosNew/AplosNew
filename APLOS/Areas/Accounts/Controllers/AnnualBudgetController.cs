#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.ManagementChartOfAccounts;
using Library.Service.ManagementChartOfAccounts;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Accounts.Controllers
{
    public class AnnualBudgetController : BaseController
    {
        #region Constructor

        private readonly IAnnualBudgetService _annualBudgetService;

        public AnnualBudgetController(IAnnualBudgetService annualBudgetService)
        {
            _annualBudgetService = annualBudgetService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCboRoutineBudget(string entityId, string fiscalYearId)
        {
            return Json(_annualBudgetService.GetCboAnnualBudget(entityId, fiscalYearId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByGL(string companyId, string glId)
        {
            return Json(_annualBudgetService.GetCboByGL(companyId, glId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboActivityByBudget(string companyId, string budgetId)
        {
            return Json(_annualBudgetService.GetCboActivityByBudget(companyId, budgetId), JsonRequestBehavior.AllowGet);
        }

       
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult Activity()
        {
            return View();
        }

        [Authorize]
        public ActionResult ResponsiblePerson()
        {
            return View();
        }

        #region -- Operations

        //[HttpGet, Authorize]
        //public ActionResult GetList(GridParameter parameters, string entityId, string fiscalYearId,string budgetMasterId)
        //{
        //    return Json(_annualBudgetService.Query(parameters, entityId, fiscalYearId, budgetMasterId), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public ActionResult GetList(string entityId, string fiscalYearId, string budgetMasterId)
        {
            return Json(_annualBudgetService.Query(entityId, fiscalYearId, budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetResponsibleEmployeeList(string budgetMasterId, string activityId)
        {
            return Json(_annualBudgetService.GetResponsibleEmployeeList(budgetMasterId, activityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(AnnualBudget annualBudget,
             IEnumerable<AnnualBudgetDetail> annualBudgetDetailList,
            IEnumerable<AnnualBudgetActivity> annualBudgetActivities,
            IEnumerable<AnnualBudgetActivityDetail> annualBudgetActivityDetailList,
            IEnumerable<AnnualBudgetOtherHead> budgetOtherHeads,
            IEnumerable<BudgetApprovalPerson> responsiblePersons)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            annualBudget.CompanyGroupId = identity.CompanyGroupId;
            annualBudget.CompanyId = identity.CompanyId;
            annualBudget.PlantId = identity.PlantId;
            _annualBudgetService.Insert(annualBudget, annualBudgetDetailList, annualBudgetActivities, annualBudgetActivityDetailList, budgetOtherHeads, responsiblePersons);
            return Json(new { RoutineBudget = annualBudget, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(AnnualBudget annualBudget,
            IEnumerable<AnnualBudgetDetail> annualBudgetDetailList,
            IEnumerable<AnnualBudgetActivity> annualBudgetActivities,
            IEnumerable<AnnualBudgetActivityDetail> annualBudgetActivityDetailList,
            IEnumerable<AnnualBudgetOtherHead> budgetOtherHeads,
            IEnumerable<BudgetApprovalPerson> responsiblePersons)
        {
            _annualBudgetService.Update(annualBudget, annualBudgetDetailList, annualBudgetActivities, annualBudgetActivityDetailList, budgetOtherHeads, responsiblePersons);
            return Json(new { Message = AplosMessage.Updated });
        }

        [Authorize, HttpGet]
        public JsonResult GetRoutineBudgetActivityList(string routineBudgetId)
        {
            return Json(_annualBudgetService.GetAnnualBudgetActivityList(routineBudgetId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetBudgetOtherHeadList(string routineBudgetId)
        {
            return Json(_annualBudgetService.GetBudgetOtherHeadList(routineBudgetId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetResponsiblePersonList(string routineBudgetId)
        {
            return Json(_annualBudgetService.GetResponsiblePersonList(routineBudgetId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _annualBudgetService.Archive(id);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetAnnualBudgetDetailList(string budgetMasterId, string entityId, string fiscalYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_annualBudgetService.GetAnnualBudgetDetailList(identity.CompanyId, identity.PlantId, entityId, budgetMasterId, fiscalYearId), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}