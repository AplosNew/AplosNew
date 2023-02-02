#region LIB
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Data.Sql;
using Library.HumanResource.Employee;
using Aplos.Properties;
using System.Data;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System.IO;
using System.Threading;
using Library.Crosscutting.Security;
using Library.Security.Core;
#endregion LIB

namespace Aplos.Areas.HumanResource.Controllers
{
    public class MedicalLogReportController : BaseController
    {

        MedicalLogReportService ml = new MedicalLogReportService();
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult GetMedicinePopUp()
        {
            try
            {
                return Json(ml.GetMedicinePopUp(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [Authorize, HttpGet]
        public ActionResult GetMedicalLogEmployee()
        {
            try
            {
                return Json(ml.GetMedicalLogEmployee(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult medicallogGridView(string from, string to, string empSystemId)
        {
            try
            {
                return Json(ml.medicallogGridView(from, to, empSystemId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [Authorize, HttpGet]
        public ActionResult GetMedinceStockGrid(string medicineId, string to)
        {
            try
            {
                return Json(ml.GetMedinceStockGrid(medicineId, to), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region workbook Excel View
        [Authorize, HttpPost]
        public ActionResult XlsMedicalLogReport(string from, string to, string empSystemId)
        {
            try
            {
                var workbook = medicallogExcelView(from, to, empSystemId);

                var strFileName = DateTime.Now.ToString("yy-MMM-dd") + " " + "MedicalLogReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        private IWorkbook medicallogExcelView(string from, string to, string empSystemId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = ml.medicallogExcelView(from, to, empSystemId);


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Medical Log Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers


            report.SetHeaderText(ref sheet, ROW, COL, "Id", 6, ExcelHAlign.HAlignCenter);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Date", 12, ExcelHAlign.HAlignCenter);
            int ColDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Medicine Category", 50, ExcelHAlign.HAlignCenter);
            int ColSicknessName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Medicines", 50, ExcelHAlign.HAlignCenter);
            int ColMedicines = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Medicine Purpose", 50, ExcelHAlign.HAlignCenter);
            int ColMedicinePurpose = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Days", 5, ExcelHAlign.HAlignCenter);
            int ColSDays = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Quantity", 5, ExcelHAlign.HAlignCenter);
            int ColQuantity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "No of Visits", 10, ExcelHAlign.HAlignCenter);
            int ColNoofVisits = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 30, ExcelHAlign.HAlignCenter);
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 20, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 20, ExcelHAlign.HAlignCenter);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Section", 20, ExcelHAlign.HAlignCenter);
            int ColSubSection = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 20, ExcelHAlign.HAlignCenter);
            int ColDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Given Designation", 20, ExcelHAlign.HAlignCenter);
            int ColGivenDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Skill", 20, ExcelHAlign.HAlignCenter);
            int ColSkill = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 20, ExcelHAlign.HAlignCenter);
            int ColEntity= COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Added By", 20, ExcelHAlign.HAlignCenter);
            int ColAddedBy = COL;
           // COL++;


            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;


            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColId].Text = data.Rows[i]["Id"].ToString();
                sheet[ROW, ColDate].Text = data.Rows[i]["Date"].ToString();
                sheet[ROW, ColEmployeeCode].Number = clsStaticInfo.dbl(data.Rows[i]["EmployeeCode"].ToString());
                sheet[ROW, ColQuantity].Text = data.Rows[i]["Quantity"].ToString();
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColSicknessName].Text = data.Rows[i]["Sickness"].ToString();
                sheet[ROW, ColMedicines].Text = data.Rows[i]["Medicines"].ToString();
                sheet[ROW, ColSDays].Number = clsStaticInfo.dbl(data.Rows[i]["Days"].ToString());
                sheet[ROW, ColNoofVisits].Number = clsStaticInfo.dbl(data.Rows[i]["NoOfVisits"].ToString());
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                sheet[ROW, ColGivenDesignation].Text = data.Rows[i]["GivenDesignation"].ToString();
                //sheet[ROW, ColSkill].Text = data.Rows[i]["Skill"].ToString();
                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColAddedBy].Text = data.Rows[i]["AddedBy"].ToString();
                sheet[ROW, ColMedicinePurpose].Text = data.Rows[i]["Purpose"].ToString();

                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            sheet.UsedRange.WrapText = false;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
            sheet["A" + startRow.ToString()].FreezePanes();

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Medical Log Report", identity.PlantId);
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
            sheet.PageSetup.LeftMargin = 1.2;
            sheet.PageSetup.RightMargin = 0.2;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
            sheet.PageSetup.FitToPagesTall = 0;
            sheet.PageSetup.FitToPagesWide = 1;
            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.PageSetup.CenterHorizontally = true;


            sheet.Range[startRow - 1, 1, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
           // sheet.Range[startRow - 1, 1, startRow, endCol].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //ReportUtility reportUtility = new ReportUtility();
            //reportUtility.CompanyPlantHeader(ref sheet, endCol, "Medical Log Report", identity.CompanyId, identity.PlantName, null);
            reportUtility.PlantHeader(ref sheet, endCol, "Medical Log Report", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);

            return workbook;
        }

        [Authorize, HttpGet]
        public ActionResult XlsGetMedinceStockReport(string medicineId, string to)
        {
            try
            {
                var workbook = medicineStockExcelView(medicineId, to);

                var strFileName = DateTime.Now.ToString("yy-MMM-dd") + " " + "MedicineStockReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        private IWorkbook medicineStockExcelView(string medicineId, string to)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = ml.medicineStockExcelView(medicineId, to);


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Medicine Stock Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Medicine Name", 16, ExcelHAlign.HAlignCenter);
            int ColMedicineName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Opening Stock", 12, ExcelHAlign.HAlignCenter);
            int ColOpeningStock = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Stock Received", 12, ExcelHAlign.HAlignCenter);
            int ColStockReceived = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Stock Issue", 50, ExcelHAlign.HAlignCenter);
            int ColStockIssue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Expiry Date", 12, ExcelHAlign.HAlignCenter);
            int ColExpDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Closed Stock", 50, ExcelHAlign.HAlignCenter);
            int ColClosedStock = COL;
            

            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;

            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColMedicineName].Text = data.Rows[i]["Medicine"].ToString();
                sheet[ROW, ColOpeningStock].Number = clsStaticInfo.dbl(data.Rows[i]["Opening Quantity"].ToString());
                sheet[ROW, ColStockReceived].Number = clsStaticInfo.dbl(data.Rows[i]["Received Quantity"].ToString());
                sheet[ROW, ColStockIssue].Number = clsStaticInfo.dbl(data.Rows[i]["IssueQty"].ToString());
                sheet[ROW, ColExpDate].Text = data.Rows[i]["Expiry Date"].ToString();
                sheet[ROW, ColClosedStock].Number = clsStaticInfo.dbl(data.Rows[i]["ClosingStock"].ToString());
               
                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            sheet.UsedRange.WrapText = false;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
            sheet["A" + startRow.ToString()].FreezePanes();

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Medicine Stock Report", identity.PlantId);
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


            sheet.Range[startRow - 1, 1, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "Medicine Stock Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion  workbook Excel View
    }
}