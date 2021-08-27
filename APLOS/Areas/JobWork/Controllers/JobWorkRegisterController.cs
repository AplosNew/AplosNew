#region Using

using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.MaterialManagement.JobWork;
using Syncfusion.XlsIO;
using System.IO;
using Library.Service.Helpers;


#endregion Using

namespace Aplos.Areas.JobWork.Controllers
{
    public class JobWorkRegisterController : BaseController
    {
        JobWorkRegisterService JWR = new JobWorkRegisterService();

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public JobWorkRegisterController(ISqlRepository R)
        {
            _sqlRepository = R;
            JWR = new JobWorkRegisterService();
    
        }


        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult LoadAllPartyVendorForSelection()
        {
            try
            {

                return Json(JWR.LoadAllPartyVendorForSelection(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpGet]
        public JsonResult LoadAllPOForSelection(string JWPOPartyId)
        {
            try
            {

                return Json(JWR.LoadAllPOForSelection(JWPOPartyId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        // MATERIAL RECONCILATION TRANSFORMATION REPORT

        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;

        }

        [HttpPost]
        public ActionResult GetTransformationRegisterReport(string FromDate, string ToDate, string PartyVendorId, string ContractId)
        {
            try
            {
                var workbook = GetTRANSFORMATIONData(FromDate, ToDate, PartyVendorId, ContractId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "JobWorkRegister.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private IWorkbook GetTRANSFORMATIONData(string FromDate, string ToDate, string PartyVendorId, string ContractId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "JWRegister";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = JWR.GetTransRegisterReportData(FromDate, ToDate, PartyVendorId, ContractId);
            DataTable dataChild = JWR.GetTransRegisterByProductReportData(FromDate, ToDate, PartyVendorId, ContractId);

            #region Headers

            //report.SetHeaderText(ref sheet, ROW, COL, "RECEIPT QUANTITY", 12, ExcelHAlign.HAlignLeft);
            //ROW++;

            report.SetHeaderText(ref sheet, ROW, COL, "Serial No", 8, ExcelHAlign.HAlignLeft);
            int ColSerialNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 8, ExcelHAlign.HAlignLeft);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 12, ExcelHAlign.HAlignLeft);
            int ColJobWorkItem = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "JW Location", 12, ExcelHAlign.HAlignLeft);
            int ColArticleCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "JW Activity", 12, ExcelHAlign.HAlignLeft);
            int ColJobWorkActivity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "JW Item Type", 12, ExcelHAlign.HAlignLeft);
            int ColJWItemType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Contract Date", 15, ExcelHAlign.HAlignLeft);
            int ColOutputUnit = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Contract Status", 15, ExcelHAlign.HAlignLeft);
            int ColContractStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Contract Id", 15, ExcelHAlign.HAlignLeft);
            int ColPlannedQuantity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Party Code", 12, ExcelHAlign.HAlignLeft);
            int ColTotalReceiptQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Party", 15, ExcelHAlign.HAlignLeft);
            int ColRatePerUnit = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GSTIN", 15, ExcelHAlign.HAlignLeft);
            int ColGSTIN = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Contract Closing Date", 15, ExcelHAlign.HAlignLeft);
            int ColTotalValue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Contract Line Item Id", 15, ExcelHAlign.HAlignLeft);
            int ColRateApply = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Item Id", 15, ExcelHAlign.HAlignLeft);
            int ColJobWorkItemMasterId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Item", 12, ExcelHAlign.HAlignLeft);
            int ColJWOutputItem = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article Id", 15, ExcelHAlign.HAlignLeft);
            int ColArticleCodeId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 15, ExcelHAlign.HAlignLeft);
            int ColJWOutputArticle = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "UOM", 15, ExcelHAlign.HAlignLeft);
            int ColOutUnit = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Planned Quantity", 15, ExcelHAlign.HAlignLeft);
            int ColPlannedQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate Applicable", 12, ExcelHAlign.HAlignLeft);
            int ColJWRateApplyId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Currency", 15, ExcelHAlign.HAlignLeft);
            int ColMPCurrency = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate/Unit", 15, ExcelHAlign.HAlignLeft);
            int ColJWRatePerUnit = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Contract Amount", 15, ExcelHAlign.HAlignLeft);
            int ColContractAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Receipt Quantity", 15, ExcelHAlign.HAlignLeft);
            int ColTotalReceiptQuantity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Balance Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColJWTotalBalQuantity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Receipt Amount", 15, ExcelHAlign.HAlignLeft);
            int ColTotalReceiptAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Receipt Location", 15, ExcelHAlign.HAlignLeft);
            int ColJWReceiptLocation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "No Of Input Item", 15, ExcelHAlign.HAlignLeft);
            int ColTotalNoOfInputItem = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "JW Input Plannned Quantity", 15, ExcelHAlign.HAlignLeft);
            int ColJWInputPlannnedQuantity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "JW Input Issue/ Return Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColJWJWInputIssueReturnQuantity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "JW Input Balance Quantity", 15, ExcelHAlign.HAlignLeft);
            int ColJWInputBalQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process Start Date", 15, ExcelHAlign.HAlignLeft);
            int ColContractProStartDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process End Date", 15, ExcelHAlign.HAlignLeft);
            int ColContractProEndDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Contract Remarks", 15, ExcelHAlign.HAlignLeft);
            int ColRemarks = COL;
            ROW++;


            endCol = COL;
            #endregion Headers

            string MPId = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string TempContractId = "";
            if (data.Rows.Count > 0)
                TempContractId = data.Rows[0]["Id"].ToString();

            int slcount = 0;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                

                if (MPId != data.Rows[i]["Id"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                        //      sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].Merge();
                        sheet.Range[RowIndex, ColId, ROW - 1, ColId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColId, ROW - 1, ColId].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                    }
                    RowIndex = ROW;
                }

                if (TempContractId != data.Rows[i]["Id"].ToString())
                {
                    dataChild.DefaultView.RowFilter = "ContractId='" + TempContractId + "'";
                    for (int CH = 0; CH < dataChild.DefaultView.Count; CH++)
                    {
                        //ROW++;

                        int BPRow = ROW++;
                        sheet[BPRow, ColSerialNo].Text = Convert.ToString(CH + 1+ i);
                        slcount = (CH + 1 + i);
                        sheet[BPRow, ColRateApply].Text = dataChild.DefaultView[CH]["Id"].ToString();

                        sheet[BPRow, ColJobWorkItemMasterId].Text = dataChild.DefaultView[CH]["JobWorkItemId"].ToString();
                        sheet[BPRow, ColJWOutputItem].Text = dataChild.DefaultView[CH]["ByProductItem"].ToString();
                        sheet[BPRow, ColArticleCodeId].Text = dataChild.DefaultView[CH]["ArticleCode"].ToString();
                        sheet[BPRow, ColJWOutputArticle].Text = dataChild.DefaultView[CH]["Article"].ToString();
                        sheet[BPRow, ColOutUnit].Text = dataChild.DefaultView[CH]["Unit"].ToString();
                        sheet[BPRow, ColPlannedQty].Text = dataChild.DefaultView[CH]["TotalReqQty"].ToString();
                        sheet[BPRow, ColJWRateApplyId].Text = dataChild.DefaultView[CH]["RateApplyId"].ToString();
                        sheet[BPRow, ColMPCurrency].Text = dataChild.DefaultView[CH]["Currency"].ToString();
                        sheet[BPRow, ColJWRatePerUnit].Text = dataChild.DefaultView[CH]["StandardRate"].ToString();
                        sheet[BPRow, ColContractAmount].Text = dataChild.DefaultView[CH]["ContractAmount"].ToString();

                        sheet[BPRow, ColTotalReceiptQuantity].Text = dataChild.DefaultView[CH]["TotalReceiptQty"].ToString();
                        sheet[BPRow, ColJWTotalBalQuantity].Text = dataChild.DefaultView[CH]["TotalReceiptBalance"].ToString();
                        sheet[BPRow, ColTotalReceiptAmount].Text = dataChild.DefaultView[CH]["TotalReceiptAmount"].ToString();

                        sheet[BPRow, ColId].Text = dataChild.DefaultView[CH]["Plant"].ToString();
                        sheet[BPRow, ColJobWorkItem].Text = dataChild.DefaultView[CH]["Entity"].ToString();
                        sheet[BPRow, ColArticleCode].Text = dataChild.DefaultView[CH]["JWLocation"].ToString();
                        sheet[BPRow, ColJobWorkActivity].Text = dataChild.DefaultView[CH]["JWActivity"].ToString();
                        sheet[BPRow, ColJWItemType].Text = dataChild.DefaultView[CH]["JWItemType"].ToString();
                        sheet[BPRow, ColOutputUnit].Text = dataChild.DefaultView[CH]["ContractDate"].ToString();
                        sheet[BPRow, ColContractStatus].Text = dataChild.DefaultView[CH]["ContractStatus"].ToString();

                        sheet[BPRow, ColPlannedQuantity].Text = dataChild.DefaultView[CH]["ContractId"].ToString();
                        sheet[BPRow, ColTotalReceiptQty].Text = dataChild.DefaultView[CH]["PartyCode"].ToString();

                        sheet[BPRow, ColRatePerUnit].Text = dataChild.DefaultView[CH]["Party"].ToString();
                        sheet[BPRow, ColGSTIN].Text = dataChild.DefaultView[CH]["GSTIN"].ToString();
                        sheet[BPRow, ColTotalValue].Text = dataChild.DefaultView[CH]["ContractCloseDate"].ToString();
                        BPRow++;
                     //   string x = i;
                    }

                }

                if (slcount!=0)
                {
                    sheet[ROW, ColSerialNo].Text = Convert.ToString(slcount + 1);
                }
                else
                {
                    sheet[ROW, ColSerialNo].Text = Convert.ToString(i + 1);
                }
                           
                sheet[ROW, ColId].Text = data.Rows[i]["Plant"].ToString();
                sheet[ROW, ColJobWorkItem].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColArticleCode].Text = data.Rows[i]["JWLocation"].ToString();
                sheet[ROW, ColJobWorkActivity].Text = data.Rows[i]["JWActivity"].ToString();
                sheet[ROW, ColJWItemType].Text = data.Rows[i]["JWItemType"].ToString();
                sheet[ROW, ColOutputUnit].Text = data.Rows[i]["ContractDate"].ToString();
                sheet[ROW, ColContractStatus].Text = data.Rows[i]["ContractStatus"].ToString();

                sheet[ROW, ColPlannedQuantity].Text = data.Rows[i]["Id"].ToString();
                sheet[ROW, ColTotalReceiptQty].Text = data.Rows[i]["PartyCode"].ToString();

                sheet[ROW, ColRatePerUnit].Text = data.Rows[i]["Party"].ToString();
                sheet[ROW, ColGSTIN].Text = data.Rows[i]["GSTIN"].ToString();
                sheet[ROW, ColTotalValue].Text = data.Rows[i]["ContractCloseDate"].ToString();
                sheet[ROW, ColRateApply].Text = data.Rows[i]["ContractLineItemId"].ToString();

                sheet[ROW, ColJobWorkItemMasterId].Text = data.Rows[i]["JobWorkItemMasterId"].ToString();
                sheet[ROW, ColJWOutputItem].Text = data.Rows[i]["JWOutputItem"].ToString();
                sheet[ROW, ColArticleCodeId].Text = data.Rows[i]["ArticleId"].ToString();
                sheet[ROW, ColJWOutputArticle].Text = data.Rows[i]["JWOutputArticle"].ToString();
                sheet[ROW, ColOutUnit].Text = data.Rows[i]["OutputUnit"].ToString();


                sheet[ROW, ColPlannedQty].Text = data.Rows[i]["PlannedQuantity"].ToString();
                sheet[ROW, ColJWRateApplyId].Text = data.Rows[i]["RateApplyId"].ToString();
                sheet[ROW, ColMPCurrency].Text = data.Rows[i]["MPCurrency"].ToString();
                sheet[ROW, ColJWRatePerUnit].Text = data.Rows[i]["RatePerUnit"].ToString();
                sheet[ROW, ColContractAmount].Text = data.Rows[i]["ContractAmount"].ToString();

                sheet[ROW, ColTotalReceiptQuantity].Text = data.Rows[i]["TotalReceiptQuantity"].ToString();
                sheet[ROW, ColJWTotalBalQuantity].Text = data.Rows[i]["TotalBalQuantity"].ToString();
                sheet[ROW, ColTotalReceiptAmount].Text = data.Rows[i]["TotalReceiptAmount"].ToString();
                sheet[ROW, ColJWReceiptLocation].Text = data.Rows[i]["ReceiptLocation"].ToString();
                sheet[ROW, ColTotalNoOfInputItem].Text = data.Rows[i]["TotalNoOfInputItem"].ToString();

                sheet[ROW, ColJWInputPlannnedQuantity].Text = data.Rows[i]["JWInputPlannnedQuantity"].ToString();
                sheet[ROW, ColJWJWInputIssueReturnQuantity].Text = data.Rows[i]["JWInputIssueReturnQuantity"].ToString();
                sheet[ROW, ColJWInputBalQty].Text = data.Rows[i]["JWInputBalQty"].ToString();
                sheet[ROW, ColContractProStartDate].Text = data.Rows[i]["ContractProStartDate"].ToString();
                sheet[ROW, ColContractProEndDate].Text = data.Rows[i]["ContractProEndDate"].ToString();

                sheet[ROW, ColRemarks].Text = data.Rows[i]["ContractRemarks"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                MPId = data.Rows[i]["Plant"].ToString();
                TempContractId= data.Rows[i]["Id"].ToString();
                ROW++;



            }
           
                dataChild.DefaultView.RowFilter = "ContractId='" + TempContractId + "'";
                for (int CH = 0; CH < dataChild.DefaultView.Count; CH++)
                {

                int BPRow = ROW++;

                sheet[BPRow, ColSerialNo].Text = Convert.ToString(CH + 1);
                sheet[BPRow, ColRateApply].Text = dataChild.DefaultView[CH]["Id"].ToString();

                sheet[BPRow, ColJobWorkItemMasterId].Text = dataChild.DefaultView[CH]["JobWorkItemId"].ToString();
                sheet[BPRow, ColJWOutputItem].Text = dataChild.DefaultView[CH]["ByProductItem"].ToString();
                sheet[BPRow, ColArticleCodeId].Text = dataChild.DefaultView[CH]["ArticleCode"].ToString();
                sheet[BPRow, ColJWOutputArticle].Text = dataChild.DefaultView[CH]["Article"].ToString();
                sheet[BPRow, ColOutUnit].Text = dataChild.DefaultView[CH]["Unit"].ToString();
                sheet[BPRow, ColPlannedQty].Text = dataChild.DefaultView[CH]["TotalReqQty"].ToString();
                sheet[BPRow, ColJWRateApplyId].Text = dataChild.DefaultView[CH]["RateApplyId"].ToString();
                sheet[BPRow, ColMPCurrency].Text = dataChild.DefaultView[CH]["Currency"].ToString();
                sheet[BPRow, ColJWRatePerUnit].Text = dataChild.DefaultView[CH]["StandardRate"].ToString();
                sheet[BPRow, ColContractAmount].Text = dataChild.DefaultView[CH]["ContractAmount"].ToString();

                sheet[BPRow, ColTotalReceiptQuantity].Text = dataChild.DefaultView[CH]["TotalReceiptQty"].ToString();
                sheet[BPRow, ColJWTotalBalQuantity].Text = dataChild.DefaultView[CH]["TotalReceiptBalance"].ToString();
                sheet[BPRow, ColTotalReceiptAmount].Text = dataChild.DefaultView[CH]["TotalReceiptAmount"].ToString();

                sheet[BPRow, ColId].Text = dataChild.DefaultView[CH]["Plant"].ToString();
                sheet[BPRow, ColJobWorkItem].Text = dataChild.DefaultView[CH]["Entity"].ToString();
                sheet[BPRow, ColArticleCode].Text = dataChild.DefaultView[CH]["JWLocation"].ToString();
                sheet[BPRow, ColJobWorkActivity].Text = dataChild.DefaultView[CH]["JWActivity"].ToString();
                sheet[BPRow, ColJWItemType].Text = dataChild.DefaultView[CH]["JWItemType"].ToString();
                sheet[BPRow, ColOutputUnit].Text = dataChild.DefaultView[CH]["ContractDate"].ToString();
                sheet[BPRow, ColContractStatus].Text = dataChild.DefaultView[CH]["ContractStatus"].ToString();

                sheet[BPRow, ColPlannedQuantity].Text = dataChild.DefaultView[CH]["ContractId"].ToString();
                sheet[BPRow, ColTotalReceiptQty].Text = dataChild.DefaultView[CH]["PartyCode"].ToString();

                sheet[BPRow, ColRatePerUnit].Text = dataChild.DefaultView[CH]["Party"].ToString();
                sheet[BPRow, ColGSTIN].Text = dataChild.DefaultView[CH]["GSTIN"].ToString();
                sheet[BPRow, ColTotalValue].Text = dataChild.DefaultView[CH]["ContractCloseDate"].ToString();
                BPRow++;
            }

            

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
                //      sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].Merge();
                sheet.Range[RowIndex, ColId, ROW - 1, ColId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColId, ROW - 1, ColId].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColArticleCode, ROW - 1, ColArticleCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColArticleCode, ROW - 1, ColArticleCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColJobWorkActivity, ROW - 1, ColJobWorkActivity].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJobWorkActivity, ROW - 1, ColJobWorkActivity].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColOutputUnit, ROW - 1, ColOutputUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColOutputUnit, ROW - 1, ColOutputUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                //sheet.Range[RowIndex, ColTotalIssuedQty, ROW - 1, ColTotalIssuedQty].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet.Range[RowIndex, ColTotalIssuedQty, ROW - 1, ColTotalIssuedQty].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColTotalReceiptQty, ROW - 1, ColTotalReceiptQty].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColTotalReceiptQty, ROW - 1, ColTotalReceiptQty].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                //sheet.Range[RowIndex, ColDiff, ROW - 1, ColDiff].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet.Range[RowIndex, ColDiff, ROW - 1, ColDiff].HorizontalAlignment = ExcelHAlign.HAlignLeft;



                sheet.Range[RowIndex, ColRatePerUnit, ROW - 1, ColRatePerUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColRatePerUnit, ROW - 1, ColRatePerUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColRateApply, ROW - 1, ColRateApply].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColRateApply, ROW - 1, ColRateApply].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColRemarks, ROW - 1, ColRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColRemarks, ROW - 1, ColRemarks].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            report.CompanyPlantHeader(ref sheet, endCol, "Job Work Transformation Register", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

    }
}