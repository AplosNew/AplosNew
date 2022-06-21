#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Payrolls;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.HumanResources.Profile;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class OTControlLimitController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        public OTControlLimitController(ISqlRepository R)
        {
            _sqlRepository=R;
        }
        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region SampleFile
        [HttpPost, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat, List<Dictionary<string, object>> GridTempList, Dictionary<string, object> fabricRollMaster)
        {
            string fileName = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            fileName = GetSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, GridTempList, fabricRollMaster);
            var reportFileName = "Fabric Roll Management Template";
            return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            //switch (reportFormat)
            //{
            //    case ReportFormat.Pdf:
            //        return RenderReportAsPdf(workbook, reportFileName);

            //    case ReportFormat.Excel:
            //        return RenderReportAsExcel(workbook, reportFileName);

            //    default:
            //        return RenderReportAsExcel(workbook, reportFileName);
            //}

        }

        public string GetSampleFile(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, List<Dictionary<string, object>> GridTempList, Dictionary<string, object> fabricRollMaster)
        {
            #region declare
            clsReport objRpt = null;
            OTSBD.clsStaticInfo objStatic = null;
            objStatic = new OTSBD.clsStaticInfo();
            string OTConsiderOn = string.Empty;

            int maxRow = 5001;

            #endregion

            try
            {
                //sorting
                //lock               
                var filePath = "";
                ReportUtility ru = new ReportUtility();

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = ru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);


                #region Lunch Out
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                IWorksheet sheetSource = null;
                sheetSource = workbook.Worksheets[1];

                int xlsRow = 1, xlsCol = 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GRNNo"); xlsCol += 1;
                sheet1[xlsRow, xlsCol].Text = fabricRollMaster["GRNNo"].ToString();
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GRN Date"); xlsCol += 1;
                if (fabricRollMaster["GRNDate"] != null)
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["GRNDate"].ToString();
                }
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Amount"); xlsCol++;
                sheet1[xlsRow, xlsCol].Text = fabricRollMaster["TransactionAmount"].ToString() + " " + fabricRollMaster["CurrencyCode"].ToString();
                xlsCol += 1;

                xlsRow++; xlsCol = 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PO No"); xlsCol++;
                //sheet1[xlsRow, xlsCol].Text = fabricRollMaster["POId"].ToString();
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["POId"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["POId"].ToString();
                }
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PO Date"); xlsCol++;
                //sheet1[xlsRow, xlsCol].Text = clsStaticInfo.SetDate(fabricRollMaster["PODate"].ToString());
                if (fabricRollMaster["PODate"] != null)
                {
                    OTSBD.clsStaticInfo.SetDate(sheet1[xlsRow, xlsCol], Convert.ToDateTime(fabricRollMaster["PODate"]).ToString("dd-MMM-yyyy"));
                }
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Vendor Ref No"); xlsCol++;
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["VendorRefNo"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["VendorRefNo"].ToString();
                }
                xlsCol += 1;

                xlsRow++; xlsCol = 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LC No"); xlsCol++;
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["PurchaseLCNo"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["PurchaseLCNo"].ToString();
                }
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LC Date"); xlsCol++;
                if (fabricRollMaster["LCDate"] != null)
                {
                    OTSBD.clsStaticInfo.SetDate(sheet1[xlsRow, xlsCol], Convert.ToDateTime(fabricRollMaster["LCDate"]).ToString("dd-MMM-yyyy"));
                }

                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PI No");
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["PINo"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["PINo"].ToString();
                }
                xlsCol += 1;

                xlsRow++; xlsCol = 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Vendor"); xlsCol++;
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["PartyName"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["PartyName"].ToString();
                }
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Opening Bank"); xlsCol++;
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["OpeningBank"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["OpeningBank"].ToString();
                }
                xlsCol += 1;

                xlsRow = 6; xlsCol = 1;
                int endXlsCol = 1;

                #region ------------------Column Header------------------

                int colSeq = 0; int colGRNRowId = 0; int colLotNo = 0; int colShade = 0; int colMarkarCode = 0; int colFabricGroup = 0; int colLength = 0;
                int colWeight = 0; int colShrinkage = 0; int colQty = 0; int colQtyUoM = 0; int colActualQty = 0; int colInvoiceQty = 0;
                int colSupplierRollNo = 0; int colOwnRollNo = 0; int colBuyerRollNo = 0; int colGrouping = 0; int colRemarks = 0;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sequence");
                colSeq = xlsCol;
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@";
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GRNRowId");
                colGRNRowId = xlsCol;
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@";
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LotNo");
                colLotNo = xlsCol;
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@";
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shade"); colShade = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "MarkarCode"); colMarkarCode = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FabricGroup"); colFabricGroup = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Length"); colLength = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Weight"); colWeight = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shrinkage"); colShrinkage = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Qty"); colQty = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "QtyUoM"); colQtyUoM = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ActualQty"); colActualQty = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "InvoiceQty"); colInvoiceQty = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SupplierRollNo"); colSupplierRollNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OwnRollNo"); colOwnRollNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BuyerRollNo"); colBuyerRollNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Grouping"); colGrouping = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarks"); colRemarks = xlsCol;


                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------
                int count = 0;
                #region DataPlot
                string grnId = string.Empty;

                foreach (var item in GridTempList)
                {
                    if (grnId == item["Id"].ToString())
                    {
                        for (int i = 0; i < Convert.ToInt32(item["RollNo"].ToString()); i++)
                        {
                            count++;
                            sheet1[xlsRow, 1].Number = count;
                            sheet1[xlsRow, 2].Text = item["Id"].ToString();

                            grnId = item["Id"].ToString();

                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Length";
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Weight";
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Qty";
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for ActualQty";
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for InvoiceQty";
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colLotNo, xlsRow, colLotNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShade, xlsRow, colShade].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colMarkarCode, xlsRow, colMarkarCode].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFabricGroup, xlsRow, colFabricGroup].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShrinkage, xlsRow, colShrinkage].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colQtyUoM, xlsRow, colQtyUoM].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colSupplierRollNo, xlsRow, colSupplierRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colOwnRollNo, xlsRow, colOwnRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colBuyerRollNo, xlsRow, colBuyerRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colGrouping, xlsRow, colGrouping].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colRemarks, xlsRow, colRemarks].CellStyle.Locked = false;

                            xlsRow++;




                        }

                    }
                    else
                    {

                        for (int i = 0; i < Convert.ToInt32(item["RollNo"].ToString()); i++)
                        {
                            count++;
                            sheet1[xlsRow, 1].Number = count;
                            sheet1[xlsRow, 2].Text = item["Id"].ToString();

                            grnId = item["Id"].ToString();

                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Length";
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Weight";
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Qty";
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for ActualQty";
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for InvoiceQty";
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colLotNo, xlsRow, colLotNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShade, xlsRow, colShade].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colMarkarCode, xlsRow, colMarkarCode].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFabricGroup, xlsRow, colFabricGroup].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShrinkage, xlsRow, colShrinkage].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colQtyUoM, xlsRow, colQtyUoM].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colSupplierRollNo, xlsRow, colSupplierRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colOwnRollNo, xlsRow, colOwnRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colBuyerRollNo, xlsRow, colBuyerRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colGrouping, xlsRow, colGrouping].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colRemarks, xlsRow, colRemarks].CellStyle.Locked = false;

                            xlsRow++;
                        }

                    }


                }


                xlsRow++;

                #endregion

                #region UsedRange Alignment

                sheet1.Protect(bplib.clsWebLib.REPORT_LOCK_PASSWORD, ExcelSheetProtection.Filtering | ExcelSheetProtection.All);
                workbook.Worksheets[1].Protect(bplib.clsWebLib.REPORT_LOCK_PASSWORD);
                workbook.Protect(false, true, bplib.clsWebLib.REPORT_LOCK_PASSWORD);

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 10;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Sheet1";
                #endregion Page Setup

                //sheetSource.Protect("2020", ExcelSheetProtection.Content);


                #endregion  Lunch Out

                //return workbook;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FabricRollManage" + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        [HttpPost, Authorize]
        public JsonResult ImportData()
        {
            string path;
            clsTemplateReadProfile objR = null;
            try
            {
                objR = new clsTemplateReadProfile();
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFiles(out path);
                var data = ReadData(identity.PlantId, path);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public void SaveFiles(out string path)
        {
            path = "";
            try
            {
               
                var file = Request.Files["file"];
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {
                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<OTControlLimitDetail> ReadData(string plantid, string path)
        {
            List<OTControlLimitDetail> data = null;
            //string path = "";
            DataSet dsExcel = null;
            try
            {
                data = new List<OTControlLimitDetail>();
                ReadFile(path, out dsExcel);
                Validation(dsExcel, plantid);
                data = dsExcel.Tables[0].ToList<OTControlLimitDetail>();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Validation(DataSet dsExcel, string plantid)
        {

            try
            {

                if (dsExcel.Tables[0].Rows.Count > 0)
                {
                    if (false)
                    {
                        for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                        {
                            string strTempPDate = "";
                            string strTempPTimee = "";
                            string strTempPType = "";

                            strTempPDate = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                            strTempPTimee = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                            strTempPType = dsExcel.Tables[0].Rows[i][3].ToString().Trim().ToUpper();

                        }//for

                    }

                }
                else
                {
                    throw new Exception("Please Select File");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void ReadFile(string path, out DataSet dsExcel)
        {
            FileInfo docFile;
            dsExcel = null;
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = excelEngine.Excel.Workbooks.Open(path);
                DataTable dt = workbook.Worksheets[0].ExportDataTable(6, 1, 5000, 18, ExcelExportDataTableOptions.ColumnNames);
                dt.DefaultView.RowFilter = "isnull(Sequence,'')<>''";
                dt = dt.DefaultView.ToTable();
                dsExcel = new DataSet();
                dsExcel.Tables.Add(dt);
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
            }
            catch (Exception ex)
            {
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> detailList)
        {
            SaveData(data, detailList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        private void SaveData(Dictionary<string, object> data, List<Dictionary<string, object>> detailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsMaster, dsDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.OTControlLimit WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id= "";
                string masterId = "";

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OTControlLimit", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.OTControlLimitDetail WHERE OTControlLimitId ='" + masterId + "'", out dsDetail, false, "1");

                int count = 0;
                foreach (var item in detailList)
                {
                    count++;
                    DataView dv = new DataView(dsDetail.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = masterId + "-" + count;
                        item["OTControlLimitId"] = masterId;

                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                }


                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        #region -- Operations



        #endregion -- Operations
    }

    public class OTControlLimitDetail
    {

        public string Id { get; set; }
        public string OTControlLimitId { get; set; }
        public string BudgetCode { get; set; }
        public string BudgetCodeId { get; set; }
        public string DailyOTLimit { get; set; }
        public string WeeklyOTLimit { get; set; }
        public string WeekOffOTLimit { get; set; }
        public string MonthlyOTLimit { get; set; }
        public string Remarks { get; set; }

    }
}