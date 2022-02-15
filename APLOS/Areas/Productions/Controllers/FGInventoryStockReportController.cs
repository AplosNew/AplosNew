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
using Library.OrderManagement.Production;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
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
    public class FGInventoryStockReportController : BaseController
    {
        FGInventoryStockReportService fg = new FGInventoryStockReportService();
        

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public FGInventoryStockReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetStocksReport(string ToDate, string FromDate)
        {

            try
            {
                var workbook = GetStocksReportForm( ToDate, FromDate);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "FGInventoryStockReport.xlsx";
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
        private IWorkbook GetStocksReportForm(string ToDate, string FromDate)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = fg.getStocksReport(ToDate, FromDate);

            

            var sheet = workbook.Worksheets[0];
            


            #region sheet1
            sheet.Name = "FG Inventory Stock Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            sheet.Range[ROW, COL].Text = "From : " + FromDate + " , To : " + ToDate;
            sheet.Range[ROW, COL].ColumnWidth = 13;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW += 2;

            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Product Category", 15, ExcelHAlign.HAlignCenter);
            int ColPCat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Product Sub Category", 15, ExcelHAlign.HAlignCenter);
            int ColPSCat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material", 40, ExcelHAlign.HAlignCenter);
            int ColMat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 40, ExcelHAlign.HAlignCenter);
            int ColArt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Product Code", 15, ExcelHAlign.HAlignCenter);
            int ColPc = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PO", 15, ExcelHAlign.HAlignCenter);
            int ColPo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Lot No", 15, ExcelHAlign.HAlignCenter);
            int ColLot = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Opening", 15, ExcelHAlign.HAlignCenter);
            int ColOpen = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Packing", 15, ExcelHAlign.HAlignCenter);
            int ColPakcing = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "RePacking", 15, ExcelHAlign.HAlignCenter);
            int ColRPakcing = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Adjustment", 15, ExcelHAlign.HAlignCenter);
            int ColAdj = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Return", 15, ExcelHAlign.HAlignCenter);
            int ColRet = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Dispatch", 15, ExcelHAlign.HAlignCenter);
            int ColDis = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Issue", 15, ExcelHAlign.HAlignCenter);
            int ColIss = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Closing", 15, ExcelHAlign.HAlignCenter);
            int ColClos = COL;
            COL++;

            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColPCat].Text = data.Rows[i]["ProductCategory"].ToString();
                sheet[ROW, ColPSCat].Text = data.Rows[i]["ProductSubCategory"].ToString();
                sheet[ROW, ColMat].Text = data.Rows[i]["Material"].ToString();
                sheet[ROW, ColArt].Text = data.Rows[i]["Article"].ToString();
                sheet[ROW, ColPc].Text = data.Rows[i]["ProductCode"].ToString();
                sheet[ROW, ColPo].Text = data.Rows[i]["POId"].ToString();
                sheet[ROW, ColLot].Text = data.Rows[i]["LotNo"].ToString();
                sheet[ROW, ColOpen].Number = clsStaticInfo.dbl(data.Rows[i]["Opening"].ToString());
                sheet[ROW, ColPakcing].Number = clsStaticInfo.dbl(data.Rows[i]["Packing"].ToString());
                sheet[ROW, ColRPakcing].Number = clsStaticInfo.dbl(data.Rows[i]["RePacking"].ToString());
                sheet[ROW, ColAdj].Number = clsStaticInfo.dbl(data.Rows[i]["Adjustment"].ToString());
                sheet[ROW, ColRet].Number = clsStaticInfo.dbl(data.Rows[i]["Retrn"].ToString());
                sheet[ROW, ColDis].Number = clsStaticInfo.dbl(data.Rows[i]["Dispatch"].ToString());
                sheet[ROW, ColIss].Number = clsStaticInfo.dbl(data.Rows[i]["Issue"].ToString());
                sheet[ROW, ColClos].Number = clsStaticInfo.dbl(data.Rows[i]["Closing"].ToString());

                sheet.Range[ROW, ColPCat, ROW, endCol-1].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, ColPCat, ROW, endCol-1].BorderAround(ExcelLineStyle.Hair);

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
            reportUtility.CompanyHeader(ref sheet, endCol, "FG Inventory Stock Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
           
            return workbook;
        }

    }   
}