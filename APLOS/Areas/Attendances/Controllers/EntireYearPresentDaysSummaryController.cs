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
    public class EntireYearPresentDaysSummaryController : BaseController
    {
        FullYearPresentDaysCount rep = new FullYearPresentDaysCount();

        public EntireYearPresentDaysSummaryController()
        {
            rep = new FullYearPresentDaysCount();
        }

        
        public ActionResult Aplos()
        {
            return View();
        }
             

        [HttpGet, Authorize]
        public JsonResult GetSummaryData()
        {
            try
            {
                var jsondata = Json(new { Error = false, DATA = rep.GetData() }, JsonRequestBehavior.AllowGet);
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

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "PresentDaysSummary.xlsx";
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
            sheet.Name = "PresentDays";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;
            DataTable data = rep.GetReportData(EmpId);

            #region Headers

            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeCode", 13, ExcelHAlign.HAlignCenter);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeName", 13, ExcelHAlign.HAlignCenter);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 13, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 13, ExcelHAlign.HAlignCenter);
            int ColSection = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "SubSection", 13, ExcelHAlign.HAlignCenter);
            int ColSubSection = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "January", 13, ExcelHAlign.HAlignCenter);
            int ColJanuary = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Feburary", 13, ExcelHAlign.HAlignCenter);
            int ColFeburary = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "March", 13, ExcelHAlign.HAlignCenter);
            int ColMarch = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "April", 13, ExcelHAlign.HAlignCenter);
            int ColApril = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "May", 13, ExcelHAlign.HAlignCenter);
            int ColMay = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "June", 13, ExcelHAlign.HAlignCenter);
            int ColJune = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "July", 13, ExcelHAlign.HAlignCenter);
            int ColJuly = COL;
            COL++;
           
            report.SetHeaderText(ref sheet, ROW, COL, "August", 13, ExcelHAlign.HAlignCenter);
            int ColAugust = COL;
            COL++; 
            
            report.SetHeaderText(ref sheet, ROW, COL, "September", 13, ExcelHAlign.HAlignCenter);
            int ColSeptember = COL;
            COL++; 
            
            report.SetHeaderText(ref sheet, ROW, COL, "October", 13, ExcelHAlign.HAlignCenter);
            int ColOctober = COL;
            COL++;
            
            report.SetHeaderText(ref sheet, ROW, COL, "November", 13, ExcelHAlign.HAlignCenter);
            int ColNovember = COL;
            COL++; 
                     
            report.SetHeaderText(ref sheet, ROW, COL, "December", 13, ExcelHAlign.HAlignCenter);
            int ColDecember = COL;
            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColJanuary].Number = clsStaticInfo.dbl(data.Rows[i]["Jan"].ToString());
                sheet[ROW, ColFeburary].Number = clsStaticInfo.dbl(data.Rows[i]["Feb"].ToString());
                sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColMarch].Number = clsStaticInfo.dbl(data.Rows[i]["Mar"].ToString());
                sheet[ROW, ColApril].Number = clsStaticInfo.dbl(data.Rows[i]["Apr"].ToString());
                sheet[ROW, ColMay].Number = clsStaticInfo.dbl(data.Rows[i]["May"].ToString());
                sheet[ROW, ColJune].Number = clsStaticInfo.dbl(data.Rows[i]["June"].ToString());
                sheet[ROW, ColJuly].Number = clsStaticInfo.dbl(data.Rows[i]["July"].ToString());
                sheet[ROW, ColAugust].Number = clsStaticInfo.dbl(data.Rows[i]["Aug"].ToString());
                sheet[ROW, ColSeptember].Number = clsStaticInfo.dbl(data.Rows[i]["Sep"].ToString());
                sheet[ROW, ColOctober].Number = clsStaticInfo.dbl(data.Rows[i]["Oct"].ToString());
                sheet[ROW, ColNovember].Number = clsStaticInfo.dbl(data.Rows[i]["Nov"].ToString());
                sheet[ROW, ColDecember].Number = clsStaticInfo.dbl(data.Rows[i]["Dec"].ToString());
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            endRow = ROW - 1;
            string reportname = "Present Days Summary Report";
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
 