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
using Library.HumanResource.Employee;

namespace Aplos.Areas.Attendances.Controllers
{
    public class EmployeeLastPunchReportController : BaseController
    {
        EmployeeLastPunchService rep = new EmployeeLastPunchService();

        public EmployeeLastPunchReportController()
        {
            rep = new EmployeeLastPunchService();
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

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "EmployeeLastPunchData.xlsx";
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
            sheet.Name = "Employee LastPunch";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;
            DataTable data = rep.GetReportData(EmpId);

            #region Headers

            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeCode", 13, ExcelHAlign.HAlignCenter);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeName", 18, ExcelHAlign.HAlignCenter);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DOJ", 13, ExcelHAlign.HAlignCenter);
            int ColDOJ = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 18, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 13, ExcelHAlign.HAlignCenter);
            int ColSection = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "SubSection", 13, ExcelHAlign.HAlignCenter);
            int ColSubSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 18, ExcelHAlign.HAlignCenter);
            int ColDesignation = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Tenure (In Months)", 13, ExcelHAlign.HAlignCenter);
            int ColTenure = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeCurrentStatus", 18, ExcelHAlign.HAlignCenter);
            int ColCurrentStatus = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "LastPunch Date", 13, ExcelHAlign.HAlignCenter);
            int ColLastPunchDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "LastPunch Time", 18, ExcelHAlign.HAlignCenter);
            int ColLastPunchTime = COL;
            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColTenure].Number = clsStaticInfo.dbl(data.Rows[i]["TenureMonth"].ToString());
                sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColDOJ].Text = data.Rows[i]["DOJ"].ToString();
                sheet[ROW, ColCurrentStatus].Text = data.Rows[i]["EmployeeCurrentStatus"].ToString();
                sheet[ROW, ColLastPunchDate].Text = data.Rows[i]["LastWorkDate"].ToString(); 
                sheet[ROW, ColLastPunchTime].Text = data.Rows[i]["LastIn"].ToString();
                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            endRow = ROW - 1;
            string reportname = "Employee LastPunch Report";
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
 