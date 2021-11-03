using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
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
using Library.Service.Productions;
using Library.Service.Helpers;
using Library.Model.Enums;
using Library.Security.Core;
using System.IO;

namespace Aplos.Areas.Productions.Controllers
{
    public class MovementScanDataReportController : BaseController
    {
        MovementScanData rep = new MovementScanData();

        public MovementScanDataReportController()
        {
            rep = new MovementScanData();
        }


        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult GetTo(string EntityId, string PurposeId, string FromId)
        {
            try
            {
                return Json(rep.GetTo(EntityId, PurposeId, FromId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetEntity()
        {
            try
            {
                return Json(rep.GetEntity(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getPurposeCategory()
        {
            try
            {
                return Json(rep.getPurposeCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetFrom(string EntityId, string PurposeId)
        {
            try
            {
                return Json(rep.GetFrom(EntityId, PurposeId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetData(string FromLoc, string ToLoc, string FromDate, string ToDate, string PurposeId, string EntityId)
        {
            try
            {
                var jsondata = Json(new { Error = false, DATA = rep.GetData(FromLoc, ToLoc, FromDate, ToDate, PurposeId, EntityId) }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPrintReport(string From, string To, string FromLoc, string ToLoc, string EntityId,
            string Shade, string ShiftId, string ProductCode, string PO, string Cones, string RefNo, string LotNo,
            string PackedBy, string Grade, string OrderStatusId, string Date, string Article, string ArticleCode, string PurposeId)
        {

            try
            {
                var workbook = GetFilterData(From, To, FromLoc, ToLoc, EntityId, Shade, ShiftId, ProductCode,
                    PO, Cones, RefNo, LotNo, PackedBy, Grade, OrderStatusId, Date, Article, ArticleCode, PurposeId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "MovementScanData.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private IWorkbook GetFilterData(string From, string To, string FromLoc, string ToLoc, string EntityId,
            string Shade, string ShiftId, string ProductCode, string PO, string Cones, string RefNo, string LotNo,
            string PackedBy, string Grade, string OrderStatusId, string Date, string Article, string ArticleCode, string PurposeId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            sheet.Name = "Movement Scan-Data";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;
            DataTable data = rep.GetReportData(From, To, FromLoc, ToLoc, EntityId, Shade, ShiftId, ProductCode,
                 PO, Cones, RefNo, LotNo, PackedBy, Grade, OrderStatusId, Date, Article, ArticleCode, PurposeId);

            #region Headers

            report.SetHeaderText(ref sheet, ROW, COL, "ProductCode", 13, ExcelHAlign.HAlignCenter);
            int ColProductCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PO", 13, ExcelHAlign.HAlignCenter);
            int ColPO = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Date", 13, ExcelHAlign.HAlignCenter);
            int ColDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "LotNo", 13, ExcelHAlign.HAlignCenter);
            int ColLotNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "RefNo", 13, ExcelHAlign.HAlignCenter);
            int ColRefNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Cones", 13, ExcelHAlign.HAlignCenter);
            int ColCones = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 15, ExcelHAlign.HAlignCenter);
            int ColArticle = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article Code", 13, ExcelHAlign.HAlignCenter);
            int ColArticleCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "NetWeight", 13, ExcelHAlign.HAlignCenter);
            int ColNetWeight = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "GrossWeight", 13, ExcelHAlign.HAlignCenter);
            int ColGWeight = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shade", 13, ExcelHAlign.HAlignCenter);
            int ColShade = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Grade", 13, ExcelHAlign.HAlignCenter);
            int ColGrade = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shift", 13, ExcelHAlign.HAlignCenter);
            int ColShift = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "OrderStatus", 13, ExcelHAlign.HAlignCenter);
            int ColOrderStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Purpose", 13, ExcelHAlign.HAlignCenter);
            int ColPurpose = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PackedBy", 13, ExcelHAlign.HAlignCenter);
            int ColPackedBy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "From", 13, ExcelHAlign.HAlignCenter);
            int ColFrom = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "To", 13, ExcelHAlign.HAlignCenter);
            int ColTo = COL;
            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColProductCode].Text = data.Rows[i]["ProductCode"].ToString();
                sheet[ROW, ColPO].Text = data.Rows[i]["PO"].ToString();
                sheet[ROW, ColDate].Text = data.Rows[i]["WorkDate"].ToString();
                sheet[ROW, ColLotNo].Text = data.Rows[i]["LotNo"].ToString();
                sheet[ROW, ColRefNo].Text = data.Rows[i]["RefNo"].ToString();
                sheet[ROW, ColCones].Number = OTSBD.clsStaticInfo.dbl(data.Rows[i]["Cones"].ToString());
                sheet[ROW, ColArticleCode].Text = data.Rows[i]["ArticleCode"].ToString();
                sheet[ROW, ColArticle].Text = data.Rows[i]["Article"].ToString();
                sheet[ROW, ColNetWeight].Number = OTSBD.clsStaticInfo.dbl(data.Rows[i]["NetWeight"].ToString());
                sheet[ROW, ColGWeight].Number = OTSBD.clsStaticInfo.dbl(data.Rows[i]["GWeight"].ToString());
                sheet[ROW, ColShade].Text = data.Rows[i]["Shade"].ToString();
                sheet[ROW, ColGrade].Text = data.Rows[i]["Grade"].ToString();
                sheet[ROW, ColShift].Text = data.Rows[i]["Shift"].ToString();
                sheet[ROW, ColOrderStatus].Text = data.Rows[i]["OrderStatus"].ToString();
                sheet[ROW, ColPurpose].Text = data.Rows[i]["Purpose"].ToString();
                sheet[ROW, ColPackedBy].Text = data.Rows[i]["PackedBy"].ToString();
                sheet[ROW, ColFrom].Text = data.Rows[i]["FromLoc"].ToString();
                sheet[ROW, ColTo].Text = data.Rows[i]["ToLoc"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            sheet.Range[startRow, ColCones, ROW, ColCones].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet.Range[startRow, ColNetWeight, ROW, ColNetWeight].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet.Range[startRow, ColGWeight, ROW, ColGWeight].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Movement Scan-Data Report", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }

    }
}
