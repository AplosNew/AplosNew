using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Model.Enums;
using Library.Model.ManagementChartOfAccounts;
using Library.Service.ManagementChartOfAccounts;
using Library.Service.Vouchers;
using Library.ViewModel.ManagementChartOfAccounts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using System.Linq;
using Library.Accounting.Accounts;

namespace Aplos.Areas.Accounts.Controllers
{
    public class BudgetMasterController : BaseController
    {
        private readonly IVoucherReportService _voucharReportService;
        private readonly IBudgetMasterService _budgetMasterService;
        private readonly AccountVoucherReportService _accountVoucherReportService;
        private readonly IBudgetMasterActivityService _budgetMasterActivityService;
        private readonly IRepositoryAsync<BudgetMaster> _budgetMasterRepository;

        public BudgetMasterController(
             IVoucherReportService voucharReportService
            , IBudgetMasterService budgetMasterService
            , IBudgetMasterActivityService budgetMasterActivityService
            , IRepositoryAsync<BudgetMaster> budgetMasterRepository
            , AccountVoucherReportService accountVoucherReportService)
        {
            _voucharReportService = voucharReportService;
            _budgetMasterService = budgetMasterService;
            _budgetMasterActivityService = budgetMasterActivityService;
            _budgetMasterRepository = budgetMasterRepository;
            _accountVoucherReportService = accountVoucherReportService;
        }

        [HttpGet, Authorize]
        public ActionResult GetBudgetMasterActivityCbo(string budgetMasterId)
        {
            return Json(_budgetMasterActivityService.GetBudgetMasterActivityCbo(budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBudgetMasterActivityLevelEmployeeCbo(string budgetMasterId, string level, string employeeId)
        {

            return Json(_budgetMasterActivityService.GetBudgetMasterActivityCbo(budgetMasterId, level, employeeId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetBudgetMasterActivityLevelPotalCbo(string budgetMasterId, string level, string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(employeeId))
                employeeId = identity.EmployeeId;
            return Json(_budgetMasterActivityService.GetBudgetMasterActivityCbo(budgetMasterId, level, employeeId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetBudgetMasterActivityLevelCbo(string budgetMasterId, string level)
        {
            return Json(_budgetMasterActivityService.GetBudgetMasterActivityCbo(budgetMasterId, level, null), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetCboActivityForSetup(string coaId, string glId, string budgetId)
        {
            return Json(_budgetMasterActivityService.GetCboActivityForSetup(coaId, glId, budgetId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboBudgetForSetup(string coaId, string glId)
        {
            return Json(_budgetMasterService.GetCboBudgetForSetup(coaId, glId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterCboList(string glId)
        {
            return Json(_budgetMasterService.GetBudgetMasterCboList(glId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterCboByCOAAndGLId(string coaId, string glId)
        {
            return Json(_budgetMasterService.GetBudgetMasterCboByCOAAndGLId(coaId, glId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterCboByCompanyAndGLId(string companyId, string glId)
        {
            return Json(_budgetMasterService.GetBudgetMasterCboByCompanyAndGLId(companyId, glId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboBudgetMasterForSetup()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_budgetMasterService.GetCboBudgetMasterForSetup(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetCategoryCbo()
        {
            return Json(_budgetMasterService.GetBudgetCategoryCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetSubCategoryCboByCategory(string categoryId)
        {
            return Json(_budgetMasterService.GetBudgetSubCategoryCboByCategory(categoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetCboBySubCategory(string subCategoryId)
        {
            return Json(_budgetMasterService.GetBudgetCboBySubCategory(subCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCboEmployeeBudgetList(string employeeId)
        {
            if (!string.IsNullOrEmpty(employeeId))
                return Json(_budgetMasterService.GetCboEmployeeBudgetList(employeeId), JsonRequestBehavior.AllowGet);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(identity.EmployeeId))
                employeeId = identity.EmployeeId;
            return Json(_budgetMasterService.GetCboEmployeeBudgetList(employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboEmployeeBudgetPopUpListByEmployeeId(GridParameter parameters, string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_budgetMasterService.GetCboEmployeeBudgetPopUpList(parameters, employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCboBudgetCategorySubCategoryActivityPopUpList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_budgetMasterService.GetCboBudgetCategorySubCategoryActivityPopUpList(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterPopUpList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_budgetMasterService.GetBudgetMasterPopUpList(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboEmployeeBudgetPopUpList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_budgetMasterService.GetCboEmployeeBudgetPopUpList(parameters, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCboEmployeeBudgetActivityList(string employeeId, string budgetMasterId)
        {
            if (string.IsNullOrEmpty(employeeId))
                return Json(_budgetMasterService.GetCboEmployeeBudgetActivityList(employeeId, budgetMasterId),
                    JsonRequestBehavior.AllowGet);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(identity.EmployeeId))
                employeeId = identity.EmployeeId;
            return Json(_budgetMasterService.GetCboEmployeeBudgetActivityList(employeeId, budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCboEmployeeBudgetActivityPhoneList(string employeeId, string budgetId, string activityId)
        {
            if (!string.IsNullOrEmpty(employeeId))
                return Json(
                    new SelectList(
                        _budgetMasterService.GetCboEmployeeBudgetActivityPhoneList(employeeId, budgetId, activityId),
                        "Value", "Text"), JsonRequestBehavior.AllowGet);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(identity.EmployeeId))
                employeeId = identity.EmployeeId;
            return Json(new SelectList(_budgetMasterService.GetCboEmployeeBudgetActivityPhoneList(employeeId, budgetId, activityId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetFALinkedList(string budgetMasterId, string activityId, string faLinked)
        {
            return Json(_budgetMasterService.GetFALinkedList(budgetMasterId, activityId, faLinked), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetBudgetCboByGL(string glgeneralInfoId)
        {
            return Json(new SelectList(_budgetMasterService.GetBudgetCboByGL(glgeneralInfoId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetBudgetCboByEmployeeActivity(string employeeId, string activityId)
        {
            if (!string.IsNullOrEmpty(employeeId))
                return Json(
                    new SelectList(_budgetMasterService.GetBudgetCboByEmployeeActivity(employeeId, activityId), "Value",
                        "Text"), JsonRequestBehavior.AllowGet);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(identity.EmployeeId))
                employeeId = identity.EmployeeId;
            return Json(new SelectList(_budgetMasterService.GetBudgetCboByEmployeeActivity(employeeId, activityId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        #region Budget Responsible Person
        [HttpGet, Authorize]
        public ActionResult GetAllEmployee(GridParameter parameters, string plantId)
        {
            return Json(_budgetMasterService.GetAllEmployee(parameters, plantId), JsonRequestBehavior.AllowGet);
        }
        #endregion

       
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult Budget()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult BudgetCategory()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult BudgetClass()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult BudgetGroup()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult BudgetSubCategory()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult FARegister()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult FiscalYearBudget()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult BudgetControl()
        {
            return View();
        }

        [HttpPost]
        public JsonResult FARegisterUpdate(BudgetMaster budgetMaster, IEnumerable<ActivityViewModel> budgetActivities)
        {
            _budgetMasterService.UpdateFARegister(budgetMaster, budgetActivities);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string coaId)
        {
            return Json(_budgetMasterService.Query(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public int GetMaxRefNo()
        {
            int budMaxRefNo= _budgetMasterRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RefNo AS INT)), 0) RefNo FROM [MST].[BudgetMaster] ").First();
            budMaxRefNo++;
            return budMaxRefNo;
        }
       

        [HttpGet, Authorize]
        public JsonResult GetBudgetActivityList(string budgetMasterId)
        {
            return Json(_budgetMasterActivityService.Query(budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFAMasterLinkList(string budgetMasterId)
        {
            return Json(_budgetMasterActivityService.GetFAMasterLinkList(budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFARegisterLinkList(string budgetMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_budgetMasterActivityService.GetFARegisterLinkList(identity.CompanyId, budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetMasterById(string id)
        {
            return Json(_budgetMasterService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetBudgetPaymentTypeList(string budgetmasterId)
        {
            return Json(_budgetMasterService.GetBudgetPaymentTypeList(budgetmasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BudgetMaster budgetmaster, IEnumerable<ActivityViewModel> budgetActivities,
            IEnumerable<BudgetMasterPaymentTerm> budgetMasterPaymentTypeList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetMasterService.Insert(budgetmaster, budgetActivities, budgetMasterPaymentTypeList, identity.CompanyGroupId);
            return Json(new { BudgetMaster = budgetmaster, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BudgetMaster budgetMaster, IEnumerable<ActivityViewModel> budgetActivities, IEnumerable<BudgetMasterPaymentTerm> budgetMasterPaymentTypeList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetMasterService.Update(budgetMaster, budgetActivities, budgetMasterPaymentTypeList, identity.CompanyGroupId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _budgetMasterService.DeleteMaster(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public bool CheckUsingActivityInTransaction(string id)
        {
            try
            {
                var sql = @"IF EXISTS(select 1 SELECT top(1) ActivityId FROM  trn.VoucherDetail where ActivityId='" + id + @"'
                            )x WHERE x.ActivityId='" + id + @"') SELECT 1 ELSE SELECT 0 RETURN ";
                return Convert.ToBoolean(_budgetMasterRepository.SqlQuery<int>(sql).Single());
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpGet]
        public ActionResult BudgetMasterReport()
        {
            return View("~/Areas/Accounts/Views/BudgetMasterReport.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetBudgetMasterReport(ReportFormat reportFormat, string coaId, bool isActivityLevel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetBudgetMasterReport(identity.CompanyGroupId, coaId, isActivityLevel);
            var reportFileName = "Budget Master";
            switch (reportFormat)
            {
                //case ReportFormat.Pdf:
                //    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        #region FiscalYearBudget
        [HttpGet, Authorize]
        public ActionResult GetFiscalYearBudgetReport(ReportFormat reportFormat, string fiscalYearPeriodId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var  workbook = _voucharReportService.GetFiscalYearBudgetReport(identity.Name, identity.CompanyGroupId,identity.CompanyId,identity.PlantId,identity.PlantName, fiscalYearPeriodId);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Monthly  Budget";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }



        #endregion FiscalYearBudget
    }
}