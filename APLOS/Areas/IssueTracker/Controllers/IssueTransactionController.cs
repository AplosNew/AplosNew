#region Using

using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using Library.Model.TaskScheduler;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.IssueTracker;
using Library.Service.TaskManagement;
using Library.Service.TaskScheduler;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.IssueTracker.Controllers
{
    public class IssueTransactionController : BaseController
    {
        private string issueTransactionId;

        #region Constructor

        private readonly IIssueTransactionService _issueTransactionService;
        private readonly IIssueUpdateAuditService _issueUpdateAuditService;
        private readonly IIssueInternalAuditService _issueInternalAuditService;
        private readonly IIssueFollowUpResponsibleService _issueFollowUpAuditService;
        private readonly IIssueExternalAuditService _issueExternalAuditService;
        private readonly ITaskManagerMasterService _taskManagerMasterService;
        private readonly ITaskAuditService _taskAuditService;
        private readonly ITaskSchedulerMasterService _taskSchedulerMasterService;
        private readonly ISqlRepository _sqlRepository;



        public IssueTransactionController(
              IIssueTransactionService IssueTransactionService,
              IIssueUpdateAuditService IssueUpdateAuditService,
              IIssueInternalAuditService IssueInternalAuditService,
              IIssueFollowUpResponsibleService issueFollowUpAuditService,
              IIssueExternalAuditService issueExternalAuditService,
              ITaskManagerMasterService taskManagerMasterService,
              ITaskAuditService TaskAuditService,
              ITaskSchedulerMasterService taskSchedulerMasterService,
               ISqlRepository R

            )
        {
            _issueTransactionService = IssueTransactionService;
            _issueUpdateAuditService = IssueUpdateAuditService;
            _issueInternalAuditService = IssueInternalAuditService;
            _issueFollowUpAuditService = issueFollowUpAuditService;
            _issueExternalAuditService = issueExternalAuditService;
            _taskManagerMasterService = taskManagerMasterService;
            _taskAuditService = TaskAuditService;
            _taskSchedulerMasterService = taskSchedulerMasterService;
            _sqlRepository = R;
        }
        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_issueTransactionService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult IssueReport()
        {
            return View();
        }

        //IssueReport
        private DataTable GetAllLoanRegisterReportData(string companyGroupId, string companyId, string plantId)
        {
            // var sql = "";



            var sql = @"               
                      select IT.IssueType,IT.Priority
                    ,format(IT.CloseDate,'dd-MMM-yyyy') CloseDate
                    ,format(IT.CommitmentDate,'dd-MMM-yyyy')CommitmentDate
                   ,TC.UserName AS Category
                    ,TSC.UserName as SubCategory, EIA.EmployeeName AS Auditor

					, ist.Code IssueStandardCode,   IST.UserName IssueCategory, ist.UserName IssueStandardName, ist.UserName IssueStandardSubCategory
					,ist.Issue, ist.IssueDetail Details ,ii.UserName IssueStandardImportance, ist.Remarks, ist.StandardName  

					,it.id IssueId, IT.Issue TaskType, it.Issue Task,  IT.IssueDetail CurrentStatus
					,it.Issue TaskCategory, it.Issue IssueSubCategory, IG.Name IssueGroup, it.StoryPoint 
					, IsExpiryApplicable=case WHEN IT.IsExpiry = 1 then 'YES' ELSE 'NO' END   
					,p.UserName Customer, ii1.UserName IssueImportance,IT.FinalStatus,format(it.IssueDate,'dd-MMM-yyyyy') IssueCreationDate, IT.CostIfAny CostApplicable, it.ObservedBy
					,EM1.EmployeeName AssaignTo
					, ei.EmployeeName as Mentor, format(IT.RequiredDate,'dd-MMM-yyyy') RequiredDate
					,format(it.CloseDate, 'dd-MMM-yyyy') TaskClosingDate
					,it.IsUpdateApplicable,it.IsInternalApplicable,it.IsFollowUpApplicable
					 ,format(IT.ExpiryDate, 'dd-MMM-yyyy')ExpiryDate

					,format(IT.IssueDate,'dd-MMM-yyyy')IssueDate

                    ,EIRP.EmployeeName AS ResponsiblePerson,ISBT.TaskDetail
					,ISBT.IsDone
					  from IssueTransaction IT
                    left join hkp.TaskCategory TC on IT.TaskCategoryId=TC.Id
                    left join hkp.TaskSubCategory TSC on IT.TaskSubCategoryId=TSC.id
                    Left join EmployeeInformation EI ON IT.MentorId = ei.SystemId
                    Left join EmployeeInformation EIA ON IT.InternalResponsiblePersonId = EIA.SystemId
                    Left join EmployeeInformation EIRP ON IT.AssignById = EIRP.SystemId
					left join IssueGroup IG on IG.Id = IT.IssueGroupId
					left join IssueStandard IST on IST.Id =it.IssueStandardId
					left join IssueImportance ii on ii.id = ist.IssueImportanceId
					left join HKP.party p on p.id = it.CustomerId
					left join IssueImportance ii1 on ii1.Id = it.IssueImportanceId
                    left join EmployeeInformation EM1 on EM1.SystemId= it.AssignToId
					left join [dbo].[IssueSubTask] ISBT ON ISBT.IssueTransactionId=IT.Id";




            return _sqlRepository.GetDataTable(sql);
        }


        [HttpGet, Authorize]
        public ActionResult GetIssueReportExcel(bool checkbox)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = IssueReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, checkbox);

                string strFileName = "Issue Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }


        private IWorkbook IssueReportList(string companyGroupId, string companyId, string plantId, bool checkbox)
        {

            //Start EmployeeAdvanceDueList

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtIssueReportList = GetAllLoanRegisterReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

            if (dtIssueReportList.Rows.Count == 0)
                throw new Exception("No data found");

            worksheet.Name = "IssueReport";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet[ROW, COL].Text = "SL. No";
            int colSLNO = COL;
            worksheet[ROW, COL].ColumnWidth = 7;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "Issue Status";
            int colIssueType = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Priority";
            int colPriority = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Close Date";
            int colCloseDate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Commitment Date";
            int colCommitmentDate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Category";
            int colCategory = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Sub Category";
            int colSubCategory = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Auditor";
            int colAuditor = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "IssueStandard Code";
            int colIssueStandardCode = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Responsible Person";
            int colResponsiblePerson = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Issue Category";
            int colIssueCategory = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Issue StandardName";
            int colIssueStandardName = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "IssueStandard SubCategory";
            int colIssueStandardSubCategory = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Issue";
            int colIssue = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;



            worksheet[ROW, COL].Text = "Details";
            int colDetails = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "IssueStandard Importance";
            int colIssueStandardImportance = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Remarks";
            int colRemarks = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Standard Name";
            int colStandardName = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Issue Id";
            int colIssueId = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;


            worksheet[ROW, COL].Text = "Task Type";
            int colTaskType = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Task";
            int colTask = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Current Status";
            int colCurrentStatus = COL;
            worksheet[ROW, COL].ColumnWidth = 50;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Task Category";
            int colTaskCategory = COL;
            worksheet[ROW, COL].ColumnWidth = 35;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Sub Category";
            int colIssSubCategory = COL;
            worksheet[ROW, COL].ColumnWidth = 35;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Issue Group";
            int colIssueGroup = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Story Point";
            int colStoryPoint = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "IsExpiry Applicable";
            int colIsExpiryApplicable = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;



            worksheet[ROW, COL].Text = "Customer";
            int colCustomer = COL;
            worksheet[ROW, COL].ColumnWidth = 18;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Issue Importance";
            int colIssueImportance = COL;
            worksheet[ROW, COL].ColumnWidth = 14;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Final Status";
            int colFinalStatus = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "IssueCreation Date";
            int colIssueCreationDate = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Cost Applicable";
            int colCostApplicable = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;



            worksheet[ROW, COL].Text = "Observed By";
            int colObservedBy = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;



            worksheet[ROW, COL].Text = "Assaign To";
            int colAssaignTo = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Mentor";
            int colMentor = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Required Date";
            int colRequiredDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Task ClosingDate";
            int colTaskClosingDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "IsUpdate Applicable";
            int colIsUpdateApplicable = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;



            worksheet[ROW, COL].Text = "IsInternal Applicable";
            int colIsInternalApplicable = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;




            worksheet[ROW, COL].Text = "IsFollowUp Applicable";
            int colIsFollowUpApplicable = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;



            worksheet[ROW, COL].Text = "Expiry Date";
            int colExpiryDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            int colTaskDetail = 0;
            if (checkbox == true)
            {
                COL++;
                colTaskDetail = COL;


                worksheet[ROW, COL].Text = "Sub Task";
                worksheet[ROW, COL].ColumnWidth = 40;
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
            }
            //COL++;

            //worksheet[ROW, COL].Text = "SubTaskStatus";
            //int colSubTaskStatus  = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            ////COL++;




            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ///worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;

            for (int i = 0; i < dtIssueReportList.Rows.Count; i++)
            {

                worksheet[ROW, colSLNO].Number = (i + 1);

                worksheet[ROW, colIssueType].Text = dtIssueReportList.Rows[i]["IssueType"].ToString();
                worksheet[ROW, colPriority].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["Priority"].ToString());
                worksheet[ROW, colCloseDate].Text = dtIssueReportList.Rows[i]["CloseDate"].ToString();
                worksheet[ROW, colCommitmentDate].Text = dtIssueReportList.Rows[i]["CommitmentDate"].ToString();
                worksheet[ROW, colCategory].Text = dtIssueReportList.Rows[i]["Category"].ToString();
                worksheet[ROW, colSubCategory].Text = dtIssueReportList.Rows[i]["SubCategory"].ToString();
                worksheet[ROW, colAuditor].Text = dtIssueReportList.Rows[i]["Auditor"].ToString();
                worksheet[ROW, colResponsiblePerson].Text = dtIssueReportList.Rows[i]["ResponsiblePerson"].ToString();


                worksheet[ROW, colIssueStandardCode].Text = dtIssueReportList.Rows[i]["IssueStandardCode"].ToString();
                worksheet[ROW, colIssueCategory].Text = dtIssueReportList.Rows[i]["IssueCategory"].ToString();
                worksheet[ROW, colIssueStandardName].Text = dtIssueReportList.Rows[i]["IssueStandardName"].ToString();

                worksheet[ROW, colIssue].Text = dtIssueReportList.Rows[i]["Issue"].ToString();
                worksheet[ROW, colDetails].Text = dtIssueReportList.Rows[i]["Details"].ToString();
                worksheet[ROW, colIssueStandardImportance].Text = dtIssueReportList.Rows[i]["IssueStandardImportance"].ToString();
                worksheet[ROW, colRemarks].Text = dtIssueReportList.Rows[i]["Remarks"].ToString();
                worksheet[ROW, colStandardName].Text = dtIssueReportList.Rows[i]["StandardName"].ToString();
                worksheet[ROW, colIssueId].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["IssueId"].ToString());
                worksheet[ROW, colTaskType].Text = dtIssueReportList.Rows[i]["TaskType"].ToString();
                worksheet[ROW, colTask].Text = dtIssueReportList.Rows[i]["Task"].ToString();
                worksheet[ROW, colCurrentStatus].Text = dtIssueReportList.Rows[i]["CurrentStatus"].ToString();


                worksheet[ROW, colTaskCategory].Text = dtIssueReportList.Rows[i]["TaskCategory"].ToString();
                worksheet[ROW, colIssueStandardSubCategory].Text = dtIssueReportList.Rows[i]["IssueStandardSubCategory"].ToString();
                worksheet[ROW, colIssSubCategory].Text = dtIssueReportList.Rows[i]["IssueSubCategory"].ToString();
                worksheet[ROW, colIssueGroup].Text = dtIssueReportList.Rows[i]["IssueGroup"].ToString();
                worksheet[ROW, colStoryPoint].Text = dtIssueReportList.Rows[i]["StoryPoint"].ToString();
                worksheet[ROW, colIsExpiryApplicable].Text = dtIssueReportList.Rows[i]["IsExpiryApplicable"].ToString();
                worksheet[ROW, colCustomer].Text = dtIssueReportList.Rows[i]["Customer"].ToString();
                worksheet[ROW, colIssueImportance].Text = dtIssueReportList.Rows[i]["IssueImportance"].ToString();
                worksheet[ROW, colFinalStatus].Text = dtIssueReportList.Rows[i]["FinalStatus"].ToString();
                worksheet[ROW, colIssueCreationDate].Text = dtIssueReportList.Rows[i]["IssueCreationDate"].ToString();
                worksheet[ROW, colCostApplicable].Text = dtIssueReportList.Rows[i]["CostApplicable"].ToString();
                worksheet[ROW, colObservedBy].Text = dtIssueReportList.Rows[i]["ObservedBy"].ToString();
                worksheet[ROW, colAssaignTo].Text = dtIssueReportList.Rows[i]["AssaignTo"].ToString();
                worksheet[ROW, colMentor].Text = dtIssueReportList.Rows[i]["Mentor"].ToString();
                worksheet[ROW, colRequiredDate].Text = dtIssueReportList.Rows[i]["RequiredDate"].ToString();

                worksheet[ROW, colTaskClosingDate].Text = dtIssueReportList.Rows[i]["TaskClosingDate"].ToString();
                worksheet[ROW, colIsUpdateApplicable].Text = dtIssueReportList.Rows[i]["IsUpdateApplicable"].ToString();
                worksheet[ROW, colIsInternalApplicable].Text = dtIssueReportList.Rows[i]["IsInternalApplicable"].ToString();
                worksheet[ROW, colIsFollowUpApplicable].Text = dtIssueReportList.Rows[i]["IsFollowUpApplicable"].ToString();
                worksheet[ROW, colExpiryDate].Text = dtIssueReportList.Rows[i]["ExpiryDate"].ToString();

                if (checkbox == true)
                {

                    worksheet[ROW, colTaskDetail].Text = dtIssueReportList.Rows[i]["TaskDetail"].ToString();

                }










                // worksheet[ROW, colPurchasePrice].NumberFormat = clsStaticInfo.NumberFormat();
                // worksheet[ROW, colScantionAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["ScantionAmount"].ToString());


                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;



            ReportUtility reportUtility = new ReportUtility();

            reportUtility.PlantHeader(ref worksheet, endCol, " Issue Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes



            return workbook;
        }
        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_issueTransactionService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult BuyerList(GridParameter parameters)
        {
            return Json(_issueTransactionService.BuyerList(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListIssueTransaction(GridParameter parameters)
        {
            return Json(_issueTransactionService.GetListIssueTransaction(parameters), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public ActionResult GetUser()
        //{

        //    return Json(new { userName = _issueTransactionService.GetLogedInUser(), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        //    //return  Json(userName = _issueTransactionService.GetLogedInUser(), JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet, Authorize]
        //public JsonResult GetAutoSequence()
        //{
        //    return Json(_issueTransactionService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        //}

        #region IssuetrancationCreation
        //[HttpPost, Authorize]
        //public JsonResult IssueTransactionCreate(IssueTransaction model)
        //{
        //    model.Priority = 4.5M;
        //    if (model.Id == null)
        //    {

        //        _issueTransactionService.Insert(model);
        //    }
        //    else
        //        _issueTransactionService.Update(model);
        //    issueTransactionId = model.Id;
        //    return Json(new { IssueTransaction = model, Message = AplosMessage.Success });
        //}
        #endregion end issuetransactionCreation

        [HttpPost, Authorize]
        public JsonResult IssueTransactionCreate(IssueTransaction issueTransactionNew, List<Dictionary<string, object>> buyers)
        {
            issueTransactionNew.Priority = 4.5M;
            if (issueTransactionNew.Id == null)
            {

                _issueTransactionService.Insert(issueTransactionNew);
            }
            else
                _issueTransactionService.Update(issueTransactionNew);
            issueTransactionId = issueTransactionNew.Id;

            CreateBuyers(buyers, issueTransactionId);
            return Json(new { IssueTransaction = issueTransactionNew, Message = AplosMessage.Success });
        }


        private void CreateBuyers(List<Dictionary<string, object>> buyers, string issueTransactionId)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("select * from IssueBuyer where IssueTransactionId = '" + issueTransactionId + "'", out dsMaster, false, "1");

            string _Id = "";

            if (buyers != null && buyers.Count > 0)
            {

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    var k = buyers.Where(ee => ee["BuyerId"].ToString() == dsMaster.Tables[0].Rows[i]["BuyerId"].ToString()).ToList();
                    if (k == null || k.Count == 0)
                        dsMaster.Tables[0].Rows[0].Delete();
                }


                for (int i = 0; i < buyers.Count; i++)
                {

                    buyers[i]["IssueTransactionId"] = issueTransactionId;
                    dsMaster.Tables[0].DefaultView.RowFilter = "BuyerId='" + buyers[i]["BuyerId"].ToString() + "'";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("Dbo.IssueBuyer", out _Id);

                        buyers[i]["Id"] = "TC" + _Id;
                        AddNewRow(dsMaster.Tables[0], buyers[i]);
                    }
                    else
                    {

                        EditRow(dsMaster.Tables[0].Rows[0], buyers[i]);
                    }
                }


            }

            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsMaster);
        }

        [HttpGet, Authorize]
        public ActionResult GetToDoList()
        {
            return Json(_issueTransactionService.GetToDoList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTodayTaskList()
        {
            return Json(_issueTransactionService.GetTodayTaskList(), JsonRequestBehavior.AllowGet);
        }

        //private void CreateSubTasks(string IssueTransactionId, string EmployeeId, string taskManagerMasterId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        //    string sqlSource = "select* from IssueSubTask where IssueTransactionId = '" + IssueTransactionId + "' and ResponsiblePersonId = '" + EmployeeId + "'";
        //    string sqlDestination = "select * from TaskManagerSubTasks where 1=2";
        //    try
        //    {
        //        DataSet dsSource, dsDestination;
        //        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
        //        con.OpenDataSetThroughAdapter(sqlSource, out dsSource, false, "1");

        //        con = new ConnectionManager.DAL.ConManager("1");
        //        con.OpenDataSetThroughAdapter(sqlDestination, out dsDestination, false, "1");

        //        string _Id = "";

        //        #region data update
        //        for (int i = 0; i < dsSource.Tables[0].Rows.Count; i++)
        //        {
        //            if (_Id == "")
        //            {
        //                bplib.clsGenID genid = new bplib.clsGenID();
        //                genid.GenID("Task Manager SubTask", out _Id);
        //            }
        //            DataRow dr = dsDestination.Tables[0].NewRow();

        //            dr["Id"] = "I" + _Id + (i + 1).ToString();
        //            dr["taskManagerMasterId"] = taskManagerMasterId;
        //            dr["ResponsiblePersonId"] = EmployeeId;
        //            dr["RequiredDate"] = dsSource.Tables[0].Rows[i]["RequiredDate"];
        //            dr["TaskDetail"] = dsSource.Tables[0].Rows[i]["TaskDetail"];
        //            dr["IsDone"] = dsSource.Tables[0].Rows[i]["IsDone"];
        //            dr["Remarks"] = dsSource.Tables[0].Rows[i]["Remarks"];

        //            dr["AddedBy"] = identity.EmployeeId;
        //            dr["AddedDate"] = System.DateTime.Now.ToString();
        //            dr["AddedFromIP"] = identity.IPAddress;
        //            dr["UpdatedBy"] = identity.EmployeeId;
        //            dr["UpdatedDate"] = System.DateTime.Now.ToString();
        //            dr["UpdatedFromIP"] = identity.IPAddress;

        //            dsDestination.Tables[0].Rows.Add(dr);


        //        }

        //        #endregion data update
        //        clsStaticInfo _info = new clsStaticInfo();
        //        _info.SaveDataSets(dsDestination);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //        //throw;
        //    }

        //}
        private void CreateSubTasks(IssueTransaction model, string taskManagerMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sqlSource = "select* from IssueSubTask where IssueTransactionId = '" + model.Id + "'";
            string sqlDestination = "select * from TaskManagerSubTasks where 1=2";
            try
            {
                DataSet dsSource, dsDestination;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sqlSource, out dsSource, false, "1");

                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sqlDestination, out dsDestination, false, "1");

                string _Id = "";

                #region data update
                for (int i = 0; i < dsSource.Tables[0].Rows.Count; i++)
                {
                    if (_Id == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("Task Manager SubTask", out _Id);
                    }
                    DataRow dr = dsDestination.Tables[0].NewRow();

                    dr["Id"] = "I" + _Id + (i + 1).ToString();
                    dr["taskManagerMasterId"] = taskManagerMasterId;

                    dr["RequiredDate"] = dsSource.Tables[0].Rows[i]["RequiredDate"];
                    dr["TaskDetail"] = dsSource.Tables[0].Rows[i]["TaskDetail"];
                    dr["IsDone"] = dsSource.Tables[0].Rows[i]["IsDone"];
                    dr["Remarks"] = dsSource.Tables[0].Rows[i]["Remarks"];
                    dr["ResponsiblePersonId"] = model.AssignToId;
                    dr["AddedBy"] = identity.EmployeeId;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.EmployeeId;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsDestination.Tables[0].Rows.Add(dr);


                }

                #endregion data update

                DataSet dsFiles;
                CreateAttachments(model, taskManagerMasterId, out dsFiles);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsDestination, dsFiles);

            }
            catch (Exception ex)
            {
                throw (ex);
                //throw;
            }

        }
        private void CreateAttachments(IssueTransaction model, string taskManagerMasterId, out DataSet dsDestination)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sqlSource = "select* from IssueTransactionDocuments where IssueTransactionId = '" + model.Id + "'";
            string sqlDestination = "select * from TaskAttachments where 1=2";
            try
            {
                DataSet dsSource;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sqlSource, out dsSource, false, "1");

                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sqlDestination, out dsDestination, false, "1");

                string _Id = "";

                #region data update
                for (int i = 0; i < dsSource.Tables[0].Rows.Count; i++)
                {
                    if (_Id == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("Task Manager Attachments", out _Id);
                    }
                    DataRow dr = dsDestination.Tables[0].NewRow();

                    dr["Id"] = "I" + _Id + (i + 1).ToString();
                    dr["taskManagerMasterId"] = taskManagerMasterId;



                    dr["UploadedById"] = model.AssignById;
                    dr["FileName"] = dsSource.Tables[0].Rows[i]["Id"].ToString() + new FileInfo(dsSource.Tables[0].Rows[i]["FileName"].ToString()).Extension;
                    dr["FileOriginalName"] = dsSource.Tables[0].Rows[i]["FileName"];
                    dr["Extension"] = new FileInfo(dsSource.Tables[0].Rows[i]["FileName"].ToString()).Extension;


                    dr["AddedBy"] = identity.EmployeeId;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.EmployeeId;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsDestination.Tables[0].Rows.Add(dr);


                }

                #endregion data update


            }
            catch (Exception ex)
            {
                throw (ex);
                //throw;
            }

        }
        private string CreateIssueTaskAudit(IssueTransaction issueTransaction)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            TaskAudit taskAudit = new TaskAudit();

            taskAudit.AddedFromIP = identity.IPAddress;
            taskAudit.UpdatedBy = identity.EmployeeId;
            taskAudit.UpdatedDate = System.DateTime.Now;
            taskAudit.UpdatedFromIP = identity.IPAddress;

            taskAudit.DueDate = issueTransaction.RequiredDate;
            TaskAudit taskAuditAssignBy = new TaskAudit();
            taskAuditAssignBy.AddedFromIP = identity.IPAddress;
            taskAuditAssignBy.UpdatedBy = identity.EmployeeId;
            taskAuditAssignBy.UpdatedDate = System.DateTime.Now;
            taskAuditAssignBy.UpdatedFromIP = identity.IPAddress;
            var taskManagerMasterDb = _taskManagerMasterService.GetTaskManagerMaster(issueTransaction.Id, TaskTypeEnum.Issue.ToString());


            if (taskManagerMasterDb != null)
            {

                taskAudit.TaskManagerMasterId = taskManagerMasterDb.Id;
                taskAudit.AuthorizationType = AuthorizationTypeEnum.AssignTo.ToString();
                taskAudit.ResponsiblePersonId = issueTransaction.AssignToId;
                _taskAuditService.Insert(taskAudit);
            }
            var taskAuditForAssignByDb = _taskAuditService.GetTaskAudit(taskAudit.TaskManagerMasterId, issueTransaction.AssignById);
            if (taskAuditForAssignByDb == null)
            {
                taskAuditAssignBy.TaskManagerMasterId = taskManagerMasterDb.Id;
                taskAuditAssignBy.AuthorizationType = AuthorizationTypeEnum.CreatedBy.ToString();
                taskAuditAssignBy.ResponsiblePersonId = issueTransaction.AssignById;
                taskAuditAssignBy.DueDate = issueTransaction.RequiredDate;
                _taskAuditService.Insert(taskAuditAssignBy);
            }

            return taskManagerMasterDb.Id;

        }
        private string CreateUpdateTaskAudit(IssueTransaction issueTransaction)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            TaskAudit taskAudit = new TaskAudit();
            taskAudit.AddedFromIP = identity.IPAddress;
            taskAudit.UpdatedBy = identity.EmployeeId;
            taskAudit.UpdatedDate = System.DateTime.Now;
            taskAudit.UpdatedFromIP = identity.IPAddress;

            taskAudit.DueDate = issueTransaction.RequiredDate;

            TaskAudit taskAuditAssignBy = new TaskAudit();
            taskAuditAssignBy.AddedFromIP = identity.IPAddress;
            taskAuditAssignBy.UpdatedBy = identity.EmployeeId;
            taskAuditAssignBy.UpdatedDate = System.DateTime.Now;
            taskAuditAssignBy.UpdatedFromIP = identity.IPAddress;

            var taskManagerMasterDb = _taskManagerMasterService.GetTaskManagerMaster(issueTransaction.Id, TaskTypeEnum.UpdateAudit.ToString());


            if (taskManagerMasterDb != null)
            {
                taskAudit.TaskManagerMasterId = taskManagerMasterDb.Id;
                taskAudit.AuthorizationType = AuthorizationTypeEnum.AssignTo.ToString();
                taskAudit.ResponsiblePersonId = issueTransaction.UpdateResponsiblePersonId;
                _taskAuditService.Insert(taskAudit);
            }
            var taskAuditForAssignByDb = _taskAuditService.GetTaskAudit(taskAudit.TaskManagerMasterId, issueTransaction.AssignById);
            if (taskAuditForAssignByDb == null)
            {
                taskAuditAssignBy.TaskManagerMasterId = taskManagerMasterDb.Id;
                taskAuditAssignBy.AuthorizationType = AuthorizationTypeEnum.CreatedBy.ToString();
                taskAuditAssignBy.ResponsiblePersonId = issueTransaction.AssignById;
                taskAuditAssignBy.DueDate = issueTransaction.RequiredDate;
                _taskAuditService.Insert(taskAuditAssignBy);
            }

            return taskManagerMasterDb.Id;

        }

        private string CreateFollowUpTaskAudit(IssueTransaction issueTransaction)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            TaskAudit taskAudit = new TaskAudit();
            taskAudit.AddedFromIP = identity.IPAddress;
            taskAudit.UpdatedBy = identity.EmployeeId;
            taskAudit.UpdatedDate = System.DateTime.Now;
            taskAudit.UpdatedFromIP = identity.IPAddress;

            taskAudit.DueDate = issueTransaction.RequiredDate;
            TaskAudit taskAuditAssignBy = new TaskAudit();
            taskAuditAssignBy.AddedFromIP = identity.IPAddress;
            taskAuditAssignBy.UpdatedBy = identity.EmployeeId;
            taskAuditAssignBy.UpdatedDate = System.DateTime.Now;
            taskAuditAssignBy.UpdatedFromIP = identity.IPAddress;
            TaskManagerMaster taskManagerMaster = new TaskManagerMaster();

            var taskManagerMasterDb = _taskManagerMasterService.GetTaskManagerMaster(issueTransaction.Id, TaskTypeEnum.FollowUpAudit.ToString());


            if (taskManagerMasterDb != null)
            {
                taskAudit.TaskManagerMasterId = taskManagerMasterDb.Id;
                taskAudit.AuthorizationType = AuthorizationTypeEnum.AssignTo.ToString();
                taskAudit.ResponsiblePersonId = issueTransaction.FollowUpResponsiblePersonId;
                _taskAuditService.Insert(taskAudit);

            }
            var taskAuditForAssignByDb = _taskAuditService.GetTaskAudit(taskAudit.TaskManagerMasterId, issueTransaction.AssignById);
            if (taskAuditForAssignByDb == null)
            {
                taskAuditAssignBy.TaskManagerMasterId = taskManagerMasterDb.Id;
                taskAuditAssignBy.AuthorizationType = AuthorizationTypeEnum.CreatedBy.ToString();
                taskAuditAssignBy.ResponsiblePersonId = issueTransaction.AssignById;
                taskAuditAssignBy.DueDate = issueTransaction.RequiredDate;
                _taskAuditService.Insert(taskAuditAssignBy);
            }
            return taskManagerMasterDb.Id;

        }

        private string CreateInternalTaskAudit(IssueTransaction issueTransaction)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            TaskAudit taskAudit = new TaskAudit();

            taskAudit.AddedFromIP = identity.IPAddress;
            taskAudit.UpdatedBy = identity.EmployeeId;
            taskAudit.UpdatedDate = System.DateTime.Now;
            taskAudit.UpdatedFromIP = identity.IPAddress;

            taskAudit.DueDate = issueTransaction.RequiredDate;
            TaskAudit taskAuditAssignBy = new TaskAudit();

            taskAuditAssignBy.AddedFromIP = identity.IPAddress;
            taskAuditAssignBy.UpdatedBy = identity.EmployeeId;
            taskAuditAssignBy.UpdatedDate = System.DateTime.Now;
            taskAuditAssignBy.UpdatedFromIP = identity.IPAddress;
            var taskManagerMasterDb = _taskManagerMasterService.GetTaskManagerMaster(issueTransaction.Id, TaskTypeEnum.InternalAudit.ToString());


            if (taskManagerMasterDb != null)
            {
                taskAudit.TaskManagerMasterId = taskManagerMasterDb.Id;
                taskAudit.AuthorizationType = AuthorizationTypeEnum.AssignTo.ToString();
                taskAudit.ResponsiblePersonId = issueTransaction.InternalResponsiblePersonId;
                _taskAuditService.Insert(taskAudit);
            }
            var taskAuditForAssignByDb = _taskAuditService.GetTaskAudit(taskAudit.TaskManagerMasterId, issueTransaction.AssignById);
            if (taskAuditForAssignByDb == null)
            {
                taskAuditAssignBy.TaskManagerMasterId = taskManagerMasterDb.Id;
                taskAuditAssignBy.AuthorizationType = AuthorizationTypeEnum.CreatedBy.ToString();
                taskAuditAssignBy.ResponsiblePersonId = issueTransaction.AssignById;
                taskAuditAssignBy.DueDate = issueTransaction.RequiredDate;
                _taskAuditService.Insert(taskAuditAssignBy);
            }
            return taskManagerMasterDb.Id;

        }

        private string CreateExternalTaskAudit(IssueTransaction issueTransaction)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            TaskAudit taskAudit = new TaskAudit();
            taskAudit.AddedFromIP = identity.IPAddress;
            taskAudit.UpdatedBy = identity.EmployeeId;
            taskAudit.UpdatedDate = System.DateTime.Now;
            taskAudit.UpdatedFromIP = identity.IPAddress;

            taskAudit.DueDate = issueTransaction.RequiredDate;
            TaskAudit taskAuditAssignBy = new TaskAudit();
            taskAuditAssignBy.AddedFromIP = identity.IPAddress;
            taskAuditAssignBy.UpdatedBy = identity.EmployeeId;
            taskAuditAssignBy.UpdatedDate = System.DateTime.Now;
            taskAuditAssignBy.UpdatedFromIP = identity.IPAddress;
            var taskManagerMasterDb = _taskManagerMasterService.GetTaskManagerMaster(issueTransaction.Id, TaskTypeEnum.ExternalAudit.ToString());


            if (taskManagerMasterDb != null)
            {
                taskAudit.TaskManagerMasterId = taskManagerMasterDb.Id;
                taskAudit.AuthorizationType = AuthorizationTypeEnum.AssignTo.ToString();
                taskAudit.ResponsiblePersonId = issueTransaction.ExternalResponsiblePersonId;

                _taskAuditService.Insert(taskAudit);
            }

            var taskAuditForAssignByDb = _taskAuditService.GetTaskAudit(taskAudit.TaskManagerMasterId, issueTransaction.AssignById);
            if (taskAuditForAssignByDb == null)
            {
                taskAuditAssignBy.TaskManagerMasterId = taskManagerMasterDb.Id;
                taskAuditAssignBy.DueDate = issueTransaction.RequiredDate;
                taskAuditAssignBy.AuthorizationType = AuthorizationTypeEnum.CreatedBy.ToString();
                taskAuditAssignBy.ResponsiblePersonId = issueTransaction.AssignById;
                _taskAuditService.Insert(taskAuditAssignBy);
            }
            return taskManagerMasterDb.Id;

        }


        private bool IsAuditReleased(string issueTrnasactionId, TaskTypeEnum auditType)
        {
            DataTable TaskManagerMasterDataTable = AllReleasedAudit(issueTrnasactionId);
            bool flag = false;
            if (TaskManagerMasterDataTable != null && TaskManagerMasterDataTable.Rows.Count > 0)
            {

                for (int i = 0; i < TaskManagerMasterDataTable.Rows.Count; i++)
                {
                    if (TaskManagerMasterDataTable.Rows[i]["TaskType"].ToString() == auditType.ToString())
                    {
                        flag = true;
                    }
                    else
                        flag = false;
                }

            }
            return flag;
        }

        [HttpPost, Authorize]
        public JsonResult IssueRelease(IssueTransaction model)
        {
            int isReleased = 0;
            string err = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            TaskManagerMaster taskManagerMaster1 = new TaskManagerMaster();
            var issueTransaction = new IssueTransaction();
            issueTransaction = _issueTransactionService.GetIssueTransaction(model.Id);
            TaskManagerMaster taskManagerMasterDb = _taskManagerMasterService.GetTaskManagerMasterByIssueTransactionId(model.Id);

            try
            {
                model.AssignById = issueTransaction.AssignById;
            }
            catch (Exception)
            {


            }
            //Add to AssignTo to the TaskManagerMaster
            if (taskManagerMasterDb == null)
            {
                taskManagerMaster1.AddedFromIP = identity.IPAddress;
                taskManagerMaster1.UpdatedBy = identity.EmployeeId;
                taskManagerMaster1.UpdatedDate = System.DateTime.Now;
                taskManagerMaster1.UpdatedFromIP = identity.IPAddress;
                taskManagerMaster1.TaskPriority = model.Priority != 0 ? model.Priority : 4.5M;

                taskManagerMaster1.TaskDetailDescription = model.IssueDetail;
                taskManagerMaster1.TaskCategoryId = model.TaskCategoryId;
                taskManagerMaster1.TaskSubCategoryId = model.TaskSubCategoryId;
                taskManagerMaster1.TaskTypeGroup = TaskTypeEnum.Issue.ToString();

                taskManagerMaster1.IssueTransactionId = model.Id;
                taskManagerMaster1.TaskType = TaskTypeEnum.Issue.ToString();
                taskManagerMaster1.TaskDescription = model.Issue;
                taskManagerMaster1.IssueTransactionId = model.Id;
                taskManagerMaster1.CurrentStatus = CurrentStatusEnum.ToStart.ToString();
                taskManagerMaster1.StoryPoint = model.StoryPoint;

                _taskManagerMasterService.InsertTaskManagerMasterForIssue(taskManagerMaster1, out string Id);
                CreateIssueTaskAudit(model);
                CreateSubTasks(model, taskManagerMaster1.Id);
            }


            if (model.IsUpdateApplicable == true)
            {
                TaskManagerMaster taskManagerMaster = new TaskManagerMaster();

                taskManagerMaster.AddedFromIP = identity.IPAddress;
                taskManagerMaster.UpdatedBy = identity.EmployeeId;
                taskManagerMaster.UpdatedDate = System.DateTime.Now;
                taskManagerMaster.UpdatedFromIP = identity.IPAddress;
                taskManagerMaster.TaskTypeGroup = TaskTypeEnum.Issue.ToString();
                taskManagerMaster.TaskSchedulerMasterId = model.UpdateAuditTaskSchedulerMasterId;
                taskManagerMaster.TaskCategoryId = model.TaskCategoryId;
                taskManagerMaster.TaskSubCategoryId = model.TaskSubCategoryId;
                taskManagerMaster.CurrentStatus = CurrentStatusEnum.ToStart.ToString();
                taskManagerMaster.StoryPoint = model.StoryPoint;

                //var returenedUpdateAudit = _issueUpdateAuditService.IsUpdateAuditReleased(model.Id);

                if (IsAuditReleased(model.Id, TaskTypeEnum.UpdateAudit))
                {

                    err += "UpdateApplicable has already been released.";
                }
                else
                {
                    var issueUpdateAudit = new IssueUpdateAudit();

                    issueUpdateAudit.AddedFromIP = identity.IPAddress;
                    issueUpdateAudit.UpdatedBy = identity.EmployeeId;
                    issueUpdateAudit.UpdatedDate = System.DateTime.Now;
                    issueUpdateAudit.UpdatedFromIP = identity.IPAddress;

                    issueUpdateAudit.DueDate = model.RequiredDate;
                    issueUpdateAudit.IsUpdateApplicable = model.IsUpdateApplicable;
                    issueUpdateAudit.UpdateResponsiblePersonId = model.UpdateResponsiblePersonId;


                    if (model.IsUpdateRecurring == true)
                    {
                        issueUpdateAudit.IsUpdateRecurring = model.IsUpdateRecurring;
                    }
                    else
                    {
                        issueUpdateAudit.UpdateOneTimeDateTime = model.UpdateOneTimeDateTime;
                    }
                    taskManagerMaster.TaskPriority = model.Priority != 0 ? model.Priority : 4.5M;
                    taskManagerMaster.TaskDetailDescription = model.IssueDetail;
                    taskManagerMaster.StoryPoint = model.StoryPoint;

                    //taskManagerMaster.TaskCategoryId = model.IssueCategoryId;

                    issueUpdateAudit.IssueTransactionId = model.Id;
                    taskManagerMaster.TaskType = TaskTypeEnum.UpdateAudit.ToString();
                    taskManagerMaster.TaskDescription = model.Issue;
                    taskManagerMaster.IssueTransactionId = model.Id;

                    model.IsReleased = true;
                    // _issueUpdateAuditService.InsertIssueUpdateAudit(issueUpdateAudit);
                    _taskManagerMasterService.Insert(taskManagerMaster);
                    var taskManagerMasterId = CreateUpdateTaskAudit(model);
                    CreateSubTasks(model, taskManagerMasterId);
                    //CreateSubTasks(issueTransaction.Id, model.UpdateResponsiblePersonId, taskManagerMasterId);
                    isReleased++;
                }

            }

            if (model.IsFollowUpApplicable == true)
            {

                TaskManagerMaster taskManagerMaster = new TaskManagerMaster();

                taskManagerMaster.AddedFromIP = identity.IPAddress;
                taskManagerMaster.UpdatedBy = identity.EmployeeId;
                taskManagerMaster.UpdatedDate = System.DateTime.Now;
                taskManagerMaster.UpdatedFromIP = identity.IPAddress;
                taskManagerMaster.TaskTypeGroup = TaskTypeEnum.Issue.ToString();
                taskManagerMaster.TaskSchedulerMasterId = model.FollowUpAuditTaskSchedulerMasterId;
                taskManagerMaster.CurrentStatus = CurrentStatusEnum.ToStart.ToString();
                taskManagerMaster.TaskCategoryId = model.TaskCategoryId;
                taskManagerMaster.TaskSubCategoryId = model.TaskSubCategoryId;
                //var returenedFollowUpAudit = _issueFollowUpAuditService.IsFollowUpAuditReleased(model.Id);
                if (IsAuditReleased(model.Id, TaskTypeEnum.FollowUpAudit))
                {
                    err += "FollowUpApplicable already been released.";
                }
                else
                {
                    var issueFollowUpAudit = new IssueFollowUpAudit();
                    issueFollowUpAudit.AddedFromIP = identity.IPAddress;
                    issueFollowUpAudit.UpdatedBy = identity.EmployeeId;
                    issueFollowUpAudit.UpdatedDate = System.DateTime.Now;
                    issueFollowUpAudit.UpdatedFromIP = identity.IPAddress;

                    issueFollowUpAudit.DueDate = model.RequiredDate;
                    issueFollowUpAudit.IsFollowUpApplicable = model.IsFollowUpApplicable;
                    issueFollowUpAudit.FollowUpResponsiblePersonId = model.FollowUpResponsiblePersonId;
                    if (model.IsFollowUpRecurring == true)
                    {
                        issueFollowUpAudit.IsFollowUpRecurring = model.IsFollowUpRecurring;
                        //issueFollowUpAudit.FollowUpFrequencyType = model.FollowUpFrequencyType;
                        //issueFollowUpAudit.FollowUpFrequencyDays = model.FollowUpFrequencyDays;
                        //issueFollowUpAudit.FollowUpEndDateTime = model.FollowUpEndDateTime;
                    }
                    else
                    {
                        issueFollowUpAudit.FollowUpOneTimeDateTime = model.FollowUpOneTimeDateTime;
                    }

                    issueFollowUpAudit.IssueTransactionId = model.Id;
                    taskManagerMaster.TaskType = TaskTypeEnum.FollowUpAudit.ToString();

                    taskManagerMaster.IssueTransactionId = model.Id;
                    taskManagerMaster.TaskDescription = model.Issue;

                    taskManagerMaster.TaskPriority = model.Priority != 0 ? model.Priority : 4.5M;
                    taskManagerMaster.TaskDetailDescription = model.IssueDetail;
                    taskManagerMaster.StoryPoint = model.StoryPoint;

                    //taskManagerMaster.TaskCategoryId = model.IssueCategoryId;

                    model.IsReleased = true;
                    //_issueFollowUpAuditService.Insert(issueFollowUpAudit);
                    _taskManagerMasterService.Insert(taskManagerMaster);
                    var taskManagerMasterId = CreateFollowUpTaskAudit(model);
                    CreateSubTasks(model, taskManagerMasterId);
                    //CreateSubTasks(issueTransaction.Id, model.FollowUpResponsiblePersonId, taskManagerMasterId);
                    isReleased++;
                }
            }

            if (model.IsInternalApplicable == true)
            {
                TaskManagerMaster taskManagerMaster = new TaskManagerMaster();

                taskManagerMaster.AddedFromIP = identity.IPAddress;
                taskManagerMaster.UpdatedBy = identity.EmployeeId;
                taskManagerMaster.UpdatedDate = System.DateTime.Now;
                taskManagerMaster.UpdatedFromIP = identity.IPAddress;
                taskManagerMaster.TaskTypeGroup = TaskTypeEnum.Issue.ToString();
                taskManagerMaster.TaskSchedulerMasterId = model.InternalAuditTaskSchedulerMasterId;
                taskManagerMaster.TaskCategoryId = model.TaskCategoryId;
                taskManagerMaster.TaskSubCategoryId = model.TaskSubCategoryId;
                taskManagerMaster.CurrentStatus = CurrentStatusEnum.ToStart.ToString();

                //var returenedInternalAudit = _issueInternalAuditService.IsInternalAuditReleased(model.Id);
                if (IsAuditReleased(model.Id, TaskTypeEnum.InternalAudit))
                {
                    err += "InternalApplicable already been released.";
                }
                else
                {
                    var issueInternalAudit = new IssueInternalAudit();

                    issueInternalAudit.AddedFromIP = identity.IPAddress;
                    issueInternalAudit.UpdatedBy = identity.EmployeeId;
                    issueInternalAudit.UpdatedDate = System.DateTime.Now;
                    issueInternalAudit.UpdatedFromIP = identity.IPAddress;

                    issueInternalAudit.DueDate = model.RequiredDate;
                    issueInternalAudit.IsInternalApplicable = model.IsInternalApplicable;
                    issueInternalAudit.InternalResponsiblePersonId = model.InternalResponsiblePersonId;

                    if (model.IsInternalRecurring == true)
                    {
                        issueInternalAudit.IsInternalRecurring = model.IsInternalRecurring;
                    }
                    else
                    {
                        issueInternalAudit.InternalOneTimeDateTime = model.InternalOneTimeDateTime;
                    }

                    issueInternalAudit.IssueTransactionId = model.Id;
                    taskManagerMaster.TaskType = TaskTypeEnum.InternalAudit.ToString();
                    // taskManagerMaster.TaskType = TaskTypeEnum.UpdateAudit.ToString();
                    //taskManagerMaster.DueDate = issueInternalAudit.DueDate;
                    taskManagerMaster.IssueTransactionId = model.Id;
                    taskManagerMaster.TaskDescription = model.Issue;

                    taskManagerMaster.TaskPriority = model.Priority != 0 ? model.Priority : 4.5M;
                    taskManagerMaster.TaskDetailDescription = model.IssueDetail;
                    taskManagerMaster.StoryPoint = model.StoryPoint;

                    //taskManagerMaster.TaskCategoryId = model.IssueCategoryId;

                    model.IsReleased = true;
                    //_issueInternalAuditService.Insert(issueInternalAudit);
                    _taskManagerMasterService.Insert(taskManagerMaster);
                    var taskManagerMasterId = CreateInternalTaskAudit(model);
                    CreateSubTasks(model, taskManagerMasterId);
                    //CreateSubTasks(issueTransaction.Id, model.InternalResponsiblePersonId, taskManagerMasterId);
                    isReleased++;
                }
            }

            if (model.IsExternalApplicable == true)
            {

                TaskManagerMaster taskManagerMaster = new TaskManagerMaster();

                taskManagerMaster.AddedFromIP = identity.IPAddress;
                taskManagerMaster.UpdatedBy = identity.EmployeeId;
                taskManagerMaster.UpdatedDate = System.DateTime.Now;
                taskManagerMaster.UpdatedFromIP = identity.IPAddress;
                taskManagerMaster.TaskTypeGroup = TaskTypeEnum.Issue.ToString();
                taskManagerMaster.TaskSchedulerMasterId = model.ExternalAuditTaskSchedulerMasterId;
                taskManagerMaster.TaskCategoryId = model.TaskCategoryId;
                taskManagerMaster.TaskSubCategoryId = model.TaskSubCategoryId;
                taskManagerMaster.CurrentStatus = CurrentStatusEnum.ToStart.ToString();
                //var returenedExternalAudit = _issueExternalAuditService.IsExternalAuditReleased(model.Id);
                if (IsAuditReleased(model.Id, TaskTypeEnum.ExternalAudit))
                {
                    err += "ExternalApplicable already been released.";
                }
                else
                {
                    var issueExternalAudit = new IssueExternalAudit();

                    issueExternalAudit.AddedFromIP = identity.IPAddress;
                    issueExternalAudit.UpdatedBy = identity.EmployeeId;
                    issueExternalAudit.UpdatedDate = System.DateTime.Now;
                    issueExternalAudit.UpdatedFromIP = identity.IPAddress;

                    issueExternalAudit.DueDate = model.RequiredDate;
                    issueExternalAudit.IsExternalApplicable = model.IsExternalApplicable;
                    issueExternalAudit.ExternalResponsiblePersonId = model.ExternalResponsiblePersonId;
                    issueExternalAudit.ExternalResponsiblePerson = model.ExternalResponsiblePerson;
                    issueExternalAudit.ExternalRespPersonEmail = model.ExternalRespPersonEmail;
                    issueExternalAudit.ExternalRespPersonDesignation = model.ExternalRespPersonDesignation;

                    if (model.IsExternalRecurring == true)
                    {

                        issueExternalAudit.IsExternalRecurring = model.IsExternalRecurring;

                    }
                    else
                    {
                        issueExternalAudit.ExternalOneTimeDateTime = model.ExternalOneTimeDateTime;
                    }

                    issueExternalAudit.IssueTransactionId = model.Id;
                    taskManagerMaster.TaskType = TaskTypeEnum.ExternalAudit.ToString();
                    //taskManagerMaster.DueDate = issueExternalAudit.DueDate;
                    taskManagerMaster.IssueTransactionId = model.Id;
                    taskManagerMaster.TaskDescription = model.Issue;
                    taskManagerMaster.TaskPriority = model.Priority != 0 ? model.Priority : 4.5M;

                    taskManagerMaster.TaskDetailDescription = model.IssueDetail;
                    taskManagerMaster.StoryPoint = model.StoryPoint;

                    //taskManagerMaster.TaskCategoryId = model.IssueCategoryId;

                    model.IsReleased = true;
                    //_issueExternalAuditService.Insert(issueExternalAudit);
                    _taskManagerMasterService.Insert(taskManagerMaster);
                    var taskManagerMasterId = CreateExternalTaskAudit(model);
                    CreateSubTasks(model, taskManagerMasterId);
                    //CreateSubTasks(issueTransaction.Id, model.ExternalResponsiblePersonId, taskManagerMasterId);
                    isReleased++;
                }
            }

            if (model.IsReleased == true)
            {
                _issueTransactionService.Update(model);
            }
            //else
            //{
            //    return Json(new { IssueTransaction = model, Message = "There is not available to release", IsSuccess = 0 });
            //}
            if (isReleased >= 1)
            {
                return Json(new { IssueTransaction = model, Message = AplosMessage.Success, IsSuccess = isReleased });
            }
            else
            {
                return Json(new { IssueTransaction = model, Message = err, IsSuccess = isReleased });
            }

        }

        /// <summary>
        /// Descared all Issue Audits 
        /// 
        /// </summary>
        /// <param name="issueTransactionId"></param>
        /// <returns></returns>

        private DataTable AllReleasedAudit(string issueTransactionId)
        {
            string sql = @" select * from TaskManagerMaster where IssueTransactionId = '" + issueTransactionId + "'";
            return _sqlRepository.GetDataTable(sql);
        }

        [HttpGet, Authorize]
        public ActionResult GetById(string issueTransactionId)
        {
            return Json(_issueTransactionService.GetById(issueTransactionId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Edit(IssueTransaction model)
        {
            issueTransactionId = model.Id;
            _issueTransactionService.Update(model);
            return Json(new { IssueTransaction = model, Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {

            _issueTransactionService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        //TaskSchedulerMaster
        //  [HttpGet, Authorize]
        //  public ActionResult GetRecurringDataForEveryAuditTaskSchedulerMasterId(List<int> auditIds)
        //  {
        //      string commasaparatedIds = "";
        //      var ErrorMessage = null;

        //      if (auditIds.Count > 0)
        //      {
        //          foreach (var item in auditIds)
        //          {
        //              if (commasaparatedIds.Length == 0)
        //              {
        //                  commasaparatedIds = "'" + item + "'";
        //              }
        //              else
        //              {
        //                  commasaparatedIds += "," + "'" + item + "'";
        //              }
        //          }

        //          string _sql = @"select [Id]
        //,[RepeatType]
        //,[EveryInterval]
        //, format( [StartDate],'dd-MMM-yyyy') AS[StartDate]
        //,format([EndDate],'dd-MMM-yyyy') AS[EndDate]
        //,[IsNever]
        //,[AfterNoOfAccurence]
        //,[WeeklyRepeatationBycommaSepDayName]
        //,[RepeatByDayNumber]
        //,[RepeatbyNthWeek]
        //,[RepeatByMonth]
        //,[RepeatbyOfEarly]
        //,[RepeatByWeek]
        //,[AddedBy]
        //,[AddedDate]
        //,[AddedFromIP]
        //,[UpdatedBy]
        //,[UpdatedDate]
        //,[UpdatedFromIP]
        //,[Details]
        //,[IsAfter]
        //,[IsOn]
        //,[isRepeatByDay]
        //,[isRepeatByTheNthWeek]
        //,[isRepeatByTheMonth]
        //,[isRepeatByTheNthWeekForMonthly]
        //  from dbo.TaskSchedulerMaster where Id where Id in(commasaparatedIds)";
        //          return Json(_sqlRepository.GetDataCollection(_sql), JsonRequestBehavior.AllowGet);
        //      }
        //      else
        //      {
        //          return Json({ ErrorMessage = null});
        //      }


        //  }


        [HttpGet, Authorize]
        public ActionResult GetTaskScheduleByAuditTaskSchedulerMasterId(string auditTaskSchedulerMasterId)
        {
            return Json(_taskSchedulerMasterService.GetTaskScheduleByAuditTaskSchedulerMasterId(auditTaskSchedulerMasterId), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetTaskManagerSubTasksByIssueTransactionId(string issueTransactionId)
        {
            string sql = @"select IST.* ,E.EmployeeName AS ResponsiblePerson from IssueSubTask AS IST LEFT JOIN EmployeeInformation E ON IST.ResponsiblePersonId = E.SystemId
            where IssueTransactionId = '" + issueTransactionId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }


        [HttpGet, Authorize]
        public ActionResult GetAuditOfReleasedIssue(string issueTransactionId, int audit)
        {
            return Json(_taskAuditService.GetAuditOfReleasedIssue(issueTransactionId, audit), JsonRequestBehavior.AllowGet);

        }



        #region IssueGroup
        [HttpGet, Authorize]
        public ActionResult GetIssueGroups()
        {
            string sql = @"SELECT IG.*, E.EmployeeName AS ResponsiblePerson FROM [dbo].[IssueGroup] IG 
                            LEFT JOIN dbo.EmployeeInformation E ON E.SystemId = IG.ResponsiblePersonId";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult CreateIssueGroup(Dictionary<string, object> issueGroup)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[IssueGroup] where 1=2", out dsMaster, false, "1");

            string _Id = "";

            #region data update
            if (dsMaster.Tables[0].Rows.Count == 0)
            {
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("Dbo.IssueGroup", out _Id);

                issueGroup["Id"] = "TC" + _Id;
                AddNewRow(dsMaster.Tables[0], issueGroup);
            }
            else
            {
                //_Id = issueGroup["Id"].ToString();
                //EditRow(dsMaster.Tables[0].Rows[0], issueGroup);
            }
            #endregion data update

            // Save to Database 
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsMaster);

            return Json(new { IssueGroup = issueGroup, Message = AplosMessage.Updated });

        }
        #endregion EndIssueGroup 
        [HttpPost, Authorize]
        public ActionResult CreateTaskSchedule(Dictionary<string, object> taskSchedule)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("select * from TaskSchedulerMaster where 1=2", out dsMaster, false, "1");

            string _Id = "";

            #region data update
            if (dsMaster.Tables[0].Rows.Count == 0)
            {
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("Dbo.TaskSchedulerMaster", out _Id);

                taskSchedule["Id"] = "TC" + _Id;
                AddNewRow(dsMaster.Tables[0], taskSchedule);
            }
            else
            {
                //_Id = taskSchedule["Id"].ToString();
                //EditRow(dsMaster.Tables[0].Rows[0], taskSchedule);
            }
            #endregion data update

            // Save to Database 
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsMaster);

            return Json(new { TaskSchedule = taskSchedule, Message = AplosMessage.Updated });

        }

        [HttpPost, Authorize]
        public ActionResult EditTaskSchedule(string auditTaskSchedulerMasterId, Dictionary<string, object> taskSchedule)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter(@"select * from dbo.TaskSchedulerMaster where Id = '" + auditTaskSchedulerMasterId + "'", out dsMaster, false, "1");

            string _Id = "";

            #region data update
            if (dsMaster.Tables[0].Rows.Count == 0)
            {
                //bplib.clsGenID genid = new bplib.clsGenID();
                //genid.GenID("Dbo.TaskSchedulerMaster", out _Id);

                //taskSchedule["Id"] = "TC" + _Id;
                //AddNewRow(dsMaster.Tables[0], taskSchedule);
            }
            else
            {
                _Id = taskSchedule["Id"].ToString();
                EditRow(dsMaster.Tables[0].Rows[0], taskSchedule);
            }
            #endregion data update

            // Save to Database 
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsMaster);

            return Json(new { TaskSchedule = taskSchedule, Message = AplosMessage.Updated });

        }

        [HttpGet, Authorize]
        public ActionResult GetTaskCategory()
        {
            string sql = "SELECT Id, UserName FROM HKP.TaskCategory where flag='" + TaskCategoryFlagEnum.Issue.ToString() + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaskSubCategory()
        {
            string sql = "SELECT Id, UserName FROM HKP.TaskSubCategory where flag='" + TaskCategoryFlagEnum.Issue.ToString() + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteGroup(string issueGroupId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[IssueGroup] where id='" + issueGroupId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("REFERENCE"))
                    return Json(new { Error = true, Message = "Selected Issue Group has been used in Issue therefor cannot delete." }, JsonRequestBehavior.AllowGet);

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = identity.EmployeeId;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.EmployeeId;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }

        [HttpPost, Authorize]
        public JsonResult CreateDocuments(FormCollection form, HttpPostedFileBase[] file)
        {
            var issueTransactionDocuments = new JavaScriptSerializer().Deserialize<IssueTransactionDocuments>(form["issueTransactionDocuments"]);

            SaveData(issueTransactionDocuments, out string docId);
            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetIssueTransactionDocumentsPath();
                var path = Path.Combine(directory);

                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }

                var fileId = "";
                var fileName = "";
                var filedata = GetFile(issueTransactionDocuments.Id);
                if (filedata.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filedata["Id"].ToString()) &&
                        !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                        fileId = filedata["Id"].ToString();
                    fileName = filedata["FileName"].ToString();

                    if (fileName != issueTransactionDocuments.FileName)
                        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }

                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + docId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + docId + Path.GetExtension(item.FileName));
                    }
                }

            }
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetIssueDocumentsData(string issueTransactionId)
        {
            var sql = @"SELECT * FROM [dbo].[IssueTransactionDocuments] WHERE IssueTransactionId='" + issueTransactionId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public Dictionary<string, object> GetFile(string systemId)
        {
            try
            {
                var sql = @"Select Id, FileName From [dbo].[IssueTransactionDocuments]  Where Id='" + systemId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetFileId(List<IssueTransactionDocuments> list, string fileName)
        {
            foreach (var ob in list)
            {
                if (ob.FileName == fileName)
                {
                    return ob.Id;
                }
            }

            return "";
        }


        bool IsValidFile(string ext)
        {
            string[] validFileFormate = { "xlsx", "xlx", "doc", "docx", "jpg", "png", "gif", "pdf" };
            for (var i = 0; i < validFileFormate.Length; i++)
            {
                string vF = "." + validFileFormate[i];
                if (vF == ext)
                {
                    return true;
                }
            }
            return false;
        }

        public void GetChildIdCount(string issueTransactionId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT Count(*) Count FROM [dbo].[IssueTransactionDocuments] where IssueTransactionId='" + issueTransactionId + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetIssueDocumentPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(IssueTransactionDocuments), out sID);
            return sID;
        }

        private void SaveData(IssueTransactionDocuments data, out string docId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = "SELECT * FROM [dbo].[IssueTransactionDocuments] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = data.IssueTransactionId + "-" + GetIssueDocumentPK();
                    dr["IssueTransactionId"] = data.IssueTransactionId;
                    dr["FileName"] = data.FileName;
                    dr["Description"] = data.Description;

                    dr["AddedBy"] = identity.EmployeeId;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["FileName"] = data.FileName;
                    dr["Description"] = data.Description;
                    dr["IssueTransactionId"] = data.IssueTransactionId;

                    dr["UpdatedBy"] = identity.EmployeeId;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                docId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public JsonResult DeleteDocument(string id)
        {
            var directory = ResourcesPathReader.GetIssueTransactionDocumentsPath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = GetFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["Id"].ToString()) &&
                !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["Id"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            string strCSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strCSQL = "DELETE FROM [dbo].[IssueTransactionDocuments] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strCSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function


        #region upload product picture
        [HttpPost, Authorize]
        public ActionResult SaveDefault(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                UploadDefault_data = UploadDefault_data.Replace("\\", "");
                DataTable AdditionalData = CustomJsonResult.ToDataTable(UploadDefault_data);

                if (string.IsNullOrEmpty(AdditionalData.Rows[0]["IssueTransactionId"].ToString()))
                    throw new Exception("Save the Issue Transaction first.");




                foreach (var file in UploadDefault)
                {

                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "SELECT * FROM dbo.IssueTransactionDocuments WHERE Id='" + AdditionalData.Rows[0]["Id"].ToString() + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["FileName"] = file.FileName;
                        dsLocal.Tables[0].Rows[0]["Description"] = AdditionalData.Rows[0]["Description"].ToString();

                        dsLocal.Tables[0].Rows[0]["UpdatedBy"] = identity.EmployeeId;
                        dsLocal.Tables[0].Rows[0]["UpdatedFromIP"] = identity.IPAddress;
                        dsLocal.Tables[0].Rows[0]["UpdatedDate"] = System.DateTime.Now.ToString();


                        dsLocal.Tables[0].Rows[0].EndEdit();

                        var fileName = Path.GetFileName(dsLocal.Tables[0].Rows[0]["Id"] + new FileInfo(file.FileName).Extension);
                        var destinationPath = Path.Combine(ResourcesPathReader.GetIssueTransactionDocumentsPath(), fileName);

                        if (System.IO.Directory.Exists(ResourcesPathReader.GetIssueTransactionDocumentsPath()) == false)
                        {
                            try
                            {
                                System.IO.Directory.CreateDirectory(ResourcesPathReader.GetIssueTransactionDocumentsPath());
                            }
                            catch (Exception ex)
                            {

                            }
                        }


                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                    else
                    {
                        DataRow dr = dsLocal.Tables[0].NewRow();

                        dr["Id"] = AdditionalData.Rows[0]["IssueTransactionId"].ToString() + "-" + GetIssueDocumentPK();
                        dr["IssueTransactionId"] = AdditionalData.Rows[0]["IssueTransactionId"].ToString();
                        dr["FileName"] = file.FileName;
                        dr["Description"] = AdditionalData.Rows[0]["Description"].ToString();

                        dr["AddedBy"] = identity.EmployeeId;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dr["UpdatedBy"] = identity.EmployeeId;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;


                        dsLocal.Tables[0].Rows.Add(dr);

                        var fileName = Path.GetFileName(dr["Id"].ToString() + new FileInfo(file.FileName).Extension);
                        var destinationPath = Path.Combine(ResourcesPathReader.GetIssueTransactionDocumentsPath(), fileName);

                        if (System.IO.Directory.Exists(ResourcesPathReader.GetIssueTransactionDocumentsPath()) == false)
                        {
                            try
                            {
                                System.IO.Directory.CreateDirectory(ResourcesPathReader.GetIssueTransactionDocumentsPath());
                            }
                            catch (Exception ex)
                            {

                            }
                        }


                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);
                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }

        [Authorize]
        public ActionResult RemoveDefault(string[] fileNames)
        {
            foreach (var fullName in fileNames)
            {
                var fileName = Path.GetFileName(fullName);
                var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            return Content("");
        }

        #endregion upload product picture

        #endregion -- Operations
    }
}