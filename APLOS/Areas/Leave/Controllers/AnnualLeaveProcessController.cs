#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using Library.Service.EmployeeServices;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using Library.HumanResource.NewAttendanceProcess;
#endregion Using

namespace Aplos.Areas.Leave.Controllers
{
    public class AnnualLeaveProcessController  : BaseController
    {
        
        #region Constructor

        LeaveOpeningUploadService _leave = new LeaveOpeningUploadService();
        private readonly ISqlRepository _sqlRepository;

        public AnnualLeaveProcessController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
     
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult getCompany()
        {
            try {
                return Json(_leave.getCompany(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
            
        }

        [HttpGet, Authorize]
        public ActionResult getPlants(string cmp)
        {
            try 
            {                
                return Json(_leave.getPlants(cmp), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSampleReport(string PlantId, string name,string LvYearId, ReportFormat reportFormat)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string date = DateTime.Now.Date.ToString("dd-MMM");
            var reportFileName = "LeaveOpeningUpload-" + name + "-" + date;
            var workbook = GetWorkSheet(PlantId, LvYearId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        private IWorkbook GetWorkSheet(string PlantId,string LvId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            DataTable data = _leave.getSampleFile(PlantId, LvId);

            sheet.Name = "LeaveOpeningUpload";

            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 15, ExcelHAlign.HAlignLeft);
            int ColEmpCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Leave Year", 16, ExcelHAlign.HAlignLeft);
            int ColLvYear = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "LeaveType", 16, ExcelHAlign.HAlignLeft);
            int ColLvType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 12, ExcelHAlign.HAlignLeft);
            int ColPlant = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Opening", 10, ExcelHAlign.HAlignLeft);
            int colOpening = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Earned", 10, ExcelHAlign.HAlignLeft);
            int ColEarned = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Availed", 10, ExcelHAlign.HAlignLeft);
            int ColAvailed = COL;
            COL++; 
            
            report.SetHeaderText(ref sheet, ROW, COL, "Adjustment", 12, ExcelHAlign.HAlignLeft);
            int ColAdjustment = COL;
            COL++;
            endCol = COL;
            #endregion Headers
           
            ROW++;
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColEmpCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColLvYear].Text = data.Rows[i]["LeaveYear"].ToString();
                sheet[ROW, ColLvType].Text = data.Rows[i]["LeaveType"].ToString();
                sheet[ROW, ColPlant].Text = data.Rows[i]["Plant"].ToString();
                sheet[ROW, colOpening].Text = data.Rows[i]["Opening"].ToString();
                sheet[ROW, ColEarned].Text = data.Rows[i]["Earned"].ToString();
                sheet[ROW, ColAvailed].Text = data.Rows[i]["Availed"].ToString();
                sheet[ROW, ColAdjustment].Text = data.Rows[i]["Adjustment"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        [HttpPost, Authorize]
        public ActionResult ImportData(string PlantId)
        {
            string path;

            try
            {
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //SaveFile(out path);
                var data = "";// ReadData(path, plantId);

                var json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

    }
}