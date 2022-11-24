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
using System.Collections;

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

        //[HttpPost, Authorize]
        //public ActionResult XGetFinishedStocksReport(string Loc, string ToDate, string FromDate)
        //{

        //    try
        //    {
        //        var workbook = GetFinishedStocksReportForm(Loc, ToDate, FromDate);

        //        var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "FinishedStockReport.xlsx";
        //        string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
        //        workbook.SaveAs(fullPath);

        //        return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        [HttpPost, Authorize]
        public ActionResult GetFinishedStocksReport(string Loc, string ToDate, string FromDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = DateTime.Now.ToString("yy-MM-dd") + " " + "FinishedStockReport.xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                var workbook = GetFinishedStocksReportForm(Loc, ToDate, FromDate);


                return Json(new { FullPath = workbook, FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, string Text)
        {

            sheet.Range[xlsRow, xlsCol].Text = Text;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);

        }
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, double Number)
        {
            sheet.Range[xlsRow, xlsCol].Number = Number;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
        }
        private void SetHeadText(IWorksheet sheet, int xlsRow, int xlsCol, string text)
        {
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
        }
        public string GetFinishedStocksReportForm(string Loc, string ToDate, string FromDate)
        {
            try
            {
                #region Variable
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataSet dsCmp = null;
                var objRpt = new clsReport();

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //Create dataset

                #region Variable

                DateTime dtFrmDt = DateTime.Now;
                DateTime dtEndDate = DateTime.Now;
                ReportUtility ru = null;
                //DataSet dsCmp = null;
                DataSet dsFactory = null;


                #endregion Variable

                try
                {
                    objRpt = new clsReport(_sqlRepository);

                    var data = det.getGroupFinishedStocksReport(Loc);
                    objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                    objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                    if (data.Rows.Count == 0)
                    {
                        throw new Exception("Data not found.");

                    }

                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;
                    ru = new ReportUtility();
                    string CmpName;
                    string FactoryName;


                    xlsRow = 5;

                    #region ColumnHeaderVariables              
                    int cArticle = 0; int cProductCode = 0; int ColProdDet = 0; int ColPOId = 0; int cLot = 0; var cBagSize = 0; var cBag = 0; int ColNtWt = 0; int ColGWt = 0;
                    #endregion
                    #region ColumnHeaders
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Article",50, ExcelHAlign.HAlignCenter); cArticle = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Product Code", 14, ExcelHAlign.HAlignCenter); cProductCode = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Product Details", 25, ExcelHAlign.HAlignCenter); ColProdDet = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "POId", 14, ExcelHAlign.HAlignCenter); ColPOId = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Lot No", 14, ExcelHAlign.HAlignCenter); cLot = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Bag Size", 14, ExcelHAlign.HAlignCenter); cBagSize = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Bag", 14, ExcelHAlign.HAlignCenter); cBag = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Net Weight", 14, ExcelHAlign.HAlignCenter); ColNtWt = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Gross Weight", 14, ExcelHAlign.HAlignCenter); ColGWt = xlsCol; xlsCol++;

                    endXlsCol = xlsCol;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 40;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    var orgCollist = xlsCol;
                    xlsRow++;


                    #endregion
                    var startXlsRow = xlsRow;
                    if (data.Rows.Count > 0)
                    {
                        string _Article = string.Empty;
                        string _ProductCode = string.Empty;
                        string _ProdDet = string.Empty;
                        string _POId = string.Empty;
                        string _Lot = string.Empty;

                        var isFirst = true;
                        var catFRow = xlsRow;
                        ArrayList al = new ArrayList();
                        var lastEmpCat = string.Empty;
                        for (int i = 0; i <= data.Rows.Count - 1; i++)
                        {
                            var catLRow = xlsRow;
                            if (_Article != data.Rows[i]["StandardName"].ToString())
                            {
                                _Article = data.Rows[i]["StandardName"].ToString();

                                #region Subtotal
                                if (catFRow < xlsRow)
                                {
                                    lastEmpCat = _Article;
                                    al.Add(xlsRow);
                                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                    sheet1.Range[xlsRow, 1, xlsRow, (cBag - 1)].Merge();
                                    sheet1.Range[xlsRow, cBag].Formula = "=SUM(" + ru.GetColumnNameForXls(cBag) + catFRow + ":" + ru.GetColumnNameForXls(cBag) + (xlsRow - 1) + ")";
                                    sheet1.Range[xlsRow, ColNtWt].Formula = "=SUM(" + ru.GetColumnNameForXls(ColNtWt) + catFRow + ":" + ru.GetColumnNameForXls(ColNtWt) + (xlsRow - 1) + ")";
                                    sheet1.Range[xlsRow, ColGWt].Formula = "=SUM(" + ru.GetColumnNameForXls(ColGWt) + catFRow + ":" + ru.GetColumnNameForXls(ColGWt) + (xlsRow - 1) + ")";
                                    sheet1.Range[xlsRow, cBag, xlsRow, ColGWt].CellStyle.Font.Bold = true;

                                    xlsRow++;
                                }
                                #endregion
                                SetCellText(sheet1, xlsRow, cArticle, _Article);
                                _ProductCode = data.Rows[i]["ProductCode"].ToString();
                                SetCellText(sheet1, xlsRow, cProductCode, _ProductCode);
                                _ProdDet = data.Rows[i]["ProdDetails"].ToString();
                                SetCellText(sheet1, xlsRow, ColProdDet, _ProdDet);
                                _POId = data.Rows[i]["POId"].ToString();
                                SetCellText(sheet1, xlsRow, ColPOId, _POId);
                                _Lot = data.Rows[i]["LotNo"].ToString();
                                SetCellText(sheet1, xlsRow, cLot, _Lot);

                                if (catFRow < xlsRow)
                                {
                                    catFRow = xlsRow;
                                }
                            }
                            else if (_ProductCode != data.Rows[i]["ProductCode"].ToString())
                            {
                                _ProductCode = data.Rows[i]["ProductCode"].ToString(); SetCellText(sheet1, xlsRow, cProductCode, _ProductCode);
                                _ProdDet = data.Rows[i]["ProdDetails"].ToString(); SetCellText(sheet1, xlsRow, ColProdDet, _ProdDet);
                                _POId = data.Rows[i]["POId"].ToString(); SetCellText(sheet1, xlsRow, ColPOId, _POId);
                                _Lot = data.Rows[i]["LotNo"].ToString();SetCellText(sheet1, xlsRow, cLot, _Lot);
                            }
                            else if (_ProdDet != data.Rows[i]["ProdDetails"].ToString())
                            {
                                _ProdDet = data.Rows[i]["ProdDetails"].ToString(); SetCellText(sheet1, xlsRow, ColProdDet, _ProdDet);
                                _POId = data.Rows[i]["POId"].ToString(); SetCellText(sheet1, xlsRow, ColPOId, _POId);
                                _Lot = data.Rows[i]["LotNo"].ToString();SetCellText(sheet1, xlsRow, cLot, _Lot);
                            }
                            else if (_POId != data.Rows[i]["POId"].ToString())
                            {
                                _POId = data.Rows[i]["POId"].ToString(); SetCellText(sheet1, xlsRow, ColPOId, _POId);
                                _Lot = data.Rows[i]["LotNo"].ToString();SetCellText(sheet1, xlsRow, cLot, _Lot);
                            }
                            else if (_Lot != data.Rows[i]["LotNo"].ToString())
                            {
                                _Lot = data.Rows[i]["LotNo"].ToString(); SetCellText(sheet1, xlsRow, cLot, _Lot);
                            }

                            SetCellText(sheet1, xlsRow, cBagSize, Convert.ToDouble(data.Rows[i]["BagSize"].ToString()));
                            sheet1.Range[xlsRow, cBagSize].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                            SetCellText(sheet1, xlsRow, cBag, Convert.ToDouble(data.Rows[i]["Bags"].ToString()));
                            sheet1.Range[xlsRow, cBag].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                            SetCellText(sheet1, xlsRow, ColNtWt, Convert.ToDouble(data.Rows[i]["NtWt"].ToString()));
                            sheet1.Range[xlsRow, ColNtWt].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                            SetCellText(sheet1, xlsRow, ColGWt, Convert.ToDouble(data.Rows[i]["GtWt"].ToString()));
                            sheet1.Range[xlsRow, ColGWt].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                            sheet1.Range[xlsRow, cBagSize, xlsRow, ColGWt].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            xlsRow++;
                        }//for emp count

                        #region Last subtotal
                        al.Add(xlsRow);
                        SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                        sheet1.Range[xlsRow, 1, xlsRow, (cBag - 1)].Merge();
                        sheet1.Range[xlsRow, cBag].Formula = "=SUM(" + ru.GetColumnNameForXls(cBag) + catFRow + ":" + ru.GetColumnNameForXls(cBag) + (xlsRow - 1) + ")";
                        sheet1.Range[xlsRow, ColNtWt].Formula = "=SUM(" + ru.GetColumnNameForXls(ColNtWt) + catFRow + ":" + ru.GetColumnNameForXls(ColNtWt) + (xlsRow - 1) + ")";
                        sheet1.Range[xlsRow, ColGWt].Formula = "=SUM(" + ru.GetColumnNameForXls(ColGWt) + catFRow + ":" + ru.GetColumnNameForXls(ColGWt) + (xlsRow - 1) + ")";
                        sheet1.Range[xlsRow, cBag, xlsRow, ColGWt].CellStyle.Font.Bold = true;
                        xlsRow++;
                        #endregion

                        #region Grand Total
                        SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                        sheet1.Range[xlsRow, 1, xlsRow, (cBag - 1)].Merge();


                        sheet1.Range[xlsRow, cBag].Formula = GetFormulaGrandTotal(al, cBag);
                        sheet1.Range[xlsRow, ColNtWt].Formula = GetFormulaGrandTotal(al, ColNtWt);
                        sheet1.Range[xlsRow, ColGWt].Formula = GetFormulaGrandTotal(al, ColGWt);
                        sheet1.Range[xlsRow, cBag, xlsRow, ColGWt].CellStyle.Font.Bold = true;

                        #endregion

                    }

                    #region ******************Report Header******************
                    xlsRow = 1;
                    xlsCol = 1;
                    //Param param = new Param();
                    var CompanyGroupId = identity.CompanyGroupId;
                    var CompanyId = identity.CompanyId;

                    string FactoryAddress = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Finished Stock Report";
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                    #endregion ******************Report Header******************


                    var fileName = "Finished Stock Report" + DateTime.Now.ToString("yyMMdd") + ".xlsx";
                    var filePath = "";
                    var SheetName = "";
                    workbook.Version = ExcelVersion.Excel2013;
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + fileName);
                    workbook.SaveAs(filePath);
                    workbook.Close();
                    excelEngine.Dispose();
                    return filePath;


                    //return workbook;
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        string GetFormulaGrandTotal(ArrayList al, int col)
        {
            string _formula = string.Empty;
            ReportUtility ru = new ReportUtility();
            try
            {
                for (int i = 0; i < al.Count; i++)
                {
                    if (_formula.Length == 0)
                    {
                        _formula = "=" + ru.GetColumnNameForXls(col) + al[i];
                    }
                    else
                    {
                        _formula += "+" + ru.GetColumnNameForXls(col) + al[i];
                    }
                }
                return _formula;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        private IWorkbook XGetFinishedStocksReportForm(string Loc, string ToDate, string FromDate)
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