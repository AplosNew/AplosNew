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

namespace Aplos.Areas.Attendances.Controllers
{
    public class AttendanceFromAppReportController : BaseController
    {
       AttendanceFromAppReportService  rep = new AttendanceFromAppReportService();

        public AttendanceFromAppReportController()
        {
            rep = new AttendanceFromAppReportService();
        }

        
        public ActionResult Aplos()
        {
            return View();
        }
             

        [HttpGet, Authorize]
        public JsonResult GetAttndData(string From,string To, string AttndType)
        {
            try
            {
                var jsondata = Json(new { Error = false, DATA = rep.GetAttndData(From, To,AttndType) }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch(Exception ex)
            {
                return Json(new { Error =true,Message= ex.Message },JsonRequestBehavior.AllowGet);
               
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPrintReport(string From, string To, string AttndType,
        string EmpName, string SubId, string PlantId, string SectionId, string DesgId, string UnitId, string DeptId,
        string EmpCode)
        {

            try
            {
                var workbook = GetFilterData(From, To, AttndType,
                EmpName,SubId, PlantId,SectionId,DesgId,UnitId,DeptId,EmpCode);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "AttendanceFromApp.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private IWorkbook GetFilterData(string From, string To, string AttndType,
        string EmpName, string SubId, string PlantId, string SectionId, string DesgId, string UnitId, string DeptId,
        string EmpCode)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            sheet.Name = "AttendanceReport";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;
            DataTable data = rep.GetReportData(From, To, AttndType,
                EmpName, SubId, PlantId, SectionId, DesgId, UnitId, DeptId, EmpCode);

            #region Headers
           
            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 13, ExcelHAlign.HAlignCenter);
            int ColPlant = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Unit", 13, ExcelHAlign.HAlignCenter);
            int ColUnit = COL;
            COL++;


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


            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 13, ExcelHAlign.HAlignCenter);
            int ColDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Date", 13, ExcelHAlign.HAlignCenter);
            int ColDate = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "InTime", 13, ExcelHAlign.HAlignCenter);
            int ColInTime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OutTime", 13, ExcelHAlign.HAlignCenter);
            int ColOutTime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "InRemarks", 13, ExcelHAlign.HAlignCenter);
            int ColInRemarks = COL;
            COL++;           

            report.SetHeaderText(ref sheet, ROW, COL, "InLocation", 13, ExcelHAlign.HAlignCenter);
            int ColInLocation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OutRemarks", 13, ExcelHAlign.HAlignCenter);
            int ColOutRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OutLocation", 13, ExcelHAlign.HAlignCenter);
            int ColOutLocation = COL;
            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColPlant].Text = data.Rows[i]["Plant"].ToString();
                sheet[ROW, ColUnit].Text = data.Rows[i]["Unit"].ToString();
                sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                sheet[ROW, ColDate].Text = data.Rows[i]["Date"].ToString();
                sheet[ROW, ColInTime].Text = data.Rows[i]["InTime"].ToString();
                sheet[ROW, ColOutTime].Text = data.Rows[i]["OutTime"].ToString();
                sheet[ROW, ColInLocation].Text = data.Rows[i]["InLocation"].ToString();
                sheet[ROW, ColOutLocation].Text = data.Rows[i]["OutLocation"].ToString();
                sheet[ROW, ColInRemarks].Text = data.Rows[i]["InRemarks"].ToString();
                sheet[ROW, ColOutRemarks].Text = data.Rows[i]["OutRemarks"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;
            string reportname;
            if(AttndType=="Both")
            {
                reportname = "Attendance Report";
            }
            else if(AttndType == "OnDuty")
            {
                reportname = "OnDuty Attendance Report";
            }
            else
            {
                reportname = "WFH Attendance Report";
            }

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
 