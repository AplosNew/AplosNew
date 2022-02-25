using Aplos.Controllers;
using System;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Library.HumanResource.Employee;
using Library.Security.Core;

namespace Aplos.Areas.Administration.Controllers
{
    public class VisitorListReportController : BaseController
    {
        #region Constructor

        FactoryVisitorService rep = new FactoryVisitorService();

        public VisitorListReportController()
        {
            rep = new FactoryVisitorService();
        }
        #endregion

        #region View
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        [HttpPost, Authorize]
        public JsonResult GetData(string In,string Out, string FromDate, string ToDate)
        {
            try
            {
                var jsondata = Json(new { Error = false, DATA = rep.GetVisitorList(In,Out,FromDate,ToDate) }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch(Exception ex)
            {
                return Json(new { Error =true,Message= ex.Message },JsonRequestBehavior.AllowGet);
               
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPrintReport(string In, string Out, string FromDate, string ToDate, string Id)
        {

            try
            {
                var workbook= GetFilterData(In,Out,FromDate,ToDate,Id);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "VisitorList.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private IWorkbook GetFilterData(string In, string Out, string FromDate, string ToDate, string Id)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            sheet.Name = "VisitorListReport";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            DataTable data = rep.GetReportData(In, Out, FromDate, ToDate, Id);

            #region Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Visitor Name", 15, ExcelHAlign.HAlignCenter);
            int ColVisitorName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Visitor Type", 13, ExcelHAlign.HAlignCenter);
            int ColType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Visitor Category", 13, ExcelHAlign.HAlignCenter);
            int ColCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Visitor Location", 15, ExcelHAlign.HAlignCenter);
            int ColLocation = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "To Meet", 15, ExcelHAlign.HAlignCenter);
            int ColToMeet = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Purpose", 15, ExcelHAlign.HAlignCenter);
            int ColPurpose = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "InDate", 15, ExcelHAlign.HAlignCenter);
            int ColIn = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "InTime", 15, ExcelHAlign.HAlignCenter);
            int ColInTime = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "OutDate", 15, ExcelHAlign.HAlignCenter);
            int ColOut = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OutTime", 15, ExcelHAlign.HAlignCenter);
            int ColOutTime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Hours", 13, ExcelHAlign.HAlignCenter);
            int ColHours = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Vehicle No", 15, ExcelHAlign.HAlignCenter);
            int ColVehicle = COL;
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
                sheet[ROW, ColIn].Text = data.Rows[i]["InDate"].ToString();
                sheet[ROW, ColOut].Text = data.Rows[i]["OutDate"].ToString();
                sheet[ROW, ColVisitorName].Text = data.Rows[i]["VisitorName"].ToString();
                sheet[ROW, ColType].Text = data.Rows[i]["VisitorType"].ToString();
                sheet[ROW, ColCategory].Text = data.Rows[i]["VisitorCategory"].ToString();
                sheet[ROW, ColLocation].Text = data.Rows[i]["VisitorLocation"].ToString();
                sheet[ROW, ColToMeet].Text = data.Rows[i]["ToMeet"].ToString();
                sheet[ROW, ColPurpose].Text = data.Rows[i]["Purpose"].ToString();
                sheet[ROW, ColInTime].Text = data.Rows[i]["InTime"].ToString();
                sheet[ROW, ColOutTime].Text = data.Rows[i]["OutTime"].ToString();
                sheet[ROW, ColAddedBy].Text = data.Rows[i]["AddedBy"].ToString();
                sheet[ROW, ColVehicle].Text = data.Rows[i]["VehicleNo"].ToString();
                sheet[ROW, ColHours].Number = clsStaticInfo.dbl(data.Rows[i]["Duration"].ToString());
                
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;
            string reportname = "Visitor List Report";


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
 