using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.OrderManagement.Costing
{
    public class CostingReport
    {
        CustomIdentity identity;
        SqlRepository _sqlRepository;
        public CostingReport()
        {
            _sqlRepository = new SqlRepository();
            identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        }
        public void GetOrderCostingReport(string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting, string MOIId)
        {
            try
            {
                if (OrderCostingId == "null")
                    throw new Exception("No costing template found for the current item.");

                string sql = OrderCostingProductInfoSQL(OrderCostingId);

                string CostingDetailsql = OrderCostingProductDetailSQL(OrderCostingId);
                string CostingMOIsql = OrderCostingMOISQL(MOIId);
                //String CostingComponentSql = OrderCostingComponentSQL(OrderCostingId,preCosting,ProcurementCosting); 

                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;
                ReportUtility reportUtility = new ReportUtility();
                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                if (preCosting == "1")
                {
                    sheet.Name = "Order Costing Report(Pre Costing)";
                }
                if (ProcurementCosting == "1")
                {
                    sheet.Name = "Order Costing Report(Procurement Costing)";
                }

                DataTable dtOrderCostingProductInfo = _sqlRepository.GetDataTable(sql);

                if (dtOrderCostingProductInfo.Rows.Count == 0)
                    throw new Exception("Selected master order item is not tagged with any order costing.");

                DataTable dtMOICostingInfo = _sqlRepository.GetDataTable(CostingMOIsql);
                string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

                DataTable dtOrderInfo = _sqlRepository.GetDataTable(OrderInformationSQL(OrderCostingId));
                int ROW = 5;
                int COL = 1;

                #region Order Information
                sheet[ROW, COL].Text = "Order Information";
                sheet[ROW, COL].RowHeight = 25;
                sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
                sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                ROW++;


                COL = 1;
                int StartRow = ROW;
                sheet[ROW, COL].Text = "Master Order";
                sheet[ROW, COL].ColumnWidth = 15;
                int colMasterOrder = COL;
                ROW++;

                sheet[ROW, COL].Text = "Customer";
                int colCustomer = COL;
                ROW = StartRow;
                COL = colCustomer + 2;

                sheet[ROW, COL].Text = "Master Order Item No";
                sheet[ROW, COL].ColumnWidth = 20;
                int colMasterOrderItemNo = COL;
                ROW++;

                sheet[ROW, COL].Text = "Material";
                int colMaterial = COL;
                ROW = StartRow;
                COL = colMaterial + 2;

                sheet[ROW, COL].Text = "Contract No";
                sheet[ROW, COL].ColumnWidth = 20;
                int colContractNo = COL;
                ROW++;

                sheet[ROW, COL].Text = "Article";
                int colArticle = COL;
                ROW++;

                StartRow = ROW;
                COL = 1;
                sheet[ROW, COL].Text = "Buyer";
                int colBuyer = COL;
                COL = colBuyer + 2;
                //ROW++;

                sheet[ROW, COL].Text = "Style";
                int colStyle = COL;
                ROW = StartRow;
                COL = colMaterial + 2;

                sheet[ROW, COL].Text = "Order Qty";
                int colOrderQty = COL;

                int ColEnd = COL;
                sheet.Range[ColEnd + 1, 1, 8, ColEnd + 1].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ColEnd + 1, 1, 8, ColEnd + 1].BorderInside(ExcelLineStyle.Hair);
                double orderquantity = 0;
                ROW++;
                StartRow = 6; //row 20
                for (int i = 0; i < dtOrderInfo.Rows.Count; i++)
                {
                    COL = 2;
                    ROW = StartRow;
                    sheet[ROW, colMasterOrder + 1].Text = dtOrderInfo.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, colMasterOrder + 1].ColumnWidth = 30;
                    ROW++;
                    sheet[ROW, colCustomer + 1].Text = dtOrderInfo.Rows[i]["Customer"].ToString();
                    ROW = StartRow;
                    COL = colCustomer + 3;
                    sheet[ROW, colMasterOrderItemNo + 1].Text = dtOrderInfo.Rows[i]["MasterOrderItemNo"].ToString();
                    sheet[ROW, colMasterOrderItemNo + 1].ColumnWidth = 20;
                    ROW++;
                    sheet[ROW, colMaterial + 1].Text = dtOrderInfo.Rows[i]["Material"].ToString();

                    COL = colMaterial + 3;
                    ROW = StartRow;
                    sheet[ROW, colContractNo + 1].Text = dtOrderInfo.Rows[i]["ContractNo"].ToString();
                    ROW++;
                    sheet[ROW, colArticle + 1].Text = dtOrderInfo.Rows[i]["Article"].ToString();
                    sheet[ROW, colArticle + 1].ColumnWidth = 20;
                    ROW++;

                    COL = 2;
                    StartRow = ROW;
                    sheet[ROW, colBuyer + 1].Text = dtOrderInfo.Rows[i]["Buyer"].ToString();

                    sheet[ROW, colStyle + 1].Text = dtOrderInfo.Rows[i]["StyleNo"].ToString();
                    //ROW++;

                    sheet[ROW, colOrderQty + 1].Number = clsStaticInfo.dbl(dtOrderInfo.Rows[i]["OrderQty"].ToString());
                    sheet[ROW, colOrderQty + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    orderquantity = clsStaticInfo.dbl(dtOrderInfo.Rows[i]["OrderQty"].ToString());
                    ROW++;

                }
                #endregion Order Information

                #region Header

                ROW = 10;
                COL = 1;

                sheet[ROW, COL].Text = "Product Information";
                sheet[ROW, COL].RowHeight = 25;
                sheet.Range[ROW, COL, ROW, COL + 5].Merge();
                sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
                sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                ROW++;

                COL = 1;
                StartRow = ROW;
                sheet[ROW, COL].Text = "Product Master";
                int colProductMaster = COL;
                ROW++;

                sheet[ROW, COL].Text = "Cost.Type";
                int colCostType = COL;
                ROW = StartRow;
                COL = colCostType + 2;

                sheet[ROW, COL].Text = "Prod. Cat.";
                int colProdCategory = COL;
                ROW++;

                sheet[ROW, COL].Text = "Costing Id";
                int colCostingId = COL;
                ROW = StartRow;
                COL = colCostingId + 2;

                sheet[ROW, COL].Text = "Prod.Sub Cat.";
                int colProdSubCategory = COL;
                ROW++;

                sheet[ROW, COL].Text = "Costing Stage";
                int colCostingStage = COL;
                ROW++;

                StartRow = ROW;
                COL = 1;
                sheet[ROW, COL].Text = "Code";
                int colCode = COL;
                ROW++;

                sheet[ROW, COL].Text = "Short Name";
                int colShortName = COL;
                ROW++;

                sheet[ROW, COL].Text = "Mkt Tgt/SPT";
                int colMktTgtSPT = COL;
                ROW++;

                sheet[ROW, COL].Text = "SPT";
                int colSPT = COL;
                ROW++;

                ROW = StartRow;
                COL = colSPT + 2;
                sheet[ROW, COL].Text = "User Name";
                int colUserName = COL;
                ROW++;

                sheet[ROW, COL].Text = "Standard Name";
                int colStandardName = COL;
                ROW++;

                sheet[ROW, COL].Text = "Target/Hour";
                int colTargetHour = COL;
                ROW++;

                sheet[ROW, COL].Text = "Efficiency %";
                int colEfficiency = COL;
                ROW++;

                ROW = StartRow;
                COL = colEfficiency + 2;
                sheet[ROW, COL].Text = "Description";
                int colDescription = COL;
                ROW++;

                sheet[ROW, COL].Text = "No Of WS";
                int colNoOfWS = COL;
                ROW++;

                sheet[ROW, COL].Text = "WC Target / Day";
                int colWCTargetDay = COL;
                ROW++;

                sheet[ROW, COL].Text = "Standard/Plan Hours";
                int colStandardPlanHours = COL;

                ColEnd = COL;
                sheet.Range[11, 1, 16, 6].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[11, 1, 16, 6].BorderInside(ExcelLineStyle.Hair);

                #endregion
                ROW = 18;
                COL = 1;
                #region General Information
                sheet[ROW, COL].Text = "General Information";
                sheet[ROW, COL].RowHeight = 25;
                sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
                sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, COL, ROW, COL + 5].Merge();
                ROW++;

                StartRow = ROW;
                sheet[ROW, COL].Text = "Prd.Avl.Days";
                int colPrdAvlDays = COL;
                ROW++;

                sheet[ROW, COL].Text = "Excess%";
                int colExcess = COL;
                ROW++;

                sheet[ROW, COL].Text = "Critical Level";
                int colCriticalLevel = COL;
                ROW++;

                sheet[ROW, COL].Text = "Packing Type";
                int colPackingType = COL;
                ROW++;

                sheet[ROW, COL].Text = "Tgt Sel. Price*";
                int colTgtSelPrice = COL;

                ROW = StartRow;
                COL = colTgtSelPrice + 2;
                sheet[ROW, COL].Text = "Specific To*";
                int colSpecificTo = COL;
                ROW++;

                sheet[ROW, COL].Text = "UOM";
                int colUOM = COL;
                ROW++;

                sheet[ROW, COL].Text = "Payment Days";
                int colPaymentDays = COL;
                ROW++;

                sheet[ROW, COL].Text = "Order Size";
                int colOrderSize = COL;

                ROW = StartRow;
                COL = colOrderSize + 2;
                sheet[ROW, COL].Text = "Target CM";
                int colTargetCM = COL;
                ROW++;

                sheet[ROW, COL].Text = "Est.NoOf Pag List";
                int colEstNoOfPagList = COL;
                ROW++;

                sheet[ROW, COL].Text = "Remarks";
                int colRemarks = COL;
                ROW++;

                sheet[ROW, COL].Text = "Currency";
                int colCurrency = COL;

                #endregion
                int endCol = colWCTargetDay + 1;

                sheet.Range[5, 1, 5, endCol].Merge();
                sheet.Range[6, 1, 18, 1].CellStyle.Font.Bold = true;
                sheet.Range[6, 3, 18, 3].CellStyle.Font.Bold = true;
                sheet.Range[6, 5, 18, 5].CellStyle.Font.Bold = true;

                sheet.Range[14, 1, 18, 6].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[14, 1, 18, 6].BorderInside(ExcelLineStyle.Hair);

                ROW++;

                StartRow = 11; //row 20
                for (int i = 0; i < dtOrderCostingProductInfo.Rows.Count; i++)
                {
                    COL = 2;
                    ROW = StartRow;
                    sheet[ROW, colProductMaster + 1].Text = dtOrderCostingProductInfo.Rows[i]["ProductMaster"].ToString();
                    ROW++;
                    sheet[ROW, colCostType + 1].Text = dtOrderCostingProductInfo.Rows[i]["CostingTypeName"].ToString();

                    ROW = StartRow;
                    COL = colCostType + 3;
                    sheet[ROW, colProdCategory + 1].Text = dtOrderCostingProductInfo.Rows[i]["ProductCategory"].ToString();

                    ROW++;
                    sheet[ROW, colCostingId + 1].Text = dtOrderCostingProductInfo.Rows[i]["Id"].ToString();

                    COL = colCostingId + 3;
                    ROW = StartRow;
                    sheet[ROW, colProdSubCategory + 1].Text = dtOrderCostingProductInfo.Rows[i]["ProductSubCategory"].ToString();
                    ROW++;
                    sheet[ROW, colCostingStage + 1].Text = dtOrderCostingProductInfo.Rows[i]["CostingStage"].ToString();

                    ROW++;

                    COL = 2;
                    StartRow = ROW;
                    sheet[ROW, colCode + 1].Text = dtOrderCostingProductInfo.Rows[i]["Code"].ToString();
                    ROW++;

                    sheet[ROW, colShortName + 1].Text = dtOrderCostingProductInfo.Rows[i]["ShortName"].ToString();
                    ROW++;

                    sheet[ROW, colMktTgtSPT + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["TargetOrSPT"].ToString());
                    sheet[ROW, colMktTgtSPT + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    ROW++;

                    sheet[ROW, colSPT + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["SPT"].ToString());
                    sheet[ROW, colSPT + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    ROW++;

                    ROW = StartRow;
                    COL = colSPT + 3;
                    sheet[ROW, colUserName + 1].Text = dtOrderCostingProductInfo.Rows[i]["UserName"].ToString();
                    ROW++;
                    sheet[ROW, colStandardName + 1].Text = dtOrderCostingProductInfo.Rows[i]["StandardName"].ToString();
                    ROW++;
                    sheet[ROW, colTargetHour + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["MKTTargetPerHour"].ToString());
                    sheet[ROW, colTargetHour + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    ROW++;
                    sheet[ROW, colEfficiency + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["EfficiencyPercentage"].ToString());
                    sheet[ROW, colEfficiency + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                    COL = colEfficiency + 3;
                    ROW = StartRow;
                    sheet[ROW, colDescription + 1].Text = dtOrderCostingProductInfo.Rows[i]["Description"].ToString();
                    ROW++;

                    sheet[ROW, colNoOfWS + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["NoOfWorkstation"].ToString());
                    sheet[ROW, colNoOfWS + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    ROW++;

                    sheet[ROW, colWCTargetDay + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["WorkCenterTargetPerDay"].ToString());
                    sheet[ROW, colWCTargetDay + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    ROW++;

                    sheet[ROW, colStandardPlanHours + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["StandardWorkingHours"].ToString());
                    sheet[ROW, colStandardPlanHours + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                    ROW = 19;
                    COL = 2;
                    StartRow = ROW;
                    sheet[ROW, colPrdAvlDays + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["ProductionAvailableDays"].ToString());
                    sheet[ROW, colPrdAvlDays + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    ROW++;

                    sheet[ROW, colExcess + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["ExcessShipmentPer"].ToString());
                    sheet[ROW, colExcess + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    ROW++;

                    sheet[ROW, colCriticalLevel + 1].Text = dtOrderCostingProductInfo.Rows[i]["CriticalLevel"].ToString();
                    ROW++;
                    sheet[ROW, colPackingType + 1].Text = dtOrderCostingProductInfo.Rows[i]["PackingType"].ToString();
                    ROW++;

                    sheet[ROW, colTgtSelPrice + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["TargetSellingPrice"].ToString());
                    sheet[ROW, colTgtSelPrice + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    ROW = StartRow;

                    COL = colTgtSelPrice + 3;
                    sheet[ROW, colSpecificTo + 1].Text = dtOrderCostingProductInfo.Rows[i]["SpecifyTo"].ToString();
                    ROW++;
                    sheet[ROW, colUOM + 1].Text = dtOrderCostingProductInfo.Rows[i]["UnitOfMeasurement"].ToString();
                    ROW++;

                    sheet[ROW, colPaymentDays + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["PaymentDays"].ToString());
                    sheet[ROW, colPaymentDays + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    ROW++;

                    sheet[ROW, colOrderSize + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["OrderSize"].ToString());
                    sheet[ROW, colOrderSize + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                    COL = colOrderSize + 3;
                    ROW = StartRow;
                    sheet[ROW, colTargetCM + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["TargetCM"].ToString());
                    sheet[ROW, colTargetCM + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                    ROW++;
                    sheet[ROW, colEstNoOfPagList + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["EstNoOfPackingList"].ToString());
                    ROW++;
                    sheet[ROW, colRemarks + 1].Text = dtOrderCostingProductInfo.Rows[i]["Remarks"].ToString();
                    ROW++;
                    sheet[ROW, colCurrency + 1].Text = dtOrderCostingProductInfo.Rows[i]["Currency"].ToString();
                }

                sheet.Range[ROW, 8, ROW, 9].Merge();
                sheet.Range[7, 1, 19, endCol].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet.Range[19, 1, 23, 6].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[19, 1, 23, 6].BorderInside(ExcelLineStyle.Hair);

                DataTable dtCostingDetailInfo = _sqlRepository.GetDataTable(CostingDetailsql);

                ROW = 25;
                COL = 1;
                #region Costing Detail
                sheet[ROW, COL].Text = "Costing summary";
                sheet[ROW, COL].RowHeight = 25;
                sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
                sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, COL, ROW, COL + 13].Merge();
                ROW++;

                sheet[ROW, COL].Text = "Sl No.";
                int colSlNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Costing Component";
                int colCostingComponent = COL;
                sheet[ROW, COL].ColumnWidth = 20;
                COL++;

                sheet[ROW, COL].Text = "Buyer Costing(A)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colBuyerCosting = COL;
                sheet[ROW, COL].ColumnWidth = 15;
                COL++;

                sheet[ROW, COL].Text = "Quick Costing(B)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colQuickCosting = COL;
                sheet[ROW, COL].ColumnWidth = 20;
                COL++;

                sheet[ROW, COL].Text = "Pre Costing(C)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPreCosting = COL;
                sheet[ROW, COL].ColumnWidth = 20;
                COL++;

                sheet[ROW, COL].Text = "Pre-Costing %";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPreCostingPer = COL;
                sheet[ROW, COL].ColumnWidth = 14;
                COL++;

                sheet[ROW, COL].Text = "Proc. Costing(D)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colProcCosting = COL;
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].WrapText = true;
                COL++;

                sheet[ROW, COL].Text = "Proc-Costing %";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colProcCostingPer = COL;
                sheet[ROW, COL].ColumnWidth = 8;
                sheet[ROW, COL].WrapText = true;
                COL++;


                sheet[ROW, COL].Text = "C-D";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colDifferencePreProCosting = COL;
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].WrapText = true;
                COL++;

                sheet[ROW, COL].Text = "Total Buyer Costing(A)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalBuyerCosting = COL;
                sheet[ROW, COL].ColumnWidth = 11;
                sheet[ROW, COL].WrapText = true;
                COL++;

                sheet[ROW, COL].Text = "Total Quick Costing(B)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalQuickCosting = COL;
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].WrapText = true;
                COL++;

                sheet[ROW, COL].Text = "Total Pre Costing(C)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalPreCosting = COL;
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].WrapText = true;
                COL++;

                sheet[ROW, COL].Text = "Total Proc. Costing(D)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalProcCosting = COL;
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].WrapText = true;
                COL++;

                sheet[ROW, COL].Text = "C-D";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colDifferenceTotalPrePro = COL;
                sheet[ROW, COL].ColumnWidth = 8;

                int CostingDetailEndCol = COL;
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[ROW - 2, 1, ROW - 2, CostingDetailEndCol].Merge();

                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;


                int CostingDetailStartRow = ROW; //row 20
                for (int i = 0; i < dtCostingDetailInfo.Rows.Count; i++)
                {
                    sheet[ROW, colSlNo].Number = (i + 1);

                    sheet[ROW, colCostingComponent].Text = dtCostingDetailInfo.Rows[i]["UserName"].ToString();
                    sheet[ROW, colBuyerCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["BuyerTarget"].ToString());
                    sheet[ROW, colQuickCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["CostingValue"].ToString());
                    sheet[ROW, colPreCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalGrossAmount"].ToString());

                    if (clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalGrossAmount"].ToString()) != 0)
                    {
                        double preSum = clsStaticInfo.dbl(dtCostingDetailInfo.Compute("SUM(TotalGrossAmount)", null));
                        sheet[ROW, colPreCostingPer].Formula = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalGrossAmount"].ToString()) + "/" + clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[0]["TargetSellingPrice"].ToString()) + "%";

                        sheet[ROW, colPreCostingPer].Formula = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalGrossAmount"].ToString()) + "/" + preSum + "*" + 100;

                        sheet.Range[ROW, colPreCostingPer].NumberFormat = "#,##0.00;(#,##0.00)";
                    }

                    sheet[ROW, colTotalBuyerCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["BuyerTarget"].ToString()) * orderquantity;
                    sheet[ROW, colTotalQuickCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["CostingValue"].ToString()) * orderquantity;
                    sheet[ROW, colTotalPreCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalGrossAmount"].ToString()) * orderquantity;
                    if (preCosting == "1")
                    {
                        sheet[ROW, colProcCosting].Number = 0;
                        sheet[ROW, colDifferencePreProCosting].Number = 0;

                        sheet[ROW, colTotalProcCosting].Number = 0;
                        sheet[ROW, colDifferenceTotalPrePro].Number = 0;
                    }
                    else
                    {
                        sheet[ROW, colProcCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalProcurementGrossAmount"].ToString());

                        if (clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalProcurementGrossAmount"].ToString()) != 0)
                        {
                            double proSum = clsStaticInfo.dbl(dtCostingDetailInfo.Compute("SUM(TotalProcurementGrossAmount)", null));
                            sheet[ROW, colProcCostingPer].Formula = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalProcurementGrossAmount"].ToString()) + "/" + proSum + "*" + 100; ;

                            sheet[ROW, colProcCostingPer].Formula = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalProcurementGrossAmount"].ToString()) + "/" + clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[0]["TargetSellingPrice"].ToString()) + "%";

                            sheet.Range[ROW, colProcCostingPer].NumberFormat = "#,##0.00;(#,##0.00)";

                        }
                        sheet[ROW, colDifferencePreProCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["DifferencePreProCosting"].ToString());
                        sheet[ROW, colTotalProcCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalProcurementGrossAmount"].ToString()) * orderquantity;
                        sheet[ROW, colDifferenceTotalPrePro].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalGrossAmount"].ToString()) * orderquantity -
                        clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalProcurementGrossAmount"].ToString()) * orderquantity;
                    }
                    sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                    var endRow = ROW;
                }
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, 1, ROW, 2].Merge();
                sheet.Range[ROW, colBuyerCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colBuyerCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colBuyerCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colBuyerCosting].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colQuickCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colQuickCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colQuickCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colQuickCosting].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colPreCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colPreCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colPreCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colPreCosting].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colPreCostingPer].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colPreCostingPer) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colPreCostingPer) + (ROW - 1) + ")";
                sheet.Range[ROW, colPreCostingPer].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colProcCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colProcCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colProcCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colProcCosting].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colTotalBuyerCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colTotalBuyerCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colTotalBuyerCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colTotalBuyerCosting].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colTotalQuickCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colTotalQuickCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colTotalQuickCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colTotalQuickCosting].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colTotalPreCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colTotalPreCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colTotalPreCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colTotalPreCosting].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colTotalProcCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colTotalProcCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colTotalProcCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colTotalProcCosting].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colProcCostingPer].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colProcCostingPer) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colProcCostingPer) + (ROW - 1) + ")";
                sheet.Range[ROW, colProcCostingPer].CellStyle.Font.Bold = true;

                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderInside(ExcelLineStyle.Hair);
                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[CostingDetailStartRow, 1, ROW, CostingDetailEndCol].CellStyle.Font.Size = 11f;
                sheet.Range[CostingDetailStartRow, colCostingComponent, ROW, CostingDetailEndCol].NumberFormat = clsStaticInfo.NumberFormat(2);

                #endregion
                ROW++;
                ROW++;
                COL = 1;

                ROW++;
                int CostingComponentEndcol = 0;

                DirectMateterial(sheet, ref ROW, OrderCostingId, orderBudget, preCosting, ProcurementCosting, dtMOICostingInfo);
                DirectProcess(sheet, ref ROW, OrderCostingId, orderBudget, preCosting, ProcurementCosting, dtMOICostingInfo);
                Operation(sheet, ref ROW, OrderCostingId, orderBudget, preCosting, ProcurementCosting, dtMOICostingInfo);
                ValueLoss(sheet, ref ROW, OrderCostingId, orderBudget, preCosting, ProcurementCosting, dtMOICostingInfo);
                Profit(sheet, ref ROW, OrderCostingId, orderBudget, preCosting, ProcurementCosting, dtMOICostingInfo);
                SalesExpense(sheet, ref ROW, OrderCostingId, orderBudget, preCosting, ProcurementCosting, dtMOICostingInfo);

                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (preCosting == "1")
                {
                    reportUtility.PlantHeader(ref sheet, endCol, "Order Costing Report(Pre Costing)", identity.PlantId);
                }
                if (ProcurementCosting == "1")
                {
                    reportUtility.PlantHeader(ref sheet, endCol, "Order Costing Report(Procurement Costing)", identity.PlantId);
                }
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet[ROW, colProcCosting].HorizontalAlignment = ExcelHAlign.HAlignRight;

                string strFileName = "OrderCostingReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void OrderBudgetReport(string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting, string MOIId)
        {
            try
            {
                if (OrderCostingId == "null")
                    throw new Exception("No costing template found for the current item.");

                string sql = OrderCostingProductInfoSQL(OrderCostingId);

                string CostingDetailsql = OrderCostingProductDetailSQL(OrderCostingId);
                string CostingMOIsql = OrderCostingMOISQL(MOIId);

                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;
                ReportUtility reportUtility = new ReportUtility();
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Order Budget Report";

                DataTable dtOrderCostingProductInfo = _sqlRepository.GetDataTable(sql);

                if (dtOrderCostingProductInfo.Rows.Count == 0)
                    throw new Exception("Selected master order item is not tagged with any order costing.");

                DataTable dtMOICostingInfo = _sqlRepository.GetDataTable(CostingMOIsql);
                //string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

                string sqlOrderInfo = OrderInformationSQL(OrderCostingId);
                DataTable dtOrderInfo = _sqlRepository.GetDataTable(sqlOrderInfo);
                int ROW = 5;
                int COL = 8;
                int COLFinal = COL;

                #region Order Information
                sheet[ROW, COL].Text = "Order Information";
                sheet[ROW, COL].RowHeight = 25;
                sheet.Range[ROW, COL].Merge();
                sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
                sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, COL, ROW, COL + 5].Merge();
                ROW++;

                int StartRow = ROW;

                sheet[ROW, COL].Text = "Style";
                sheet[ROW, COL].ColumnWidth = 11;
                int colStyle = COL;
                COL = colStyle + 2;

                sheet[ROW, COL].Text = "Contract No";
                sheet[ROW, COL].ColumnWidth = 18;
                int colContractNo = COL;
                COL = colContractNo + 2;

                sheet[ROW, COL].Text = "FOB";
                sheet[ROW, COL].ColumnWidth = 15;
                int colFOB = COL;
                ROW++;

                ROW = StartRow + 1;
                COL = COLFinal;
                sheet[ROW, COL].Text = "Master Order";
                int colMasterOrder = COL;
                COL = colMasterOrder + 2;

                sheet[ROW, COL].Text = "Master Order Item No";
                int colMasterOrderItemNo = COL;
                COL = colMasterOrderItemNo + 2;

                sheet[ROW, COL].Text = "CM";
                int colCM = COL;
                ROW++;

                ROW = StartRow + 2;
                COL = COLFinal;
                sheet[ROW, COL].Text = "Customer";
                int colCustomer = COL;
                COL = colCustomer + 2;

                sheet[ROW, COL].Text = "Costing Id";
                int colCostingId = COL;
                COL = colCostingId + 2;

                sheet[ROW, COL].Text = "Order Qty";
                int colOrderQty = COL;
                ROW++;

                ROW = StartRow + 3;
                COL = COLFinal;
                sheet[ROW, COL].Text = "Buyer";
                int colBuyer = COL;
                COL = colBuyer + 2;

                sheet[ROW, COL].Text = "Costing Stage";
                int colCostingStage = COL;
                COL = colCostingStage + 2;

                sheet[ROW, COL].Text = "SPT/SMV";
                int colSPTSMV = COL;
                ROW++;

                ROW = StartRow + 4;
                COL = COLFinal;
                sheet[ROW, COL].Text = "Material";
                int colMaterial = COL;
                COL = colMaterial + 2;

                sheet[ROW, COL].Text = "Standard Name";
                int colStandardName = COL;
                COL = colStandardName + 2;

                sheet[ROW, COL].Text = "Efficiency %";
                int colEfficiency = COL;
                ROW++;

                ROW = StartRow + 5;
                COL = COLFinal;
                sheet[ROW, COL].Text = "Article";
                int colArticle = COL;
                COL = colArticle + 2;

                sheet[ROW, COL].Text = "Standard/Plan Hours";
                int colStandardPlanHours = COL;
                COL = colStandardPlanHours + 2;

                sheet[ROW, COL].Text = "Target/Hour";
                int colTargetHour = COL;
                ROW++;

                ROW = StartRow + 6;
                COL = COLFinal;
                sheet[ROW, COL].Text = "Prd.Avl.Days";
                int colPrdAvlDays = COL;
                COL = colPrdAvlDays + 2;

                sheet[ROW, COL].Text = "No Of WS";
                int colNoOfWS = COL;
                COL = colNoOfWS + 2;

                sheet[ROW, COL].Text = "WC Target/Day";
                int colWCTargetDay = COL;

                int ColEnd = COL;
                sheet.Range[StartRow, COLFinal, ROW, ColEnd + 1].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[StartRow, COLFinal, ROW, ColEnd + 1].BorderInside(ExcelLineStyle.Hair);

                sheet.Range[StartRow, COLFinal, ROW, COLFinal].CellStyle.Font.Bold = true;
                sheet.Range[StartRow, COLFinal + 2, ROW, COLFinal + 2].CellStyle.Font.Bold = true;
                sheet.Range[StartRow, COLFinal + 4, ROW, COLFinal + 4].CellStyle.Font.Bold = true;

                double orderquantity = 0;
                ROW++;
                StartRow = 6;
                //row 20
                //for (int i = 0; i < dtOrderInfo.Rows.Count; i++)
                //{
                COL = COLFinal + 1;
                ROW = StartRow;
                sheet[ROW, colStyle + 1].Text = dtOrderInfo.Rows[0]["StyleNo"].ToString();
                sheet[ROW, colStyle + 1].ColumnWidth = 13;

                sheet[ROW, colContractNo + 1].Text = dtOrderInfo.Rows[0]["ContractNo"].ToString();
                sheet[ROW, colContractNo + 1].ColumnWidth = 15;

                sheet[ROW, colFOB + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[0]["TargetSellingPrice"].ToString());
                sheet[ROW, colFOB + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                ROW++;

                sheet[ROW, colMasterOrder + 1].Text = dtOrderInfo.Rows[0]["MasterOrderNo"].ToString();
                sheet[ROW, colMasterOrder + 1].ColumnWidth = 30;
                sheet[ROW, colMasterOrderItemNo + 1].Text = dtOrderInfo.Rows[0]["MasterOrderItemNo"].ToString();
                sheet[ROW, colMasterOrderItemNo + 1].ColumnWidth = 20;
                sheet[ROW, colCM + 1].Text = dtOrderCostingProductInfo.Rows[0]["TargetCM"].ToString();

                ROW++;

                sheet[ROW, colCustomer + 1].Text = dtOrderInfo.Rows[0]["Customer"].ToString();
                sheet[ROW, colCostingId + 1].Text = dtOrderCostingProductInfo.Rows[0]["Id"].ToString();

                sheet[ROW, colOrderQty + 1].Number = clsStaticInfo.dbl(dtOrderInfo.Rows[0]["OrderQty"].ToString());
                sheet[ROW, colOrderQty + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                ROW++;

                sheet[ROW, colBuyer + 1].Text = dtOrderInfo.Rows[0]["Buyer"].ToString();
                sheet[ROW, colCostingStage + 1].Text = dtOrderCostingProductInfo.Rows[0]["CostingStage"].ToString();

                sheet[ROW, colSPTSMV + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[0]["SPT"].ToString());
                sheet[ROW, colSPTSMV + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                ROW++;

                sheet[ROW, colMaterial + 1].Text = dtOrderInfo.Rows[0]["Material"].ToString();
                sheet[ROW, colStandardName + 1].Text = dtOrderCostingProductInfo.Rows[0]["StandardName"].ToString();

                sheet[ROW, colEfficiency + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[0]["EfficiencyPercentage"].ToString());
                sheet[ROW, colEfficiency + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                ROW++;

                sheet[ROW, colArticle + 1].Text = dtOrderInfo.Rows[0]["Article"].ToString();

                sheet[ROW, colStandardPlanHours + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[0]["StandardWorkingHours"].ToString());
                sheet[ROW, colStandardPlanHours + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet[ROW, colTargetHour + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[0]["MKTTargetPerHour"].ToString());
                sheet[ROW, colTargetHour + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                ROW++;

                sheet[ROW, colPrdAvlDays + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[0]["ProductionAvailableDays"].ToString());
                sheet[ROW, colPrdAvlDays + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet[ROW, colNoOfWS + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[0]["NoOfWorkstation"].ToString());
                sheet[ROW, colNoOfWS + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet[ROW, colWCTargetDay + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[0]["WorkCenterTargetPerDay"].ToString());
                sheet[ROW, colWCTargetDay + 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                orderquantity = clsStaticInfo.dbl(dtOrderInfo.Rows[0]["OrderQty"].ToString());

                ROW++;
                //}
                int endCol = colWCTargetDay + 1;

                DataTable dtCostingDetailInfo = _sqlRepository.GetDataTable(CostingDetailsql);

                ROW = 5;
                COL = 1;
                #region Costing Detail
                sheet[ROW, COL].Text = "Costing summary";
                sheet[ROW, COL].RowHeight = 25;
                sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
                sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, COL, ROW, COL + 4].Merge();
                ROW++;

                sheet[ROW, COL].Text = "Costing Component";
                int colCostingComponent = COL;
                sheet[ROW, COL].ColumnWidth = 33;
                COL++;

                sheet[ROW, COL].Text = "Initial Costing";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colQuickCosting = COL;
                sheet[ROW, COL].ColumnWidth = 18;
                COL++;

                sheet[ROW, COL].Text = "Execution Cost";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPreCosting = COL;
                sheet[ROW, COL].ColumnWidth = 14;
                COL++;

                sheet[ROW, COL].Text = "Execution Cost%";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPreCostingPer = COL;
                sheet[ROW, COL].ColumnWidth = 18;
                COL++;

                sheet[ROW, COL].Text = "Execution Total Cost";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalPreCosting = COL;
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].WrapText = true;
                COL++;

                int CostingDetailEndCol = COL;
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol - 1].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol - 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                sheet.Range[ROW - 2, 1, ROW - 2, CostingDetailEndCol - 1].Merge();

                sheet.Range[ROW, 1, ROW, CostingDetailEndCol - 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol - 1].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol - 1].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                int CostingDetailStartRow = ROW;
                for (int i = 0; i < dtCostingDetailInfo.Rows.Count; i++)
                {
                    sheet[ROW, colCostingComponent].Text = dtCostingDetailInfo.Rows[i]["UserName"].ToString();
                    sheet[ROW, colPreCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalGrossAmount"].ToString());

                    if (clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalGrossAmount"].ToString()) != 0)
                    {
                        double preSum = clsStaticInfo.dbl(dtCostingDetailInfo.Compute("SUM(TotalGrossAmount)", null));
                        sheet[ROW, colPreCostingPer].Formula = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalGrossAmount"].ToString()) + "/" + preSum + "*" + 100;

                        sheet.Range[ROW, colPreCostingPer].NumberFormat = "#,##0.00;(#,##0.00)";
                    }

                    sheet[ROW, colTotalPreCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalGrossAmount"].ToString()) * orderquantity;
                    if (preCosting == "1")
                    {
                        //sheet[ROW, colProcCosting].Number = 0;
                        //sheet[ROW, colDifferencePreProCosting].Number = 0;

                        //sheet[ROW, colTotalProcCosting].Number = 0;
                        //sheet[ROW, colDifferenceTotalPrePro].Number = 0;
                    }
                    else
                    {
                        //sheet[ROW, colProcCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalProcurementGrossAmount"].ToString());

                        if (clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalProcurementGrossAmount"].ToString()) != 0)
                        {
                            double proSum = clsStaticInfo.dbl(dtCostingDetailInfo.Compute("SUM(TotalProcurementGrossAmount)", null));

                        }
                    }
                    sheet.Range[ROW, 1, ROW, CostingDetailEndCol - 1].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingDetailEndCol - 1].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                    var endRow = ROW;
                }
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, 1, ROW, 2].Merge();
                sheet.Range[ROW, colPreCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colPreCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colPreCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colPreCosting].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colPreCostingPer].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colPreCostingPer) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colPreCostingPer) + (ROW - 1) + ")";
                sheet.Range[ROW, colPreCostingPer].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colTotalPreCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colTotalPreCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colTotalPreCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colTotalPreCosting].CellStyle.Font.Bold = true;

                //double TotalExecutionTC = clsStaticInfo.dbl("SUM(" + reportUtility.GetColumnNameForXls(colTotalPreCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colTotalPreCosting) + (ROW - 1) + ")");

                double TotalExecutionTC = clsStaticInfo.dbl(dtCostingDetailInfo.Compute("SUM(TotalGrossAmount)", null)) * orderquantity;



                sheet.Range[ROW, 1, ROW, CostingDetailEndCol - 1].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol - 1].BorderInside(ExcelLineStyle.Hair);
                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[CostingDetailStartRow, 1, ROW, CostingDetailEndCol].CellStyle.Font.Size = 11f;
                sheet.Range[CostingDetailStartRow, colCostingComponent, ROW, CostingDetailEndCol].NumberFormat = clsStaticInfo.NumberFormat(2);

                #endregion
                ROW++;
                ROW++;
                COL = 1;

                //ROW++;
                int CostingComponentEndcol = 0;

                DirectMateterial(sheet, ref ROW, OrderCostingId, orderBudget, preCosting, ProcurementCosting, dtMOICostingInfo);
                int fundROW = ROW;
                DirectProcess(sheet, ref ROW, OrderCostingId, orderBudget, preCosting, ProcurementCosting, dtMOICostingInfo);
                FundRequired(sheet, ref fundROW, OrderCostingId, orderBudget, preCosting, ProcurementCosting, dtMOICostingInfo, TotalExecutionTC);
                Operation(sheet, ref ROW, OrderCostingId, orderBudget, preCosting, ProcurementCosting, dtMOICostingInfo);
                ValueLoss(sheet, ref ROW, OrderCostingId, orderBudget, preCosting, ProcurementCosting, dtMOICostingInfo);
                Profit(sheet, ref ROW, OrderCostingId, orderBudget, preCosting, ProcurementCosting, dtMOICostingInfo);
                SalesExpense(sheet, ref ROW, OrderCostingId, orderBudget, preCosting, ProcurementCosting, dtMOICostingInfo);

                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility.PlantHeader(ref sheet, endCol, "OrderBudgetReport", identity.PlantId);

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.CellStyle.Font.Size = 11f;

                string strFileName = "OrderBudgetReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void DirectMateterial(IWorksheet sheet, ref int ROW, string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting, DataTable dtMOICostingInfo)
        {
            ReportUtility reportUtility = new ReportUtility();
            String CostingDirectMaterialSQL = OrderPreCostingDirectMaterialSQL(OrderCostingId, orderBudget, preCosting, ProcurementCosting);

            DataTable dtOrderCostingDirectMaterial = _sqlRepository.GetDataTable(CostingDirectMaterialSQL);
            if (dtOrderCostingDirectMaterial.Rows.Count == 0)
                return;

            DataTable dvDistinctCostingComponent = dtOrderCostingDirectMaterial.DefaultView.ToTable(true, "CostingComponentId", "CostingComponentName");
            string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

            int CostingComponentEndcol = 0;
            int COL = 1;

            sheet[ROW, COL].Text = dtOrderCostingDirectMaterial.Rows[0]["CostingComponentName"].ToString() + " breakdown.";
            sheet[ROW, COL].RowHeight = 25;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
            sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
            sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, COL, ROW, 13].Merge();


            for (int i = 0; i < dvDistinctCostingComponent.Rows.Count; i++)

            {
                ROW++;
                //sheet[1, 1].Text = dvDistinctCostingComponent.Rows[i]["CostingComponentName"].ToString();

                sheet[ROW, COL].Text = "Costing Item";
                int colCostingItem = COL;
                COL++;

                sheet[ROW, COL].Text = "Particulars";
                int colParticulars = COL;
                COL++;

                sheet[ROW, COL].Text = "Costing Category";
                int colCostingCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "UOM";
                int colUOM2 = COL;
                COL++;

                sheet[ROW, COL].Text = "Consumption";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colConsumption = COL;
                COL++;

                sheet[ROW, COL].Text = "Value Loss(%)";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colValueLoss = COL;
                COL++;

                sheet[ROW, COL].Text = "Gross Consumption";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].WrapText = true;
                int colGrossConsumption = COL;
                COL++;

                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 20;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colRate = COL;
                COL++;

                sheet[ROW, COL].Text = "Gross Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colGrossAmount = COL;
                sheet[ROW, COL].WrapText = true;
                COL++;

                sheet[ROW, COL].Text = "Order Size";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colOrderSize = COL;
                sheet[ROW, COL].WrapText = true;
                COL++;

                sheet[ROW, COL].Text = "Total Material Requirement";
                sheet[ROW, COL].ColumnWidth = 20;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalMaterialRequirement = COL;
                sheet[ROW, COL].WrapText = true;
                COL++;

                sheet[ROW, COL].Text = "Total Order Cost";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalOrderCost = COL;
                sheet[ROW, COL].WrapText = true;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 13;
                int colCurrency2 = COL;
                CostingComponentEndcol = COL;

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                //sheet.Range[ROW, colGrossConsumption, ROW, colGrossConsumption + 1].Merge();
                //sheet.Range[ROW, colParticulars, ROW, colParticulars + 1].Merge();
                sheet.Range[ROW - 2, 1, ROW - 2, CostingComponentEndcol].Merge();
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

                dtOrderCostingDirectMaterial.DefaultView.RowFilter = "CostingComponentId='" + dvDistinctCostingComponent.Rows[i]["CostingComponentId"].ToString() + "'";
                DataTable dtComponentRelatedItems = dtOrderCostingDirectMaterial.DefaultView.ToTable();

                int CostingComponentStartRow = ROW;

                for (int M = 0; M < dtComponentRelatedItems.Rows.Count; M++)
                {
                    sheet[ROW, colCostingItem].Text = dtComponentRelatedItems.DefaultView[M]["CostingItem"].ToString();
                    sheet[ROW, colParticulars].Text = dtComponentRelatedItems.DefaultView[M]["Particulars"].ToString();
                    sheet[ROW, colCostingCategory].Text = dtComponentRelatedItems.DefaultView[M]["CostingCategory"].ToString();
                    sheet[ROW, colUOM2].Text = dtComponentRelatedItems.DefaultView[M]["UOM"].ToString();
                    sheet[ROW, colConsumption].Number = Convert.ToDouble(dtComponentRelatedItems.DefaultView[M]["Consumption"].ToString());
                    sheet[ROW, colValueLoss].Number = Convert.ToDouble(dtComponentRelatedItems.DefaultView[M]["ValueLoss"].ToString());
                    sheet[ROW, colGrossConsumption].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["GrossConsumption"].ToString());
                    sheet[ROW, colRate].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Rate"].ToString());
                    sheet[ROW, colGrossAmount].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["GrossAmount"].ToString());

                    sheet[ROW, colOrderSize].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["TotalQty"].ToString());
                    sheet[ROW, colTotalMaterialRequirement].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["TotalMaterialRequirement"].ToString());
                    sheet[ROW, colTotalOrderCost].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["GrossAmount"].ToString()) * clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["TotalQty"].ToString());
                    sheet[ROW, colCurrency2].Text = dtComponentRelatedItems.DefaultView[M]["Currency"].ToString();
                    //sheet[ROW, colGrossAmount].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["GrossAmount"].ToString());

                    //sheet.Range[ROW, colGrossConsumption, ROW, colGrossConsumption + 1].Merge();
                    //sheet.Range[ROW, colParticulars, ROW, colParticulars + 1].Merge();

                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }

                int CostingComponentEndRow = ROW - 1;
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCostingItem, ROW, colParticulars + 1].Merge();

                sheet.Range[ROW, colConsumption].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colConsumption) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colConsumption) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colConsumption].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[ROW, colValueLoss].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colValueLoss) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colValueLoss) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colValueLoss].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[ROW, colGrossConsumption].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colGrossConsumption) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colGrossConsumption) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colGrossConsumption].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[ROW, colGrossAmount].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colGrossAmount) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colGrossAmount) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colGrossAmount].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[ROW, colTotalOrderCost].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colTotalOrderCost) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colTotalOrderCost) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colTotalOrderCost].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[ROW, colCurrency2].Text = dtComponentRelatedItems.DefaultView[0]["Currency"].ToString();
                sheet.Range[ROW, colCurrency2].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, CostingComponentEndcol].NumberFormat = clsStaticInfo.NumberFormat(4);

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                ROW++;

                sheet.Range[CostingComponentStartRow, colConsumption, ROW, colConsumption].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colValueLoss, ROW, colValueLoss].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colGrossAmount, ROW, colGrossAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colTotalOrderCost, ROW, colTotalOrderCost].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colConsumption, ROW, colConsumption].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colOrderSize, ROW, colOrderSize].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colTotalMaterialRequirement, ROW, colTotalMaterialRequirement].NumberFormat = clsStaticInfo.NumberFormat(2);
            }
        }

        private void DirectProcess(IWorksheet sheet, ref int ROW, string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting, DataTable dtMOICostingInfo)
        {
            ReportUtility reportUtility = new ReportUtility();
            String CostingDirectProcessSQL = OrderPreCostingDirectProcessSQL(OrderCostingId, orderBudget, preCosting, ProcurementCosting);
            DataTable dtOrderCostingDirectProcess = _sqlRepository.GetDataTable(CostingDirectProcessSQL);

            if (dtOrderCostingDirectProcess.Rows.Count == 0)
                return;

            DataTable dvDistinctCostingComponent = dtOrderCostingDirectProcess.DefaultView.ToTable(true, "CostingComponentId", "CostingComponentName");
            string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

            int CostingComponentEndcol = 0;

            int COL = 1;

            //sheet[ROW, COL].Text = dtOrderCostingDirectProcess.Rows[0]["CostingComponentName"].ToString() + " breakdown.";
            //sheet[ROW, COL].RowHeight = 30;
            //sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
            //sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
            //sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            //sheet.Range[ROW, COL, ROW, 7].Merge();

            for (int i = 0; i < dvDistinctCostingComponent.Rows.Count; i++)
            {
                sheet[ROW, COL].Text = dvDistinctCostingComponent.Rows[i]["CostingComponentName"].ToString() + " breakdown.";
                sheet[ROW, COL].RowHeight = 30;
                sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
                sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, COL, ROW, 7].Merge();

                ROW++;
                COL = 1;

                sheet[ROW, COL].Text = "Costing Item";
                int colCostingItem = COL;
                COL++;

                sheet[ROW, COL].Text = "Type";
                int colType = COL;
                COL++;

                sheet[ROW, COL].Text = "Value Loss(%)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colRate = COL;
                COL++;
                 
                sheet[ROW, COL].Text = "Qty";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Order Cost";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalOrderCost = COL;
                sheet[ROW, COL].WrapText = true;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                int colCurrency2 = COL;

                CostingComponentEndcol = COL;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                //sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 1].Merge();
                //sheet.Range[ROW, colType, ROW, colType + 1].Merge();
                sheet.Range[ROW - 2, 1, ROW - 2, CostingComponentEndcol].Merge();
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

                dtOrderCostingDirectProcess.DefaultView.RowFilter = "CostingComponentId='" + dvDistinctCostingComponent.Rows[i]["CostingComponentId"].ToString() + "'";
                DataTable dtComponentRelatedItems = dtOrderCostingDirectProcess.DefaultView.ToTable();

                int CostingComponentStartRow = ROW;

                for (int M = 0; M < dtComponentRelatedItems.Rows.Count; M++)
                {
                    sheet[ROW, colCostingItem].Text = dtComponentRelatedItems.DefaultView[M]["CostingItem"].ToString();
                    sheet[ROW, colType].Text = dtComponentRelatedItems.DefaultView[M]["Type"].ToString();

                    sheet[ROW, colValue].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString());
                    sheet[ROW, colRate].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Rate"].ToString());
                     
                    sheet[ROW, colQty].Number = clsStaticInfo.dbl(OrderQTY);

                    //if (preCosting == "1" || ProcurementCosting == "1")
                    //{
                    //    sheet[ROW, colTotalOrderCost].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Rate"].ToString()) * clsStaticInfo.dbl(OrderQTY);
                    //}
                    //if (orderBudget == "1")
                    //{
                    //    sheet[ROW, colTotalOrderCost].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["TotalOrderCost"].ToString());
                    //}
                    sheet[ROW, colTotalOrderCost].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Rate"].ToString()) * clsStaticInfo.dbl(OrderQTY);

                    sheet[ROW, colCurrency2].Text = dtComponentRelatedItems.DefaultView[M]["Currency"].ToString();

                    //sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 1].Merge();
                    //sheet.Range[ROW, colType, ROW, colType + 1].Merge();
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }
                int CostingComponentEndRow = ROW - 1;
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCostingItem, ROW, colType + 1].Merge();

                sheet.Range[ROW, colValue].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colValue].CellStyle.Font.Bold = true;
  
                sheet.Range[ROW, colQty].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colQty) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colQty) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colQty].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colTotalOrderCost].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colTotalOrderCost) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colTotalOrderCost) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colTotalOrderCost].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colCurrency2].Formula = reportUtility.GetColumnNameForXls(colCurrency2) + (ROW - 1);
                sheet.Range[ROW, colCurrency2].CellStyle.Font.Bold = true;

                sheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, CostingComponentEndcol].NumberFormat = clsStaticInfo.NumberFormat(4);

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                ROW++;

                sheet.Range[CostingComponentStartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colQty, ROW, colQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colTotalOrderCost, ROW, colTotalOrderCost].NumberFormat = clsStaticInfo.NumberFormat(2);
                COL = 1;
            }
        }
         
        private void Operation(IWorksheet sheet, ref int ROW, string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting, DataTable dtMOICostingInfo)
        {
            ReportUtility reportUtility = new ReportUtility();
            String CostingOperationSQL = OrderPreCostingOperationSQL(OrderCostingId, orderBudget, preCosting, ProcurementCosting);
            DataTable dtOrderCostingOperation = _sqlRepository.GetDataTable(CostingOperationSQL);

            if (dtOrderCostingOperation.Rows.Count == 0)
                return;
            DataTable dvDistinctCostingComponent = dtOrderCostingOperation.DefaultView.ToTable(true, "CostingComponentId", "CostingComponentName");
            string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

            int CostingComponentEndcol = 0;
            int COL = 1;

            sheet[ROW, COL].Text = dtOrderCostingOperation.Rows[0]["CostingComponentName"].ToString() + " breakdown.";
            sheet[ROW, COL].RowHeight = 30;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
            sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
            sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, COL, ROW, 5].Merge();

            for (int i = 0; i < dvDistinctCostingComponent.Rows.Count; i++)
            {
                ROW++;
                // sheet[1, 1].Text = dvDistinctCostingComponent.Rows[i]["CostingComponentName"].ToString();
                COL = 1;

                sheet[ROW, COL].Text = "CostingItem";
                int colCostingItem = COL;
                COL++;

                sheet[ROW, COL].Text = "Value";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Qty";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Order Cost";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalOrderCost = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                int colCurrency = COL;

                CostingComponentEndcol = COL;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                //sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 1].Merge();
                //sheet.Range[ROW - 2, 1, ROW - 2, CostingComponentEndcol].Merge();
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                dtOrderCostingOperation.DefaultView.RowFilter = "CostingComponentId='" + dvDistinctCostingComponent.Rows[i]["CostingComponentId"].ToString() + "'";
                DataTable dtComponentRelatedItems = dtOrderCostingOperation.DefaultView.ToTable();

                int CostingComponentStartRow = ROW;

                for (int M = 0; M < dtComponentRelatedItems.Rows.Count; M++)
                {
                    sheet[ROW, colCostingItem].Text = dtComponentRelatedItems.DefaultView[M]["CostingItem"].ToString();
                    sheet[ROW, colValue].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString());

                    sheet[ROW, colQty].Number = sheet[ROW, colQty].Number = clsStaticInfo.dbl(OrderQTY); ;
                    //sheet[ROW, colQty].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["TotalQty"].ToString());

                    if (orderBudget == "1")
                    {
                        //sheet[ROW, colTotalOrderCost].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["TotalOrderCost"].ToString());
                        sheet[ROW, colTotalOrderCost].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString())* clsStaticInfo.dbl(OrderQTY);
                    }
                    if (preCosting == "1" || ProcurementCosting == "1")
                    {
                        sheet[ROW, colTotalOrderCost].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString()) * clsStaticInfo.dbl(OrderQTY);
                    }
                    sheet[ROW, colCurrency].Text = dtComponentRelatedItems.DefaultView[M]["Currency"].ToString();
                    //sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 1].Merge();
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                    ROW++;
                }
                int CostingComponentEndRow = ROW - 1;
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 1].Merge();
                sheet.Range[ROW, colQty, ROW, colQty].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colQty) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colQty) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colQty].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colTotalOrderCost].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colTotalOrderCost) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colTotalOrderCost) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colTotalOrderCost].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colCurrency].Formula = reportUtility.GetColumnNameForXls(colCurrency) + (ROW - 1);
                sheet.Range[ROW, colCurrency].CellStyle.Font.Bold = true;

                sheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, CostingComponentEndcol].NumberFormat = clsStaticInfo.NumberFormat(4);

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                ROW++;

                sheet.Range[CostingComponentStartRow, colQty, ROW, colQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colTotalOrderCost, ROW, colTotalOrderCost].NumberFormat = clsStaticInfo.NumberFormat(2);
            }
        }

        private void FundRequired(IWorksheet sheet, ref int ROW, string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting, DataTable dtMOICostingInfo, double TotalExecutionTC)
        {
            ReportUtility reportUtility = new ReportUtility();
            String CostingDirectMaterialSQL = OrderPreCostingDirectMaterialSQL(OrderCostingId, orderBudget, preCosting, ProcurementCosting);
            DataTable dtFundRequired = _sqlRepository.GetDataTable(CostingDirectMaterialSQL);

            String CostingDirectProcessSQL = OrderPreCostingDirectProcessSQL(OrderCostingId, orderBudget, preCosting, ProcurementCosting);
            DataTable dtFundRequiredWash = _sqlRepository.GetDataTable(CostingDirectProcessSQL);

            if (dtFundRequired.Rows.Count == 0)
                return;
            int COL = 9;
            int COLFinal = COL;

            #region Order Information
            sheet[ROW, COL].Text = "Fund Required for BTB";
            sheet[ROW, COL].RowHeight = 25;
            sheet.Range[ROW, COL].Merge();
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
            sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
            sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, COL, ROW, COL + 2].Merge();
            ROW++;

            int StartRow = ROW;

            sheet[ROW, COL].Text = "Costing Category";
            sheet[ROW, COL].ColumnWidth = 18;
            COL++;

            sheet[ROW, COL].Text = "Amount";
            sheet[ROW, COL].ColumnWidth = 18;
            COL++;

            sheet[ROW, COL].Text = "% of Total FOB";
            sheet[ROW, COL].ColumnWidth = 18;

            ROW++;

            COL = COLFinal;
            sheet[ROW, COL].Text = "Fabric Cost";
            int colFabricCost = COL;
            ROW++;

            sheet[ROW, COL].Text = "Trims Cost";
            int colTrimsCost = COL;
            ROW++;

            sheet[ROW, COL].Text = "Accessories Cost";
            int colAccessoriesCost = COL;
            ROW++;

            sheet[ROW, COL].Text = "Washing Cost";
            int colWashingCost = COL;
            ROW++;

            sheet[ROW, COL].Text = "Direct Process Cost";
            int colDirectProcessCost = COL;

            int ColEnd = COL;
            sheet.Range[StartRow, COLFinal, ROW, COLFinal + 2].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[StartRow, COLFinal, ROW, COLFinal + 2].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[StartRow, COLFinal, StartRow, COLFinal + 2].CellStyle.Font.Bold = true;
            sheet.Range[StartRow, COLFinal, StartRow, COLFinal + 2].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;

            ROW = StartRow;
            ROW++;
            COL++;
            int CostingComponentStartRow = ROW;

            double MTotalOtherFabricOrderCost = clsStaticInfo.dbl(dtFundRequired.Compute("SUM(TotalOrderCost)", "CostingCategory='" + "Other Fabric" + "'"));
            double MTotalMainFabricOrderCost = clsStaticInfo.dbl(dtFundRequired.Compute("SUM(TotalOrderCost)", "CostingCategory='" + "Main Fabric" + "'"));
            double totalOtherOrderCost = MTotalOtherFabricOrderCost + MTotalMainFabricOrderCost;
            sheet.Range[ROW, COL].Number = totalOtherOrderCost;
            sheet.Range[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[ROW, COL, ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL, ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW++;


            double MTotalTrimsOrderCost = clsStaticInfo.dbl(dtFundRequired.Compute("SUM(TotalOrderCost)", "CostingCategory='" + "Trims" + "'"));
            sheet.Range[ROW, COL].Number = MTotalTrimsOrderCost;
            sheet.Range[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[ROW, COL, ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL, ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW++;


            double MTotalAccessoriesOrderCost = clsStaticInfo.dbl(dtFundRequired.Compute("SUM(TotalOrderCost)", "CostingCategory='" + "Accessories" + "'"));
            sheet.Range[ROW, COL].Number = MTotalAccessoriesOrderCost;
            sheet.Range[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[ROW, COL, ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL, ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW++;


            double MWetProcessDirectOrderCost = clsStaticInfo.dbl(dtFundRequired.Compute("SUM(TotalOrderCost)", "CostingCategory='" + "Wet-Process" + "'"));
            double MDRYPROCESSDirectOrderCost = clsStaticInfo.dbl(dtFundRequired.Compute("SUM(TotalOrderCost)", "CostingCategory='" + "DRY PROCESS" + "'"));
            double totalDirectWashingCost = MWetProcessDirectOrderCost + MDRYPROCESSDirectOrderCost;

            double MWetProcessOrderCost = clsStaticInfo.dbl(dtFundRequiredWash.Compute("SUM(TOC)", "CostingCategory='" + "Wet-Process" + "'"));
            double MDRYPROCESSOrderCost = clsStaticInfo.dbl(dtFundRequiredWash.Compute("SUM(TOC)", "CostingCategory='" + "DRY PROCESS" + "'"));
            double totalWashingCost = MWetProcessOrderCost + MDRYPROCESSOrderCost;

            sheet.Range[ROW, COL].Number = totalWashingCost + totalDirectWashingCost;
            sheet.Range[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[ROW, COL, ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL, ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW++;


            double MDirectProcessCost = clsStaticInfo.dbl(dtFundRequiredWash.Compute("SUM(TOC)", "CostingCategory='" + "Embriodery" + "'"));
            sheet.Range[ROW, COL].Number = MDirectProcessCost;
            sheet.Range[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[ROW, COL, ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL, ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW++;

            ROW = StartRow;
            ROW++;
            COL++;
            int FOBStartRow = ROW;

            double totalFabricCostFOB = (MTotalOtherFabricOrderCost + MTotalMainFabricOrderCost) / TotalExecutionTC * 100;
            sheet.Range[ROW, COL].Number = totalFabricCostFOB;
            sheet.Range[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[ROW, COL, ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL, ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW++;


            double MTotalFOBTrimsCost = MTotalTrimsOrderCost / TotalExecutionTC * 100;
            sheet.Range[ROW, COL].Number = MTotalFOBTrimsCost;
            sheet.Range[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[ROW, COL, ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL, ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW++;

            double MTotalFOBAccessoriesCost = MTotalAccessoriesOrderCost / TotalExecutionTC * 100;
            sheet.Range[ROW, COL].Number = MTotalFOBAccessoriesCost;
            sheet.Range[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[ROW, COL, ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL, ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW++;

            double totalFOBWashingCost = (totalWashingCost + totalDirectWashingCost) / TotalExecutionTC * 100;
            sheet.Range[ROW, COL].Number = totalFOBWashingCost;
            sheet.Range[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[ROW, COL, ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL, ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW++;

            double totalDirectProcessCost = MDirectProcessCost / TotalExecutionTC * 100;
            sheet.Range[ROW, COL].Number = totalDirectProcessCost;
            sheet.Range[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[ROW, COL, ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL, ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW++;

            int CostingComponentEndRow = ROW - 1;
            sheet[ROW, COL - 2].Text = "Total:";
            sheet.Range[ROW, COL - 2].CellStyle.Font.Bold = true;

            sheet.Range[ROW, COL - 1, ROW, COL - 1].Formula = "SUM(" + reportUtility.GetColumnNameForXls(COL - 1) + FOBStartRow + ":" + reportUtility.GetColumnNameForXls(COL - 1) + CostingComponentEndRow + ")";
            sheet.Range[ROW, COL - 1].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL - 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW, COL - 1].NumberFormat = clsStaticInfo.NumberFormat(2);

            sheet.Range[ROW, COL, ROW, COL].Formula = "SUM(" + reportUtility.GetColumnNameForXls(COL) + FOBStartRow + ":" + reportUtility.GetColumnNameForXls(COL) + CostingComponentEndRow + ")";
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat(2);

            sheet.Range[ROW, COL - 2, ROW, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW, COL - 2, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            ROW = ROW + 2;

            sheet[ROW, COL - 2].Text = "Note: 68% of FOB:";
            sheet.Range[ROW, COL - 2].CellStyle.Font.Bold = true;

            sheet.Range[ROW, COL - 1].Number = TotalExecutionTC * 68 / 100;
            sheet.Range[ROW, COL - 1].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL - 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW, COL - 1].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[ROW, COL - 2, ROW, COL - 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Yellow;
        }

        private void ValueLoss(IWorksheet sheet, ref int ROW, string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting, DataTable dtMOICostingInfo)
        {
            ReportUtility reportUtility = new ReportUtility();
            String CostingValueLossSQL = OrderPreCostingValueLossSQL(OrderCostingId, orderBudget, preCosting, ProcurementCosting);
            DataTable dtOrderCostingValueLoss = _sqlRepository.GetDataTable(CostingValueLossSQL);
            if (dtOrderCostingValueLoss.Rows.Count == 0)
                return;

            DataTable dvDistinctCostingComponent = dtOrderCostingValueLoss.DefaultView.ToTable(true, "CostingComponentId", "CostingComponentName");
            string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

            int CostingComponentEndcol = 0;
            int COL = 1;

            sheet[ROW, COL].Text = dtOrderCostingValueLoss.Rows[0]["CostingComponentName"].ToString() + " breakdown.";
            sheet[ROW, COL].RowHeight = 30;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
            sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
            sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, COL, ROW, 6].Merge();

            for (int i = 0; i < dvDistinctCostingComponent.Rows.Count; i++)
            {
                ROW++;
                //sheet[1, 1].Text = dvDistinctCostingComponent.Rows[i]["CostingComponentName"].ToString();

                sheet[ROW, COL].Text = "CostingItem";
                int colCostingItem = COL;
                COL++;

                sheet[ROW, COL].Text = "Type";
                int colType = COL;
                COL++;

                sheet[ROW, COL].Text = "Value";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Qty";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Order Cost";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalOrderCost = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                int colCurrency = COL;

                CostingComponentEndcol = COL;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                //sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 1].Merge();
                //sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                sheet.Range[ROW - 2, 1, ROW - 2, CostingComponentEndcol].Merge();
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                dtOrderCostingValueLoss.DefaultView.RowFilter = "CostingComponentId='" + dvDistinctCostingComponent.Rows[i]["CostingComponentId"].ToString() + "'";
                DataTable dtComponentRelatedItems = dtOrderCostingValueLoss.DefaultView.ToTable();
                int CostingComponentStartRow = ROW;

                for (int M = 0; M < dtComponentRelatedItems.Rows.Count; M++)
                {
                    sheet[ROW, colCostingItem].Text = dtComponentRelatedItems.DefaultView[M]["CostingItem"].ToString();
                    sheet[ROW, colType].Text = dtComponentRelatedItems.DefaultView[M]["Type"].ToString();
                    sheet[ROW, colValue].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString());
                    sheet[ROW, colQty].Number = clsStaticInfo.dbl(OrderQTY);
                    if (preCosting == "1" || orderBudget == "1")
                    {
                        sheet[ROW, colTotalOrderCost].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString()) * clsStaticInfo.dbl(OrderQTY);
                    }
                    if (orderBudget == "1")
                    {
                        sheet[ROW, colTotalOrderCost].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["TotalOrderCost"].ToString());
                    }
                    sheet[ROW, colCurrency].Text = dtComponentRelatedItems.DefaultView[M]["Currency"].ToString();

                    //sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 1].Merge();
                    //sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                    ROW++;
                }
                int CostingComponentEndRow = ROW - 1;
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 3].Merge();

                sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();

                sheet.Range[ROW, colValue].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colValue].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colQty].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colQty) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colQty) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colQty].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colTotalOrderCost].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colTotalOrderCost) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colTotalOrderCost) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colTotalOrderCost].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colCurrency].Formula = reportUtility.GetColumnNameForXls(colCurrency) + (ROW - 1);
                sheet.Range[ROW, colCurrency].CellStyle.Font.Bold = true;

                sheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, CostingComponentEndcol].NumberFormat = clsStaticInfo.NumberFormat(4);

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                ROW++;

                sheet.Range[CostingComponentStartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colQty, ROW, colQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colTotalOrderCost, ROW, colTotalOrderCost].NumberFormat = clsStaticInfo.NumberFormat(2);
            }
        }

        private void Profit(IWorksheet sheet, ref int ROW, string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting, DataTable dtMOICostingInfo)
        {
            ReportUtility reportUtility = new ReportUtility();
            String CostingProfitSQL = OrderPreCostingProfitSQL(OrderCostingId, orderBudget, preCosting, ProcurementCosting);
            DataTable dtOrderCostingProfit = _sqlRepository.GetDataTable(CostingProfitSQL);

            if (dtOrderCostingProfit.Rows.Count == 0)
                return;

            DataTable dvDistinctCostingComponent = dtOrderCostingProfit.DefaultView.ToTable(true, "CostingComponentId", "CostingComponentName");
            string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

            int CostingComponentEndcol = 0;
            int COL = 1;

            sheet[ROW, COL].Text = dtOrderCostingProfit.Rows[0]["CostingComponentName"].ToString() + " breakdown.";
            sheet[ROW, COL].RowHeight = 30;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
            sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
            sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, COL, ROW, 6].Merge();

            for (int i = 0; i < dvDistinctCostingComponent.Rows.Count; i++)
            {
                ROW++;
                //sheet[1, 1].Text = dvDistinctCostingComponent.Rows[i]["CostingComponentName"].ToString();
                COL = 1;

                sheet[ROW, COL].Text = "CostingItem";
                int colCostingItem = COL;
                COL++;

                sheet[ROW, COL].Text = "Type";
                int colType = COL;
                COL++;

                sheet[ROW, COL].Text = "Value Loss(%)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Order Cost";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalOrderCost = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                int colCurrency = COL;

                CostingComponentEndcol = COL;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                ////sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 1].Merge();
                //sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                sheet.Range[ROW - 2, 1, ROW - 2, CostingComponentEndcol].Merge();

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                dtOrderCostingProfit.DefaultView.RowFilter = "CostingComponentId='" + dvDistinctCostingComponent.Rows[i]["CostingComponentId"].ToString() + "'";
                DataTable dtComponentRelatedItems = dtOrderCostingProfit.DefaultView.ToTable();
                int CostingComponentStartRow = ROW;

                for (int M = 0; M < dtComponentRelatedItems.Rows.Count; M++)
                {
                    sheet[ROW, colCostingItem].Text = dtComponentRelatedItems.DefaultView[M]["CostingItem"].ToString();
                    sheet[ROW, colType].Text = dtComponentRelatedItems.DefaultView[M]["Type"].ToString();
                    sheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Amount"].ToString());
                    sheet[ROW, colValue].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString());
                    if (preCosting == "1" || ProcurementCosting == "1")
                    {
                        sheet[ROW, colTotalOrderCost].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Amount"].ToString()) * clsStaticInfo.dbl(OrderQTY);
                    }
                    if (orderBudget == "1")
                    {
                        sheet[ROW, colTotalOrderCost].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["TotalOrderCost"].ToString());
                    }
                    sheet[ROW, colCurrency].Text = dtComponentRelatedItems.DefaultView[M]["Currency"].ToString();

                    //sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 1].Merge();
                    //sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }
                int CostingComponentEndRow = ROW - 1;
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 3].Merge();
                sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();

                sheet.Range[ROW, colValue].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colValue].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colAmount].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colAmount) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colAmount) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colAmount].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colTotalOrderCost].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colTotalOrderCost) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colTotalOrderCost) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colTotalOrderCost].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colCurrency].Formula = reportUtility.GetColumnNameForXls(colCurrency) + (ROW - 1);
                sheet.Range[ROW, colCurrency].CellStyle.Font.Bold = true;

                sheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, CostingComponentEndcol].NumberFormat = clsStaticInfo.NumberFormat(4);

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                ROW++;

                sheet.Range[CostingComponentStartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colAmount, ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colTotalOrderCost, ROW, colTotalOrderCost].NumberFormat = clsStaticInfo.NumberFormat(2);
            }
        }

        private void SalesExpense(IWorksheet sheet, ref int ROW, string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting, DataTable dtMOICostingInfo)
        {
            ReportUtility reportUtility = new ReportUtility();
            String CostingSalesExpenseSQL = OrderPreCostingSalesExpenseSQL(OrderCostingId, orderBudget, preCosting, ProcurementCosting);
            DataTable dtOrderCostingSalesExpense = _sqlRepository.GetDataTable(CostingSalesExpenseSQL);

            if (dtOrderCostingSalesExpense.Rows.Count == 0)
                return;

            DataTable dvDistinctCostingComponent = dtOrderCostingSalesExpense.DefaultView.ToTable(true, "CostingComponentId", "CostingComponentName");
            string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

            int CostingComponentEndcol = 0;
            int COL = 1;

            sheet[ROW, COL].Text = dtOrderCostingSalesExpense.Rows[0]["CostingComponentName"].ToString() + " breakdown.";
            sheet[ROW, COL].RowHeight = 30;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
            sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
            sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, COL, ROW, 6].Merge();

            for (int i = 0; i < dvDistinctCostingComponent.Rows.Count; i++)
            {
                ROW++;
                //sheet[1, 1].Text = dvDistinctCostingComponent.Rows[i]["CostingComponentName"].ToString();
                COL = 1;

                sheet[ROW, COL].Text = "CostingItem";
                int colCostingItem = COL;
                COL++;

                sheet[ROW, COL].Text = "Type";
                int colType = COL;
                COL++;

                sheet[ROW, COL].Text = "Value";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Qty";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Order Cost";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalOrderCost = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCurrency = COL;

                CostingComponentEndcol = COL;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                //sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 1].Merge();
                //sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                sheet.Range[ROW - 2, 1, ROW - 2, CostingComponentEndcol].Merge();
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                dtOrderCostingSalesExpense.DefaultView.RowFilter = "CostingComponentId='" + dvDistinctCostingComponent.Rows[i]["CostingComponentId"].ToString() + "'";
                DataTable dtComponentRelatedItems = dtOrderCostingSalesExpense.DefaultView.ToTable();

                int CostingComponentStartRow = ROW;

                for (int M = 0; M < dtComponentRelatedItems.Rows.Count; M++)
                {
                    sheet[ROW, colCostingItem].Text = dtComponentRelatedItems.DefaultView[M]["CostingItem"].ToString();
                    sheet[ROW, colType].Text = dtComponentRelatedItems.DefaultView[M]["Type"].ToString();
                    sheet[ROW, colQty].Number = clsStaticInfo.dbl(OrderQTY);
                    sheet[ROW, colValue].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString());
                    if (preCosting == "1" || ProcurementCosting == "1")
                    {
                        sheet[ROW, colTotalOrderCost].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString()) * clsStaticInfo.dbl(OrderQTY);
                    }
                    if (orderBudget == "1")
                    {
                        sheet[ROW, colTotalOrderCost].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["TotalOrderCost"].ToString());
                    }
                    sheet[ROW, colCurrency].Text = dtComponentRelatedItems.DefaultView[M]["Currency"].ToString();

                    //sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 1].Merge();
                    //sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                    ROW++;
                }
                int CostingComponentEndRow = ROW - 1;
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                sheet.Range[ROW, colValue, ROW, colValue].Merge();

                sheet.Range[ROW, colQty].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colQty) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colQty) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colQty].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colTotalOrderCost].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colTotalOrderCost) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colTotalOrderCost) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colTotalOrderCost].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colCurrency].Formula = reportUtility.GetColumnNameForXls(colCurrency) + (ROW - 1);
                sheet.Range[ROW, colCurrency].CellStyle.Font.Bold = true;

                sheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, CostingComponentEndcol].NumberFormat = clsStaticInfo.NumberFormat(4);

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                ROW++;

                sheet.Range[CostingComponentStartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colQty, ROW, colQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colTotalOrderCost, ROW, colTotalOrderCost].NumberFormat = clsStaticInfo.NumberFormat(2);
            }
        }

        private string FundRequiredSQL(string OrderCostingId, string preCosting, string ProcurementCosting)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string TableName = "";

            if (preCosting == "1")
            {
                TableName = "OrderPreCostingOperation";
            }
            if (ProcurementCosting == "1")
            {
                TableName = "OrderProcurementCostingOperation ";
            }

            return @"SELECT pc.Id,I.Id as CostingId,I.UserName as CostingItem,I.CostingComponentId,pc.Sequence
				,ISNULL(pc.Value,0) AS Value,OCMT.Id as OrderCostingMasterTemplateId
				,cc.UserName as CostingComponentName,c.Code as Currency 
				
				
				FROM " + TableName + @" AS pc       
				LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId 
				LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
				LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId 
				LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId

				where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
				order by pc.Sequence";
        }

        private string OrderCostingProductInfoSQL(string OrderCostingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory
							,psc.UserName as ProductSubCategory,ct.UserName AS CostingTypeName
                             ,pm.CostingType,eff.StandardWorkingHours AS StandardWorkingHoursForProduct
							,c.Code Currency,u.UserName UnitOfMeasurement
							from OrderCostingMasterTemplate qcm 
							left outer join SCS.Currency c on c.Id=qcm.CurrencyId
							left join SCS.UnitOfMeasurement u on u.Id=qcm.UOM
                            left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT JOIN [TRN].[ProductMasterEfficency] EFF ON eff.ProductMasterId=qcm.ProductMasterId AND EfficencyName='Costing'  
							LEFT OUTER JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType
                            WHERE QCM.ID='" + OrderCostingId + @"'";

        }


        private string OrderCostingProductDetailSQL(string OrderCostingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select isnull(d.id,'New') isNewId, case when isnull(d.Id,'')<>'' THEN isnull(TEMPLATE.CostingComponentId,'DELETE') ELSE '' END AS isToBeDeleted,
                         d.Id
                        ,0 as Status,CC.CalculationMethod
	                    ,d.CostingValue
	                    ,d.BuyerTarget
	                    --,d.CostingVersionMasterTemplateId
                        ,cc.Id as CostingComponentId
	                    ,cc.Code
	                    ,cc.ShortName
	                    ,cc.UserName
                        ,ctc.Sequence
	                    ,cc.StandardName
	                    ,ctc.CostingType
                        ,cc.CostingSegment
                        ,isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount 
                        ,isnull(itemvalp.TotalGrossAmount,0) AS TotalProcurementGrossAmount 
                        ,isnull(itemval.TotalGrossAmount - itemvalp.TotalGrossAmount,0) AS DifferencePreProCosting
                        ,CC.ProcurementCostingSavingsPercentage, CC.PreCostingSavingsPercentage
						 from hkp.CostingComponent CC
                        left outer join [dbo].[CostingTypeComponent] AS ctc  ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = (select ProductMasterId from OrderCostingMasterTemplate  where id='" + OrderCostingId + @"'))
                        left outer join OrderCostingDetailTemplate D on cc.id=d.CostingComponentId and d.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
                         LEFT OUTER JOIN ( SELECT i.CostingComponentId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM OrderPreCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=    '" + OrderCostingId + @"' GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + OrderCostingId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.[Value]) AS TotalGrossAmount FROM OrderPreCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + OrderCostingId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + OrderCostingId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + OrderCostingId + @"'	GROUP BY i.CostingComponentId
                                  )AS ITEMVAL ON  itemval.CostingComponentId=d.CostingComponentId
                        LEFT OUTER JOIN ( SELECT i.CostingComponentId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM OrderProcurementCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=    '" + OrderCostingId + @"' GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderProcurementCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + OrderCostingId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.[Value]) AS TotalGrossAmount FROM OrderProcurementCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderProcurementCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + OrderCostingId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderProcurementCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + OrderCostingId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderProcurementCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + OrderCostingId + @"'	GROUP BY i.CostingComponentId
                                  )AS ITEMVALP ON  ITEMVALP.CostingComponentId=d.CostingComponentId
                        left outer join  (
                        select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = (select ProductMasterId from OrderCostingMasterTemplate  where id='" + OrderCostingId + @"'))) AS TEMPLATE 
					    on template.CostingComponentId=d.CostingComponentId


                        where   cc.Id IN (
                            select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = (select ProductMasterId from OrderCostingMasterTemplate  where id='" + OrderCostingId + @"'))

					    UNION

					    select CostingComponentId from OrderCostingDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')='" + OrderCostingId + @"'

					--union

					--select CostingComponentId from CostingVersionDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')= '" + OrderCostingId + @"'
                    )  order by isnull(ctc.Sequence,999999),cc.Description";

        }

        private string OrderCostingMOISQL(string MOIId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select MOI.Id MasterOrderId,MOI.TotalQty OrderQty  from TRN.MasterOrderItem MOI where MOI.Id in (" + MOIId + @")";

        }



        private string OrderPreCostingDirectMaterialSQL(string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string TableName = "";

            if (preCosting == "1" || orderBudget == "1")
            {
                TableName = "OrderPreCostingDirectMaterial";
            }
            if (ProcurementCosting == "1")
            {
                TableName = "OrderProcurementCostingDirectMaterial";
            }

            return @"SELECT pc.Id,I.Id as CostingId,pc.Sequence,UOM.Code as UOM,pc.Particulars,I.UserName as CostingItem,ccg.UserName CostingCategory,I.CostingComponentId
					,CC.CostingSegment,cc.UserName as CostingComponentName,ISNULL(pc.Consumption,0) AS Consumption,ISNULL(pc.Rate,0) AS Rate
					,ISNULL(pc.ValueLoss,0) AS ValueLoss,pc.MinimumOfQuantity
					,ISNULL(pc.GrossConsumption,0) AS GrossConsumption
					,C.Code as Currency,OCMT.Id as OrderCostingMasterTemplateId
					,EI.EmployeeName as ResponsiblePerson,pc.SourcingType,MM.UserName as Material,MMA.StandardName as Article,pc.VendorId
					--,ISNULL(MOI.TotalQty,0) TotalQty
					,TotalQty=(select sum(TotalQty) from  trn.MasterOrderItem where OrderCostingMasterTemplateId=PC.OrderCostingMasterTemplateId)
					--,TotalMaterialRequirement=(ISNULL(MOI.TotalQty,0) * ISNULL(pc.GrossConsumption,0))
					,TotalMaterialRequirement=sum(ISNULL(TotalQty,0) * ISNULL(pc.GrossConsumption,0))
					,ISNULL(pc.GrossAmount,0) AS GrossAmount
					,TotalOrderCost=ISNULL(pc.GrossAmount,0)*(select sum(TotalQty) from  trn.MasterOrderItem where OrderCostingMasterTemplateId=PC.OrderCostingMasterTemplateId)
					 
					FROM " + TableName + @" AS pc  
					LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
					LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
                    LEFT JOIN SCS.UnitOfMeasurement as UOM on UOM.Id=I.UnitOfMeasurementId
					LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId
					LEFT JOIN TRN.MasterOrderItem MOI on MOI.OrderCostingMasterTemplateId=OCMT.Id
					LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId
					LEFT JOIN EmployeeInformation EI on EI.SystemId=pc.ResponsiblePersonId
					LEFT JOIN MST.MaterialMasterArticle MMA on MMA.Id=pc.ArticleId
					LEFT JOIN MST.MaterialMaster MM on MM.Id=pc.MaterialMasterId
					LEFT JOIN HKP.Party P on P.Id=pc.VendorId
					LEFT JOIN [HKP].[CostingCategory] AS ccg ON ccg.Id = I.CostingCategoryId

					where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"' and I.Id is not null
                    group by pc.Id,I.Id,pc.Sequence,UOM.Code,pc.Particulars,I.UserName,I.CostingComponentId	,CC.CostingSegment,ccg.UserName
					,cc.UserName,pc.Consumption,pc.Rate,pc.ValueLoss,pc.MinimumOfQuantity,pc.GrossConsumption,pc.GrossAmount
					,C.Code,OCMT.Id,EI.EmployeeName,pc.SourcingType,MM.UserName,MMA.StandardName,pc.VendorId,PC.OrderCostingMasterTemplateId
					order by pc.Sequence";



        }

        private string OrderPreCostingDirectProcessSQL(string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string TableName = "";

            if (preCosting == "1")
            {
                TableName = "OrderPreCostingDirectProcess";
            }
            if (ProcurementCosting == "1")
            {
                TableName = "OrderProcurementCostingDirectProcess";
            }
            if (orderBudget == "1")
            {
                TableName = "OrderPreCostingDirectProcess";
                return @"SELECT pc.Id,I.Id as CostingId,pc.Sequence,I.UserName as CostingItem,I.CostingComponentId
            ,ISNULL(pc.ExecutionType,'Fixed') as [Type],ccg.UserName CostingCategory
			,OCMT.Id as OrderCostingMasterTemplateId,cc.UserName as CostingComponentName
			,ISNULL(pc.Value,0) AS Value,ISNULL(pc.Rate,0) AS Rate
			,C.Code as Currency ,EI.EmployeeName as ResponsiblePerson
			
            ,OrderQty=(select sum(moi.TotalQty)
			from trn.MasterOrderItem moi 
			where moi.OrderCostingMasterTemplateId=OCMT.Id
			group by moi.OrderCostingMasterTemplateId)

			,Amount=(select sum(pc.Amount)
			from " + TableName + @" pc
			where pc.OrderCostingMasterTemplateId=OCMT.Id) 
			 
			,TotalOrderCost= (select sum(moi.TotalQty)
			from trn.MasterOrderItem moi 
			where moi.OrderCostingMasterTemplateId=OCMT.Id
			group by moi.OrderCostingMasterTemplateId)
			*(select sum(pc.Amount)
			from " + TableName + @" pc
			where pc.OrderCostingMasterTemplateId=OCMT.Id) 
			,TOC=(select sum(moi.TotalQty)
			from trn.MasterOrderItem moi 
			where moi.OrderCostingMasterTemplateId=OCMT.Id
			group by moi.OrderCostingMasterTemplateId)*
			ISNULL(pc.Rate,0)
			
			FROM " + TableName + @" AS pc 
			LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId 
			LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
			LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId
			LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId 
			LEFT JOIN EmployeeInformation EI on EI.SystemId=pc.ResponsiblePersonId
			LEFT JOIN [HKP].[CostingCategory] AS ccg ON ccg.Id = I.CostingCategoryId

			where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
			order by pc.Sequence";
            }

            return @"SELECT pc.Id,I.Id as CostingId,pc.Sequence,I.UserName as CostingItem,I.CostingComponentId
            ,ISNULL(pc.ExecutionType,'Fixed') as [Type],ccg.UserName CostingCategory
			,OCMT.Id as OrderCostingMasterTemplateId,cc.UserName as CostingComponentName
			,ISNULL(pc.Value,0) AS Value,ISNULL(pc.Rate,0) AS Rate,ISNULL(pc.Amount,0) AS Amount
			,C.Code as Currency,EI.EmployeeName as ResponsiblePerson
			,TotalOrderCost=ISNULL(pc.Amount,0)*(select sum(TotalQty) from  trn.MasterOrderItem where OrderCostingMasterTemplateId=PC.OrderCostingMasterTemplateId)
			
			FROM " + TableName + @" AS pc   
			LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId 
			LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
			LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId
			LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId
			LEFT JOIN EmployeeInformation EI on EI.SystemId=pc.ResponsiblePersonId
			LEFT JOIN [HKP].[CostingCategory] AS ccg ON ccg.Id = I.CostingCategoryId

			where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
			order by pc.Sequence";
        }

        private string OrderPreCostingOperationSQL(string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string TableName = "";

            if (preCosting == "1")
            {
                TableName = "OrderPreCostingOperation";
            }
            if (ProcurementCosting == "1")
            {
                TableName = "OrderProcurementCostingOperation ";
            }
            if (orderBudget == "1")
            {
                TableName = "OrderPreCostingOperation ";
                return @"SELECT pc.Id,I.Id as CostingId,I.UserName as CostingItem,I.CostingComponentId,pc.Sequence
				,ISNULL(pc.Value,0) AS Value,OCMT.Id as OrderCostingMasterTemplateId
				,cc.UserName as CostingComponentName,c.Code as Currency 
				,ISNULL(pc.Value,0) AS Amount
                --, sum(ISNULL(moi.TotalQty,0)) TotalQty
                ,TotalQty= case when Value<>0 then sum(ISNULL(moi.TotalQty,0)) else 0 end
				,TotalOrderCost=ISNULL(pc.Value,0)*sum(ISNULL(moi.TotalQty,0))

				FROM " + TableName + @" AS pc       
				LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId 
				LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
				LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId 
				LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId
				LEFT JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=ocmt.Id

				where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
				group by pc.Id,pc.CostingItemId,I.Id,I.UserName,I.CostingComponentId,pc.Sequence
				,pc.Value,OCMT.Id,cc.UserName,c.Code
                order by pc.Sequence";
            }

            return @"SELECT pc.Id,I.Id as CostingId,I.UserName as CostingItem,I.CostingComponentId,pc.Sequence
				,ISNULL(pc.Value,0) AS Value,OCMT.Id as OrderCostingMasterTemplateId
				,cc.UserName as CostingComponentName,c.Code as Currency 
				
				
				FROM " + TableName + @" AS pc       
				LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId 
				LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
				LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId 
				LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId

				where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
				order by pc.Sequence";
        }

        private string OrderPreCostingValueLossSQL(string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string TableName = "";

            if (preCosting == "1")
            {
                TableName = "OrderPreCostingValueLoss";
            }
            if (ProcurementCosting == "1")
            {
                TableName = "OrderProcurementCostingValueLoss";
            }
            if (orderBudget == "1")
            {
                TableName = "OrderPreCostingValueLoss";
                return @"SELECT pc.Id,I.Id as CostingId,I.UserName as CostingItem,I.CostingComponentId,pc.Sequence
			                        ,OCMT.Id as OrderCostingMasterTemplateId,ISNULL(pc.Type,'Fixed') as [Type],ISNULL(pc.Value,0) AS Value
			                        ,C.Code as Currency,cc.UserName as CostingComponentName 
                                    ,OrderQty=(select sum(moi.TotalQty)
									from trn.MasterOrderItem moi 
									where moi.OrderCostingMasterTemplateId=OCMT.Id
									group by moi.OrderCostingMasterTemplateId)

									,Amount=(select sum(pc.Amount)
									from " + TableName + @" pc
									where pc.OrderCostingMasterTemplateId=OCMT.Id) 
			 
									,TotalOrderCost= (select sum(moi.TotalQty)
									from trn.MasterOrderItem moi 
									where moi.OrderCostingMasterTemplateId=OCMT.Id
									group by moi.OrderCostingMasterTemplateId)
									*(select sum(pc.Amount)
									from " + TableName + @" pc
									where pc.OrderCostingMasterTemplateId=OCMT.Id) 

			FROM " + TableName + @" AS pc 
			LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
			LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
			LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId 
			LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId 
			LEFT JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=ocmt.Id
			
			where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
			order by pc.Sequence";
            }

            return @"SELECT		pc.Id,I.Id as CostingId,I.UserName as CostingItem,I.CostingComponentId,pc.Sequence
			,OCMT.Id as OrderCostingMasterTemplateId,ISNULL(pc.Type,'Fixed') as [Type],ISNULL(pc.Value,0) AS Value,ISNULL(pc.Amount,0) AS Amount
			,C.Code as Currency,EI.EmployeeName as ResponsiblePerson,cc.UserName as CostingComponentName 


			FROM " + TableName + @" AS pc 
			LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
			LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
			LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId 
			LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId
			LEFT JOIN EmployeeInformation EI on EI.SystemId=pc.ResponsiblePersonId
			
			where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
			order by pc.Sequence";
        }

        private string OrderPreCostingProfitSQL(string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string TableName = "";

            if (preCosting == "1")
            {
                TableName = "OrderPreCostingProfit";
            }
            if (ProcurementCosting == "1")
            {
                TableName = "OrderProcurementCostingProfit";
            }
            if (orderBudget == "1")
            {
                TableName = "OrderPreCostingProfit";
                return @"SELECT pc.Id,I.Id as CostingId,pc.Sequence,I.UserName as CostingItem,I.CostingComponentId
			                            ,ISNULL(pc.Type,'Fixed') as [Type],ISNULL(pc.Value,0) AS Value,C.Code as Currency
			                            ,OCMT.CurrencyId,PC.OrderCostingMasterTemplateId,cc.UserName as CostingComponentName  
			                            
										,OrderQty=(select sum(moi.TotalQty)
										from trn.MasterOrderItem moi 
										where moi.OrderCostingMasterTemplateId=OCMT.Id
										group by moi.OrderCostingMasterTemplateId)

										,Amount=(select sum(pc.Amount)
										from " + TableName + @" pc
										where pc.OrderCostingMasterTemplateId=OCMT.Id) 
			 
										,TotalOrderCost= (select sum(moi.TotalQty)
										from trn.MasterOrderItem moi 
										where moi.OrderCostingMasterTemplateId=OCMT.Id
										group by moi.OrderCostingMasterTemplateId)
										*(select sum(pc.Amount)
										from " + TableName + @" pc
										where pc.OrderCostingMasterTemplateId=OCMT.Id)	 

			                            FROM " + TableName + @" AS pc 
			                            LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
			                            LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
			                            LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId 
			                            LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId 
			                            LEFT JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=ocmt.Id

			where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
			order by pc.Sequence";
            }
            return @"SELECT		 pc.Id,I.Id as CostingId,pc.Sequence,I.UserName as CostingItem,I.CostingComponentId
			,ISNULL(pc.Type,'Fixed') as [Type],ISNULL(pc.Value,0) AS Value,ISNULL(pc.Amount,0) AS Amount,C.Code as Currency
			,OCMT.CurrencyId,PC.OrderCostingMasterTemplateId,cc.UserName as CostingComponentName  

			FROM " + TableName + @" AS pc 
			LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
			LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
			LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId 
			LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId 
			
			
			where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
			order by pc.Sequence";
        }


        private string OrderPreCostingSalesExpenseSQL(string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string TableName = "";

            if (preCosting == "1")
            {
                TableName = "OrderPreCostingSalesExpense";
            }
            if (ProcurementCosting == "1")
            {
                TableName = "OrderProcurementCostingSalesExpense ";
            }
            if (orderBudget == "1")
            {
                TableName = "OrderPreCostingSalesExpense";
                return @" SELECT pc.Id,I.Id as CostingId,pc.Sequence,I.UserName as CostingItem,I.CostingComponentId
		                            ,ISNULL(pc.Type,'Fixed') as [Type],ISNULL(pc.Value,0) Value ,C.Code as Currency
		                            ,cc.UserName as CostingComponentName
                                     ,ISNULL(pc.Value,0) AS Amount, sum(ISNULL(moi.TotalQty,0)) TotalQty
									,TotalOrderCost=ISNULL(pc.Value,0)*sum(ISNULL(moi.TotalQty,0))

		                            FROM " + TableName + @" AS pc    
		                            LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
		                            LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
		                            LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId 
		                            LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId
		                            LEFT JOIN EmployeeInformation EI on EI.SystemId=pc.ResponsiblePersonId 
									LEFT JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=ocmt.Id

		                            where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
                                    group by pc.Id,I.Id,pc.Sequence,I.UserName,I.CostingComponentId,pc.Type
									,pc.Value,OCMT.Id,cc.UserName,c.Code
		                            order by pc.Sequence";
            }
            return @"SELECT pc.Id,I.Id as CostingId,pc.Sequence,I.UserName as CostingItem,I.CostingComponentId
		,ISNULL(pc.Type,'Fixed') as [Type],ISNULL(pc.Value,0) AS Value,ISNULL(pc.Amount,0) AS Amount,C.Code as Currency
		,cc.UserName as CostingComponentName

		FROM " + TableName + @" AS pc    
		LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
		LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
		LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId 
		LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId
		LEFT JOIN EmployeeInformation EI on EI.SystemId=pc.ResponsiblePersonId

		where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
		order by pc.Sequence";

        }



        //---------Costing Templete---
        public void CostingTempleteReport(string CostingTempleteId)
        {
            try
            {
                if (CostingTempleteId == "null")
                    throw new Exception("No costing template found for the current item.");

                string sql = PreCostingProductInfoSQL(CostingTempleteId);
                string CostingDetailsql = PreCostingProductDetailSQL(CostingTempleteId);

                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;
                ReportUtility reportUtility = new ReportUtility();
                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];


                sheet.Name = "Costing Templete Report";


                DataTable dtOrderCostingProductInfo = _sqlRepository.GetDataTable(sql);

                if (dtOrderCostingProductInfo.Rows.Count == 0)
                    throw new Exception("No Data Found");

                int ROW = 6;
                int COL = 1;

                #region Header
                sheet[ROW, COL].Text = "Product Information";
                sheet[ROW, COL].RowHeight = 25;
                sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
                sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                ROW++;

                COL = 1;
                int StartRow = ROW;
                sheet[ROW, COL].Text = "Product Master";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductMaster = COL;
                ROW++;
                sheet[ROW, COL].Text = "Cost.Type";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCostType = COL;
                ROW = StartRow;
                COL = 4;
                sheet[ROW, COL].Text = "Prod. Cat.";
                sheet[ROW, COL].ColumnWidth = 11;
                int colProdCategory = COL;
                ROW++;
                sheet[ROW, COL].Text = "Costing Id";
                int colCostingId = COL;
                ROW = StartRow;
                COL = 7;
                sheet[ROW, COL].Text = "Prod.Sub Cat.";
                sheet[ROW, COL].ColumnWidth = 13;
                int colProdSubCategory = COL;
                //ROW++;
                //sheet[ROW, COL].Text = "Costing Stage";
                //int colCostingStage = COL;
                ROW++;
                ROW++;

                COL = 1;
                StartRow = ROW;
                sheet[ROW, COL].Text = "Code";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCode = COL;
                ROW++;
                sheet[ROW, COL].Text = "Short Name";
                sheet[ROW, COL].ColumnWidth = 10;
                int colShortName = COL;
                ROW++;
                sheet[ROW, COL].Text = "Mkt Tgt / SPT";
                sheet[ROW, COL].ColumnWidth = 10;
                int colMktTgtSPT = COL;
                ROW++;
                sheet[ROW, COL].Text = "SPT";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSPT = COL;
                ROW++;
                sheet[ROW, COL].Text = "Standard/Plan Hours";
                sheet[ROW, COL].ColumnWidth = 12;
                int colStandardPlanHours = COL;
                ROW++;
                ROW = StartRow;
                COL = 4;
                sheet[ROW, COL].Text = "User Name";
                int colUserName = COL;
                ROW++;
                sheet[ROW, COL].Text = "Standard Name";
                int colStandardName = COL;
                ROW++;
                sheet[ROW, COL].Text = "Target / Hour";
                int colTargetHour = COL;
                ROW++;
                sheet[ROW, COL].Text = "Efficiency %";
                int colEfficiency = COL;
                ROW++;
                ROW = StartRow;
                COL = 7;
                sheet[ROW, COL].Text = "Description";
                int colDescription = COL;
                ROW++;
                sheet[ROW, COL].Text = "No Of WS";
                int colNoOfWS = COL;
                ROW++;
                sheet[ROW, COL].Text = "WC Target / Day";
                int colWCTargetDay = COL;

                #endregion
                ROW = 16;
                COL = 1;
                #region General Information
                sheet[ROW, COL].Text = "General Information";
                sheet[ROW, COL].RowHeight = 25;
                sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
                sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                ROW++;

                StartRow = ROW;
                sheet[ROW, COL].Text = "Prd.Avl.Days";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPrdAvlDays = COL;
                ROW++;
                sheet[ROW, COL].Text = "Excess%";
                sheet[ROW, COL].ColumnWidth = 10;
                int colExcess = COL;
                ROW++;
                sheet[ROW, COL].Text = "Critical Level";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCriticalLevel = COL;
                ROW++;
                sheet[ROW, COL].Text = "Packing Type";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPackingType = COL;
                ROW++;
                sheet[ROW, COL].Text = "Tgt Sel. Price*";
                sheet[ROW, COL].ColumnWidth = 20;
                int colTgtSelPrice = COL;
                ROW++;

                ROW = StartRow;
                COL = 4;
                sheet[ROW, COL].Text = "Specific To*";
                int colSpecificTo = COL;
                ROW++;
                sheet[ROW, COL].Text = "UOM";
                int colUOM = COL;
                ROW++;
                sheet[ROW, COL].Text = "Payment Days";
                int colPaymentDays = COL;
                ROW++;
                sheet[ROW, COL].Text = "Order Size";
                int colOrderSize = COL;
                ROW++;
                ROW = StartRow;
                COL = 7;
                sheet[ROW, COL].Text = "Target CM";
                int colTargetCM = COL;
                ROW++;
                sheet[ROW, COL].Text = "Est.NoOf Pag List";
                int colEstNoOfPagList = COL;
                ROW++;
                sheet[ROW, COL].Text = "Remarks";
                int colRemarks = COL;
                ROW++;
                sheet[ROW, COL].Text = "Currency";
                int colCurrency = COL;

                #endregion



                int endCol = colWCTargetDay + 2;

                sheet.Range[6, 1, 6, endCol].Merge();
                sheet.Range[16, 1, 16, endCol].Merge();
                sheet.Range[7, 1, 21, 1].CellStyle.Font.Bold = true;
                sheet.Range[7, 4, 21, 4].CellStyle.Font.Bold = true;
                sheet.Range[7, 7, 21, 7].CellStyle.Font.Bold = true;

                ROW++;

                StartRow = 7; //row 20
                for (int i = 0; i < dtOrderCostingProductInfo.Rows.Count; i++)
                {
                    COL = 2;
                    ROW = StartRow;
                    sheet[ROW, colProductMaster + 1].Text = dtOrderCostingProductInfo.Rows[i]["ProductMaster"].ToString();
                    ROW++;
                    sheet[ROW, colCostType + 1].Text = dtOrderCostingProductInfo.Rows[i]["CostingTypeName"].ToString();

                    ROW = StartRow;
                    COL = 5;
                    sheet[ROW, colProdCategory + 1].Text = dtOrderCostingProductInfo.Rows[i]["ProductCategory"].ToString();

                    ROW++;
                    sheet[ROW, colCostingId + 1].Text = dtOrderCostingProductInfo.Rows[i]["Id"].ToString();

                    COL = 8;
                    ROW = StartRow;
                    sheet[ROW, colProdSubCategory + 1].Text = dtOrderCostingProductInfo.Rows[i]["ProductSubCategory"].ToString();
                    //ROW++;
                    //sheet[ROW, colCostingStage + 1].Text = dtOrderCostingProductInfo.Rows[i]["CostingStage"].ToString();

                    ROW++;
                    ROW++;

                    COL = 2;
                    StartRow = ROW;
                    sheet[ROW, colCode + 1].Text = dtOrderCostingProductInfo.Rows[i]["Code"].ToString();
                    ROW++;
                    sheet[ROW, colShortName + 1].Text = dtOrderCostingProductInfo.Rows[i]["ShortName"].ToString();
                    ROW++;
                    sheet[ROW, colMktTgtSPT + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["TargetOrSPT"].ToString());
                    ROW++;
                    sheet[ROW, colSPT + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["SPT"].ToString());
                    ROW++;
                    sheet[ROW, colStandardPlanHours + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["StandardWorkingHours"].ToString());
                    ROW = StartRow;
                    COL = 5;
                    sheet[ROW, colUserName + 1].Text = dtOrderCostingProductInfo.Rows[i]["UserName"].ToString();
                    ROW++;
                    sheet[ROW, colStandardName + 1].Text = dtOrderCostingProductInfo.Rows[i]["StandardName"].ToString();
                    ROW++;
                    sheet[ROW, colTargetHour + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["MKTTargetPerHour"].ToString());
                    ROW++;
                    sheet[ROW, colEfficiency + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["EfficiencyPercentage"].ToString());

                    COL = 8;
                    ROW = StartRow;
                    sheet[ROW, colDescription + 1].Text = dtOrderCostingProductInfo.Rows[i]["Description"].ToString();
                    ROW++;
                    sheet[ROW, colNoOfWS + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["NoOfWorkstation"].ToString());
                    ROW++;
                    sheet[ROW, colWCTargetDay + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["WorkCenterTargetPerDay"].ToString());

                    ROW = 17;
                    COL = 2;
                    StartRow = ROW;
                    sheet[ROW, colPrdAvlDays + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["ProductionAvailableDays"].ToString());
                    ROW++;
                    sheet[ROW, colExcess + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["ExcessShipmentPer"].ToString());
                    ROW++;
                    sheet[ROW, colCriticalLevel + 1].Text = dtOrderCostingProductInfo.Rows[i]["CriticalLevel"].ToString();
                    ROW++;
                    sheet[ROW, colPackingType + 1].Text = dtOrderCostingProductInfo.Rows[i]["PackingType"].ToString();
                    ROW++;
                    sheet[ROW, colTgtSelPrice + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["TargetSellingPrice"].ToString());
                    ROW = StartRow;
                    COL = 5;
                    sheet[ROW, colSpecificTo + 1].Text = dtOrderCostingProductInfo.Rows[i]["SpecifyTo"].ToString();
                    ROW++;
                    sheet[ROW, colUOM + 1].Text = dtOrderCostingProductInfo.Rows[i]["UnitOfMeasurement"].ToString();
                    ROW++;
                    sheet[ROW, colPaymentDays + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["PaymentDays"].ToString());
                    ROW++;
                    sheet[ROW, colOrderSize + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["OrderSize"].ToString());

                    COL = 8;
                    ROW = StartRow;
                    sheet[ROW, colTargetCM + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["TargetCM"].ToString());
                    ROW++;
                    sheet[ROW, colEstNoOfPagList + 1].Number = clsStaticInfo.dbl(dtOrderCostingProductInfo.Rows[i]["EstNoOfPackingList"].ToString());
                    ROW++;
                    sheet[ROW, colRemarks + 1].Text = dtOrderCostingProductInfo.Rows[i]["Remarks"].ToString();
                    ROW++;
                    sheet[ROW, colCurrency + 1].Text = dtOrderCostingProductInfo.Rows[i]["Currency"].ToString();



                }
                ROW = 7;
                sheet.Range[ROW, colProductMaster + 1, ROW, colProductMaster + 2].Merge();
                sheet.Range[ROW, colProdCategory + 1, ROW, colProdCategory + 2].Merge();
                sheet.Range[ROW, colProdSubCategory + 1, ROW, colProdSubCategory + 2].Merge();
                //ROW++;
                //sheet.Range[ROW, colCostingStage + 1, ROW, colCostingStage + 2].Merge();
                //sheet.Range[ROW, colCostingId + 1, ROW, colCostingId + 2].Merge();
                //sheet.Range[ROW, colCostType + 1, ROW, colCostType + 2].Merge();

                ROW = 10;
                sheet.Range[ROW, colCode + 1, ROW, colCode + 2].Merge();
                sheet.Range[ROW, colUserName + 1, ROW, colUserName + 2].Merge();
                sheet.Range[ROW, colDescription + 1, ROW, colDescription + 2].Merge();
                ROW++;
                sheet.Range[ROW, colShortName + 1, ROW, colShortName + 2].Merge();
                sheet.Range[ROW, colStandardName + 1, ROW, colStandardName + 2].Merge();
                sheet.Range[ROW, colNoOfWS + 1, ROW, colNoOfWS + 2].Merge();
                ROW++;
                sheet.Range[ROW, colMktTgtSPT + 1, ROW, colMktTgtSPT + 2].Merge();
                sheet.Range[ROW, colTargetHour + 1, ROW, colTargetHour + 2].Merge();
                sheet.Range[ROW, colWCTargetDay + 1, ROW, colWCTargetDay + 2].Merge();
                ROW++;
                sheet.Range[ROW, colSPT + 1, ROW, colSPT + 2].Merge();
                sheet.Range[ROW, colEfficiency + 1, ROW, colEfficiency + 2].Merge();
                ROW++;
                sheet.Range[ROW, colStandardPlanHours + 1, ROW, colStandardPlanHours + 2].Merge();

                ROW = 17;
                sheet.Range[ROW, colPrdAvlDays + 1, ROW, colPrdAvlDays + 2].Merge();
                sheet.Range[ROW, colSpecificTo + 1, ROW, colSpecificTo + 2].Merge();
                sheet.Range[ROW, colTargetCM + 1, ROW, colTargetCM + 2].Merge();
                ROW++;
                sheet.Range[ROW, colExcess + 1, ROW, colExcess + 2].Merge();
                sheet.Range[ROW, colUOM + 1, ROW, colUOM + 2].Merge();
                sheet.Range[ROW, colEstNoOfPagList + 1, ROW, colEstNoOfPagList + 2].Merge();
                ROW++;
                sheet.Range[ROW, colCriticalLevel + 1, ROW, colCriticalLevel + 2].Merge();
                sheet.Range[ROW, colPaymentDays + 1, ROW, colPaymentDays + 2].Merge();
                sheet.Range[ROW, colRemarks + 1, ROW, colRemarks + 2].Merge();
                ROW++;
                sheet.Range[ROW, colPackingType + 1, ROW, colPackingType + 2].Merge();
                sheet.Range[ROW, colOrderSize + 1, ROW, colOrderSize + 2].Merge();
                sheet.Range[ROW, colCurrency + 1, ROW, colCurrency + 2].Merge();
                ROW++;
                sheet.Range[ROW, colTgtSelPrice + 1, ROW, colTgtSelPrice + 2].Merge();

                sheet.Range[7, 1, 21, endCol].NumberFormat = clsStaticInfo.NumberFormat(2);

                DataTable dtCostingDetailInfo = _sqlRepository.GetDataTable(CostingDetailsql);

                ROW = 23;
                COL = 1;
                #region Costing Detail
                sheet[ROW, COL].Text = "Costing summary";
                sheet[ROW, COL].RowHeight = 25;
                sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
                sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, COL, ROW, 7].Merge();
                ROW++;
                ROW++;



                sheet[ROW, COL].Text = "Sl No.";
                sheet[ROW, COL].ColumnWidth = 5;
                int colSlNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Costing Component";
                sheet[ROW, COL].ColumnWidth = 12;
                int colCostingComponent = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Costing(A)";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colBuyerCosting = COL;
                COL++;
                sheet[ROW, COL].Text = "Quick Costing(B)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colQuickCosting = COL;
                COL++;
                sheet[ROW, COL].Text = "Pre Costing(C)";
                sheet[ROW, COL].ColumnWidth = 20;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPreCosting = COL;



                int CostingDetailEndCol = COL;
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                //sheet.Range[ROW, 1, ROW, CostingDetailEndCol].CellStyle.Font.Color = ExcelKnownColors.White;
                //sheet.Range[ROW - 2, 1, ROW - 2, CostingDetailEndCol].Merge();

                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderInside(ExcelLineStyle.Hair);
                //ROW++;

                int CostingDetailStartRow = ROW; //row 20
                for (int i = 0; i < dtCostingDetailInfo.Rows.Count; i++)
                {
                    sheet[ROW, colSlNo].Number = (i + 1);

                    sheet[ROW, colCostingComponent].Text = dtCostingDetailInfo.Rows[i]["UserName"].ToString();
                    sheet[ROW, colBuyerCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["BuyerTarget"].ToString());
                    sheet[ROW, colQuickCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["CostingValue"].ToString());
                    sheet[ROW, colPreCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalGrossAmount"].ToString());


                    sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderInside(ExcelLineStyle.Hair);


                    ROW++;

                }
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;


                sheet.Range[ROW, 1, ROW, 2].Merge();
                sheet.Range[ROW, colBuyerCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colBuyerCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colBuyerCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colBuyerCosting].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colQuickCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colQuickCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colQuickCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colQuickCosting].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colPreCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colPreCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colPreCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colPreCosting].CellStyle.Font.Bold = true;


                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderInside(ExcelLineStyle.Hair);
                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[CostingDetailStartRow, 1, ROW, CostingDetailEndCol].CellStyle.Font.Size = 8f;
                sheet.Range[CostingDetailStartRow, colCostingComponent, ROW, CostingDetailEndCol].NumberFormat = clsStaticInfo.NumberFormat(2);

                #endregion

                ROW++;
                ROW++;
                COL = 1;

                ROW++;
                int CostingComponentEndcol = 0;



                PreCostingDirectMateterial(sheet, ref ROW, CostingTempleteId);
                PreCostingDirectProcess(sheet, ref ROW, CostingTempleteId);
                PreCostingOperation(sheet, ref ROW, CostingTempleteId);
                PreCostingValueLoss(sheet, ref ROW, CostingTempleteId);
                PreCostingProfit(sheet, ref ROW, CostingTempleteId);
                PreCostingSalesExpense(sheet, ref ROW, CostingTempleteId);

                //sheet.Range[34, 1, 34, CostingComponentEndcol].Merge();


                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[7, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                //sheet.Range[34, 1,34, CostingComponentEndcol].CellStyle.Font.Size = 15;


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                reportUtility.PlantHeader(ref sheet, endCol, "Costing Templete Report #" + CostingTempleteId + @"", identity.PlantId);


                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;



                string strFileName = "CostingTempleteReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PreCostingDirectMateterial(IWorksheet sheet, ref int ROW, string CostingTempleteId)
        {
            ReportUtility reportUtility = new ReportUtility();
            String CostingDirectMaterialSQL = PreCostingDirectMaterialSQL(CostingTempleteId);


            DataTable dtPreCostingDirectMaterial = _sqlRepository.GetDataTable(CostingDirectMaterialSQL);
            if (dtPreCostingDirectMaterial.Rows.Count == 0)
                return;

            //dataset for material sorted with costing component,Sequence
            DataTable dvDistinctCostingComponent = dtPreCostingDirectMaterial.DefaultView.ToTable(true, "CostingComponentId", "CostingComponentName");
            //string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

            int CostingComponentEndcol = 0;
            int COL = 1;

            sheet[ROW, COL].Text = dtPreCostingDirectMaterial.Rows[0]["CostingComponentName"].ToString() + " breakdown.";
            sheet[ROW, COL].RowHeight = 25;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 14;
            sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, COL, ROW, 12].Merge();


            for (int i = 0; i < dvDistinctCostingComponent.Rows.Count; i++)

            {
                ROW++;
                sheet[1, 1].Text = dvDistinctCostingComponent.Rows[i]["CostingComponentName"].ToString();
                ROW++;


                sheet[ROW, COL].Text = "Costing Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCostingItem = COL;
                COL += 3;


                sheet[ROW, COL].Text = "UOM";
                int colUOM2 = COL;
                COL++;

                sheet[ROW, COL].Text = "Particulars";
                sheet[ROW, COL].ColumnWidth = 10;
                int colParticulars = COL;
                COL += 2;

                sheet[ROW, COL].Text = "Consumption";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colConsumption = COL;
                COL++;

                sheet[ROW, COL].Text = "Value Loss";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colValueLoss = COL;
                COL++;

                sheet[ROW, COL].Text = "Gross Consumption";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colGrossConsumption = COL;
                COL++;

                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colRate = COL;
                COL++;

                sheet[ROW, COL].Text = "Gross Amount";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colGrossAmount = COL;
                COL++;


                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCurrency2 = COL;

                CostingComponentEndcol = COL;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                sheet.Range[ROW, colParticulars, ROW, colParticulars + 1].Merge();
                sheet.Range[ROW - 2, 1, ROW - 2, CostingComponentEndcol].Merge();
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

                dtPreCostingDirectMaterial.DefaultView.RowFilter = "CostingComponentId='" + dvDistinctCostingComponent.Rows[i]["CostingComponentId"].ToString() + "'";
                DataTable dtComponentRelatedItems = dtPreCostingDirectMaterial.DefaultView.ToTable();

                int CostingComponentStartRow = ROW;

                for (int M = 0; M < dtComponentRelatedItems.Rows.Count; M++)
                {
                    sheet[ROW, colParticulars].Text = dtComponentRelatedItems.DefaultView[M]["Particulars"].ToString();
                    sheet[ROW, colCostingItem].Text = dtComponentRelatedItems.DefaultView[M]["CostingItem"].ToString();
                    sheet[ROW, colUOM2].Text = dtComponentRelatedItems.DefaultView[M]["UOM"].ToString();
                    sheet[ROW, colConsumption].Number = Convert.ToDouble(dtComponentRelatedItems.DefaultView[M]["Consumption"].ToString());
                    sheet[ROW, colRate].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Rate"].ToString());
                    sheet[ROW, colValueLoss].Number = Convert.ToDouble(dtComponentRelatedItems.DefaultView[M]["ValueLoss"].ToString());
                    sheet[ROW, colGrossConsumption].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["GrossConsumption"].ToString());
                    sheet[ROW, colGrossAmount].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["GrossAmount"].ToString());
                    sheet[ROW, colCurrency2].Text = dtComponentRelatedItems.DefaultView[M]["Currency"].ToString();

                    sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                    sheet.Range[ROW, colParticulars, ROW, colParticulars + 1].Merge();
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }

                int CostingComponentEndRow = ROW - 1;
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCostingItem, ROW, colParticulars + 1].Merge();

                sheet.Range[ROW, colConsumption].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colConsumption) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colConsumption) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colConsumption].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colValueLoss].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colValueLoss) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colValueLoss) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colValueLoss].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colGrossConsumption].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colGrossConsumption) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colGrossConsumption) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colGrossConsumption].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colGrossAmount].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colGrossAmount) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colGrossAmount) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colGrossAmount].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCurrency2].Text = dtComponentRelatedItems.DefaultView[0]["Currency"].ToString();
                sheet.Range[ROW, colCurrency2].CellStyle.Font.Bold = true;

                sheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, CostingComponentEndcol].NumberFormat = clsStaticInfo.NumberFormat(4);

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                ROW++;

                sheet.Range[CostingComponentStartRow, colConsumption, ROW, colConsumption].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colValueLoss, ROW, colValueLoss].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colGrossConsumption, ROW, colGrossConsumption].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colGrossAmount, ROW, colGrossAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
            }

            //crate all headers ColSl, COlMaterial

        }

        private void PreCostingDirectProcess(IWorksheet sheet, ref int ROW, string CostingTempleteId)
        {
            ReportUtility reportUtility = new ReportUtility();

            //dataset for material sorted with costing component,Sequence
            String CostingDirectProcessSQL = PreCostingDirectProcessSQL(CostingTempleteId);
            DataTable dtOrderCostingDirectProcess = _sqlRepository.GetDataTable(CostingDirectProcessSQL);

            if (dtOrderCostingDirectProcess.Rows.Count == 0)
                return;

            DataTable dvDistinctCostingComponent = dtOrderCostingDirectProcess.DefaultView.ToTable(true, "CostingComponentId", "CostingComponentName");
            //string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

            int CostingComponentEndcol = 0;

            int COL = 1;

            sheet[ROW, COL].Text = dtOrderCostingDirectProcess.Rows[0]["CostingComponentName"].ToString() + " breakdown.";
            sheet[ROW, COL].RowHeight = 30;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            sheet.Range[ROW, COL, ROW, 9].Merge();
            sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;


            for (int i = 0; i < dvDistinctCostingComponent.Rows.Count; i++)
            {
                ROW++;
                sheet[1, 1].Text = dvDistinctCostingComponent.Rows[i]["CostingComponentName"].ToString();
                ROW++;

                COL = 1;

                sheet[ROW, COL].Text = "Costing Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCostingItem = COL;
                COL += 3;


                sheet[ROW, COL].Text = "Type";
                int colType = COL;
                COL += 2;

                sheet[ROW, COL].Text = "Value";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colRate = COL;
                COL++;

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 15;
                int colCurrency2 = COL;

                CostingComponentEndcol = COL;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                sheet.Range[ROW, colType, ROW, colType + 1].Merge();
                sheet.Range[ROW - 2, 1, ROW - 2, CostingComponentEndcol].Merge();
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

                dtOrderCostingDirectProcess.DefaultView.RowFilter = "CostingComponentId='" + dvDistinctCostingComponent.Rows[i]["CostingComponentId"].ToString() + "'";
                DataTable dtComponentRelatedItems = dtOrderCostingDirectProcess.DefaultView.ToTable();

                int CostingComponentStartRow = ROW;

                for (int M = 0; M < dtComponentRelatedItems.Rows.Count; M++)
                {
                    //sheet[ROW, colParticulars].Text = dtComponentRelatedItems.Rows[M]["Particulars"].ToString();
                    sheet[ROW, colCostingItem].Text = dtComponentRelatedItems.DefaultView[M]["CostingItem"].ToString();
                    sheet[ROW, colType].Text = dtComponentRelatedItems.DefaultView[M]["Type"].ToString();

                    sheet[ROW, colValue].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString());
                    sheet[ROW, colRate].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Rate"].ToString());
                    sheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Amount"].ToString());
                    sheet[ROW, colCurrency2].Text = dtComponentRelatedItems.DefaultView[M]["Currency"].ToString();

                    sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                    sheet.Range[ROW, colType, ROW, colType + 1].Merge();
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }
                int CostingComponentEndRow = ROW - 1;
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCostingItem, ROW, colType + 1].Merge();

                sheet.Range[ROW, colValue].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colValue].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colAmount].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colAmount) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colAmount) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colAmount].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCurrency2].Formula = reportUtility.GetColumnNameForXls(colCurrency2) + (ROW - 1);
                sheet.Range[ROW, colCurrency2].CellStyle.Font.Bold = true;

                sheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, CostingComponentEndcol].NumberFormat = clsStaticInfo.NumberFormat(4);

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                ROW++;

                sheet.Range[CostingComponentStartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colAmount, ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

            }

            //crate all headers ColSl, COlMaterial



        }

        private void PreCostingOperation(IWorksheet sheet, ref int ROW, string CostingTempleteId)
        {
            ReportUtility reportUtility = new ReportUtility();

            //dataset for material sorted with costing component,Sequence
            String CostingOperationSQL = PreCostingOperationSQL(CostingTempleteId);
            DataTable dtOrderCostingOperation = _sqlRepository.GetDataTable(CostingOperationSQL);

            if (dtOrderCostingOperation.Rows.Count == 0)
                return;

            DataTable dvDistinctCostingComponent = dtOrderCostingOperation.DefaultView.ToTable(true, "CostingComponentId", "CostingComponentName");
            //string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

            int CostingComponentEndcol = 0;

            int COL = 1;

            sheet[ROW, COL].Text = dtOrderCostingOperation.Rows[0]["CostingComponentName"].ToString() + " breakdown.";
            sheet[ROW, COL].RowHeight = 30;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            sheet.Range[ROW, COL, ROW, 5].Merge();
            sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;


            for (int i = 0; i < dvDistinctCostingComponent.Rows.Count; i++)
            {
                ROW++;
                sheet[1, 1].Text = dvDistinctCostingComponent.Rows[i]["CostingComponentName"].ToString();
                ROW++;

                COL = 1;

                sheet[ROW, COL].Text = "Costing Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCostingItem = COL;
                COL += 3;

                sheet[ROW, COL].Text = "Value";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCurrency = COL;

                CostingComponentEndcol = COL;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                //sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                sheet.Range[ROW - 2, 1, ROW - 2, CostingComponentEndcol].Merge();
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

                dtOrderCostingOperation.DefaultView.RowFilter = "CostingComponentId='" + dvDistinctCostingComponent.Rows[i]["CostingComponentId"].ToString() + "'";
                DataTable dtComponentRelatedItems = dtOrderCostingOperation.DefaultView.ToTable();

                int CostingComponentStartRow = ROW;

                for (int M = 0; M < dtComponentRelatedItems.Rows.Count; M++)
                {
                    sheet[ROW, colCostingItem].Text = dtComponentRelatedItems.DefaultView[M]["CostingItem"].ToString();
                    sheet[ROW, colValue].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString());
                    sheet[ROW, colCurrency].Text = dtComponentRelatedItems.DefaultView[M]["Currency"].ToString();

                    sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                    //sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }
                int CostingComponentEndRow = ROW - 1;
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                //sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                sheet.Range[ROW, colValue, ROW, colValue].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colValue].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colCurrency].Formula = reportUtility.GetColumnNameForXls(colCurrency) + (ROW - 1);
                sheet.Range[ROW, colCurrency].CellStyle.Font.Bold = true;

                sheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, CostingComponentEndcol].NumberFormat = clsStaticInfo.NumberFormat(4);

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                ROW++;

                sheet.Range[CostingComponentStartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
            }

            //crate all headers ColSl, COlMaterial

        }

        private void PreCostingValueLoss(IWorksheet sheet, ref int ROW, string CostingTempleteId)
        {
            ReportUtility reportUtility = new ReportUtility();

            //dataset for material sorted with costing component,Sequence

            String CostingValueLossSQL = PreCostingValueLossSQL(CostingTempleteId);
            DataTable dtOrderCostingValueLoss = _sqlRepository.GetDataTable(CostingValueLossSQL);

            if (dtOrderCostingValueLoss.Rows.Count == 0)
                return;

            DataTable dvDistinctCostingComponent = dtOrderCostingValueLoss.DefaultView.ToTable(true, "CostingComponentId", "CostingComponentName");
            //string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

            int CostingComponentEndcol = 0;

            int COL = 1;

            sheet[ROW, COL].Text = dtOrderCostingValueLoss.Rows[0]["CostingComponentName"].ToString() + " breakdown.";
            sheet[ROW, COL].RowHeight = 30;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            sheet.Range[ROW, COL, ROW, 8].Merge();
            sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;


            for (int i = 0; i < dvDistinctCostingComponent.Rows.Count; i++)
            {
                ROW++;
                sheet[1, 1].Text = dvDistinctCostingComponent.Rows[i]["CostingComponentName"].ToString();
                ROW++;

                sheet[ROW, COL].Text = "Costing Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCostingItem = COL;
                COL += 3;

                sheet[ROW, COL].Text = "Type";
                int colType = COL;
                COL++;

                sheet[ROW, COL].Text = "Value";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colValue = COL;
                COL += 2;

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 15;
                int colCurrency = COL;

                CostingComponentEndcol = COL;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                sheet.Range[ROW - 2, 1, ROW - 2, CostingComponentEndcol].Merge();
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

                dtOrderCostingValueLoss.DefaultView.RowFilter = "CostingComponentId='" + dvDistinctCostingComponent.Rows[i]["CostingComponentId"].ToString() + "'";
                DataTable dtComponentRelatedItems = dtOrderCostingValueLoss.DefaultView.ToTable();

                int CostingComponentStartRow = ROW;

                for (int M = 0; M < dtComponentRelatedItems.Rows.Count; M++)
                {
                    sheet[ROW, colCostingItem].Text = dtComponentRelatedItems.DefaultView[M]["CostingItem"].ToString();
                    sheet[ROW, colType].Text = dtComponentRelatedItems.DefaultView[M]["Type"].ToString();
                    sheet[ROW, colValue].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString());
                    sheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Amount"].ToString());
                    sheet[ROW, colCurrency].Text = dtComponentRelatedItems.DefaultView[M]["Currency"].ToString();

                    sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                    sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }
                int CostingComponentEndRow = ROW - 1;
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 3].Merge();

                sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();

                sheet.Range[ROW, colValue].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colValue].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colAmount].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colAmount) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colAmount) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colAmount].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colCurrency].Formula = reportUtility.GetColumnNameForXls(colCurrency) + (ROW - 1);
                sheet.Range[ROW, colCurrency].CellStyle.Font.Bold = true;

                sheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, CostingComponentEndcol].NumberFormat = clsStaticInfo.NumberFormat(4);

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                ROW++;

                sheet.Range[CostingComponentStartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colAmount, ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

            }

            //crate all headers ColSl, COlMaterial

        }

        private void PreCostingProfit(IWorksheet sheet, ref int ROW, string CostingTempleteId)
        {
            ReportUtility reportUtility = new ReportUtility();

            //dataset for material sorted with costing component,Sequence
            String CostingProfitSQL = PreCostingProfitSQL(CostingTempleteId);
            DataTable dtOrderCostingProfit = _sqlRepository.GetDataTable(CostingProfitSQL);

            if (dtOrderCostingProfit.Rows.Count == 0)
                return;

            DataTable dvDistinctCostingComponent = dtOrderCostingProfit.DefaultView.ToTable(true, "CostingComponentId", "CostingComponentName");
            //string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

            int CostingComponentEndcol = 0;

            int COL = 1;

            sheet[ROW, COL].Text = dtOrderCostingProfit.Rows[0]["CostingComponentName"].ToString() + " breakdown.";
            sheet[ROW, COL].RowHeight = 30;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            sheet.Range[ROW, COL, ROW, 8].Merge();
            sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;


            for (int i = 0; i < dvDistinctCostingComponent.Rows.Count; i++)
            {
                ROW++;
                sheet[1, 1].Text = dvDistinctCostingComponent.Rows[i]["CostingComponentName"].ToString();
                ROW++;

                COL = 1;

                sheet[ROW, COL].Text = "Costing Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCostingItem = COL;
                COL += 3;


                sheet[ROW, COL].Text = "Type";
                int colType = COL;
                COL++;

                sheet[ROW, COL].Text = "Value";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colValue = COL;
                COL += 2;

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 15;
                int colCurrency = COL;

                CostingComponentEndcol = COL;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                sheet.Range[ROW - 2, 1, ROW - 2, CostingComponentEndcol].Merge();
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

                dtOrderCostingProfit.DefaultView.RowFilter = "CostingComponentId='" + dvDistinctCostingComponent.Rows[i]["CostingComponentId"].ToString() + "'";
                DataTable dtComponentRelatedItems = dtOrderCostingProfit.DefaultView.ToTable();

                int CostingComponentStartRow = ROW;

                for (int M = 0; M < dtComponentRelatedItems.Rows.Count; M++)
                {
                    sheet[ROW, colCostingItem].Text = dtComponentRelatedItems.DefaultView[M]["CostingItem"].ToString();
                    sheet[ROW, colType].Text = dtComponentRelatedItems.DefaultView[M]["Type"].ToString();
                    sheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Amount"].ToString());
                    sheet[ROW, colValue].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString());
                    sheet[ROW, colCurrency].Text = dtComponentRelatedItems.DefaultView[M]["Currency"].ToString();

                    sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                    sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }
                int CostingComponentEndRow = ROW - 1;
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 3].Merge();
                sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();

                sheet.Range[ROW, colValue].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colValue].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colAmount].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colAmount) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colAmount) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colAmount].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colCurrency].Formula = reportUtility.GetColumnNameForXls(colCurrency) + (ROW - 1);
                sheet.Range[ROW, colCurrency].CellStyle.Font.Bold = true;

                sheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, CostingComponentEndcol].NumberFormat = clsStaticInfo.NumberFormat(4);

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                ROW++;

                sheet.Range[CostingComponentStartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colAmount, ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
            }

            //crate all headers ColSl, COlMaterial

        }

        private void PreCostingSalesExpense(IWorksheet sheet, ref int ROW, string CostingTempleteId)
        {
            ReportUtility reportUtility = new ReportUtility();

            //dataset for material sorted with costing component,Sequence
            String CostingSalesExpenseSQL = PreCostingSalesExpenseSQL(CostingTempleteId);
            DataTable dtOrderCostingSalesExpense = _sqlRepository.GetDataTable(CostingSalesExpenseSQL);

            if (dtOrderCostingSalesExpense.Rows.Count == 0)
                return;

            DataTable dvDistinctCostingComponent = dtOrderCostingSalesExpense.DefaultView.ToTable(true, "CostingComponentId", "CostingComponentName");
            //string OrderQTY = clsStaticInfo.dbl(dtMOICostingInfo.DefaultView[0]["OrderQty"].ToString()).ToString();

            int CostingComponentEndcol = 0;

            int COL = 1;

            sheet[ROW, COL].Text = dtOrderCostingSalesExpense.Rows[0]["CostingComponentName"].ToString() + " breakdown.";
            sheet[ROW, COL].RowHeight = 30;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            sheet.Range[ROW, COL, ROW, 8].Merge();
            sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;



            for (int i = 0; i < dvDistinctCostingComponent.Rows.Count; i++)
            {
                ROW++;
                sheet[1, 1].Text = dvDistinctCostingComponent.Rows[i]["CostingComponentName"].ToString();
                ROW++;

                COL = 1;


                sheet[ROW, COL].Text = "Costing Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCostingItem = COL;
                COL += 3;


                sheet[ROW, COL].Text = "Type";
                int colType = COL;
                COL++;

                sheet[ROW, COL].Text = "Value";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colValue = COL;
                COL += 2;

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 15;
                int colCurrency = COL;

                CostingComponentEndcol = COL;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                sheet.Range[ROW - 2, 1, ROW - 2, CostingComponentEndcol].Merge();
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

                dtOrderCostingSalesExpense.DefaultView.RowFilter = "CostingComponentId='" + dvDistinctCostingComponent.Rows[i]["CostingComponentId"].ToString() + "'";
                DataTable dtComponentRelatedItems = dtOrderCostingSalesExpense.DefaultView.ToTable();

                int CostingComponentStartRow = ROW;

                for (int M = 0; M < dtComponentRelatedItems.Rows.Count; M++)
                {
                    //sheet[ROW, colParticulars].Text = dtComponentRelatedItems.Rows[M]["Particulars"].ToString();
                    sheet[ROW, colCostingItem].Text = dtComponentRelatedItems.DefaultView[M]["CostingItem"].ToString();
                    sheet[ROW, colType].Text = dtComponentRelatedItems.DefaultView[M]["Type"].ToString();
                    sheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Amount"].ToString());
                    sheet[ROW, colValue].Number = clsStaticInfo.dbl(dtComponentRelatedItems.DefaultView[M]["Value"].ToString());
                    sheet[ROW, colCurrency].Text = dtComponentRelatedItems.DefaultView[M]["Currency"].ToString();

                    sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                    sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }
                int CostingComponentEndRow = ROW - 1;
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 3].Merge();
                sheet.Range[ROW, colValue, ROW, colValue + 1].Merge();

                sheet.Range[ROW, colValue].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colValue) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colValue].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colAmount].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colAmount) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colAmount) + CostingComponentEndRow + ")";
                sheet.Range[ROW, colAmount].CellStyle.Font.Bold = true;
                sheet.Range[ROW, colCurrency].Formula = reportUtility.GetColumnNameForXls(colCurrency) + (ROW - 1);
                sheet.Range[ROW, colCurrency].CellStyle.Font.Bold = true;

                sheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, CostingComponentEndcol].NumberFormat = clsStaticInfo.NumberFormat(4);

                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                ROW++;

                sheet.Range[CostingComponentStartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[CostingComponentStartRow, colAmount, ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
            }

            //crate all headers ColSl, COlMaterial



        }


        private string PreCostingProductInfoSQL(string CostingTempleteId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory
							,psc.UserName as ProductSubCategory,ct.UserName AS CostingTypeName
                             ,pm.CostingType,eff.StandardWorkingHours AS StandardWorkingHoursForProduct
							,c.Code Currency,u.UserName UnitOfMeasurement
							from CostingMasterTemplate qcm 
							left outer join SCS.Currency c on c.Id=qcm.CurrencyId
							left join SCS.UnitOfMeasurement u on u.Id=qcm.UOM
                            left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT JOIN [TRN].[ProductMasterEfficency] EFF ON eff.ProductMasterId=qcm.ProductMasterId AND EfficencyName='Costing'  
							LEFT OUTER JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType
                            WHERE QCM.ID='" + CostingTempleteId + @"'";

        }

        private string PreCostingProductDetailSQL(string CostingTempleteId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"  select isnull(d.id,'New') isNewId, case when isnull(d.Id,'')<>'' THEN isnull(TEMPLATE.CostingComponentId,'DELETE') ELSE '' END AS isToBeDeleted,
                         d.Id
                        ,0 as Status,CC.CalculationMethod
	                    ,d.CostingValue
	                    ,d.BuyerTarget
	                    --,d.CostingVersionMasterTemplateId
                        ,cc.Id as CostingComponentId
	                    ,cc.Code
	                    ,cc.ShortName
	                    ,cc.UserName
                        ,ctc.Sequence
	                    ,cc.StandardName
	                    ,ctc.CostingType
                        ,cc.CostingSegment
                        ,isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount 

                        ,CC.ProcurementCostingSavingsPercentage, CC.PreCostingSavingsPercentage
						 from hkp.CostingComponent CC
                        left outer join [dbo].[CostingTypeComponent] AS ctc  ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = (select ProductMasterId from CostingMasterTemplate  where id='" + CostingTempleteId + @"'))
                        left outer join CostingDetailTemplate D on cc.id=d.CostingComponentId and d.CostingMasterTemplateId='" + CostingTempleteId + @"'
                         LEFT OUTER JOIN ( SELECT i.CostingComponentId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM PreCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=    '" + CostingTempleteId + @"' GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM PreCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + CostingTempleteId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.[Value]) AS TotalGrossAmount FROM PreCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId='" + CostingTempleteId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM PreCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + CostingTempleteId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM PreCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + CostingTempleteId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM PreCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + CostingTempleteId + @"'	GROUP BY i.CostingComponentId
                                  )AS ITEMVAL ON  itemval.CostingComponentId=d.CostingComponentId
                       
                        left outer join  (
                        select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = (select ProductMasterId from CostingMasterTemplate  where id='" + CostingTempleteId + @"'))) AS TEMPLATE 
					    on template.CostingComponentId=d.CostingComponentId


                        where   cc.Id IN (
                            select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = (select ProductMasterId from CostingMasterTemplate  where id='" + CostingTempleteId + @"'))

					    UNION

					    select CostingComponentId from CostingDetailTemplate where  ISNULL(CostingMasterTemplateId,'')='" + CostingTempleteId + @"'

					--union

					--select CostingComponentId from CostingVersionDetailTemplate where  ISNULL(CostingMasterTemplateId,'')= '" + CostingTempleteId + @"'
                    )  order by isnull(ctc.Sequence,999999),cc.Description";
        }

        private string PreCostingDirectMaterialSQL(string CostingTempleteId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;



            return @"SELECT pc.Id,I.Id as CostingId,pc.Sequence,UOM.Code as UOM,pc.Particulars,I.UserName as CostingItem,I.CostingComponentId
					,CC.CostingSegment,cc.UserName as CostingComponentName,ISNULL(pc.Consumption,0) AS Consumption,ISNULL(pc.Rate,0) AS Rate
					,ISNULL(pc.ValueLoss,0) AS ValueLoss
					,ISNULL(pc.GrossConsumption,0) AS GrossConsumption,ISNULL(pc.GrossAmount,0) AS GrossAmount
					,C.Code as Currency,OCMT.Id as OrderCostingMasterTemplateId,
					pc.SourcingType,MM.UserName as Material,MMA.StandardName as Article
					,pc.MinimumOfQuantity

					FROM PreCostingDirectMaterial AS pc  
					LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
					LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
                    LEFT JOIN SCS.UnitOfMeasurement as UOM on UOM.Id=I.UnitOfMeasurementId
					LEFT JOIN CostingMasterTemplate OCMT on OCMT.Id=PC.CostingMasterTemplateId
					LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId
					LEFT JOIN MST.MaterialMasterArticle MMA on MMA.Id=pc.ArticleId
					LEFT JOIN MST.MaterialMaster MM on MM.Id=pc.MaterialMasterId

					
					where pc.CostingMasterTemplateId='" + CostingTempleteId + @"' and I.CostingComponentId is not null
					order by pc.Sequence";


        }

        private string PreCostingDirectProcessSQL(string CostingTempleteId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return @"SELECT pc.Id,I.Id as CostingId,pc.Sequence,I.UserName as CostingItem,I.CostingComponentId
            ,ISNULL(pc.ExecutionType,'Fixed') as [Type]
			,OCMT.Id as OrderCostingMasterTemplateId,cc.UserName as CostingComponentName
			,ISNULL(pc.Value,0) AS Value,ISNULL(pc.Rate,0) AS Rate,ISNULL(pc.Amount,0) AS Amount
			,C.Code as Currency
			
			
			FROM PreCostingDirectProcess AS pc   
			LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId 
			LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
			LEFT JOIN CostingMasterTemplate OCMT on OCMT.Id=PC.CostingMasterTemplateId
			LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId
            
            where pc.CostingMasterTemplateId='" + CostingTempleteId + @"' 
					order by pc.Sequence";


        }

        private string PreCostingOperationSQL(string CostingTempleteId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            return @"SELECT pc.Id,I.Id as CostingId,I.UserName as CostingItem,I.CostingComponentId,pc.Sequence
				,ISNULL(pc.Value,0) AS Value,OCMT.Id as OrderCostingMasterTemplateId
				,cc.UserName as CostingComponentName,c.Code as Currency 
				
				
				FROM PreCostingOperation AS pc       
				LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId 
				LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
				LEFT JOIN CostingMasterTemplate OCMT on OCMT.Id=PC.CostingMasterTemplateId 
				LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId

                where pc.CostingMasterTemplateId='" + CostingTempleteId + @"' 
					order by pc.Sequence";


        }

        private string PreCostingValueLossSQL(string CostingTempleteId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            return @"SELECT pc.Id,I.Id as CostingId,pc.Sequence,I.UserName as CostingItem,I.CostingComponentId
            ,ISNULL(pc.Type,'Fixed') as [Type]
			,OCMT.Id as OrderCostingMasterTemplateId,cc.UserName as CostingComponentName
			,ISNULL(pc.Value,0) AS Value,ISNULL(pc.Amount,0) AS Amount
			,C.Code as Currency
			
			
			FROM PreCostingValueLoss AS pc   
			LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId 
			LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
			LEFT JOIN CostingMasterTemplate OCMT on OCMT.Id=PC.CostingMasterTemplateId
			LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId
			
			where pc.CostingMasterTemplateId='" + CostingTempleteId + @"'
			order by pc.Sequence";

        }

        private string PreCostingProfitSQL(string CostingTempleteId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            return @"SELECT		 pc.Id,I.Id as CostingId,pc.Sequence,I.UserName as CostingItem,I.CostingComponentId
			,ISNULL(pc.Type,'Fixed') as [Type],ISNULL(pc.Value,0) AS Value,ISNULL(pc.Amount,0) AS Amount,C.Code as Currency
			,OCMT.CurrencyId,PC.CostingMasterTemplateId,cc.UserName as CostingComponentName  

			FROM PreCostingProfit AS pc 
			LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
			LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
			LEFT JOIN CostingMasterTemplate OCMT on OCMT.Id=PC.CostingMasterTemplateId 
			LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId 
			
			
			where pc.CostingMasterTemplateId='" + CostingTempleteId + @"'
			order by pc.Sequence";


        }


        private string PreCostingSalesExpenseSQL(string CostingTempleteId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return @"SELECT pc.Id,I.Id as CostingId,pc.Sequence,I.UserName as CostingItem,I.CostingComponentId
		,ISNULL(pc.Type,'Fixed') as [Type],ISNULL(pc.Value,0) AS Value,ISNULL(pc.Amount,0) AS Amount,C.Code as Currency
		,cc.UserName as CostingComponentName

		FROM PreCostingSalesExpense AS pc    
		LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
		LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
		LEFT JOIN CostingMasterTemplate OCMT on OCMT.Id=PC.CostingMasterTemplateId 
		LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId

		where pc.CostingMasterTemplateId='" + CostingTempleteId + @"'
		order by pc.Sequence";

        }

        private string OrderInformationSQL(string OrderCostingId)
        {
            return @"select 
								OrderQty=(select sum(moi.TotalQty) OrderQty from   trn.MasterOrderItem MOI 
								                             where moi.OrderCostingMasterTemplateId=qcm.Id group by moi.OrderCostingMasterTemplateId)
								,MasterOrderItemNo=STUFF((select distinct ','+moi.Id from   trn.MasterOrderItem MOI 
								                             where moi.OrderCostingMasterTemplateId=qcm.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,MasterOrderNo=STUFF((select distinct ','+moi.MasterOrderId from   trn.MasterOrderItem MOI 
								                             where moi.OrderCostingMasterTemplateId=qcm.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,StyleNo=STUFF((select distinct ','+moi.BuyerReferenceNo from   trn.MasterOrderItem MOI 
								                             where moi.OrderCostingMasterTemplateId=qcm.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,OwnReferenceNo=STUFF((select distinct ','+moi.OwnReferenceNo from   trn.MasterOrderItem MOI 
								                             where moi.OrderCostingMasterTemplateId=qcm.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
															 ,Material=STUFF((select distinct ','+mm.UserName from  mst.MaterialMaster mm
															left join trn.MasterOrderItem moi on mm.Id=moi.MaterialMasterId
								                             where moi.OrderCostingMasterTemplateId=qcm.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	
								,Article=STUFF((select distinct ','+mma.StandardName from  mst.MaterialMasterArticle mma
															left join trn.MasterOrderItem moi on mma.Id=moi.ArticleId
								                             where moi.OrderCostingMasterTemplateId=qcm.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+c.ContractNo from  trn.SalesOrder so
															left join trn.MasterOrderItem moi on so.MasterOrderItemId=moi.Id
															left join Contract c on c.Id=so.ContractId
								                             where moi.OrderCostingMasterTemplateId=qcm.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	
								,Customer=STUFF((select distinct ','+p.UserName from   trn.MasterOrderItem MOI 
															left join trn.MasterOrder mo on mo.Id=MOI.MasterOrderId
															left join HKP.Party p on p.Id=mo.PartyId
								                             where moi.OrderCostingMasterTemplateId=qcm.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,Buyer=STUFF((select distinct ','+b.UserName from   trn.MasterOrderItem MOI 
															left join trn.MasterOrder mo on mo.Id=MOI.MasterOrderId
															left join HKP.Buyer b on B.Id=mo.BuyerId
								                             where moi.OrderCostingMasterTemplateId=qcm.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								from OrderCostingMasterTemplate qcm

                                WHERE isnull(qcm.Id,'')= (" + OrderCostingId + @")";


        }

    }
    #endregion
    #endregion
}
