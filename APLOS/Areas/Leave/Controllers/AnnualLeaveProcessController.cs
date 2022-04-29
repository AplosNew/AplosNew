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
        public ActionResult GetSampleReport(string PlantId, string name,string LvYearId, ReportFormat reportFormat)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string date = DateTime.Now.Date.ToString("dd-MMM");
            var reportFileName = "LeaveOpeningUpload-" + name + "-" + date;
            var workbook = GetRosterBudgetWorkSheet(PlantId, LvYearId);
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

        private IWorkbook GetRosterBudgetWorkSheet(string PlantId,string LvId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            RosterPatternService rs = new RosterPatternService();

            DataTable data = rs.getRosterBudgetFile(PlantId);
           // DataTable data = _leave.getSampleFile(PlantId, LvId);

            sheet.Name = "LeaveOpeningUpload";



            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "RosterId", 12, ExcelHAlign.HAlignLeft);
            int ColRosterId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "BudgetId", 8, ExcelHAlign.HAlignLeft);
            int ColBudgetId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "BudgetCode", 8, ExcelHAlign.HAlignLeft);
            int ColBudgetCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 8, ExcelHAlign.HAlignLeft);
            int ColPlant = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 8, ExcelHAlign.HAlignLeft);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Position", 8, ExcelHAlign.HAlignLeft);
            int ColPosition = COL;
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
                sheet[ROW, ColRosterId].Text = data.Rows[i]["RosterId"].ToString();
                sheet[ROW, ColBudgetId].Text = data.Rows[i]["BudgetId"].ToString();
                sheet[ROW, ColBudgetCode].Text = data.Rows[i]["BudgetCode"].ToString();
                sheet[ROW, ColPlant].Text = data.Rows[i]["Plant"].ToString();
                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColPosition].Text = data.Rows[i]["Position"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }


    }
}