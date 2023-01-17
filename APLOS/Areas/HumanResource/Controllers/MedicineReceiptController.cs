#region LIB
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Data.Sql;
using Library.HumanResource.Employee;
using Aplos.Properties;
using System.Data;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System.IO;
using System.Threading;
using Library.Crosscutting.Security;
using Library.Security.Core;
#endregion LIB

namespace Aplos.Areas.HumanResource.Controllers
{
    public class MedicineReceiptController : BaseController
    {
        MedicineReceiptService mr = new MedicineReceiptService();
        #region PAGE
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion PAGE

        #region GET FUN
        [Authorize, HttpPost]
        public ActionResult getMedicineData()
        {
            try
            {
                return Json(mr.getMedicineData(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult getMedicineReceipt()
        {
            try
            {
                return Json(mr.getMedicineReceipt(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpGet]
        public ActionResult GetChildValue(string masterId)
        {
            try
            {
                return Json(mr.GetChildValue(masterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult getPlant()
        {
            try
            {
                return Json(mr.getPlant(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion GET FUN
        #region SEARCH SAVED DATA IN GRID 
        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            return Json(mr.GetList(column, value), JsonRequestBehavior.AllowGet);
        }
        #endregion SEARCH SAVED DATA IN GRID
        #region SAVE

        [HttpPost]
        public ActionResult SaveHeader(Dictionary<string, object> data, List<Dictionary<string, object>> medicinelist, string partyId)
        {
            try
            {
                return Json(new { Error = false, Data = mr.SaveHeader(data, medicinelist, partyId), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion SAVE

        #region Update
        [HttpPost]
        public ActionResult Update(Dictionary<string, object> data, List<Dictionary<string, object>> medicinelist, string partyId)
        {
            try
            {
                return Json(new { Error = false, Data = mr.Update(data, medicinelist, partyId), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion  Update

        #region Excel Report
        [Authorize, HttpGet]
        public ActionResult XlsMedicineReceipt(string headerid)
        {
            try
            {
                var workbook = MedicineReceiptExcel(headerid);

                var strFileName = DateTime.Now.ToString("yy-MMM-dd") + " " + "MedicineReceipt.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        private IWorkbook MedicineReceiptExcel(string headerid)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = mr.GetMedicineReceiptReport(headerid);


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Medicine Receipt";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers


            

            report.SetHeaderText(ref sheet, ROW, COL, "Invoice No.", 10, ExcelHAlign.HAlignCenter);
            int ColInvoiceNumber = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Vendor", 20, ExcelHAlign.HAlignCenter);
            int ColVendor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Invoice Date", 12, ExcelHAlign.HAlignCenter);
            int ColInvoiceDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Medicine", 20, ExcelHAlign.HAlignCenter);
            int ColMedicine = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Quantity", 12, ExcelHAlign.HAlignCenter);
            int ColQuantity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Amount", 12, ExcelHAlign.HAlignCenter);
            int ColAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate", 12, ExcelHAlign.HAlignCenter);
            int ColRate = COL;
            //COL++;


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
                sheet[ROW, ColInvoiceNumber].Number = clsStaticInfo.dbl(data.Rows[i]["InvoiceNumber"].ToString());
                sheet[ROW, ColVendor].Text = data.Rows[i]["PartyName"].ToString();
                sheet[ROW, ColInvoiceDate].Text = data.Rows[i]["InvoiceDate"].ToString();
                sheet[ROW, ColMedicine].Text = data.Rows[i]["Medicine"].ToString();
                sheet[ROW, ColQuantity].Number = clsStaticInfo.dbl(data.Rows[i]["Quantity"].ToString());
                sheet[ROW, ColAmount].Number = clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());
                sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data.Rows[i]["Rate"].ToString());

                arr[0] += clsStaticInfo.dbl(data.Rows[i]["Quantity"].ToString());
                arr[1] += clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());

                ROW++;

            }

            ROW++;

            sheet[ROW, ColInvoiceNumber].Text = "Grand Total";
            sheet[ROW, ColQuantity].Number = arr[0];
            sheet[ROW, ColAmount].Number = arr[1];

            sheet.Range[ROW, ColInvoiceNumber, ROW, endCol].CellStyle.Font.Bold = true;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, startRow, endCol];


            sheet.Range[startRow - 1, 1, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            ReportUtility reportUtility = new ReportUtility();
            
            return workbook;
        }

        [Authorize, HttpPost]
        public ActionResult XlsDownloadMedicineInvoiceReport(string from, string to)
        {
            try
            {

                string fileName = "";
                fileName = ContractTransactionSummaryExcelView(from, to, "Medicine Invoice Report");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string ContractTransactionSummaryExcelView(string from, string to, string SheetName)
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
                workbook.Worksheets[0].Name = "Medicine Invoice Report";
                sheet = workbook.Worksheets[0];
                DataTable data;
                mr.GetAllInvoiceDataPrint(from, to, out data);

                int ROW = 6; int COL = 1;

                #region Columns


                sheet[ROW, COL].Text = "Vendor";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColVendor = COL;
                COL++;

                sheet[ROW, COL].Text = "Invoice Number";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColInvoiceNum = COL;
                COL++;

                sheet[ROW, COL].Text = "Invoice Date";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColInvoiceDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Amount";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColTotalAmount = COL;

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

                    sheet[ROW, ColVendor].Text = data.Rows[i]["PartyName"].ToString();
                    sheet[ROW, ColInvoiceNum].Number = clsStaticInfo.dbl(data.Rows[i]["InvoiceNumber"].ToString());
                    sheet[ROW, ColInvoiceDate].Text = data.Rows[i]["InvoiceDate"].ToString().ToString();
                    sheet[ROW, ColTotalAmount].Number = clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());

                   
                    ROW++;
                }

              
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Medicine Invoice Report", identity.PlantId);
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

        #endregion Excel Report

        #region Delete
        public ActionResult RemoveParticular(string id)
        {
            try
            {

                string ret = mr.RemoveParticular(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        #endregion
    }
}