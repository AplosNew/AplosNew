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
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System.IO;
using System.Drawing;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionGeneralReportController : BaseController
    {

        ProductionGeneralReportService ps = new ProductionGeneralReportService();
        
        
        #region Constructor

        
        public ProductionGeneralReportController()
        {
        }

        #endregion Constructor

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page



        #region Get Operations

        [Authorize, HttpPost]
        public ActionResult getProcess()
        {
            return Json(ps.getProcess(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet , Authorize]
        public ActionResult getFilters()
        {
            var jsondata = Json( ps.getFilters(), JsonRequestBehavior.AllowGet );
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

            //return Json(ps.getFilters() , JsonRequestBehavior.AllowGet);
        }

        [HttpPost , Authorize]
        public ActionResult getMasterGrid(Dictionary<string , object> filters , string ProcessId)
        {
            try
            {
                return Json(new { Error = false, Data = ps.getMasterGrid(filters, ProcessId) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message =  ex.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult getPos()
        {
            return Json(ps.getPo() , JsonRequestBehavior.AllowGet);
        }

        #endregion Get Operations

        #region Modals

        [HttpPost , Authorize]
        public ActionResult masterDetail(string PRId , string Col , Dictionary<string, object> Filters, string ProcessId)
        {
            return Json(ps.masterDetail(PRId , Col , Filters , ProcessId), JsonRequestBehavior.AllowGet);
        }

        #endregion Modals

        #region Report Operations

        [HttpPost, Authorize]
        public ActionResult getReports(string PRId , Dictionary<string, object> Filters, string ProcessId)
        {

            try
            {
                var workbook = getReportsDown(PRId , Filters, ProcessId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "ProductionReport.xlsx";
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
        private IWorkbook getReportsDown(string PRId , Dictionary<string, object> Filters, string ProcessId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = ps.getReports(PRId , Filters , ProcessId);


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Production-Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers
            report.SetHeaderText(ref sheet, ROW, COL, "PR", 40, ExcelHAlign.HAlignCenter);
            int ColPR = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Buyer", 40, ExcelHAlign.HAlignCenter);
            int ColBuyer = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Master Order No", 13, ExcelHAlign.HAlignCenter);
            int ColMO = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Own Order No", 13, ExcelHAlign.HAlignCenter);
            int ColOwnOrder = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Buyer Ref", 13, ExcelHAlign.HAlignCenter);
            int ColBuyRef = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO No", 13, ExcelHAlign.HAlignCenter);
            int ColSONo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Color", 20, ExcelHAlign.HAlignCenter);
            int ColColor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Size", 13, ExcelHAlign.HAlignCenter);
            int ColSize = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Order Qty", 13, ExcelHAlign.HAlignCenter);
            int ColOQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plan Qty", 13, ExcelHAlign.HAlignCenter);
            int ColPlQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Produced Qty", 13, ExcelHAlign.HAlignCenter);
            int ColPRQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "To Produce", 13, ExcelHAlign.HAlignCenter);
            int ColToProd = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Excess Produce", 13, ExcelHAlign.HAlignCenter);
            int ColEx = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Cutting %", 13, ExcelHAlign.HAlignCenter);
            int ColCut = COL;
            COL++;

            sheet.Range[ROW, ColPR, ROW, ColCut].CellStyle.Color = Color.Black;
            sheet.Range[ROW, ColPR, ROW, ColCut].CellStyle.Font.Bold = true;
            sheet.Range[ROW, ColPR, ROW, ColCut].CellStyle.Font.Color = ExcelKnownColors.White;

            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Customer = "";
            string So = "";
            string PR = "";
            string Colors = "";
            int CusRow = 0;
            int SoRow = 0;
            int PrRow = 0;
            int ColorsRows = 0;

            double[] arr = new double[5];
            double[] farr = new double[5];
            Array.Clear(farr, 0, farr.Length);
            for (int i = 0; i < data.Rows.Count; i++)
            {
               
                if (PR != data.Rows[i]["ProductionOrderId"].ToString())
                {
                    PR = data.Rows[i]["ProductionOrderId"].ToString();
                    sheet[ROW, ColPR].Text = data.Rows[i]["ProductionOrderId"].ToString();

                    //if (i != 0 && CusRow != (ROW - 1))
                    //{
                    //    sheet.Range[CusRow, ColBuyer, ROW - 1, ColBuyRef].Merge();
                    //    sheet.Range[CusRow, ColBuyer, ROW - 1, ColBuyRef].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //}
                    PrRow = ROW;

                }
                else
                {
                    sheet.Range[PrRow, ColPR, ROW, ColPR].Merge();
                    sheet.Range[PrRow, ColPR, ROW, ColPR].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;

                }

                if (Customer != data.Rows[i]["Buyer"].ToString() || PR != data.Rows[i]["ProductionOrderId"].ToString())
                {
                    Customer = data.Rows[i]["Buyer"].ToString();
                    sheet[ROW, ColBuyer].Text = data.Rows[i]["Buyer"].ToString();
                    sheet[ROW, ColMO].Text = data.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, ColOwnOrder].Text = data.Rows[i]["OwnRef"].ToString();
                    sheet[ROW, ColBuyRef].Text = data.Rows[i]["BuyerRef"].ToString();

                    //if (i != 0 && CusRow != (ROW - 1))
                    //{
                    //    sheet.Range[CusRow, ColBuyer, ROW - 1, ColBuyRef].Merge();
                    //    sheet.Range[CusRow, ColBuyer, ROW - 1, ColBuyRef].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //}
                    CusRow = ROW;

                }
                else
                {
                    sheet.Range[CusRow, ColBuyer, ROW , ColBuyer].Merge();
                    sheet.Range[CusRow, ColMO, ROW , ColMO].Merge();
                    sheet.Range[CusRow, ColOwnOrder, ROW , ColOwnOrder].Merge();
                    sheet.Range[CusRow, ColBuyRef, ROW , ColBuyRef].Merge();
                    sheet.Range[CusRow, ColBuyer, ROW , ColBuyRef].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;

                }

                if (So != data.Rows[i]["SalesOrderId"].ToString())
                {
                    So = data.Rows[i]["SalesOrderId"].ToString();
                    sheet[ROW, ColSONo].Text = data.Rows[i]["SalesOrderId"].ToString();

                    SoRow = ROW;

                }
                else
                {
                    sheet.Range[SoRow, ColSONo, ROW, ColSONo].Merge();
                    sheet.Range[SoRow, ColSONo, ROW, ColSONo].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;

                }

                if (So != data.Rows[i]["SalesOrderId"].ToString() || Colors != data.Rows[i]["CharV"].ToString())
                {
                    if(i != 0)
                    {
                        sheet[ROW, ColColor].Text = Colors + " Sub Total";

                        sheet[ROW, ColOQty].Number = arr[0];
                        sheet[ROW, ColPlQty].Number = arr[1];
                        sheet[ROW, ColPRQty].Number = arr[2];
                        sheet[ROW, ColToProd].Number = arr[3];
                        sheet[ROW, ColEx].Number = arr[4];
                        farr[0] += arr[0];
                        farr[1] += arr[1];
                        farr[2] += arr[2];
                        farr[3] += arr[3];
                        farr[4] += arr[4];
                        Array.Clear(arr, 0, arr.Length);
                        sheet.Range[ROW, ColColor, ROW, ColCut].CellStyle.Color = Color.Gray;
                        sheet.Range[ROW, ColColor, ROW, ColCut].CellStyle.Font.Bold = false;
                        sheet.Range[ROW, ColColor, ROW, ColCut].CellStyle.Font.Size = 4;

                        ROW++;
                    }



                    Colors = data.Rows[i]["CharV"].ToString();
                    sheet[ROW, ColColor].Text = data.Rows[i]["CharV"].ToString();
                    ColorsRows = ROW;
                }
                else
                {
                    sheet.Range[ColorsRows, ColColor, ROW, ColColor].Merge();
                    sheet.Range[ColorsRows, ColColor, ROW, ColColor].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                }

                sheet[ROW, ColSize].Text = data.Rows[i]["Char2V"].ToString();
                sheet[ROW, ColOQty].Number = clsStaticInfo.dbl(data.Rows[i]["OrderQty"].ToString());
                sheet[ROW, ColPlQty].Number = clsStaticInfo.dbl(data.Rows[i]["PlanQty"].ToString());
                sheet[ROW, ColPRQty].Number = clsStaticInfo.dbl(data.Rows[i]["ProducedQty"].ToString());
                sheet[ROW, ColToProd].Number = clsStaticInfo.dbl(data.Rows[i]["ToProduce"].ToString());
                sheet[ROW, ColEx].Number = clsStaticInfo.dbl(data.Rows[i]["ExcessProduce"].ToString());
                sheet[ROW, ColCut].Text = (clsStaticInfo.dbl(data.Rows[i]["Percents"].ToString())).ToString() +'%';

                arr[0] += clsStaticInfo.dbl(data.Rows[i]["OrderQty"].ToString());
                arr[1] += clsStaticInfo.dbl(data.Rows[i]["PlanQty"].ToString());
                arr[2] += clsStaticInfo.dbl(data.Rows[i]["ProducedQty"].ToString());
                arr[3] += clsStaticInfo.dbl(data.Rows[i]["ToProduce"].ToString());
                arr[4] += clsStaticInfo.dbl(data.Rows[i]["ExcessProduce"].ToString());

                sheet.Range[ROW, ColSize, ROW, ColCut].BorderInside(ExcelLineStyle.Medium);
                sheet.Range[ROW, ColSize, ROW, ColCut].BorderAround(ExcelLineStyle.Medium);

                ROW++;

            }
            sheet[ROW, ColColor].Text = Colors + " Sub Total";

            sheet[ROW, ColOQty].Number = arr[0];
            sheet[ROW, ColPlQty].Number = arr[1];
            sheet[ROW, ColPRQty].Number = arr[2];
            sheet[ROW, ColToProd].Number = arr[3];
            sheet[ROW, ColEx].Number = arr[4];
            farr[0] += arr[0];
            farr[1] += arr[1];
            farr[2] += arr[2];
            farr[3] += arr[3];
            farr[4] += arr[4];
            sheet.Range[ROW, ColColor, ROW, ColCut].CellStyle.Color = Color.Gray;
            sheet.Range[ROW, ColColor, ROW, ColCut].CellStyle.Font.Bold = false;
            sheet.Range[ROW, ColColor, ROW, ColCut].CellStyle.Font.Size = 4;


            ROW++;

            sheet[ROW, ColPR].Text = "Total";

            sheet[ROW, ColOQty].Number = farr[0];
            sheet[ROW, ColPlQty].Number = farr[1];
            sheet[ROW, ColPRQty].Number = farr[2];
            sheet[ROW, ColToProd].Number = farr[3];
            sheet[ROW, ColEx].Number = farr[4];

            sheet.Range[ROW, ColPR, ROW, ColCut].CellStyle.Color = Color.Black;
            sheet.Range[ROW, ColPR, ROW, ColCut].CellStyle.Font.Bold = false;
            sheet.Range[ROW, ColPR, ROW, ColCut].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, ColPR, ROW, ColCut].CellStyle.Font.Size = 4;
            ROW++;
            //sheet[ROW, ColArt].Text = "TOTAL";
            //sheet[ROW, ColBags].Number = arr[0];
            //sheet[ROW, ColNtWt].Number = arr[1];
            //sheet[ROW, ColGWt].Number = arr[2];

            //sheet.Range[ROW, ColArt, ROW, ColBagSize].Merge();
            //sheet.Range[ROW, ColArt, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            //sheet.Range[ROW, ColArt, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            //sheet.Range[ROW, ColArt, ROW, endCol].CellStyle.Font.Bold = true;
            //ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1



            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 12;

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "Cutting Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion Report Operations

        #region secondTabOperations

        [HttpPost, Authorize]
        public ActionResult generate(string PO)
        {
            var data = ps.generate(PO, out List<string> DynCols);
            return Json( new { Data = data , Cols = DynCols}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult generateReport(string PO)
        {

            try
            {
                var workbook = generateReportForm(PO);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "POWiseReport.xlsx";
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
        private IWorkbook generateReportForm(string PO)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = ps.generateReport( PO, out List<string> DynCols);

            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "PO Wise Report";

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

            report.SetHeaderText(ref sheet, ROW, COL, "PO", 15, ExcelHAlign.HAlignCenter);
            int ColPo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO", 15, ExcelHAlign.HAlignCenter);
            int ColSo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Buyer", 40, ExcelHAlign.HAlignCenter);
            int ColBuy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Buyer Ref", 40, ExcelHAlign.HAlignCenter);
            int ColBuyRef = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Own Ref", 15, ExcelHAlign.HAlignCenter);
            int ColOwnRef = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SKU1", 15, ExcelHAlign.HAlignCenter);
            int ColSku = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Order Qty", 15, ExcelHAlign.HAlignCenter);
            int ColOrQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plan Qty", 20, ExcelHAlign.HAlignCenter);
            int ColPlQty = COL;
            COL++;

            int ColSt = COL;

            for (int i = 0; i < DynCols.Count; i++)
            {
                report.SetHeaderText(ref sheet, ROW, COL, DynCols[i], 10, ExcelHAlign.HAlignCenter);

                COL++;
            }

           


            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;

            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {
                //clsStaticInfo.dbl()
                sheet[ROW, ColPo].Text = data.Rows[i]["PO"].ToString();
                sheet[ROW, ColSo].Text = data.Rows[i]["SO"].ToString();
                sheet[ROW, ColBuy].Text = data.Rows[i]["Buyer"].ToString();
                sheet[ROW, ColBuyRef].Text = data.Rows[i]["BuyerRef"].ToString();
                sheet[ROW, ColOwnRef].Text = data.Rows[i]["OwnRef"].ToString();
                sheet[ROW, ColSku].Text = data.Rows[i]["SKU1"].ToString();
               
                sheet[ROW, ColOrQty].Number = clsStaticInfo.dbl(data.Rows[i]["OrderQty"].ToString());
                sheet[ROW, ColPlQty].Number = clsStaticInfo.dbl(data.Rows[i]["PlanQty"].ToString());
                int k = ColSt;
                for (int j = 0; j < DynCols.Count; j++)
                {
                    sheet[ROW, k].Number = clsStaticInfo.dbl(data.Rows[i][DynCols[j]].ToString());
                    k++;
                }


                sheet.Range[ROW, ColPo, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, ColPo, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

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
            reportUtility.CompanyHeader(ref sheet, endCol, "PO Wise Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion secondTabOperations

    }
}