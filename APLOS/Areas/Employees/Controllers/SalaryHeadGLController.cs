#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Library.Model.Organizations;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Service.ChartOfAccounts;
using Library.Data;
using Library.Service.Logs;
using System.Reflection;
using Library.Service.Enums;
using System;
using Library.Data.Sql;
using Syncfusion.XlsIO;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class SalaryHeadGLController : BaseController
    {
        #region Constructor
        /// <summary>   The SalaryHeadGLService service. </summary>
        private readonly ISalaryHeadGLService _salaryHeadGLService;
        private readonly ISqlRepository _sqlRepository;

        public SalaryHeadGLController(
              ISalaryHeadGLService fixedAssetGLService
            , ISqlRepository sqlRepository
            )
        {
            _salaryHeadGLService = fixedAssetGLService;
            _sqlRepository = sqlRepository;
        }
        #endregion
            
        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return  View("~/Areas/Employees/Views/SalaryHeadGL/Aplos.cshtml");
        }
        #endregion

        #region CBO

        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public ActionResult GetSalaryHead(string plantId, string manPowerBudgetId)
        {
            return Json(_salaryHeadGLService.GetSalaryHead(plantId, manPowerBudgetId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadData()
        {
            return Json(_salaryHeadGLService.GetSalaryHeadData(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadGL(string plantId, string salaryHeadId)
        {
            return Json(_salaryHeadGLService.GetSalaryHeadGL(plantId, salaryHeadId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetManPowerBudgetList(GridParameter parameters, string companyId)
        {
            return Json(_salaryHeadGLService.GetManPowerBudgetList(parameters, companyId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetManPowerBudgetSavedList(GridParameter parameters, string plantId)
        {
            return Json(_salaryHeadGLService.GetManPowerBudgetSavedList(parameters,plantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetBudgetListWithGL(GridParameter parameters, string glId)
        {
            return Json(_salaryHeadGLService.GetBudgetListWithGL(parameters, glId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetActivityListWithBudget(GridParameter parameters, string budgetId)
        {
            return Json(_salaryHeadGLService.GetActivityListWithBudget(parameters, budgetId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetCoaInfo(string companyId)
        {
            return Json(_salaryHeadGLService.CoaInfo(companyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SaveSalaryHeadGL(IEnumerable<SalaryHeadGL> salaryHeadGL)
        {
            _salaryHeadGLService.InsertOrUpdate(salaryHeadGL);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult EditSalaryHeadGL(SalaryHeadGL editSalaryHeadGL)
        {
            _salaryHeadGLService.Update(editSalaryHeadGL);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _salaryHeadGLService.DeleteGraph(id);
            return Json(new {  Message = AplosMessage.Deleted });
        }


        [HttpGet, Authorize]
        public ActionResult GetListWithCombine(GridParameter parameters, string coaId)
        {
            return Json(_salaryHeadGLService.GetSearchWithCombine(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithSalaryHead(GridParameter parameters, string coaId)
        {
            return Json(_salaryHeadGLService.GetSearchWithCombineSalaryHead(parameters, coaId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadGlbySalaryHead(GridParameter parameters, string SalaryHeadId)
        {
            return Json(_salaryHeadGLService.GetSearchWithCombine(parameters, SalaryHeadId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineAssing(GridParameter parameters, string coaId)
        {
            return Json(_salaryHeadGLService.GetSearchWithCombineWithAssing(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineNotAssing(GridParameter parameters, string coaId)
        {
            return Json(_salaryHeadGLService.GetSearchWithCombineWithNotAssing(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCRDirectIndirectGL(GridParameter parameters, string coaId)
        {
            return Json(GetExpensesLiabilityCurrentAssetGL(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        private GridModel GetExpensesLiabilityCurrentAssetGL(GridParameter parameters, string coaId)
        {
            try
            {
                parameters.CmdText = @"SELECT  C.Id AS COAId, AG.UserName AS AccountGroupName, C.UserName AS COAName
		                            , GLGI.UserName AS GLGeneralInfoName, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.Id AS GLGeneralInfoId
                                    , BM.BudgetId, B.UserName BudgetName, BM.RefNo
                                    , BMA.ActivityId, A.UserName ActivityName, BMA.BudgetMasterId
		                            FROM [MST].[BudgetMasterActivity] BMA
									LEFT JOIN [MST].[BudgetMaster] AS BM ON BMA.BudgetMasterId=BM.Id
									LEFT JOIN  HKP.Budget AS B ON BM.BudgetId=B.Id
									LEFT JOIN  HKP.Activity AS A ON BMA.ActivityId=A.Id
									LEFT JOIN HKP.GLGeneralInfo AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
									JOIN HKP.COA AS C ON C.Id=GLGI.COAId
		                            LEFT OUTER JOIN HKP.GLAccountType AS GLAT ON GLAT.GLGeneralInfoId = GLGI.Id
		                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id = GLGI.AccountGroupId
									LEFT JOIN HKP.AccountType AS ACT ON ACT.Id =AG.AccountTypeId
                                    WHERE GLGI.COAId = '" + coaId + @"' AND ACT.Id in ('" + AccountTypeEnum.Expense + "','"+ AccountTypeEnum.Asset + "','"+ AccountTypeEnum.Liability + @"')  
                                    AND AG.UserName not in ('Fixed Asset') ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        private GridModel GetSalaryHeadGls(GridParameter parameters, string SalaryHeadId)
        {
            try
            {
                parameters.CmdText = @"select a.UserName AccountsGroup,a.Id AccountsGroupId
                                        ,s.DrDirectGLId,s.DrDirectBudgetMasterId,s.DrDirectActivityId,s.DrDirectOtherGL,s.DrDirectOtherGLCode,s.CrDirectGLId,s.CrDirectBudgetMasterId,s.CrDirectActivityId,s.CrDirectOtherGL,s.CrDirectOtherGLCode
                                        , s.DrInDirectGLId,s.DrInDirectBudgetMasterId,s.DrInDirectActivityId,s.DrInDirectOtherGL,s.DrInDirectOtherGLCode,s.CrInDirectGLId,s.CrInDirectBudgetMasterId,s.CrInDirectActivityId,s.CrInDirectOtherGL,s.CrInDirectOtherGLCode
                                        From AccountsGroup a
                                        left join MST.SalaryHeadGL s on s.AccountsGroupId = a.Id and s.SalaryHeadId='" + SalaryHeadId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Salary Head Gl Report
        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadGlReport()
        {
          //  var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           // AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();

                //IWorkbook workbook = IssueReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, checkbox);
                 //IWorkbook workbook = GetAutoMailReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                 IWorkbook workbook = _salaryHeadGLService.GetSalaryHeadGlReport (/*identity.CompanyGroupId, identity.CompanyId, identity.PlantId*/);
               // return Json(_salaryHeadGLService.GetSalaryHeadGL(plantId, salaryHeadId), JsonRequestBehavior.AllowGet);

                string strFileName = "SalaryHeadGL.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }


        #endregion Salary Head Gl Report 
    }
}