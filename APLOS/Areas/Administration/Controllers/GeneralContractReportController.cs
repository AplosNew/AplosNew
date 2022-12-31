using Aplos.Controllers;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.Service.Administration.Contract;
using Aplos.Properties;
using Library.Security.Core;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using System.IO;
using System.Data;

namespace Aplos.Areas.Administration.Controllers
{
    public class GeneralContractReportController : BaseController
    {
        ContractReportService cr = new ContractReportService();
        private readonly SqlRepository _sqlRepository;
        public GeneralContractReportController()
        {
            _sqlRepository = new SqlRepository();
        }
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult GetAllTransactionData(string from, string to, string contractid, string entityid)
        {
            var sql = "";
            try
            {
                if (entityid == null || entityid == "null")
                {
                    sql = @"select FORMAT([GCE].[Date], 'dd-MMM-yyyy')[Date], E.UserName Entity, GCI.UserName Item, 
CIE.TransactionQuantity Quantity
, CIE.Rate, CIE.Amount, EI.EmployeeName 'To Be Check', EMP.EmployeeName 'To Be Approved', GCE.ApprovedStatus, 
GCE.CheckedByStatus
from TRN.GeneralContractEntry GCE
LEFT JOIN TRN.ContractItemEntry CIE on CIE.GeneralContractEntryId = GCE.Id
LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId
left join EmployeeInformation EI on EI.SystemId = GCE.CheckBySystemId
left join EmployeeInformation EMP on EMP.SystemId = GCE.ApprovedById
                        where GCE.Date between '" + from + "' and '" + to + "' and GCE.GeneralContractId = '" + contractid + "' and ApprovedStatus = 'Approved'";
                }
                else
                {
                    sql = @"select FORMAT([GCE].[Date], 'dd-MMM-yyyy')[Date], E.UserName Entity, GCI.UserName Item, 
CIE.TransactionQuantity Quantity
, CIE.Rate, CIE.Amount, EI.EmployeeName 'To Be Check', EMP.EmployeeName 'To Be Approved', GCE.ApprovedStatus, 
GCE.CheckedByStatus
from TRN.GeneralContractEntry GCE
LEFT JOIN TRN.ContractItemEntry CIE on CIE.GeneralContractEntryId = GCE.Id
LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId
left join EmployeeInformation EI on EI.SystemId = GCE.CheckBySystemId
left join EmployeeInformation EMP on EMP.SystemId = GCE.ApprovedById
                        where GCE.Date between '" + from + "' and '" + to + "' and GCE.GeneralContractId = '" + contractid + "' and E.Id = '" + entityid + "' and ApprovedStatus = 'Approved'";
                }

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetSummaryData(string from, string to, string contractid, string entityid)
        {
            var sql = "";
            try
            {
                if (entityid == null || entityid == "null")
                {
                    sql = @"select  GCI.UserName Item, CIE.TransactionQuantity Quantity
                            , CIE.Amount
                            from TRN.GeneralContractEntry GCE
                            LEFT JOIN TRN.ContractItemEntry CIE on CIE.GeneralContractEntryId = GCE.Id
                            LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
                            left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId
                        where GCE.Date between '" + from + "' and '" + to + "' and GCE.GeneralContractId = '" + contractid + "'";
                }
                else
                {
                    sql = @"select  GCI.UserName Item, CIE.TransactionQuantity Quantity
                            , CIE.Amount
                            from TRN.GeneralContractEntry GCE
                            LEFT JOIN TRN.ContractItemEntry CIE on CIE.GeneralContractEntryId = GCE.Id
                            LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
                            left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId
                        where GCE.Date between '" + from + "' and '" + to + "' and GCE.GeneralContractId = '" + contractid + "' and E.Id = '" + entityid + "'";
                }

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [Authorize, HttpPost]
        public ActionResult XlsDownloadContractTransactionReport(string from, string to, string contractid, string entityid)
        {
            try
            {

                string fileName = "";
                fileName = ContractTransactionExcelView(from, to, contractid, entityid, "ContractTransactionReport");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string ContractTransactionExcelView(string from, string to, string contractid, string entityid, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";

            try
            {

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Contract Transaction";
                sheet = workbook.Worksheets[0];
                DataTable data;
                cr.GetContractTransactionExcelReport(from, to, contractid, entityid, out data);
                
                int ROW = 6; int COL = 1;

                #region Columns
                sheet[ROW, COL].Text = "Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "Item";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColItem = COL;
                COL++;

                sheet[ROW, COL].Text = "Qty";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColQuantity = COL;
                COL++;

                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRate = COL;
                COL++;

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Check By";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColTBC = COL;
                COL++;
                

                sheet[ROW, COL].Text = "Check Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCheckedSts = COL;
                COL++;

                sheet[ROW, COL].Text = "Approve By";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColTBA = COL;
                COL++;

                sheet[ROW, COL].Text = "Approve Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColApprovedSts = COL;
                //COL++;

                //COL++;
                #endregion Columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;
                double[] arr = new double[3];
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColDate].Text = data.Rows[i]["Date"].ToString();
                    sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, ColItem].Text = data.Rows[i]["Item"].ToString();
                    sheet[ROW, ColQuantity].Number = clsStaticInfo.dbl(data.Rows[i]["Quantity"].ToString());
                    sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data.Rows[i]["Rate"].ToString());
                    sheet[ROW, ColAmount].Number = clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());
                    sheet[ROW, ColTBC].Text = data.Rows[i]["To Be Check"].ToString();
                    sheet[ROW, ColTBA].Text = data.Rows[i]["To Be Approved"].ToString();
                    sheet[ROW, ColCheckedSts].Text = data.Rows[i]["CheckedByStatus"].ToString();
                    sheet[ROW, ColApprovedSts].Text = data.Rows[i]["ApprovedStatus"].ToString();

                    arr[0] += clsStaticInfo.dbl(data.Rows[i]["Quantity"].ToString());
                    arr[1] += clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());
                    
                    ROW++;
                }

                sheet[ROW, ColDate].Text = "Total";
                sheet[ROW, ColQuantity].Number = arr[0];
                sheet[ROW, ColAmount].Number = arr[1];
                sheet.Range[ROW, ColDate, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, ColDate, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, ColDate, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;


                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();
                
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Contract Transaction Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = true;
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;


                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
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

        // Summary Report
        [Authorize, HttpPost]
        public ActionResult XlsDownloadSummaryReport(string from, string to, string contractid, string entityid)
        {
            try
            {

                string fileName = "";
                fileName = ContractTransactionSummaryExcelView(from, to, contractid, entityid, "ContractTransactionSummaryReport");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string ContractTransactionSummaryExcelView(string from, string to, string contractid, string entityid, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";

            try
            {

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Contract Transaction Summary";
                sheet = workbook.Worksheets[0];
                DataTable data;
                cr.GetContractTransactionExcelReport(from, to, contractid, entityid, out data);

                int ROW = 6; int COL = 1;

                #region Columns
                

                sheet[ROW, COL].Text = "Item";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColItem = COL;
                COL++;

                sheet[ROW, COL].Text = "Qty";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColQuantity = COL;
                COL++;

                

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColAmount = COL;
                //COL++;

               // COL++;
                #endregion Columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                int startRow = ROW;
                double[] arr = new double[3];
                for (int i = 0; i < data.Rows.Count; i++)
                {
                   
                    sheet[ROW, ColItem].Text = data.Rows[i]["Item"].ToString();
                    sheet[ROW, ColQuantity].Number = clsStaticInfo.dbl(data.Rows[i]["Quantity"].ToString());                    
                    sheet[ROW, ColAmount].Number = clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());

                    arr[0] += clsStaticInfo.dbl(data.Rows[i]["Quantity"].ToString());
                    arr[1] += clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());
                    
                    ROW++;
                }

                sheet[ROW, ColItem].Text = "Total";
                sheet[ROW, ColQuantity].Number = arr[0];
                sheet[ROW, ColAmount].Number = arr[1];
                sheet.Range[ROW, ColItem, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, ColItem, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, ColItem, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, ColAmount, ROW, ColAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
               
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Contract Transaction Summary Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = true;
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;


                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
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

    }
}