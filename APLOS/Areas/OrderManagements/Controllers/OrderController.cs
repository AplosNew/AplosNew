using Library.Model.IE;
using Aplos.Properties;
using Library.Data;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using Library.Core;
using System;
using System.Collections.Generic;
using System.IO;
using OTSBD;
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
using Library.Security.Core;

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
                workbook = application.Workbooks.Create(3);
                workbook.Worksheets[2].Name = "Data";
                sheet = workbook.Worksheets[2];
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
                sheet[ROW, COL].Text = "Commitment Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCommitmentDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Item Reference No.";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyerRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 22;
                int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Shipment Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Ex Factory Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanExFactoryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Id";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer PO";
                sheet[ROW, COL].ColumnWidth = 12;
                int colCustomerPO = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order Id";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Start Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionStartDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order Category";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionOrderCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "LSD";
                sheet[ROW, COL].ColumnWidth = 12;
                int colLSD = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Raw Material Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colMainrawMaterialDate = COL;
                COL++;
                sheet[ROW, COL].Text = "other Raw Material Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOtherRawMaterialDate = COL;
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
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 16;
                int colMaterial = COL;
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
                int colorderRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colorderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Material Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colMainMaterialRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Material Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colMainMaterialStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Other Raw Material Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOtherRawMaterialRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Other Raw Material Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOtherRawMaterialStatus = COL;
                COL++;

                
                sheet[ROW, COL].Text = "Input Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colInputRemarks = COL;
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
                int colCMValue = COL;
              
                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {
                    sheet[ROW, colResponsiblePerson].Text = dtOrder.Rows[i]["ResponsiblePerson"].ToString();

                    sheet[ROW, colBuyer].Text = dtOrder.Rows[i]["Buyer"].ToString();
                    sheet[ROW, colCustomer].Text = dtOrder.Rows[i]["Customer"].ToString();
                    sheet[ROW, colCommitmentDate].Text = dtOrder.Rows[i]["CommitmentDate"].ToString();
                    sheet[ROW, colBuyerRefNo].Text = dtOrder.Rows[i]["BuyerReferenceNo"].ToString();
                    sheet[ROW, colArticle].Text = dtOrder.Rows[i]["Article"].ToString();

                    sheet[ROW, colDeliveryDate].Text = dtOrder.Rows[i]["DeliveryDate"].ToString();
                    sheet[ROW, colPlanExFactoryDate].Text = dtOrder.Rows[i]["PlanExFactoryDate"].ToString();
                    sheet[ROW, colSalesOrderId].Text = dtOrder.Rows[i]["SalesOrderId"].ToString();
                    sheet[ROW, colSalesOrderStatus].Text = dtOrder.Rows[i]["SalseOrderStatus"].ToString();
                    sheet[ROW, colProductionOrderId].Text = dtOrder.Rows[i]["ProductionOrderID"].ToString();
                    sheet[ROW, colProductionStatus].Text = dtOrder.Rows[i]["ProductionStatus"].ToString();
                    sheet[ROW, colCustomerPO].Text = dtOrder.Rows[i]["PONumber"].ToString();
                    sheet[ROW, colLSD].Text = dtOrder.Rows[i]["SOLSD"].ToString();
                    sheet[ROW, colMainrawMaterialDate].Text = dtOrder.Rows[i]["SOMainRawMaterialInhouseDate"].ToString();
                    sheet[ROW, colOtherRawMaterialDate].Text = dtOrder.Rows[i]["SOOtherRawMaterialInhouseDate"].ToString();
                    sheet[ROW, colRate].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["Rate"].ToString());


                    sheet[ROW, colCM].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["CM"].ToString());
                    sheet[ROW, colSPT].Number = OTSBD.clsStaticInfo.dbl(dtOrder.Rows[i]["SPT"].ToString());
                    sheet[ROW, colRemarks].Text =dtOrder.Rows[i]["Remarks"].ToString();
                    sheet[ROW, colSOQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["SOQty"].ToString());
                    sheet[ROW, colShippedQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ShippedQty"].ToString());
                    sheet[ROW, colBalShipment].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["BalShipment"].ToString());


                    sheet[ROW, colPlan].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["TotalPlanQty"].ToString());
                    sheet[ROW, colToPlan].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["RemainingPlanQuantity"].ToString());
                    //sheet[ROW, colProcessStatus].Text = dtOrder.Rows[i]["BuyerReferenceNo"].ToString();

                    sheet[ROW, colProductCode].Text = dtOrder.Rows[i]["ProductCode"].ToString();
                    sheet[ROW, colProduct].Text = dtOrder.Rows[i]["Product"].ToString();
                    sheet[ROW, colDescription].Text = dtOrder.Rows[i]["Description"].ToString();


                    sheet[ROW, colMaterial].Text = dtOrder.Rows[i]["Material"].ToString();
                    sheet[ROW, colOwnRef].Text = dtOrder.Rows[i]["OwnOrderNo"].ToString();
                    sheet[ROW, colorderRemarks].Text = dtOrder.Rows[i]["OrderRemarks"].ToString();
                    sheet[ROW, colorderStatus].Text = dtOrder.Rows[i]["OrderControlStatus"].ToString();
                    sheet[ROW, colMainMaterialRemarks].Text = dtOrder.Rows[i]["MainRMInhouseRemarks"].ToString();
                    sheet[ROW, colMainMaterialStatus].Text = dtOrder.Rows[i]["MainRMInhouseStatus"].ToString();
                    sheet[ROW, colOtherRawMaterialRemarks].Text = dtOrder.Rows[i]["OtherRMInhouseRemarks"].ToString();
                    sheet[ROW, colOtherRawMaterialStatus].Text = dtOrder.Rows[i]["OtherRMInhouseStatus"].ToString();
                    sheet[ROW, colInputRemarks].Text = dtOrder.Rows[i]["InputRemarks"].ToString();
                    sheet[ROW, colInputStatus].Text = dtOrder.Rows[i]["InputStatus"].ToString();
                    sheet[ROW, colLineTarget].Text = dtOrder.Rows[i]["PlannedLinePreference"].ToString();
                    sheet[ROW, colNoOfLinePlan].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["AllocatedLines"].ToString());
                    sheet[ROW, colPriority].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ProductionPriority"].ToString());
                    sheet[ROW, colLineNo].Text = dtOrder.Rows[i]["RunningOrderLinePreference"].ToString();
                    sheet[ROW, colOrderValue].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["OrderValue"].ToString());
                    sheet[ROW, colCMValue].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["CMValue"].ToString());
                    sheet[ROW, colProductionStartDate].Text = dtOrder.Rows[i]["ProductionStartDate"].ToString();
                    sheet[ROW, colProductionOrderCategory].Text = dtOrder.Rows[i]["ProductionOrderCategory"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
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
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

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



                #region Sheet Report
                workbook.Worksheets[1].Name = "Report";
                sheet = workbook.Worksheets[1];


                ROW = 6; COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
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
                sheet[ROW, COL].Text = "Commitment Date";
                sheet[ROW, COL].ColumnWidth = 16;
                 colCommitmentDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Item Reference No.";
                sheet[ROW, COL].ColumnWidth = 16;
                 colBuyerRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 22;
                 colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Shipment Date";
                sheet[ROW, COL].ColumnWidth = 12;
                 colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Ex Factory Date";
                sheet[ROW, COL].ColumnWidth = 12;
                 colPlanExFactoryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Id";
                sheet[ROW, COL].ColumnWidth = 16;
                 colSalesOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Status";
                sheet[ROW, COL].ColumnWidth = 16;
                 colSalesOrderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer PO";
                sheet[ROW, COL].ColumnWidth = 12;
                 colCustomerPO = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order Id";
                sheet[ROW, COL].ColumnWidth = 12;
                 colProductionOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Status";
                sheet[ROW, COL].ColumnWidth = 12;
                 colProductionStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Start Date";
                sheet[ROW, COL].ColumnWidth = 12;
                 colProductionStartDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order Category";
                sheet[ROW, COL].ColumnWidth = 12;
                 colProductionOrderCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "LSD";
                sheet[ROW, COL].ColumnWidth = 12;
                 colLSD = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Raw Material Date";
                sheet[ROW, COL].ColumnWidth = 12;
                 colMainrawMaterialDate = COL;
                COL++;
                sheet[ROW, COL].Text = "other Raw Material Date";
                sheet[ROW, COL].ColumnWidth = 12;
                 colOtherRawMaterialDate = COL;
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
                COL++;
                sheet[ROW, COL].Text = "Order Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                 colorderRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Status";
                sheet[ROW, COL].ColumnWidth = 12;
                 colorderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Material Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                 colMainMaterialRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Material Status";
                sheet[ROW, COL].ColumnWidth = 12;
                 colMainMaterialStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Other Raw Material Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                 colOtherRawMaterialRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Other Raw Material Status";
                sheet[ROW, COL].ColumnWidth = 12;
                 colOtherRawMaterialStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Input Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                 colInputRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Input Status";
                sheet[ROW, COL].ColumnWidth = 12;
                colInputStatus = COL;
                #endregion columns

                endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;               
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                startRow = ROW;

                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {

                    sheet[ROW, colPlant].Text = dtOrder.Rows[i]["Plant"].ToString();
                    sheet[ROW, colEntity].Text = dtOrder.Rows[i]["MasterOrderEntity"].ToString();
                    sheet[ROW, colResponsiblePerson].Text = dtOrder.Rows[i]["ResponsiblePerson"].ToString();

                    sheet[ROW, colBuyer].Text = dtOrder.Rows[i]["Buyer"].ToString();
                    sheet[ROW, colCustomer].Text = dtOrder.Rows[i]["Customer"].ToString();
                    sheet[ROW, colCommitmentDate].Text = dtOrder.Rows[i]["CommitmentDate"].ToString();
                    sheet[ROW, colBuyerRefNo].Text = dtOrder.Rows[i]["BuyerReferenceNo"].ToString();
                    sheet[ROW, colArticle].Text = dtOrder.Rows[i]["Article"].ToString();
                    sheet[ROW, colCustomerPO].Text = dtOrder.Rows[i]["PONumber"].ToString();
                    sheet[ROW, colDeliveryDate].Text = dtOrder.Rows[i]["DeliveryDate"].ToString();
                    sheet[ROW, colPlanExFactoryDate].Text = dtOrder.Rows[i]["PlanExFactoryDate"].ToString();
                    sheet[ROW, colSalesOrderId].Text = dtOrder.Rows[i]["SalesOrderId"].ToString();
                    sheet[ROW, colSalesOrderStatus].Text = dtOrder.Rows[i]["SalseOrderStatus"].ToString();
                    sheet[ROW, colProductionOrderId].Text = dtOrder.Rows[i]["ProductionOrderID"].ToString();
                    sheet[ROW, colProductionStatus].Text = dtOrder.Rows[i]["ProductionStatus"].ToString();
                    sheet[ROW, colLSD].Text = dtOrder.Rows[i]["SOLSD"].ToString();
                    sheet[ROW, colMainrawMaterialDate].Text = dtOrder.Rows[i]["SOMainRawMaterialInhouseDate"].ToString();
                    sheet[ROW, colOtherRawMaterialDate].Text = dtOrder.Rows[i]["SOOtherRawMaterialInhouseDate"].ToString();
                    sheet[ROW, colRate].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["Rate"].ToString());


                    sheet[ROW, colCM].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["CM"].ToString());
                    sheet[ROW, colSPT].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["SPT"].ToString());
                    sheet[ROW, colRemarks].Text = dtOrder.Rows[i]["Remarks"].ToString();
                    sheet[ROW, colSOQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["SOQty"].ToString());
                    sheet[ROW, colShippedQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ShippedQty"].ToString());
                    sheet[ROW, colBalShipment].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["BalShipment"].ToString());


                    sheet[ROW, colPlan].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["TotalPlanQty"].ToString());
                    sheet[ROW, colToPlan].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["RemainingPlanQuantity"].ToString());
                    sheet[ROW, colorderRemarks].Text = dtOrder.Rows[i]["OrderRemarks"].ToString();
                    sheet[ROW, colorderStatus].Text = dtOrder.Rows[i]["OrderControlStatus"].ToString();
                    sheet[ROW, colMainMaterialRemarks].Text = dtOrder.Rows[i]["MainRMInhouseRemarks"].ToString();
                    sheet[ROW, colMainMaterialStatus].Text = dtOrder.Rows[i]["MainRMInhouseStatus"].ToString();
                    sheet[ROW, colOtherRawMaterialRemarks].Text = dtOrder.Rows[i]["OtherRMInhouseRemarks"].ToString();
                    sheet[ROW, colOtherRawMaterialStatus].Text = dtOrder.Rows[i]["OtherRMInhouseStatus"].ToString();
                    sheet[ROW, colInputRemarks].Text = dtOrder.Rows[i]["InputRemarks"].ToString();
                    sheet[ROW, colInputStatus].Text = dtOrder.Rows[i]["InputStatus"].ToString();
                    sheet[ROW, colProductionStartDate].Text = dtOrder.Rows[i]["ProductionStartDate"].ToString();
                    sheet[ROW, colProductionOrderCategory].Text = dtOrder.Rows[i]["ProductionOrderCategory"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                //IListObject table = sheet.ListObjects.Create("Table1", sheet[(1) + (6).ToString() + ":" + (endCol) + (ROW).ToString()]);
                 table = sheet.ListObjects.Create("Table2", sheet.Range[6,1, ROW, endCol]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                reportUtility.PlantHeader(ref sheet, endCol, "Order Report", identity.PlantId);
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange["A7"].FreezePanes();

                 identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                 reportUtility = new ReportUtility();
               // reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "OrderReport", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.IsGridLinesVisible = false;
                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

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


                #endregion
                #region Pivot
                string fPath = fPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "TempReport" + identity.UserId + ".xlsx";

                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }

                workbook.Worksheets[0].Name = "Order";

                IWorksheet pivotSheet = workbook.Worksheets[0];
                IPivotCache cache = workbook.PivotCaches.Add(workbook.Worksheets[1][startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);

                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colResponsiblePerson - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCommitmentDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyerRefNo - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colArticle - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDeliveryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPlanExFactoryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCustomerPO - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionStartDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionOrderCategory - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colRate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCM - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSOQty - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colShippedQty - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colBalShipment - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPlan - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colToPlan - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colLSD - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colMainrawMaterialDate - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colOtherRawMaterialDate - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colSPT - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colRemarks - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colorderRemarks - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colorderStatus - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colMainMaterialRemarks - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colMainMaterialStatus - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colOtherRawMaterialRemarks - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colOtherRawMaterialStatus - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colInputRemarks - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colInputStatus - 1].Axis = PivotAxisTypes.Row;

                IPivotField field = pivotTable.Fields[colShippedQty - 1];

                //field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                //pivotTable.DataFields.Add(field, "Rate", PivotSubtotalTypes.Sum);

                //field = pivotTable.Fields[colCM - 1];
                //field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                //pivotTable.DataFields.Add(field, "CM", PivotSubtotalTypes.Sum);


                //field = pivotTable.Fields[colSOQty - 1];
                //field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                //pivotTable.DataFields.Add(field, "SO Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colShippedQty - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "Shipped Qty", PivotSubtotalTypes.Sum);

                //int colB = colShippedQty + 1;
                //field = pivotTable.Fields[colB - 1];
                //field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                //pivotTable.DataFields.Add(field, "Bal Shipment", colSOQty - colShippedQty);


                //  pivotTable.Fields[colB - 1].Axis = PivotAxisTypes.Row;

                //field = pivotTable.Fields[colBalShipment - 1];
                //field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                //pivotTable.DataFields.Add(field, "Bal Shipment", PivotSubtotalTypes.Sum);


                //field = pivotTable.Fields[colPlan - 1];
                //field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(0);
                //pivotTable.DataFields.Add(field, "Plan", PivotSubtotalTypes.Sum);

                //field = pivotTable.Fields[colToPlan - 1];
                //field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(0);
                //pivotTable.DataFields.Add(field, "To Plan", PivotSubtotalTypes.Sum);


                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colPlant - 1 || i == colEntity - 1 || i == colResponsiblePerson - 1 || i == colCustomer - 1 || i == colBuyer - 1|| i == colCommitmentDate - 1 || i == colBuyerRefNo - 1 || i == colArticle - 1 || i == colDeliveryDate - 1 || i == colPlanExFactoryDate - 1
                        || i == colRate - 1 || i == colSalesOrderStatus - 1 || i == colProductionOrderId - 1 || i == colProductionStatus - 1 || i == colProductionStartDate - 1 || i == colProductionOrderCategory - 1 || i == colCM - 1 || i == colSOQty - 1 || i == colPlan - 1 || i == colToPlan - 1||i==colCustomerPO)
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }

                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Order Report", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
                workbook.Worksheets[0].UsedRange["A7"].FreezePanes();


                #endregion Buyer Summary
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
        private void OrderReportSQL(Dictionary<string, string> parameters, string fromDate, string toDate, string dateType, out DataTable dtOrder)
        {
            string date = "";

            if (dateType == "ExFactoryD" && !string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                date = " AND so.PlanExFactoryDate between '" + fromDate + @"' and '"+ toDate + @"' ";
            }
            if (dateType == "ShipmentD" && !string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                date = " AND so.DeliveryDate between '" + fromDate + @"' and '" + toDate + @"' ";
            }
            if (dateType == "CommitmentD" && !string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {                
                date = " AND so.CommitmentDate between '" + fromDate + @"' and '" + toDate + @"' ";
            }


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT p2.Id PlantId,p2.UserName AS Plant,
e.Id AS MasterOrderEntityId,e.UserName AS MasterOrderEntity,
e2.Id AS ProductionOrderEntityId,e2.UserName AS ProductionOrderEntity,
p.UserName AS Customer,MO.Remarks,
b.UserName AS Buyer,ss.UserName AS Season,
ISNULL(CASE WHEN ISNULL(T.Qty,0)>0 THEN T.Qty ELSE PO.PlannedQty END,0) AS TotalPlanQty,
ISNULL(PRODPR.ProductionQtyAtPR,0) AS ProducedQty,
ISNULL(CASE WHEN ISNULL(T.Qty,0)>0 THEN T.Qty ELSE PO.PlannedQty END,0)-(ISNULL(PRODPR.ProductionQtyAtPR,0)-ISNULL(PRDQ.ProductionBookedQty,0)) AS RemainingPlanQuantity,


BDEP.UserName AS BuyerDepartment,bd.UserName AS BuyerDivision, ei.EmployeeName AS ResponsiblePerson,mo.MasterOrderNo,MO.TotalQty MasterOrderQty,
FORMAT(MO.AddedDate,'dd-MMM-yyyy') MasterOrderCreationDate,OC.UserName AS OrderCategory,os.UserName AS OrderStatus, mo.BuyerReferenceNo AS BuyerOrderNo
,MO.OwnReferenceNo AS OwnOrderNo,
MOI.Id AS LineItemId,MOI.BuyerReferenceNo,moi.ProductionGrouping,FORMAT(MOI.AddedDate,'dd-MMM-yyyy') MasterOrderItemCreationDate,
mm.UserName AS Material,mma.StandardName AS Article, pc.UserName AS ProductCategory, pm.UserName AS Product,MOI.TotalQty AS ItemQty,uom.UserName AS UOM,
PL.Id ProductLibrayId,PL.Code ProductCode,OrderRemarks=(FORMAT(SC.AddedDate,'dd-MMM-yyyy')+'-'+SC.Remarks),SC.[Status] OrderControlStatus,SC.CriticalityLevel
,MainRMInhouseRemarks=(FORMAT(M.AddedDate,'dd-MMM-yyyy')+'-'+M.Remarks),M.[Status] MainRMInhouseStatus
,OtherRMInhouseRemarks=(FORMAT(O.AddedDate,'dd-MMM-yyyy')+'-'+O.Remarks),O.[Status] OtherRMInhouseStatus
,InputRemarks=(FORMAT(I.AddedDate,'dd-MMM-yyyy')+'-'+I.Remarks),I.[Status] InputStatus



,so.Id AS SalesOrderId, so.DestinationId,dest.UserName AS Destination,
so.ShipmentModeId,smo.UserName AS ShipMode, OCS.Id SalesOrderCategoryId,OCS.UserName AS SalesOrderCategory,
OSS.Id SalseOrderStatusId,osS.UserName AS SalseOrderStatus, ISNULL(so.Qty,0) SOQty,SO.CM,SO.Rate,
FORMAT(so.DeliveryDate,'dd-MMM-yyyy') DeliveryDate, FORMAT(so.CommitmentDate,'dd-MMM-yyyy') CommitmentDate, FORMAT(so.PlanExFactoryDate,'dd-MMM-yyyy') PlanExFactoryDate
, FORMAT(so.MainRawMaterialInhouseDate,'dd-MMM-yyyy') SOMainRawMaterialInhouseDate,
FORMAT(so.OtherRawMaterialInhouseDate,'dd-MMM-yyyy') SOOtherRawMaterialInhouseDate,FORMAT(so.LSD,'dd-MMM-yyyy') SOLSD
,CP.PONumber,SO.Description,FORMAT(so.AddedDate,'dd-MMM-yyyy') SalesOrderCreationDate,
t.ProductionOrderID,ps.UserName AS ProductionStatus, t.NoOfWorkStation, t.Efficiency,
t.SPT, t.PlanWorkingHoursPerDay, t.FirstDayOutPut,
t.PlanTargetPerHour, t.IncrementValue, t.IncrementType,
t.DayToReachTheTarget,
--t.CommitmentDate ,
t.ProductionPriority, t.TargetPerHour, t.TargetPerDay,
t.MinimumLineDays, t.RequiredLineDays,
t.RequiredNoOfLines, t.AllocatedLines, t.Qty AS ExplicitProductionQty,
t.LSD AS PRLSD, t.MainRawMaterialInhouseDate AS PRMainRawMaterialInhouseDate, t.OtherRawMaterialInhouseDate AS PROtherRawMaterialInhouseDate,
t.RunningOrderBlockSize,l.LastProcessDate AS SewingCompletionDate,
ActiveOrderLinePreference=STUFF((select distinct ','+xw.UserName from
trn.ProductionOrderWorkCenter AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, ''),
RunningOrderLinePreference=STUFF((select distinct ','+xw.UserName from
trn.RunningOrderWorkCenter AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, ''),



PlannedLinePreference=STUFF((select distinct ','+xw.UserName from
ProductionPlanningType1 AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, ''),


Format( case when  isnull(PRDD.ProductionDate,'')='' and  isnull(PLND.ProductionDate,'')='' THEN null
else case when 
isnull(PRDD.ProductionDate,PLND.ProductionDate) <= isnull(PLND.ProductionDate,PRDD.ProductionDate) THEN PRDD.ProductionDate
else PLND.ProductionDate END END,'dd-MMM-yyyy') AS ProductionStartDate,

case when isnull(PRDD.ProductionDate,'')='' then 'ToStart' else 'Started' END AS ProductionOrderCategory
,isnull(SM.TransactionQty,0) ShippedQty,isnull(SO.Qty,0)-ISNUll(SM.TransactionQty,0) BalShipment,
Isnull(so.CM,0)*isnull(so.Rate,0) CMValue
, Isnull(so.Qty,0)*isnull(so.Rate,0) OrderValue
FROM trn.MasterOrder MO
LEFT JOIN org.Plant AS p2 ON p2.id=mo.PlantId
LEFT JOIN org.Entity AS e ON e.Id=mo.EntityId
left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
LEFT join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
LEFT OUTER JOIN hkp.Season SS ON ss.Id=mo.SeasonId

LEFT OUTER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId

LEFT JOIN org.Entity AS e2 ON e2.Id=po.EntityId
LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T ON t.ProductionOrderID=po.Id
LEFT OUTER JOIN (
SELECT K.ProductionOrderID,max(K.LastProcessDate) AS LastProcessDate FROM (
SELECT ppt.ProductionOrderID,ppt.ProductionDate AS LastProcessDate
FROM ProductionPlanningType1 AS ppt
UNION ALL
SELECT ppt.ProductionOrderID,ppt.ProductionDate AS LastProcessDate
FROM trn.ProductionSummary AS ppt
) AS K GROUP BY K.ProductionOrderID
) AS L ON l.ProductionOrderID=po.Id
--production at PR Level
LEFT OUTER JOIN (
SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
FROM trn.ProductionSummary S
WHERE CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<=CONVERT(DATETIME, format(getdate(),'dd-MMM-yyyy'))
GROUP BY s.ProductionOrderId,s.ProcessId
) AS PRODPR ON PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
left outer join (SELECT pod.ProductionOrderId,
sum(isnull(so.ProductionBookedQty,0)) ProductionBookedQty
FROM trn.SalesOrder AS so
INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id



GROUP BY pod.ProductionOrderId
) AS PRDQ ON PRDQ.ProductionOrderId=po.Id
left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
left outer join mst.MaterialMasterArticle AS mma on mma.id=moi.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId



left outer join [HKP].[Party] p on P.Id=MO.PartyId
left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerDivisionId
left outer join [HKP].[BuyerDEPARTMENT] BDEP on BDEP.id=mo.BuyerDepartmentId
left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
left outer join mst.Destination DEST on dest.Id=so.DestinationId
left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId



left outer join [HKP].[OrderCategory] OCS on ocS.id=So.OrderCategoryId
left outer join [HKP].[OrderStatus] OSS on OSS.id=So.OrderStatusId



left outer join hkp.Season S on s.id=mo.SeasonId
left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=MO.TotalQtyUOMId
LEFT JOIN dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId



LEFT JOIN(
SELECT AMTR.Remarks,B.ProductionOrderId,AMTR.AddedDate,B.[Status]
FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'MainRMInhouse'
) M ON M.ProductionOrderId=PO.Id



LEFT JOIN(
SELECT AMTR.Remarks,B.ProductionOrderId,AMTR.AddedDate ,B.[Status]
FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'OtherRMInhouse'
) O ON O.ProductionOrderId=PO.Id



LEFT JOIN(
SELECT AMTR.Remarks,B.ProductionOrderId,AMTR.AddedDate ,B.[Status]
FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'BaseProcessInput'
) I ON I.ProductionOrderId=PO.Id



LEFT JOIN(
SELECT AMTR.Remarks,B.SalesOrderId,AMTR.AddedDate ,B.[Status],B.CriticalityLevel
FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'ShipmentControl'
) SC ON SC.SalesOrderId=SO.Id



LEFT OUTER JOIN (select PS.ProductionOrderId,min( PS.ProductionDate) ProductionDate from TRN.ProductionSummary PS group by PS.ProductionOrderId) PRDD on PRDD.ProductionOrderId=po.Id 
LEFT OUTER JOIN (select PPT.ProductionOrderID,min(PPT.ProductionDate) ProductionDate from dbo.ProductionPlanningType1 PPT  group by PPT.ProductionOrderID) PLND on PLND.ProductionOrderID=po.Id
LEFT OUTER JOIN TRN.SalesMaterial SM on SM.SalesOrderId=SO.Id



   WHERE os.UserName='Active'
AND MO.PlantId in(" + parameters["PlantId"]+ @")
AND MO.EntityId in(" + parameters["EntityId"] + @")
AND MO.PartyId in(" + parameters["CustomerId"] + @")
AND MO.BuyerId in(" + parameters["BuyerId"] + @")
AND MO.ResponsiblePersonId in(" + parameters["ResponsiblePersonId"] + @")
AND MO.OrderStatusId in(" + parameters["MOStatusId"] + @")
AND OSS.Id in(" + parameters["SOStatusId"] + @")
AND ps.Id in(" + parameters["ProductionStatusId"] + @")

" + date + @"

ORDER BY p2.UserName,e.UserName, mo.MasterOrderNo";
            dtOrder = _sqlRepository.GetDataTable(sql);


        }

    }

}