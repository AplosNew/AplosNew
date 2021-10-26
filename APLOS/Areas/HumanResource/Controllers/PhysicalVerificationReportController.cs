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
using Library.Service.Helpers;
using Library.Model.Enums;
using Library.Security.Core;
using System.IO;
using Library.HumanResource.NewAttendanceProcess;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class PhysicalVerificationReportController : BaseController
    {
        PhysicalVerificationReportService rep = new PhysicalVerificationReportService();

        public PhysicalVerificationReportController()
        {
            rep = new PhysicalVerificationReportService();
        }

        
        public ActionResult Aplos()
        {
            return View();
        }
             

        [HttpGet, Authorize]
        public JsonResult GetData(string WkDate)
        {
            try
            {
                var jsondata = Json(new { Error = false, DATA = rep.GetData(WkDate) }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch(Exception ex)
            {
                return Json(new { Error =true,Message= ex.Message },JsonRequestBehavior.AllowGet);
               
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPrintReport(string WkDate,string EmpId)
        {

            try
            {
                var workbook = GetFilterData(WkDate, EmpId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "PhysicalVerification.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private IWorkbook GetFilterData(string WkDate, string EmpId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            sheet.Name = "PhysicalVerificationReport";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;
            DataTable data = rep.GetReportData(WkDate, EmpId);

            #region Headers

            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeCode", 13, ExcelHAlign.HAlignCenter);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeName", 18, ExcelHAlign.HAlignCenter);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 15, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 15, ExcelHAlign.HAlignCenter);
            int ColSection = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "SubSection", 15, ExcelHAlign.HAlignCenter);
            int ColSubSection = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 15, ExcelHAlign.HAlignCenter);
            int ColLglDesgn = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Unit", 15, ExcelHAlign.HAlignCenter);
            int ColUnit = COL;
            COL++;
                       
            report.SetHeaderText(ref sheet, ROW, COL, "Date", 13, ExcelHAlign.HAlignCenter);
            int ColDate = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "InTime", 18, ExcelHAlign.HAlignCenter);
            int ColInTime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OutTime", 18, ExcelHAlign.HAlignCenter);
            int ColOutTime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "AddedBy", 13, ExcelHAlign.HAlignCenter);
            int ColAddedBy = COL;
            ROW++;
            endCol = COL;
          
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColUnit].Text = data.Rows[i]["Unit"].ToString();
                sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColLglDesgn].Text = data.Rows[i]["LegalDesignation"].ToString();
                sheet[ROW, ColDate].Text = data.Rows[i]["WorkDate"].ToString();
                sheet[ROW, ColInTime].Text = data.Rows[i]["InTime"].ToString();
                sheet[ROW, ColOutTime].Text = data.Rows[i]["OutTime"].ToString();
                sheet[ROW, ColAddedBy].Text = data.Rows[i]["AddedBy"].ToString();
      
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;
            string reportname = "Physical Verification Report";
            

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
 