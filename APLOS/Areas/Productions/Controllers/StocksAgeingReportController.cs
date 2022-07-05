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

    public class StocksAgeingReportController : BaseController
    {
        StocksAgeingReportService sa = new StocksAgeingReportService();

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public StocksAgeingReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost , Authorize]
        public ActionResult getData()
        {
            return Json(sa.getData( ), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getReport( )
        {

            try
            {
                var workbook = getReportForm( );

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
        private IWorkbook getReportForm()
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = sa.getReport();

            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Finished Stock Ageing Report";

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

            report.SetHeaderText(ref sheet, ROW, COL, "Product Details", 40, ExcelHAlign.HAlignCenter);
            int ColPD = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PO", 15, ExcelHAlign.HAlignCenter);
            int ColPo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Lot No", 15, ExcelHAlign.HAlignCenter);
            int ColLot = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Customer", 20, ExcelHAlign.HAlignCenter);
            int ColCus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Customer Type", 20, ExcelHAlign.HAlignCenter);
            int ColCusTyp = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Upto 15", 10, ExcelHAlign.HAlignCenter);
            int D15 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "15 - 30", 10, ExcelHAlign.HAlignCenter);
            int D15T30 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "30 - 60", 10, ExcelHAlign.HAlignCenter);
            int D30T60 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "60 - 90", 10, ExcelHAlign.HAlignCenter);
            int D60T90 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "90 - 120", 10, ExcelHAlign.HAlignCenter);
            int D90T120 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "120 - 150", 10, ExcelHAlign.HAlignCenter);
            int D120T150 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "150 - 180", 10, ExcelHAlign.HAlignCenter);
            int D150T180 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "180 - 360", 10, ExcelHAlign.HAlignCenter);
            int D180T360 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, ">360", 10, ExcelHAlign.HAlignCenter);
            int DG360 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total", 12, ExcelHAlign.HAlignCenter);
            int ColTot = COL;
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
                //clsStaticInfo.dbl()
                sheet[ROW, ColPCat].Text = data.Rows[i]["ProductCategory"].ToString();
                sheet[ROW, ColPSCat].Text = data.Rows[i]["ProductSubCategory"].ToString();
                sheet[ROW, ColMat].Text = data.Rows[i]["Material"].ToString();
                sheet[ROW, ColArt].Text = data.Rows[i]["Article"].ToString();
                sheet[ROW, ColPc].Text = data.Rows[i]["ProductCode"].ToString();
                sheet[ROW, ColPD].Text = data.Rows[i]["ProdDetails"].ToString();
                sheet[ROW, ColPo].Text = data.Rows[i]["POId"].ToString();
                sheet[ROW, ColLot].Text = data.Rows[i]["LotNo"].ToString();
                sheet[ROW, ColCus].Text = data.Rows[i]["Customers"].ToString();
                sheet[ROW, ColCusTyp].Text = data.Rows[i]["CustomerType"].ToString();
                sheet[ROW, D15].Number = clsStaticInfo.dbl(data.Rows[i]["D15"].ToString());
                sheet[ROW, D15T30].Number = clsStaticInfo.dbl(data.Rows[i]["D15T30"].ToString());
                sheet[ROW, D30T60].Number = clsStaticInfo.dbl(data.Rows[i]["D30T60"].ToString());
                sheet[ROW, D60T90].Number = clsStaticInfo.dbl(data.Rows[i]["D60T90"].ToString());
                sheet[ROW, D90T120].Number = clsStaticInfo.dbl(data.Rows[i]["D90T120"].ToString());
                sheet[ROW, D120T150].Number = clsStaticInfo.dbl(data.Rows[i]["D120T150"].ToString());
                sheet[ROW, D150T180].Number = clsStaticInfo.dbl(data.Rows[i]["D150T180"].ToString());
                sheet[ROW, D180T360].Number = clsStaticInfo.dbl(data.Rows[i]["D180T360"].ToString());
                sheet[ROW, DG360].Number = clsStaticInfo.dbl(data.Rows[i]["DG360"].ToString());
                sheet[ROW, ColTot].Number = sheet.Range[ROW, D15, ROW, DG360].Sum();


                sheet.Range[ROW, ColPCat, ROW, endCol-1].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, ColPCat, ROW, endCol-1].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            sheet[ROW, ColPCat].Text = "Total";
            sheet[ROW, D15].Number = sheet.Range[startRow, D15, ROW-1, D15].Sum();
            sheet[ROW, D15T30].Number = sheet.Range[startRow, D15T30, ROW-1, D15T30].Sum();
            sheet[ROW, D30T60].Number = sheet.Range[startRow, D30T60, ROW-1, D30T60].Sum();
            sheet[ROW, D60T90].Number = sheet.Range[startRow, D60T90, ROW-1, D60T90].Sum();
            sheet[ROW, D90T120].Number = sheet.Range[startRow, D90T120, ROW-1, D90T120].Sum();
            sheet[ROW, D120T150].Number = sheet.Range[startRow, D120T150, ROW-1, D120T150].Sum();
            sheet[ROW, D150T180].Number = sheet.Range[startRow, D150T180, ROW-1, D150T180].Sum();
            sheet[ROW, D180T360].Number = sheet.Range[startRow, D180T360, ROW-1, D180T360].Sum();
            sheet[ROW, DG360].Number = sheet.Range[startRow, DG360, ROW-1, DG360].Sum();
            sheet[ROW, ColTot].Number = sheet.Range[startRow, ColTot, ROW-1, ColTot].Sum();
            sheet.Range[ROW, ColPCat, ROW, endCol - 1].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, ColPCat, ROW, endCol - 1].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW, ColPCat, ROW, endCol - 1].CellStyle.Font.Bold= true;

            ROW++;

          

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1


         

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;


            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "Finished Stock Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
    }   
}