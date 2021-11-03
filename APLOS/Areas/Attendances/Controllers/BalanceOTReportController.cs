using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System.Data;
using Syncfusion.XlsIO;
using Library.Service.Attendances;
using Library.Service.Helpers;
using Library.Model.Enums;
using Library.Security.Core;
using System.IO;
using Library.HumanResource.NewAttendanceProcess;

namespace Aplos.Areas.Attendances.Controllers
{
    public class BalanceOTReportController : BaseController
    {
        FullYearPresentDaysCount rep = new FullYearPresentDaysCount();

        public BalanceOTReportController()
        {
            rep = new FullYearPresentDaysCount();
        }

        
        public ActionResult Aplos()
        {
            return View();
        }
             

        [HttpGet, Authorize]
        public JsonResult GetBalanceData()
        {
            try
            {
                var jsondata = Json(new { Error = false, DATA = rep.GetBalanceData() }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch(Exception ex)
            {
                return Json(new { Error =true,Message= ex.Message },JsonRequestBehavior.AllowGet);
               
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPrintReport(string EmpId)
        {

            try
            {
                var workbook = GetFilterData(EmpId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "BalanceOTReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private IWorkbook GetFilterData(string EmpId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            sheet.Name = "Balance Hours";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;
            DataTable data = rep.GetBalanceDataReport(EmpId);

            #region Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Position Code", 13, ExcelHAlign.HAlignCenter);
            int ColPosCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Budget", 15, ExcelHAlign.HAlignCenter);
            int ColBud = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Legal Designation", 15, ExcelHAlign.HAlignCenter);
            int ColLDesg = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Unit", 15, ExcelHAlign.HAlignCenter);
            int ColUnit = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 15, ExcelHAlign.HAlignCenter);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SubSection", 15, ExcelHAlign.HAlignCenter);
            int ColSubSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 13, ExcelHAlign.HAlignCenter);
            int ColCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Name", 13, ExcelHAlign.HAlignCenter);
            int ColName = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Phone No", 13, ExcelHAlign.HAlignCenter);
            int ColPh = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Address", 13, ExcelHAlign.HAlignCenter);
            int ColAdd = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Balance Hours", 13, ExcelHAlign.HAlignCenter);
            int ColBal = COL;
            COL++;
            ROW++;

            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColPosCode].Text = data.Rows[i]["PositionCode"].ToString();
                sheet[ROW, ColBud].Text = data.Rows[i]["BudgetCode"].ToString();
                sheet[ROW, ColLDesg].Text = data.Rows[i]["LegalDesg"].ToString();
                sheet[ROW, ColUnit].Text = data.Rows[i]["Unit"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColPh].Text = data.Rows[i]["CellPhnNo"].ToString();
                sheet[ROW, ColAdd].Text = data.Rows[i]["PresentAddress1"].ToString();
                sheet[ROW, ColBal].Number = clsStaticInfo.dbl(data.Rows[i]["BalanceOT"].ToString());
               
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            endRow = ROW - 1;
            string reportname = "Balance OT Report";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, reportname, identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);

            return workbook;
        }

    }
}
 