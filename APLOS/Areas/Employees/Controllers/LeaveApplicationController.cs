using Aplos.Controllers;
using Aplos.HumanResource;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Biometrics;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Security.Core;
using Library.Service.Biometrics;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class LeaveApplicationController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly ILeaveTransectionService _leaveTransactionService;
        private readonly IEmployeeProfileService _employeeProfileService;

        public LeaveApplicationController(
            ISqlRepository sqlRepository,
             ILeaveTransectionService leaveTransactionService,
              IEmployeeProfileService employeeProfileService
            )
        {
            _sqlRepository = sqlRepository;
            _leaveTransactionService = leaveTransactionService;
            _employeeProfileService = employeeProfileService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult LeaveApply()
        {
            return View();
        }

        public ActionResult LeaveApp()
        {
            return View();
        }
        public ActionResult LeaveDelete()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult getLeavePolicy()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT PolicyName FROM [dbo].[LeavePolicyMaster] WHERE GroupID='" + identity.CompanyGroupId + "' and PlantID='" + identity.PlantId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(new
            {
                data
            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string yearNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.EmployeeId, yearNo), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetEmpLeaveList(GridParameter parameters, string EmpsystemId, string yearNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, EmpsystemId, yearNo), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmpLeaveListForDelete(GridParameter parameters, string EmpsystemId, string yearNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.QueryGetLeaveListForDelete(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, EmpsystemId, yearNo), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSectionEmployeeList(string sectionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(_employeeProfileService.GetSectionEmployeeList(identity.PlantId, identity.CompanyId, sectionId), JsonRequestBehavior.AllowGet);
            //JsonResult json = Json(_employeeProfileService.GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeList()
        {
            EmployeeProfile employeeProfile = new EmployeeProfile();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_employeeProfileService.GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            JsonResult json = Json(employeeProfile.GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.LoadLeaveTypeCbo(identity.PlantId, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetLeaveTypeCbo(string EmpsystemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.LoadLeaveTypeCbo(identity.PlantId, EmpsystemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetYearCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.LoadYearCbo(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLeaveBalance(string calanderYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_leaveTransactionService.LoadGrdAllocatedLvDetails(identity.CompanyGroupId, identity.PlantId, identity.EmployeeId, calanderYearId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmpLeaveBalance(string EmpsystemId, string calanderYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.LoadGrdAllocatedLvDetails(identity.CompanyGroupId, identity.PlantId, EmpsystemId, calanderYearId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(LeaveTransaction leaveApplication)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            leaveApplication.GroupID = identity.CompanyGroupId;
            leaveApplication.CompanyId = identity.CompanyId;
            leaveApplication.PlantID = identity.PlantId;
            leaveApplication.EmpSystemID = identity.EmployeeId;
            leaveApplication.AddedBy = identity.Name;
            leaveApplication.AppliedBy = AppliedBy.Self.ToString();
            if (string.IsNullOrEmpty(leaveApplication.AppliedDate.ToString()))
            {
                leaveApplication.AppliedDate = DateTime.Now;
            }
            string yearId = null;

            _leaveTransactionService.SaveData(leaveApplication, yearId);
            return Json(new { LeaveApplication = leaveApplication, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Save(LeaveTransaction leaveApplication, string yearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            leaveApplication.GroupID = identity.CompanyGroupId;
            leaveApplication.CompanyId = identity.CompanyId;
            leaveApplication.PlantID = identity.PlantId;
            leaveApplication.AddedBy = identity.Name;
            leaveApplication.AppliedBy = AppliedBy.Self.ToString();
            leaveApplication.AppliedDate = DateTime.Now;
            _leaveTransactionService.SaveAndUpdateData(leaveApplication, yearId);
            return Json(new { LeaveApplication = leaveApplication, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(LeaveTransaction leaveApplication)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            leaveApplication.UpdatedBy = identity.Name;
             string yearId = null;
            _leaveTransactionService.SaveData(leaveApplication, yearId);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _leaveTransactionService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });

        }

        public ActionResult DeleteApprovedLeave(string id, string EmpSystemid)
        {
            _leaveTransactionService.DeleteApprovedLeaveGraph(id, EmpSystemid);
            return Json(new { Message = AplosMessage.Deleted });

        }
        [HttpGet, Authorize]
        public ActionResult LoadYearlyCalendar()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"select * from YearlyCalendar where  PlantId='" + identity.PlantId + @"' and IsYearEndClosed=0";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;



            //return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetEmpInfo(string SearchValue)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        WHERE emp.PlantID='" + identity.PlantId + @"'  and EMP.CompanyId='" + identity.CompanyId + @"' and EMP.EmployeeStatus='Active' 
                                        And emp.EmployeeCode='" + SearchValue + @"'
                                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        private string GetDate(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            try
            {
                return Convert.ToDateTime(s).ToString("dd-MMM-yyyy");
            }
            catch (Exception)
            {
                return "";
            }
        }

        public ActionResult LeaveAppReportExcelFormat(ReportFormat reportFormat, string employeeId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //var reportFileName = "Leave App Report";
            var workbook = GetLeaveAppReportWorkSheet(out string reportFileName, employeeId);

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);
                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);
                default:
                    return RenderReportAsExcelx(workbook, reportFileName);
            }
        }

        private IWorkbook GetLeaveAppReportWorkSheet(out string reportFileName, string employeeId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "LeaveAppReport";


            int ROW = 5;
            int endCol = 1;
            int COL = 1;

            var header = LeaveAppReportHeader(employeeId);

            reportFileName = "Leave App Report";

            DataTable data = GetLeaveAppData(employeeId);


            #region Headers


            report.SetMasterHeaderText(ref sheet, ROW, 1, "Employee Name");
            sheet[ROW, 1].ColumnWidth = 20;
            sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            report.SetText(ref sheet, ROW, 2, header["EmployeeName"].ToString());
            sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(5) + ROW].Merge();
            sheet[ROW, 2].ColumnWidth = 20;
            sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            report.SetMasterHeaderText(ref sheet, ROW, 6, "Department");
            sheet[ROW, 6].ColumnWidth = 25;
            sheet.Range[ROW, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            report.SetText(ref sheet, ROW, 7, header["Department"].ToString());
            sheet[report.GetColumnNameForXls(7) + ROW + ":" + report.GetColumnNameForXls(10) + ROW].Merge();
            sheet[ROW, 7].ColumnWidth = 25;
            sheet.Range[ROW, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            ROW++;

            report.SetMasterHeaderText(ref sheet, ROW, 1, "Section");
            report.SetText(ref sheet, ROW, 2, header["Section"].ToString());
            sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(5) + ROW].Merge();
            sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            report.SetMasterHeaderText(ref sheet, ROW, 6, "Designation");
            report.SetText(ref sheet, ROW, 7, header["Designation"].ToString());
            sheet[report.GetColumnNameForXls(7) + ROW + ":" + report.GetColumnNameForXls(10) + ROW].Merge();
            sheet.Range[ROW, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[ROW, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            ROW++;
            ROW++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 20, ExcelHAlign.HAlignLeft);
            //int ColEmployeeName = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Leave Type", 15, ExcelHAlign.HAlignLeft);
            int ColLeaveType = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Carry Forward", 12, ExcelHAlign.HAlignRight);
            int ColCarryForward = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Current Year Allocation", 12, ExcelHAlign.HAlignRight);
            int ColCurrentYearAllocation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Carry Forward Opening Balance", 10, ExcelHAlign.HAlignRight);
            int ColCarryForwardOpeningBalance = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "BroughtForward", 15, ExcelHAlign.HAlignRight);
            int ColBroughtForward = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Year End Encash", 15, ExcelHAlign.HAlignRight);
            int ColYearEndEncash = COL;
            COL++;
             
            report.SetHeaderText(ref sheet, ROW, COL, "Year End Lapse", 15, ExcelHAlign.HAlignRight);
            int ColYearEndLapse = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Availed Leave", 10, ExcelHAlign.HAlignRight);
            int ColAvailedLeave = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Balance", 10, ExcelHAlign.HAlignRight);
            int ColBalance = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Calculated Earning Days", 15, ExcelHAlign.HAlignRight);
            int ColCalculatedEarningDays = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "From Date", 10, ExcelHAlign.HAlignLeft);
            int ColFromDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "To Date", 10, ExcelHAlign.HAlignLeft);
            int ColToDate = COL;


            endCol = COL;
            #endregion Headers

            var startRow = 0;

            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {

                //sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColLeaveType].Text = data.Rows[i]["LeaveType"].ToString();
                sheet[ROW, ColCarryForward].Number = clsStaticInfo.dbl(data.Rows[i]["CarryForward"].ToString());
                sheet[ROW, ColCarryForward].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
               
                sheet[ROW, ColCurrentYearAllocation].Number = clsStaticInfo.dbl(data.Rows[i]["CurrentYearAllocation"].ToString());
                sheet[ROW, ColCurrentYearAllocation].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColCarryForwardOpeningBalance].Number = clsStaticInfo.dbl(data.Rows[i]["CarryForwardOpeningBalance"].ToString());
                sheet[ROW, ColCarryForwardOpeningBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColBroughtForward].Number = clsStaticInfo.dbl(data.Rows[i]["BroughtForward"].ToString());
                sheet[ROW, ColBroughtForward].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColYearEndEncash].Number = clsStaticInfo.dbl(data.Rows[i]["YearEndEncash"].ToString());
                sheet[ROW, ColYearEndEncash].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColYearEndLapse].Number = clsStaticInfo.dbl(data.Rows[i]["YearEndLapse"].ToString());
                sheet[ROW, ColYearEndLapse].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColAvailedLeave].Number = clsStaticInfo.dbl(data.Rows[i]["AvailedLeave"].ToString());
                sheet[ROW, ColAvailedLeave].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColCalculatedEarningDays].Number = clsStaticInfo.dbl(data.Rows[i]["CalculatedEarningDays"].ToString());
                sheet[ROW, ColCalculatedEarningDays].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColFromDate].Text = GetDate(data.Rows[i]["FromDate"].ToString());
                sheet[ROW, ColToDate].Text = GetDate(data.Rows[i]["ToDate"].ToString());

                sheet[ROW, ColBalance].Formula = OTSBD.clsStaticInfo.GetxlsCol(ColCurrentYearAllocation) + ROW + "+" + 
                                                 OTSBD.clsStaticInfo.GetxlsCol(ColCarryForwardOpeningBalance) + ROW + "+"+ 
                                                 OTSBD.clsStaticInfo.GetxlsCol(ColBroughtForward) + ROW + "-"+
                                                 OTSBD.clsStaticInfo.GetxlsCol(ColYearEndEncash) + ROW + "-" +
                                                 OTSBD.clsStaticInfo.GetxlsCol(ColYearEndLapse) + ROW + "-" +
                                                 OTSBD.clsStaticInfo.GetxlsCol(ColAvailedLeave) + ROW;

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }
           
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.00";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8f;
            report.CompanyHeader(ref sheet, endCol, "Leave App Report", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }


        private Dictionary<string, object> LeaveAppReportHeader(string employeeId)
        {
            var cmdText = @"SELECT ei.systemId,ei.EmployeeCode,ei.EmployeeName,ei.GenderID,FORMAT(ei.DOJ,'dd-MMM-yyyy')DOJ,En.UserName EmployeeCategory,dp.UserName Department,SE.UserName Section,ISNULL(Li.UserName,'') Line
                                    ,Deg.UserName Designation
                                    FROM EmployeeInformation AS ei 
                                    LEFT JOIN MST.ManpowerBudget PMB ON ei.BudgetCode = PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                    LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id
                                    LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                                    LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = ei.LegalDesignationId
                                    LEFT join [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                                    left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                                    left join HKP.Designation DeG on DeG.Id=dm.DesignationId
                                    left join HKP.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
                                    left join ORG.Section SE on SE.Id=PR.SectionId
                                    LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                    LEFT JOIN ORG.Line AS Li ON Li.Id= PMB.LineId

                                    where ei.SystemId='" + employeeId + "'";
            return _sqlRepository.GetData(cmdText);
        }

        private DataTable GetLeaveAppData(string employeeId)
        {
            try
            {
                var sql = @"select ELS.EmployeeId,EMP.EmployeeName,LT.UserName LeaveType,ELS.CarryForward,ELS.CurrentYearAllocation,ELS.CarryForwardOpeningBalance
				,ELS.BroughtForward,ELS.YearEndEncash,ELS.YearEndLapse,ELS.CalculatedEarningDays,
				(
                                			SELECT SUM(ltdx.LeaveDuration)
                                			FROM LeaveTransaction AS ltx
                                			JOIN LeaveTransactionDetails AS ltdx ON ltdx.LvTrnsSystemID = ltx.SystemID
                                			WHERE ltx.IsApproved = 1
                                				AND ltdx.WorkDate BETWEEN els.FromDate
                                					AND els.ToDate
                                				AND ltx.EmpSystemID = ELS.EmployeeId
                                				AND ltx.LTSystemID = els.LeaveTypeId
                                			) AvailedLeave
											,FORMAT(ELS.FromDate,'dd-MMM-yyyy') FromDate
				,format(ELS.ToDate,'dd-MMM-yyyy') ToDate
				from TRN.EmployeeLeaveSummary ELS
				left join EmployeeInformation EMP on EMP.SystemId=ELS.EmployeeId
				left join LeaveType LT on LT.Id=ELS.LeaveTypeId

				where ELS.EmployeeId='" + employeeId + "' order by lt.Id,  ELS.FromDate";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion -- Operations
    }
}