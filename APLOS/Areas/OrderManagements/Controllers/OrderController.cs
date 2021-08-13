using Library.Model.IE;
using Aplos.Properties;
using Library.Data;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using Library.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using Library.Service.IE;
using Library.Model.Inventory;
using Library.Service.Systems;
using Library.Service.Enums;
using Library.Planning.OrderManagement;
using Syncfusion.XlsIO;
using System.Data;
using Library.Data.Sql;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class OrderController : Controller
    {
        #region Constructor

        Order Order = new Order();
        private SqlRepository _sqlRepository;

        public OrderController()
        {
            _sqlRepository = new SqlRepository();
        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }


        #endregion -- Pages

        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            return Json(Order.filters(), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetOrderReport(Dictionary<string, string> parameters, string fromDate, string toDate, string dateType)
        {
            try
            {
               
                //Library.Planning.OrderManagement.Order Report = new Library.Planning.OrderManagement.Order();
                //Report.OrderReport( parameters,  fromDate,  toDate,  dateType);
                
                string fileName = "";
                fileName = OrderReport(parameters, fromDate, toDate, dateType, "OrderReport");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string OrderReport(Dictionary<string, string> parameters, string fromDate, string toDate, string dateType,string SheetName)
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
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[0].Name = "Data";
                sheet = workbook.Worksheets[0];
                DataTable dtOrder;
                OrderReportSQL(parameters, fromDate, toDate, dateType, out dtOrder);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 16;
                int colResponsiblePerson = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Reference No.";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyerRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 22;
                int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Delivery Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Ex Factory Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanExFactoryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer Group";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomerAccountGroup = COL;
                COL++;

                sheet[ROW, COL].Text = "Material ROW ID";
                sheet[ROW, COL].ColumnWidth = 22;
                int colMaterialRowId = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 22;
                int colMaterial = COL;
                COL++;

                sheet[ROW, COL].Text = "Product Category";
                sheet[ROW, COL].ColumnWidth = 14;
                int colProductCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 14;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 14;
                int colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order Creation Date";
                sheet[ROW, COL].ColumnWidth = 14;
                int colMasterOrderCreationDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Id";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "PR No";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colRate = COL;
                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colCM = COL;
                COL++;
                sheet[ROW, COL].Text = "SPT";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSPT = COL;
                COL++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int colRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSOQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Shipped Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colShippedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Bal Shipment";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBalShipment = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlan = COL;
                COL++;
                sheet[ROW, COL].Text = "To Plan";
                sheet[ROW, COL].ColumnWidth = 16;
                int colToPlan = COL;
                COL++;
                sheet[ROW, COL].Text = "Process Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProcessStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Product Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductCode = COL;
                //COL++;
                //sheet[ROW, COL].Text = "Product";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int colProduct = COL;
                //COL++;
                //sheet[ROW, COL].Text = "Material";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Ref";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOwnRef = COL;
                COL++;
                sheet[ROW, COL].Text = "Description";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDescription = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOtherRawMaterialInhouseDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Material Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colMainMaterialRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Other Raw Material Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOtherRawMaterialRemarks = COL;
                COL++;


                sheet[ROW, COL].Text = "Input Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colInputStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Line Target";
                sheet[ROW, COL].ColumnWidth = 12;
                int colLineTarget = COL;
                COL++;
                sheet[ROW, COL].Text = "No of Line Plan";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colNoOfLinePlan = COL;
                COL++;

                sheet[ROW, COL].Text = "Priority";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPriority = COL;
                COL++;
                sheet[ROW, COL].Text = "Line No.";
                sheet[ROW, COL].ColumnWidth = 12;
                int colLineNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Value";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOrderValue = COL;
                COL++;
                sheet[ROW, COL].Text = "CM Value";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOrderStatus = COL;
                //COL++;
                //sheet[ROW, COL].Text = "Remarks";
                //sheet[ROW, COL].ColumnWidth = 12;
                //int colRemarks = COL;


                #endregion columns

                int endCol = COL;
                //sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
                //sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                //for (int i = 0; i < dtOrder.Rows.Count; i++)
                //{
                //    sheet[ROW, colPlant].Text = dtOrder.Rows[i]["Plant"].ToString();
                //    sheet[ROW, colEntity].Text = dtOrder.Rows[i]["Entity"].ToString();
                //    sheet[ROW, colBuyer].Text = dtOrder.Rows[i]["Buyer"].ToString();
                //    sheet[ROW, colCustomer].Text = dtOrder.Rows[i]["Customer"].ToString();
                //    sheet[ROW, colCustomerAccountGroup].Text = dtOrder.Rows[i]["CustomerAccountGroup"].ToString();
                //    sheet[ROW, colCommitmentDate].Text = GetDate(dtOrder.Rows[i]["CommitmentDate"].ToString());
                //    sheet[ROW, colDeliveryDate].Text = GetDate(dtOrder.Rows[i]["DeliveryDate"].ToString());
                //    sheet[ROW, colMasterOrderNo].Text = dtOrder.Rows[i]["MasterOrderNo"].ToString();
                //    sheet[ROW, colMaterial].Text = dtOrder.Rows[i]["Material"].ToString();
                //    sheet[ROW, colProductCategory].Text = dtOrder.Rows[i]["ProductCategory"].ToString();
                //    sheet[ROW, colProduct].Text = dtOrder.Rows[i]["Product"].ToString();
                //    sheet[ROW, colSalesOrderDesc].Text = dtOrder.Rows[i]["SODesc"].ToString();
                //    sheet[ROW, colUOM].Text = dtOrder.Rows[i]["UOM"].ToString();
                //    sheet[ROW, colCurrency].Text = dtOrder.Rows[i]["Currency"].ToString();
                //    sheet[ROW, colMasterOrderCreationDate].Text = dtOrder.Rows[i]["MasterOrderCreationDate"].ToString();


                //    sheet[ROW, colBulletinId].Text = dtOrder.Rows[i]["BulletinId"].ToString();
                //    sheet[ROW, colTotalSPT].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["TotalSPT"].ToString());
                //    sheet[ROW, colNoOfWS].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["NoOfWS"].ToString());
                //    sheet[ROW, colContractId].Text = dtOrder.Rows[i]["ContractId"].ToString();
                //    sheet[ROW, colContractName].Text = dtOrder.Rows[i]["ContractName"].ToString();
                //    sheet[ROW, colLCNo].Text = dtOrder.Rows[i]["LCNo"].ToString();


                //    sheet[ROW, colArticle].Text = dtOrder.Rows[i]["Article"].ToString();
                //    sheet[ROW, colOwnReferenceNo].Text = dtOrder.Rows[i]["OwnReferenceNo"].ToString();
                //    sheet[ROW, colBuyerReferenceNo].Text = dtOrder.Rows[i]["BuyerReferenceNo"].ToString();

                //    sheet[ROW, colBuyerOrderNo].Text = dtOrder.Rows[i]["BuyerOrderNo"].ToString();
                //    sheet[ROW, colOwnOrderNo].Text = dtOrder.Rows[i]["OwnOrderNo"].ToString();


                //    sheet[ROW, colMaterialRowId].Text = dtOrder.Rows[i]["MaterialRowId"].ToString();
                //    sheet[ROW, colProductionOrderId].Text = dtOrder.Rows[i]["ProductionOrderId"].ToString();

                //    sheet[ROW, colProductionOrderRemarks].Text = dtOrder.Rows[i]["Remarks"].ToString();
                //    if (dtOrder.Rows[i]["ProductionOrderId"].ToString().Trim() == "")
                //        sheet[ROW, colProductionOrderRemarks].Text = "Yet to plan";

                //    sheet[ROW, colProductionStatus].Text = dtOrder.Rows[i]["ProductionStatus"].ToString();

                //    sheet[ROW, colReason].Text = dtOrder.Rows[i]["Reason"].ToString();


                //    sheet[ROW, colOrderCategory].Text = dtOrder.Rows[i]["OrderCategory"].ToString();
                //    sheet[ROW, colOrderStatus].Text = dtOrder.Rows[i]["OrderStatus"].ToString();
                //    sheet[ROW, colSOCategory].Text = dtOrder.Rows[i]["SOCategory"].ToString();
                //    sheet[ROW, colSOStatus].Text = dtOrder.Rows[i]["SOStatus"].ToString();
                //    sheet[ROW, colResponsiblePerson].Text = dtOrder.Rows[i]["ResponsiblePerson"].ToString();
                //    sheet[ROW, colType].Text = dtOrder.Rows[i]["Type"].ToString();
                //    sheet[ROW, colSOQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["SOQty"].ToString());
                //    sheet[ROW, colSalesOrderId].Text = dtOrder.Rows[i]["SalesOrderId"].ToString();
                //    sheet[ROW, colPONo].Text = dtOrder.Rows[i]["PONumber"].ToString();
                //    sheet[ROW, colPODate].Text = dtOrder.Rows[i]["PODate"].ToString();


                //    sheet[ROW, colPlannedQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["PlannedQty"].ToString());
                //    sheet[ROW, colFOB].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["FOB"].ToString());
                //    sheet[ROW, colCM].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["CM"].ToString());
                //    sheet[ROW, colDiff].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["Diff"].ToString());

                //    sheet[ROW, colOrderAmount].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["OrderAmount"].ToString());
                //    sheet[ROW, colCMAmount].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["CMAmount"].ToString());

                //    sheet[ROW, colSOAddedDate].Text = dtOrder.Rows[i]["SOAddedDate"].ToString();
                //    sheet[ROW, colMainRawMaterialInhouseDate].Text = dtOrder.Rows[i]["MainRawMaterialInhouseDate"].ToString();
                //    sheet[ROW, colOtherRawMaterialInhouseDate].Text = dtOrder.Rows[i]["OtherRawMaterialInhouseDate"].ToString();
                //    sheet[ROW, colLSD].Text = dtOrder.Rows[i]["LSD"].ToString();

                //    sheet[ROW, colDeliveryMonth].Formula = string.Concat("MONTH(", CellAddr(colDeliveryDate, ROW), ")");
                //    sheet[ROW, colCommitmentMonth].Formula = string.Concat("MONTH(", CellAddr(colCommitmentDate, ROW), ")");


                //    sheet[ROW, colDeliveryMonth].Formula = "CONCATENATE(Month(" + CellAddr(colDeliveryDate, ROW) + "),\"/\",Year(" + CellAddr(colDeliveryDate, ROW) + "))";
                //    sheet[ROW, colCommitmentMonth].Formula = "CONCATENATE(Month(" + CellAddr(colCommitmentDate, ROW) + "),\"/\",Year(" + CellAddr(colCommitmentDate, ROW) + "))";


                //    sheet[ROW, colPRBookedQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["PRBookedQuantity"].ToString());
                //    sheet[ROW, colSOBookedQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["SOBookedQuantity"].ToString());
                //    sheet[ROW, colTotalPRProducedQty].Formula = CellAddr(colPRBookedQty, ROW) + "+" + CellAddr(colSOBookedQty, ROW);
                //    sheet[ROW, colPRPlanQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["PRPlanQty"].ToString());


                //    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                //    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                //    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                //    ROW++;

                //}


                //sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                //sheet.UsedRange.WrapText = true;
                //sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.UsedRange["A7"].FreezePanes();

                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //ReportUtility reportUtility = new ReportUtility();
                //reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Order Report", identity.CompanyId, identity.CompanyName, "");

                //reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Order Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;


                #region Sheet Report
                workbook.Worksheets[1].Name = "Report";
                sheet = workbook.Worksheets[1];

                //DataTable dtOrder = _sqlRepository.GetDataTable(sql);

                ROW = 6; COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 16;
                colResponsiblePerson = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                colBuyer = COL;
                COL++;

                sheet[ROW, COL].Text = "Buyer Reference No.";
                sheet[ROW, COL].ColumnWidth = 16;
                colBuyerRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 22;
                colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Delivery Date";
                sheet[ROW, COL].ColumnWidth = 12;
                colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Ex Factory Date";
                sheet[ROW, COL].ColumnWidth = 12;
                colPlanExFactoryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer Group";
                sheet[ROW, COL].ColumnWidth = 16;
                colCustomerAccountGroup = COL;
                COL++;

                sheet[ROW, COL].Text = "Material ROW ID";
                sheet[ROW, COL].ColumnWidth = 22;
                colMaterialRowId = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 22;
                colMaterial = COL;
                COL++;

                sheet[ROW, COL].Text = "Product Category";
                sheet[ROW, COL].ColumnWidth = 14;
                colProductCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 14;
                colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 14;
                colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order Creation Date";
                sheet[ROW, COL].ColumnWidth = 14;
                colMasterOrderCreationDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Id";
                sheet[ROW, COL].ColumnWidth = 16;
                colSalesOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Status";
                sheet[ROW, COL].ColumnWidth = 16;
                colSalesOrderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "PR No";
                sheet[ROW, COL].ColumnWidth = 12;
                colProductionOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Status";
                sheet[ROW, COL].ColumnWidth = 12;
                colProductionStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                colRate = COL;
                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                colCM = COL;
                COL++;
                sheet[ROW, COL].Text = "SPT";
                sheet[ROW, COL].ColumnWidth = 10;
                colSPT = COL;
                COL++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                colRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                colSOQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Shipped Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                colShippedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Bal Shipment";
                sheet[ROW, COL].ColumnWidth = 16;
                colBalShipment = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan";
                sheet[ROW, COL].ColumnWidth = 16;
                colPlan = COL;
                COL++;
                sheet[ROW, COL].Text = "To Plan";
                sheet[ROW, COL].ColumnWidth = 16;
                colToPlan = COL;


                #endregion columns

                endCol = COL;
                //sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
                //sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                startRow = ROW;

                //for (int i = 0; i < dtOrder.Rows.Count; i++)
                //{
                //    sheet[ROW, colPlant].Text = dtOrder.Rows[i]["Plant"].ToString();
                //    sheet[ROW, colEntity].Text = dtOrder.Rows[i]["Entity"].ToString();
                //    sheet[ROW, colBuyer].Text = dtOrder.Rows[i]["Buyer"].ToString();
                //    sheet[ROW, colCustomer].Text = dtOrder.Rows[i]["Customer"].ToString();
                //    sheet[ROW, colCustomerAccountGroup].Text = dtOrder.Rows[i]["CustomerAccountGroup"].ToString();
                //    sheet[ROW, colCommitmentDate].Text = GetDate(dtOrder.Rows[i]["CommitmentDate"].ToString());
                //    sheet[ROW, colDeliveryDate].Text = GetDate(dtOrder.Rows[i]["DeliveryDate"].ToString());
                //    sheet[ROW, colMasterOrderNo].Text = dtOrder.Rows[i]["MasterOrderNo"].ToString();
                //    sheet[ROW, colMaterial].Text = dtOrder.Rows[i]["Material"].ToString();
                //    sheet[ROW, colProductCategory].Text = dtOrder.Rows[i]["ProductCategory"].ToString();
                //    sheet[ROW, colProduct].Text = dtOrder.Rows[i]["Product"].ToString();
                //    sheet[ROW, colSalesOrderDesc].Text = dtOrder.Rows[i]["SODesc"].ToString();
                //    sheet[ROW, colUOM].Text = dtOrder.Rows[i]["UOM"].ToString();
                //    sheet[ROW, colCurrency].Text = dtOrder.Rows[i]["Currency"].ToString();
                //    sheet[ROW, colMasterOrderCreationDate].Text = dtOrder.Rows[i]["MasterOrderCreationDate"].ToString();


                //    sheet[ROW, colBulletinId].Text = dtOrder.Rows[i]["BulletinId"].ToString();
                //    sheet[ROW, colTotalSPT].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["TotalSPT"].ToString());
                //    sheet[ROW, colNoOfWS].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["NoOfWS"].ToString());
                //    sheet[ROW, colContractId].Text = dtOrder.Rows[i]["ContractId"].ToString();
                //    sheet[ROW, colContractName].Text = dtOrder.Rows[i]["ContractName"].ToString();
                //    sheet[ROW, colLCNo].Text = dtOrder.Rows[i]["LCNo"].ToString();


                //    sheet[ROW, colArticle].Text = dtOrder.Rows[i]["Article"].ToString();
                //    sheet[ROW, colOwnReferenceNo].Text = dtOrder.Rows[i]["OwnReferenceNo"].ToString();
                //    sheet[ROW, colBuyerReferenceNo].Text = dtOrder.Rows[i]["BuyerReferenceNo"].ToString();

                //    sheet[ROW, colBuyerOrderNo].Text = dtOrder.Rows[i]["BuyerOrderNo"].ToString();
                //    sheet[ROW, colOwnOrderNo].Text = dtOrder.Rows[i]["OwnOrderNo"].ToString();


                //    sheet[ROW, colMaterialRowId].Text = dtOrder.Rows[i]["MaterialRowId"].ToString();
                //    sheet[ROW, colProductionOrderId].Text = dtOrder.Rows[i]["ProductionOrderId"].ToString();

                //    sheet[ROW, colProductionOrderRemarks].Text = dtOrder.Rows[i]["Remarks"].ToString();
                //    if (dtOrder.Rows[i]["ProductionOrderId"].ToString().Trim() == "")
                //        sheet[ROW, colProductionOrderRemarks].Text = "Yet to plan";

                //    sheet[ROW, colProductionStatus].Text = dtOrder.Rows[i]["ProductionStatus"].ToString();

                //    sheet[ROW, colReason].Text = dtOrder.Rows[i]["Reason"].ToString();


                //    sheet[ROW, colOrderCategory].Text = dtOrder.Rows[i]["OrderCategory"].ToString();
                //    sheet[ROW, colOrderStatus].Text = dtOrder.Rows[i]["OrderStatus"].ToString();
                //    sheet[ROW, colSOCategory].Text = dtOrder.Rows[i]["SOCategory"].ToString();
                //    sheet[ROW, colSOStatus].Text = dtOrder.Rows[i]["SOStatus"].ToString();
                //    sheet[ROW, colResponsiblePerson].Text = dtOrder.Rows[i]["ResponsiblePerson"].ToString();
                //    sheet[ROW, colType].Text = dtOrder.Rows[i]["Type"].ToString();
                //    sheet[ROW, colSOQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["SOQty"].ToString());
                //    sheet[ROW, colSalesOrderId].Text = dtOrder.Rows[i]["SalesOrderId"].ToString();
                //    sheet[ROW, colPONo].Text = dtOrder.Rows[i]["PONumber"].ToString();
                //    sheet[ROW, colPODate].Text = dtOrder.Rows[i]["PODate"].ToString();


                //    sheet[ROW, colPlannedQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["PlannedQty"].ToString());
                //    sheet[ROW, colFOB].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["FOB"].ToString());
                //    sheet[ROW, colCM].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["CM"].ToString());
                //    sheet[ROW, colDiff].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["Diff"].ToString());

                //    sheet[ROW, colOrderAmount].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["OrderAmount"].ToString());
                //    sheet[ROW, colCMAmount].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["CMAmount"].ToString());

                //    sheet[ROW, colSOAddedDate].Text = dtOrder.Rows[i]["SOAddedDate"].ToString();
                //    sheet[ROW, colMainRawMaterialInhouseDate].Text = dtOrder.Rows[i]["MainRawMaterialInhouseDate"].ToString();
                //    sheet[ROW, colOtherRawMaterialInhouseDate].Text = dtOrder.Rows[i]["OtherRawMaterialInhouseDate"].ToString();
                //    sheet[ROW, colLSD].Text = dtOrder.Rows[i]["LSD"].ToString();

                //    sheet[ROW, colDeliveryMonth].Formula = string.Concat("MONTH(", CellAddr(colDeliveryDate, ROW), ")");
                //    sheet[ROW, colCommitmentMonth].Formula = string.Concat("MONTH(", CellAddr(colCommitmentDate, ROW), ")");


                //    sheet[ROW, colDeliveryMonth].Formula = "CONCATENATE(Month(" + CellAddr(colDeliveryDate, ROW) + "),\"/\",Year(" + CellAddr(colDeliveryDate, ROW) + "))";
                //    sheet[ROW, colCommitmentMonth].Formula = "CONCATENATE(Month(" + CellAddr(colCommitmentDate, ROW) + "),\"/\",Year(" + CellAddr(colCommitmentDate, ROW) + "))";


                //    sheet[ROW, colPRBookedQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["PRBookedQuantity"].ToString());
                //    sheet[ROW, colSOBookedQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["SOBookedQuantity"].ToString());
                //    sheet[ROW, colTotalPRProducedQty].Formula = CellAddr(colPRBookedQty, ROW) + "+" + CellAddr(colSOBookedQty, ROW);
                //    sheet[ROW, colPRPlanQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["PRPlanQty"].ToString());


                //    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                //    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                //    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                //    ROW++;

                //}


                //sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                //sheet.UsedRange.WrapText = true;
                //sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.UsedRange["A7"].FreezePanes();

                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //ReportUtility reportUtility = new ReportUtility();
                //reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Order Report", identity.CompanyId, identity.CompanyName, "");

                //reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + startRow.ToString()].FreezePanes();

                identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Order Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;



                #endregion

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
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


        private void OrderReportSQL(Dictionary<string, string> parameters, string fromDate, string toDate, string dateType, out DataTable dtOrder)
        {
            string PlantId = parameters["PlantId"];
            string cu = parameters["CustomerId"];
            string b = parameters["BuyerId"];
            string d = parameters["ProductionStatusId"];
            string e = parameters["EntityId"];

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT 
	trkp.UserName AS Plant,trke.UserName AS Entity,
	so.Id AS SalesOrderId,btn.TotalSPT,

					format(mo.AddedDate,'dd-MMM-yyyy') AS MasterOrderCreationDate,PO.Remarks,
					
                                               Buyer=STUFF((select distinct ','+XB.UserName from 
	                                                        trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),



                            os.UserName AS OrderStatus,os1.UserName AS SOStatus,    MA.StandardName AS Article,                   
                            pc.UserName AS ProductCategory,  pm.UserName AS Product,moi.BuyerReferenceNo,MOI.OwnReferenceNo
							,mo.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo AS OwnOrderNo,
                            mm.Id AS MaterialRowId,pod.ProductionOrderId,CASE WHEN isnull(sed.ID,0)<>0 THEN 'YES' ELSE 'NO' END AS isProductionScheduled
							,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') AS DeliveryDate
							,FORMAT(SO.CommitmentDate,'dd-MMM-yyyy') AS CommitmentDate
							,FORMAT(SO.PlanExFactoryDate,'dd-MMM-yyyy') AS ExFactoryDate
                          
							,so.Qty AS SOQty,SO.Reason, cp.PONumber,format(cp.PODate,'dd-MMM-yyyy') AS PODate,ps.Id ProductionStatusId,ps.UserName AS ProductionStatus
                            ,CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) AS PlannedQty
                            ,FORMAT(SO.AddedDate,'dd-MMM-yyyy') AS SOAddedDate
							,FORMAT(SO.MainRawMaterialInhouseDate,'dd-MMM-yyyy') AS MainRawMaterialInhouseDate
							,FORMAT(SO.OtherRawMaterialInhouseDate,'dd-MMM-yyyy') AS OtherRawMaterialInhouseDate
                            
							,CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate ELSE  so.Rate * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END AS FOB,
							 CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM ELSE  so.CM * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END AS CM,
							 (CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate ELSE  so.Rate * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)*SO.Qty AS OrderAmount,
							 (CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM ELSE  so.CM * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)*SO.Qty AS CMAmount,
							                           

                            FORMAT(SO.LSD,'dd-MMM-yyyy') AS LSD,isnull(DATEDIFF(DAY,so.LSD,so.DeliveryDate),0) AS Diff
                            ,uom.UserName AS UOM,so.[Description] AS SODesc,cur.Code AS Currency,MOI.[Type]
                            ,PRPD.PRBookedQuantity,sopd.SOBookedQuantity,PLN.PRPlanQty,p.UserName AS Customer
							,PAG.UserName AS CustomerAccountGroup
                              FROM trn.MasterOrder MO
                            left outer join MasterOrderExchangeRates RT on RT.TransactionId=MO.Id
							left JOIN org.Company AS com ON com.Id=mo.CompanyId
                            LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + parameters["EntityId"] + @"))
                            LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + parameters["EntityId"] + @"))

                            left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
							left outer join dbo.[Contract] con on con.Id=MOI.ContractId
							left outer join HKP.Party PA on PA.Id=con.CustomerId
							left outer join MasterLC M on m.Id=con.MasterLCId

                            left join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
                            LEFT OUTER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS SED ON sed.ProductionOrderID=pod.ProductionOrderId
                            LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                            LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
                            LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId

                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = po.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId

							LEFT JOIN (SELECT ps.ProductionOrderId,SUM(ps.Quantity) AS  PRBookedQuantity
                                            FROM trn.ProductionSummary AS ps 
                                            WHERE ISNULL(ps.SalesOrderId,'')='' AND ps.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=ps.ProductionOrderId)
                                       GROUP BY ps.ProductionOrderId) AS PRPD ON prpd.ProductionOrderId=pod.ProductionOrderId
                                       
                                        
                            LEFT JOIN (SELECT ps.SalesOrderId,SUM(ps.Quantity) AS  SOBookedQuantity
                                         FROM trn.ProductionSummary AS ps 
                                       WHERE ISNULL(ps.SalesOrderId,'')<>''  AND ps.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=ps.ProductionOrderId)
                                       GROUP BY ps.SalesOrderId) AS SOPD ON SOPD.SalesOrderId=so.Id

                         LEFT JOIN (SELECT ps.ProductionOrderID,SUM(ps.Quantity) AS  PRPlanQty
                                         FROM ProductionPlanningType1 AS ps 
                                       GROUP BY ps.ProductionOrderID) AS PLN ON PLN.ProductionOrderID=pod.ProductionOrderId

                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                           
                            left outer join [HKP].[Party] p on P.Id=MO.PartyId
                            LEFT JOIN [HKP].[CompanyParty] AS COMP ON COMP.PartyId=P.Id AND COMP.PartyType='Customer' AND (TRKP.Id=COMP.PlantId OR isnull(COMP.PlantId,'')='')
                            LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=COMP.PartyAccountGroupId

                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            left outer join [HKP].[OrderCategory] OC1 on oc1.id=so.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS1 on OS1.id=so.OrderStatusId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
							LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=MO.TotalQtyUOMId
							LEFT OUTER JOIN scs.Currency AS cur ON cur.Id=mo.CurrencyId
							left outer join (select pbt.Id BulletinId,pbt.productionOrderId,pbtm.MaxNoOfWS NoOfWS,sum( pbtd.TotalSPT ) TotalSPT from trn.ProductionBulletinTemplate pbt
left outer join trn.ProductionBulletinTemplateMaster pbtm on pbtm.ProductionBulletinTemplateId=pbt.id
left outer join trn.ProductionBulletinTemplateDetail pbtd on pbtd.ProductionBulletinTemplateMasterId=pbtm.Id
AND  pbtm.ProcessId=(select top 1 sx.ProcessId from trn.ProductionOrderProcessSet SX where SX.ProductionOrderId=pbt.productionOrderId and isnull(SX.IsBaseProcess,0)=1)
group by pbt.productionOrderId,pbtm.MaxNoOfWS, pbt.Id ) Btn on Btn.ProductionOrderId=po.Id
--left outer join BOQ on boq.SalesOrderId=so.Id and boq.SalesOrderId=(select top 1 SalesOrderId from boq where SalesOrderId=so.Id)
                            WHERE os.Id='Active' AND MO.EntityId IN (" + parameters["EntityId"] + @")
						AND MO.PlantId IN(" + parameters["PlantId"] + @"') 
						AND MO.PartyId in(" + parameters["CustomerId"] + @")
						AND MO.BuyerId in(" + parameters["BuyerId"] + @")
						AND MO.OrderStatusId in(" + parameters["ProductionStatusId"] + @")

						--AND(so.PlanExFactoryDate between (" + fromDate + @") AND (" + toDate + @"))

            ORDER BY	trkp.UserName,trke.UserName,PAG.UserName DESC, p.UserName, b.UserName,convert(date,so.DeliveryDate),SO.ID";
            dtOrder = _sqlRepository.GetDataTable(sql);


        }

    }

}