#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.OrderManagement.Production;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Library.Data;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using System.Text.RegularExpressions;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Aplos.Areas.Commercial.Controllers;
using System.Drawing;


#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class WasteTransactionReportController : BaseController
    {

        WasteTransactionReportService ws = new WasteTransactionReportService();

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public WasteTransactionReportController(ISqlRepository R)
        {
            _sqlRepository = R;
            ws = new WasteTransactionReportService();

        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult getEntity()
        {
            return Json(ws.getEntity(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getData(string EntityId, string ToDate, string FromDate)
        {
            return Json(ws.getData(EntityId, ToDate, FromDate), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getClickedData(string Id)
        {
            return Json(ws.getClickedData(Id), JsonRequestBehavior.AllowGet);
        }


        [HttpPost , Authorize]
        public JsonResult saveQuantity(Dictionary<string, object> data)
        {
            try
            {
                var datas = ws.saveQuantity(data);
                return Json(new { Error = false, Data = datas, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }



        [HttpPost, Authorize]
        public ActionResult GetWasteReport(string EntityId, string ToDate , string FromDate)
        {

            try
            {
                var workbook = GetWasteReportForm(EntityId, FromDate, ToDate);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "Report.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        private IWorkbook GetWasteReportForm(string EntityId, string FromDate, string ToDate)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

           

            var data = ws.getGroupWasteReport(EntityId, FromDate, ToDate);

            var sheet = workbook.Worksheets[0];



            #region sheet1
            sheet.Name = "Report";

            int ROW = 8;
            int endCol = 1;
            int COL = 1;

            //sheet.Range[ROW, COL].Text = "From - "+FromDate+" , To - "+ToDate;
            //sheet.Range[ROW, COL].ColumnWidth = 13;
            //sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            //sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //ROW += 2;

            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "ID", 13, ExcelHAlign.HAlignCenter);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Dates", 13, ExcelHAlign.HAlignCenter);
            int ColDat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Item Name", 13, ExcelHAlign.HAlignCenter);
            int ColItem = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Category", 13, ExcelHAlign.HAlignCenter);
            int ColCate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SubCategory", 13, ExcelHAlign.HAlignCenter);
            int ColScate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Quantity", 13, ExcelHAlign.HAlignCenter);
            int ColQtt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 13, ExcelHAlign.HAlignCenter);
            int ColRem = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "AddedBy", 13, ExcelHAlign.HAlignCenter);
            int ColAb = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 13, ExcelHAlign.HAlignCenter);
            int ColEnt = COL;
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
                sheet[ROW, ColId].Text =data.Rows[i]["WTDId"].ToString();
                sheet[ROW, ColDat].Text =data.Rows[i]["Dates"].ToString();
                sheet[ROW, ColItem].Text = data.Rows[i]["ItemName"].ToString();
                sheet[ROW, ColCate].Text =  data.Rows[i]["Category"].ToString();
                sheet[ROW, ColScate].Text =  data.Rows[i]["SubCategory"].ToString();
                sheet[ROW, ColQtt].Number =  clsStaticInfo.dbl(data.Rows[i]["Quantity"].ToString());
                sheet[ROW, ColRem].Text =  data.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColAb].Text =  data.Rows[i]["AddedBy"].ToString();
                sheet[ROW, ColEnt].Text =  data.Rows[i]["EntityName"].ToString();

                sheet.Range[ROW, ColId, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, ColId, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            ROW++;


            endRow = ROW - 1;
            endRow = ROW - 1;

            #endregion sheet1



            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

           

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
    }
}