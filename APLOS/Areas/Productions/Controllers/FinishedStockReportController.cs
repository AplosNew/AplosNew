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
    public class FinishedStockReportController : BaseController
    {
        PackingData det = new PackingData();

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public FinishedStockReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetFinishedStocksReport(string Loc, string ToDate, string FromDate)
        {

            try
            {
                var workbook = GetFinishedStocksReportForm(Loc, ToDate, FromDate);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "FinishedStockReport.xlsx";
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
        private IWorkbook GetFinishedStocksReportForm(string Loc, string ToDate, string FromDate)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = det.getGroupFinishedStocksReport(Loc);

            var data1 = det.getAllFinishedStocksReport(Loc, ToDate, FromDate);

            var sheet = workbook.Worksheets[0];
            var sheet1 = workbook.Worksheets[1];


            #region sheet1
            sheet.Name = "Finished Stock Report";

            int ROW = 6;
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

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 40, ExcelHAlign.HAlignCenter);
            int ColArt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ProductCode", 13, ExcelHAlign.HAlignCenter);
            int ColProductCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Product Details", 40, ExcelHAlign.HAlignCenter);
            int ColProdDet = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "POId", 13, ExcelHAlign.HAlignCenter);
            int ColPOId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Lot No", 13, ExcelHAlign.HAlignCenter);
            int ColLot = COL;
            COL++;



            report.SetHeaderText(ref sheet, ROW, COL, "Bag Size", 13, ExcelHAlign.HAlignCenter);
            int ColBagSize = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Bags", 13, ExcelHAlign.HAlignCenter);
            int ColBags = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Net Weight", 13, ExcelHAlign.HAlignCenter);
            int ColNtWt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Gross Weight", 13, ExcelHAlign.HAlignCenter);
            int ColGWt = COL;
            COL++;





            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            string ProdDetails = "";
            string POId = "";
            string ProductCode = "";
            //string roduct = "";
            int ArtRow = 0;
            int LotRow = 0;
            int ProductCodeRow = 0;

            int ProdDetailsRow = 0;
            int POIdRow = 0;

            double[] arr = new double[3];


            for (int i = 0; i < data.Rows.Count; i++)
            {
                if (Article != data.Rows[i]["StandardName"].ToString())
                {

                    Article = data.Rows[i]["StandardName"].ToString();
                    sheet[ROW, ColArt].Text = data.Rows[i]["StandardName"].ToString();
                    ProdDetails = data.Rows[i]["ProdDetails"].ToString();
                    sheet[ROW, ColProdDet].Text = data.Rows[i]["ProdDetails"].ToString();

                    if (i != 0 && ArtRow != (ROW - 1))
                    {
                        sheet.Range[ArtRow, ColArt, ROW - 1, ColArt].Merge();
                        sheet.Range[ArtRow, ColArt, ROW - 1, ColArt].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    }
                    ArtRow = ROW;
                }

                // Product Detail
                else if (ProdDetails != data.Rows[i]["ProdDetails"].ToString())
                {
                    ProdDetails = data.Rows[i]["ProdDetails"].ToString();
                    sheet[ROW, ColProdDet].Text = data.Rows[i]["ProdDetails"].ToString();

                    if (i != 0 && LotRow != (ROW - 1))
                    {
                        sheet.Range[ProdDetailsRow, ColProdDet, ROW - 1, ColProdDet].Merge();
                        sheet.Range[ProdDetailsRow, ColProdDet, ROW - 1, ColProdDet].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;

                    }
                    ProdDetailsRow = ROW;
                }
                // Product Detail End

                if (LotNum != data.Rows[i]["LotNo"].ToString())
                {

                    LotNum = data.Rows[i]["LotNo"].ToString();

                    sheet[ROW, ColLot].Text = data.Rows[i]["LotNo"].ToString();

                    if (i != 0 && LotRow != (ROW - 1))
                    {
                        //sheet.Range[LotRow, ColProdDet, ROW - 1, ColProdDet].Merge();
                        //sheet.Range[LotRow, ColProdDet, ROW - 1, ColProdDet].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[LotRow, ColLot, ROW - 1, ColLot].Merge();
                        sheet.Range[LotRow, ColLot, ROW - 1, ColLot].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;

                    }
                    LotRow = ROW;

                    // PRODUCT ID

                    if (POId != data.Rows[i]["POId"].ToString())
                    {
                        POId = data.Rows[i]["POId"].ToString();

                        sheet[ROW, ColPOId].Number = clsStaticInfo.dbl(data.Rows[i]["POId"].ToString());
                        if (i != 0 && POIdRow != (ROW - 1))
                        {
                            sheet.Range[POIdRow, ColPOId, ROW - 1, ColPOId].Merge();
                            sheet.Range[POIdRow, ColPOId, ROW - 1, ColPOId].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        POIdRow = ROW;
                    }
                }

                //Product Code
                if (ProductCode != data.Rows[i]["ProductCode"].ToString())
                {
                    ProductCode = data.Rows[i]["ProductCode"].ToString();

                    sheet[ROW, ColProductCode].Number = clsStaticInfo.dbl(data.Rows[i]["ProductCode"].ToString());
                    if (i != 0 && ProductCodeRow != (ROW - 1))
                    {
                        sheet.Range[ProductCodeRow, ColProductCode, ROW - 1, ColProductCode].Merge();
                        sheet.Range[ProductCodeRow, ColProductCode, ROW - 1, ColProductCode].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    }
                    ProductCodeRow = ROW;
                }

                sheet[ROW, ColBagSize].Number = clsStaticInfo.dbl(data.Rows[i]["BagSize"].ToString());
                sheet[ROW, ColBags].Number = clsStaticInfo.dbl(data.Rows[i]["Bags"].ToString());
                sheet[ROW, ColNtWt].Number = clsStaticInfo.dbl(data.Rows[i]["NtWt"].ToString());
                sheet[ROW, ColGWt].Number = clsStaticInfo.dbl(data.Rows[i]["GtWt"].ToString());


                arr[0] += clsStaticInfo.dbl(data.Rows[i]["Bags"].ToString());
                arr[1] += clsStaticInfo.dbl(data.Rows[i]["NtWt"].ToString());
                arr[2] += clsStaticInfo.dbl(data.Rows[i]["GtWt"].ToString());

                sheet.Range[ROW, ColBagSize, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, ColBagSize, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            ROW++;

            sheet[ROW, ColArt].Text = "TOTAL";
            sheet[ROW, ColBags].Number = arr[0];
            sheet[ROW, ColNtWt].Number = arr[1];
            sheet[ROW, ColGWt].Number = arr[2];

            sheet.Range[ROW, ColArt, ROW, ColBagSize].Merge();
            sheet.Range[ROW, ColArt, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, ColArt, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW, ColArt, ROW, endCol].CellStyle.Font.Bold = true;
            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1


            #region sheet2

            sheet1.Name = "All Stocks";

            int ROW1 = 6;
            int endCol1 = 1;
            int COL1 = 1;

            //sheet.Range[ROW, COL].Text = "From - "+FromDate+" , To - "+ToDate;
            //sheet.Range[ROW, COL].ColumnWidth = 13;
            //sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            //sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //ROW += 2;

            #region Grid Headers

            report.SetHeaderText(ref sheet1, ROW1, COL1, "Article", 40, ExcelHAlign.HAlignCenter);
            int ColArt1 = COL1;
            COL1++;

            report.SetHeaderText(ref sheet1, ROW1, COL1, "Lot No", 13, ExcelHAlign.HAlignCenter);
            int ColLot1 = COL1;
            COL1++;

            report.SetHeaderText(ref sheet1, ROW1, COL1, "Cartons", 13, ExcelHAlign.HAlignCenter);
            int ColCarton = COL1;
            COL1++;

            report.SetHeaderText(ref sheet1, ROW1, COL1, "Net Weight", 13, ExcelHAlign.HAlignCenter);
            int ColNtWt1 = COL1;
            COL1++;

            report.SetHeaderText(ref sheet1, ROW1, COL1, "Gross Weight", 13, ExcelHAlign.HAlignCenter);
            int ColGWt1 = COL1;
            COL1++;

            ROW1++;
            endCol1 = COL1;
            #endregion Headers


            var startRow1 = 0;
            var endRow1 = 0;
            int RowIndex1 = ROW1;
            startRow1 = ROW1;

            //string Article1 = "";
            //string LotNum1 = "";
            //int ArtRow1 = 0;
            //int LotRow1 = 0;

            //double[] arr1 = new double[3];

            for (int i = 0; i < data1.Rows.Count; i++)
            {
                sheet1[ROW1, ColArt1].Text = data1.Rows[i]["StandardName"].ToString();
                sheet1[ROW1, ColLot1].Text = data1.Rows[i]["LotNo"].ToString();
                sheet1[ROW1, ColCarton].Text = data1.Rows[i]["Cartons"].ToString();
                sheet1[ROW1, ColNtWt1].Number = clsStaticInfo.dbl(data1.Rows[i]["NtWt"].ToString());
                sheet1[ROW1, ColGWt1].Number = clsStaticInfo.dbl(data1.Rows[i]["GtWt"].ToString());

                //arr1[0] += clsStaticInfo.dbl(data1.Rows[i]["Bags"].ToString());
                //arr1[1] += clsStaticInfo.dbl(data1.Rows[i]["NtWt"].ToString());
                //arr1[2] += clsStaticInfo.dbl(data1.Rows[i]["GtWt"].ToString());

                sheet1.Range[ROW1, ColArt1, ROW1, endCol1].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[ROW1, ColArt1, ROW1, endCol1].BorderAround(ExcelLineStyle.Hair);

                ROW1++;

            }

            ROW1++;

            //sheet[ROW, ColArt].Text = "TOTAL";
            //sheet[ROW, ColBags].Number = arr[0];
            //sheet[ROW, ColNtWt].Number = arr[1];
            //sheet[ROW, ColGWt].Number = arr[2];

            //sheet.Range[ROW, ColArt, ROW, ColBagSize].Merge();
            //sheet.Range[ROW, ColArt, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            //sheet.Range[ROW, ColArt, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            //sheet.Range[ROW, ColArt, ROW, endCol].CellStyle.Font.Bold = true;
            //ROW++;

            endRow1 = ROW1 - 1;
            endRow1 = ROW1 - 1;
            #endregion sheet2


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            sheet1.UsedRange.WrapText = true;
            sheet1.UsedRange.CellStyle.Font.Size = 8;

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "Finished Stock Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            reportUtility.CompanyHeader(ref sheet1, endCol1, "All Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet1, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
      

    }   
}