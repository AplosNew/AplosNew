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

                arr[0] += clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());

                ROW++;

            }

            ROW++;

            sheet[ROW, ColInvoiceNumber].Text = "Sum Of Amount";
            sheet[ROW, ColAmount].Number = arr[0];

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
        #endregion Excel Report
    }
}