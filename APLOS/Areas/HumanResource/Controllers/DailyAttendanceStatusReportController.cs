using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll;
using Library.HumanResource.Payroll.Report;
using Library.HumanResource.Report.OT;
using Library.Model.HumanResources;
using Library.Security.Core;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc;
namespace Aplos.Areas.HumanResource.Controllers
{
    public class DailyAttendanceStatusReportController : Controller
    {
        private readonly SqlRepository _sqlRepository;
        public DailyAttendanceStatusReportController()
        {
            _sqlRepository = new SqlRepository();
        }
        public ActionResult Aplos()
        {
            return View();
        }

        public JsonResult GetShift()
        {
            try
            {
                var sql = @"select SystemID Value, UserName Text from ShiftDefination";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        #region Report

        [HttpPost, Authorize]
        public ActionResult GetDailyAttendanceStatusXls()
        {
            try
            {

                string fileName = "";
                fileName = DailyAttendanceStatusReport("DailyAttendanceStatusReport");

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        public string DailyAttendanceStatusReport(string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";

            try
            {

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Daily Attendance Status Report";
                sheet = workbook.Worksheets[0];
                DataTable data;
                DailyAttdnStatusReportQry(out data);

                int ROW = 6; int COL = 1;

                #region Columns
                sheet[ROW, COL].Text = "SrlNo";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSrlNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "Division";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDivision = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDepartment = COL;
                COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Sub Section";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColSubSec = COL;
                COL++;

                sheet[ROW, COL].Text = "Activity";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColActivity = COL;
                COL++;

                sheet[ROW, COL].Text = "Designation";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDesignation = COL;
                COL++;

                sheet[ROW, COL].Text = "Given Designation";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColGivenDesignation = COL;
                COL++;

                sheet[ROW, COL].Text = "Shift";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColShift = COL;
                COL++;

                sheet[ROW, COL].Text = "Budget Code";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColBudgetCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColEmpCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Name";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColEmployeeName = COL;
                COL++;

                sheet[ROW, COL].Text = "State";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColState = COL;
                COL++;

                sheet[ROW, COL].Text = "Mobile No";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColMobileNo = COL;
                COL++;

                sheet[ROW, COL].Text = "DOJ";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDOJ = COL;
                COL++;

                sheet[ROW, COL].Text = "DOS";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDOS = COL;
                COL++;

                sheet[ROW, COL].Text = "Day Status";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDaySts = COL;
                COL++;

                sheet[ROW, COL].Text = "In Time";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColInTime = COL;
                COL++;

                sheet[ROW, COL].Text = "InStatus";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColInStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "LateIn";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColLateIn = COL;
                COL++;

                sheet[ROW, COL].Text = "InActive";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColInActive = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Status";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColEmpStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColResPerson = COL;
                COL++;

                sheet[ROW, COL].Text = "Team Leader";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColTeamLeader = COL;
                COL++;

                sheet[ROW, COL].Text = "Feedback";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColFeedback = COL;
                COL++;

                sheet[ROW, COL].Text = "Reason";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColReason = COL;
                COL++;

                sheet[ROW, COL].Text = "Feedback By";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColFeedbackBy = COL;
                COL++;

                sheet[ROW, COL].Text = "Residence Status";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColResidenceStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Verification Status";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColVerifStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Verified By";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColVerifiedBy = COL;
                COL++;

                sheet[ROW, COL].Text = "RO Budget Code";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColROBudgetCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Transport Status";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColTransportStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Category";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColEmployeeCategory = COL;
               
                #endregion Columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                int startRow = ROW;
                double[] arr = new double[3];
                for (int i = 0; i < data.Rows.Count; i++)
                {

                    sheet[ROW, ColSrlNo].Number = clsStaticInfo.dbl(data.Rows[i]["SrlNo"].ToString());
                    sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, ColDivision].Text = data.Rows[i]["Division"].ToString();
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                    //sheet[ROW, ColSection].DateTime = Convert.ToDateTime(data.Rows[i]["DOJ"].ToString());
                    sheet[ROW, ColSubSec].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, ColActivity].Text = data.Rows[i]["Activity"].ToString();
                    sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                    sheet[ROW, ColShift].Text = data.Rows[i]["Shift"].ToString();
                    sheet[ROW, ColBudgetCode].Number = clsStaticInfo.dbl(data.Rows[i]["BudgetCode"].ToString());
                    sheet[ROW, ColEmpCode].Number = clsStaticInfo.dbl(data.Rows[i]["EmployeeCode"].ToString());
                    sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, ColMobileNo].Text = data.Rows[i]["CellPhnNo"].ToString();
                    sheet[ROW, ColDOJ].DateTime = Convert.ToDateTime(data.Rows[i]["DOJ"].ToString());
                    sheet[ROW, ColDOS].Text = data.Rows[i]["DOS"].ToString();
                    sheet[ROW, ColInTime].DateTime = Convert.ToDateTime(data.Rows[i]["InTime"].ToString());
                    sheet[ROW, ColDaySts].Text = data.Rows[i]["DayStatus"].ToString();
                    sheet[ROW, ColInStatus].Text = data.Rows[i]["InStatus"].ToString();
                    sheet[ROW, ColLateIn].Text = data.Rows[i]["LateIn"].ToString();
                    sheet[ROW, ColInActive].Text = data.Rows[i]["InActive"].ToString();                   
                    sheet[ROW, ColEmpStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                    sheet[ROW, ColResPerson].Text = data.Rows[i]["ResponsiblePerson"].ToString();
                    sheet[ROW, ColTeamLeader].Text = data.Rows[i]["TeamLeader"].ToString();
                    sheet[ROW, ColFeedback].Text = data.Rows[i]["Feedback"].ToString();
                    sheet[ROW, ColReason].DateTime = Convert.ToDateTime(data.Rows[i]["FeedbackDate"].ToString());
                    sheet[ROW, ColReason].Text = data.Rows[i]["FeedbackRason"].ToString();
                    //sheet[ROW, ColFeedbackBy].Text = data.Rows[i]["FeedbackBy"].ToString();
                    sheet[ROW, ColEmployeeCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, ColGivenDesignation].Text = data.Rows[i]["GivenDesignation"].ToString();                    
                    sheet[ROW, ColResidenceStatus].Text = data.Rows[i]["isOccupied"].ToString();                  
                    sheet[ROW, ColVerifStatus].Text = data.Rows[i]["ApprovedStatus"].ToString();
                    sheet[ROW, ColVerifiedBy].Text = data.Rows[i]["ApprovedBy"].ToString();
                    sheet[ROW, ColROBudgetCode].Text = data.Rows[i]["ROBudgetCode"].ToString();
                    sheet[ROW, ColTransportStatus].Text = data.Rows[i]["AssignStatus"].ToString();


                    ROW++;
                }



                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Daily Attendance Status Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = true;
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;


                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public void DailyAttdnStatusReportQry(out DataTable data)
        {
            string strSQL;
            //var sqlCondition = "";
            try
            {

                
                strSQL = @"Select ROW_NUMBER() OVER(ORDER BY APD.WorkDate DESC) SrlNo, UN.UserName Entity, D.UserName Division, DP.UserName Department, SC.UserName Section, SBC.UserName SubSection, POS.Activity, DM.UserName Designation, LDSG.UserName GivenDesignation
, ST.UserName [Shift], MBGT.Code BudgetCode, EMP.EmployeeCode, EMP.EmployeeName, EMP.CellPhnNo, S.UserName [State], EMP.DOJ, EMP.DOS, EC.UserName EmployeeCategory , APD.DayStatus, APD.InStatus, APD.InTime, APD.LateIn, ''InActive, EMP.EmployeeStatus
,''ResponsiblePerson, ''TeamLeader, EFB.Action Feedback, EFB.AddedDate FeedbackDate, ARM.UserName FeedbackRason,  RG.IsResidenceApplicable, RAE.isOccupied, ETA.AssignStatus
,MBGT.ROBudgetCode, EI2.EmployeeName ApprovedBy,
ApprovedStatus = case when PV.AddedBy is not null then 'Approved' else 'Not Approved' end

from AttdnProcessData APD
left join EmployeeInformation EMP on EMP.SystemId = APD.EmpSystemID
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
LEFT JOIN ORG.Division D on D.Id = EMP.DivisionId
LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
left join dbo.ShiftDefination ST on ST.SystemID = MBGT.ShiftDefinationId
left join ORG.Entity UN on UN.Id = MBGT.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
left join hkp.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
left join EmployeeFeedback EFB on EFB.EmpSystemId = EMP.SystemId
left join [HKP].[AbsentismReasoningMaster] ARM on ARM.Id = EFB.ReasoningId
--left join EmployeeInformation EI on EI.SystemId = EFB.EmpSystemId
LEFT JOIN ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId 
LEFT JOIN ResidenceAllocatedEmployees RAE on RAE.EmployeeSystemId = EMP.SystemId
LEFT JOIN ResidenceMaster RM on RM.Id = RAE.ResidenceId
left join EmployeeTransportAllocation ETA on ETA.EmployeeSystemId = EMP.SystemId
left join SCS.[State] S on S.Id = EMP.ParmStateId
left join PhysicalVerification PV on PV.EmpSystemID = EMP.SystemId
left join EmployeeInformation EI2 on EI2.SystemId = PV.EmpSystemID
 --where EMP.EmployeeStatus = 'Active'

where APD.EmpSystemID = '2015116' and APD.LateIn > 0
--EMP.SystemId = '2015116' --and LateIn > 0 or EarlyIn > 0 
order by APD.WorkDate DESC
";


                data = _sqlRepository.GetDataTable(strSQL);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }//End Function
        #endregion
    }
}