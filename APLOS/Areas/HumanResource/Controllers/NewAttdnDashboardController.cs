using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.HumanResource.NewAttendanceProcess;
using Library.HumanResource.Payroll.Allowance;
using Library.OrderManagement.Production;
using Library.Service.Helpers;
using Library.Service.Organizations;
using Library.ViewModel.Accounts;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class NewAttdnDashboardController : BaseController
    {
        // private readonly IManpowerBudgetDashboardService na;

        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        NewAttdnDashboardService na = new NewAttdnDashboardService();

        public NewAttdnDashboardController()
        {

        }

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetGroupWiseCompanyList(string date, string stat, string EmpCat, string EmpStat, string EmpShift)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data = na.GroupWiseCompanyList(identity.CompanyGroupId, date, stat, EmpCat, EmpStat , EmpShift);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetDrillDownListJSON(string CompanyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(na.DrillDownList(identity.CompanyGroupId, CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCompanyDrillDownListJSON(string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(na.CompanyWiseDrillDownList(identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetDetailDrillDownTable(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string stat, string EmpCat, string EmpStat, string EmpShift)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(na.DetailDrillDownTable(ChartColumnList, seq, date, identity.CompanyGroupId, stat, EmpCat, EmpStat, EmpShift), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DetailTableClick(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string Column, Dictionary<string, string> data, string stat, string EmpCat, string EmpStat,string EmpShift)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var JsonData = Json(na.DetailTableClick(ChartColumnList, seq, date, identity.CompanyGroupId, Column, data, stat, EmpCat, EmpStat, EmpShift), JsonRequestBehavior.AllowGet); ;
            JsonData.MaxJsonLength = int.MaxValue;
            return JsonData;
        }

        [HttpPost, Authorize]
        public ActionResult GetPrintReport(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string Column, Dictionary<string, string> data, string stat, string EmpCat, string EmpStat,string EmpShift)
        {

            try
            {
                var workbook = GetFilterData(ChartColumnList, seq, date, Column, data, stat, EmpCat, EmpStat, EmpShift);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + "-" + Column + "-" + "EmpReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        // Aman
        [HttpGet, Authorize]
        public ActionResult GetShift()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsDiscreteAllowanceReport ep = new clsDiscreteAllowanceReport();
                return Json(ep.GetShift(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        private IWorkbook GetFilterData(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string Column, Dictionary<string, string> data, string stat, string EmpCat, string EmpStat,string EmpShift)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Employee Attdn Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            DataTable dtData = na.ReportDownloadSvc(ChartColumnList, seq, date, identity.CompanyGroupId, Column, data, stat, EmpCat, EmpStat, EmpShift);


            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 13, ExcelHAlign.HAlignCenter);
            int ColCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 13, ExcelHAlign.HAlignCenter);
            int ColName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category", 13, ExcelHAlign.HAlignCenter);
            int ColEmployeeCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Day Status", 13, ExcelHAlign.HAlignCenter);
            int ColDStat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "In Status", 13, ExcelHAlign.HAlignCenter);
            int ColInStat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "In Time", 13, ExcelHAlign.HAlignCenter);
            int ColITime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Out Time", 13, ExcelHAlign.HAlignCenter);
            int ColOTime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Physical In Time", 13, ExcelHAlign.HAlignCenter);
            int ColPITime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Physical Out Time", 13, ExcelHAlign.HAlignCenter);
            int ColPOTime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "In Time Difference", 13, ExcelHAlign.HAlignCenter);
            int ColInD = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Out Time Difference", 13, ExcelHAlign.HAlignCenter);
            int ColOutD = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OT Hour", 13, ExcelHAlign.HAlignCenter);
            int ColOTHour = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 13, ExcelHAlign.HAlignCenter);
            int ColDesg = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Legal Designation", 13, ExcelHAlign.HAlignCenter);
            int ColLDesg = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Budget Code", 15, ExcelHAlign.HAlignCenter);
            int ColBudCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shift", 13, ExcelHAlign.HAlignCenter);
            int ColShift = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Seub Section", 13, ExcelHAlign.HAlignCenter);
            int ColSubSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 13, ExcelHAlign.HAlignCenter);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 13, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 13, ExcelHAlign.HAlignCenter);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Unit", 13, ExcelHAlign.HAlignCenter);
            int ColUnit = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Line", 13, ExcelHAlign.HAlignCenter);
            int ColLine = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 13, ExcelHAlign.HAlignCenter);
            int ColPlant = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Scanned By", 13, ExcelHAlign.HAlignCenter);
            int ColScan = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Scan Department", 13, ExcelHAlign.HAlignCenter);
            int ColSDept = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Scan Section", 13, ExcelHAlign.HAlignCenter);
            int ColSSec = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Scan Sub Section", 13, ExcelHAlign.HAlignCenter);
            int ColSSubSec = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Transport", 13, ExcelHAlign.HAlignCenter);
            int ColTransport = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Block", 13, ExcelHAlign.HAlignCenter);
            int ColResidence = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entry Type", 13, ExcelHAlign.HAlignCenter);
            int ColEntryType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Mobile No", 13, ExcelHAlign.HAlignCenter);
            int ColMobileNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PR1 Name", 13, ExcelHAlign.HAlignCenter);
            int ColPR1Name = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "RO1 Name", 13, ExcelHAlign.HAlignCenter);
            int ColRO1Name = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Current Status", 13, ExcelHAlign.HAlignCenter);
            int ColEmployeeCurrentStatus = COL;

            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                sheet[ROW, ColCode].Text = dtData.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColName].Text = dtData.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColEmployeeCategory].Text = dtData.Rows[i]["EmployeeCategory"].ToString();
                sheet[ROW, ColDStat].Text = dtData.Rows[i]["DayStatus"].ToString();
                sheet[ROW, ColInStat].Text = dtData.Rows[i]["InStatus"].ToString();
                sheet[ROW, ColITime].Text = dtData.Rows[i]["InTime"].ToString();
                sheet[ROW, ColOTime].Text = dtData.Rows[i]["OutTime"].ToString();
                sheet[ROW, ColPOTime].Text = dtData.Rows[i]["PVOut"].ToString();
                sheet[ROW, ColPITime].Text = dtData.Rows[i]["PVIn"].ToString();
                sheet[ROW, ColInD].Text = dtData.Rows[i]["InDuration"].ToString();
                sheet[ROW, ColOutD].Text = dtData.Rows[i]["OutDuration"].ToString();
                sheet[ROW, ColDesg].Text = dtData.Rows[i]["Designation"].ToString();
                sheet[ROW, ColLDesg].Text = dtData.Rows[i]["LDesignation"].ToString();
                sheet[ROW, ColBudCode].Text = dtData.Rows[i]["BudgetCode"].ToString();
                sheet[ROW, ColShift].Text = dtData.Rows[i]["Shift"].ToString();
                sheet[ROW, ColSubSection].Text = dtData.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColSection].Text = dtData.Rows[i]["Section"].ToString();
                sheet[ROW, ColDepartment].Text = dtData.Rows[i]["Department"].ToString();
                sheet[ROW, ColEntity].Text = dtData.Rows[i]["Entity"].ToString();
                sheet[ROW, ColUnit].Text = dtData.Rows[i]["Unit"].ToString();
                sheet[ROW, ColLine].Text = dtData.Rows[i]["Line"].ToString();
                sheet[ROW, ColPlant].Text = dtData.Rows[i]["Plant"].ToString();
                sheet[ROW, ColScan].Text = dtData.Rows[i]["ScanName"].ToString();
                sheet[ROW, ColSDept].Text = dtData.Rows[i]["SDept"].ToString();
                sheet[ROW, ColSSec].Text = dtData.Rows[i]["SSec"].ToString();
                sheet[ROW, ColSSubSec].Text = dtData.Rows[i]["SSubSec"].ToString();
                sheet[ROW, ColOTHour].Text = dtData.Rows[i]["OThour"].ToString();
                sheet[ROW, ColTransport].Text = dtData.Rows[i]["Transport"].ToString();
                sheet[ROW, ColResidence].Text = dtData.Rows[i]["Residence"].ToString();
                sheet[ROW, ColResidence].Text = dtData.Rows[i]["Residence"].ToString();
                sheet[ROW, ColEntryType].Text = dtData.Rows[i]["EntryType"].ToString();
                sheet[ROW, ColMobileNo].Text = dtData.Rows[i]["MobileNo"].ToString();
                sheet[ROW, ColPR1Name].Text = dtData.Rows[i]["PREmployeeName"].ToString();
                sheet[ROW, ColRO1Name].Text = dtData.Rows[i]["ROEmployeeName"].ToString();
                sheet[ROW, ColEmployeeCurrentStatus].Text = dtData.Rows[i]["EmployeeCurrentStatus"].ToString();


                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Employee Attdn Report", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }

        [HttpPost, Authorize]
        public ActionResult GetOnRolePrintReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                string fileName = "";

                fileName = _productionSummaryData.OnRolePrintReport(data, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}