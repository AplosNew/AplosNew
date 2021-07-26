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
        public void OrderCostingReport(string OrderCostingId,string ProductMasterId, string preCosting, string ProcurementCosting)
        {
            try
            {
                string sql = OrderCostingProductInfoSQL(OrderCostingId);
                string CostingDetailsql = OrderCostingProductDetailSQL(OrderCostingId, ProductMasterId);
                String CostingComponentSql = OrderCostingComponentSQL(OrderCostingId,preCosting,ProcurementCosting);


                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;
                ReportUtility reportUtility = new ReportUtility();
                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Order Costing Report";

                DataTable dtOrderCostingProductInfo = _sqlRepository.GetDataTable(sql);
                DataTable dtOrderCostingComponent = _sqlRepository.GetDataTable(CostingComponentSql);

                int ROW = 6;
                int COL = 1;

                #region Header
                sheet[ROW, COL].Text = "Product Information";
                sheet[ROW, COL].RowHeight = 15;
                sheet.Range[ROW,COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW,COL].CellStyle.Font.Size = 10;
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
                sheet[ROW, COL].ColumnWidth = 10;
                int colProdCategory = COL;
                ROW++;
                sheet[ROW, COL].Text = "Costing Id";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCostingId = COL;
                ROW = StartRow;
                COL = 7;
                sheet[ROW, COL].Text = "Prod.Sub Cat.";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProdSubCategory = COL;
                ROW++;
                sheet[ROW, COL].Text = "Costing Stage";
                sheet[ROW, COL].ColumnWidth = 20;
                int colCostingStage = COL;
                ROW++;
                ROW++;

                COL = 1;
                StartRow = ROW;
                sheet[ROW, COL].Text = "Code";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCode= COL;
                ROW++;
                sheet[ROW, COL].Text = "Short Name";
                sheet[ROW, COL].ColumnWidth = 10;
                int colShortName= COL;
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
                sheet[ROW, COL].ColumnWidth = 10;
                int colStandardPlanHours = COL;
                ROW++;
                ROW = StartRow;
                COL = 4;
                sheet[ROW, COL].Text = "User Name";
                sheet[ROW, COL].ColumnWidth = 10;
                int colUserName = COL;
                ROW++;
                sheet[ROW, COL].Text = "Standard Name";
                sheet[ROW, COL].ColumnWidth = 10;
                int colStandardName = COL;
                ROW++;
                sheet[ROW, COL].Text = "Target / Hour";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTargetHour = COL;
                ROW++;
                sheet[ROW, COL].Text = "Efficiency %";
                sheet[ROW, COL].ColumnWidth = 10;
                int colEfficiency= COL;
                ROW++;
                ROW = StartRow;
                COL = 7;
                sheet[ROW, COL].Text = "Description";
                sheet[ROW, COL].ColumnWidth = 10;
                int colDescription = COL;
                ROW++;
                sheet[ROW, COL].Text = "No Of WS";
                sheet[ROW, COL].ColumnWidth = 10;
                int colNoOfWS = COL;
                ROW++;
                sheet[ROW, COL].Text = "WC Target / Day";
                sheet[ROW, COL].ColumnWidth = 20;
                int colWCTargetDay = COL;

                #endregion
                ROW = 16;
                COL = 1;
                #region General Information
                sheet[ROW, COL].Text = "General Information";
                sheet[ROW, COL].RowHeight = 15;
                sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, COL].CellStyle.Font.Size = 10;
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
                sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                ROW++;

                StartRow = ROW;                
                sheet[ROW, COL].Text = "Prd.Avl.Days";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPrdAvlDays= COL;
                ROW++;
                sheet[ROW, COL].Text = "Excess%";
                sheet[ROW, COL].ColumnWidth = 10;
                int colExcess= COL;
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
                int colTgtSelPrice= COL;
                ROW++;

                ROW = StartRow;
                COL = 4;
                sheet[ROW, COL].Text = "Specific To*";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSpecificTo = COL;
                ROW++;
                sheet[ROW, COL].Text = "UOM";
                sheet[ROW, COL].ColumnWidth = 10;
                int colUOM = COL;
                ROW++;
                sheet[ROW, COL].Text = "Payment Days";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPaymentDays = COL;
                ROW++;
                sheet[ROW, COL].Text = "Order Size";
                sheet[ROW, COL].ColumnWidth = 20;
                int colOrderSize= COL;
                ROW++;
                ROW = StartRow;
                COL = 7;
                sheet[ROW, COL].Text = "Target CM";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTargetCM = COL;
                ROW++;
                sheet[ROW, COL].Text = "Est.NoOf Pag List";
                sheet[ROW, COL].ColumnWidth = 10;
                int colEstNoOfPagList= COL;
                ROW++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 20;
                int colRemarks = COL;
                ROW++;
                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 20;
                int colCurrency = COL;

                #endregion



                int endCol = colWCTargetDay+2;

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
                    sheet[ROW, colProductMaster+1].Text = dtOrderCostingProductInfo.Rows[i]["ProductMaster"].ToString();
                    ROW++;
                    sheet[ROW, colCostType+1].Text = dtOrderCostingProductInfo.Rows[i]["CostingTypeName"].ToString();

                    ROW = StartRow;
                    COL = 5;
                    sheet[ROW, colProdCategory+1].Text = dtOrderCostingProductInfo.Rows[i]["ProductCategory"].ToString();

                    ROW++;
                    sheet[ROW, colCostingId+1].Text = dtOrderCostingProductInfo.Rows[i]["Id"].ToString();

                    COL = 8;
                    ROW = StartRow;
                    sheet[ROW, colProdSubCategory+1].Text = dtOrderCostingProductInfo.Rows[i]["ProductSubCategory"].ToString();
                    ROW++;
                    sheet[ROW, colCostingStage+1].Text = dtOrderCostingProductInfo.Rows[i]["CostingStage"].ToString();

                    ROW++;
                    ROW++;

                    COL = 2;
                    StartRow = ROW;
                    sheet[ROW, colCode + 1].Text = dtOrderCostingProductInfo.Rows[i]["Code"].ToString();
                    ROW++;
                    sheet[ROW, colShortName + 1].Text = dtOrderCostingProductInfo.Rows[i]["ShortName"].ToString();
                    ROW++;
                    sheet[ROW, colMktTgtSPT + 1].Text = dtOrderCostingProductInfo.Rows[i]["TargetOrSPT"].ToString();
                    ROW++;
                    sheet[ROW, colSPT + 1].Text = dtOrderCostingProductInfo.Rows[i]["SPT"].ToString();
                    ROW++;
                    sheet[ROW, colStandardPlanHours + 1].Text = dtOrderCostingProductInfo.Rows[i]["StandardWorkingHours"].ToString();
                    ROW = StartRow;
                    COL = 5;
                    sheet[ROW, colUserName + 1].Text = dtOrderCostingProductInfo.Rows[i]["UserName"].ToString();
                    ROW++;
                    sheet[ROW, colStandardName + 1].Text = dtOrderCostingProductInfo.Rows[i]["StandardName"].ToString();
                    ROW++;
                    sheet[ROW, colTargetHour + 1].Text = dtOrderCostingProductInfo.Rows[i]["MKTTargetPerHour"].ToString();
                    ROW++;
                    sheet[ROW, colEfficiency + 1].Text = dtOrderCostingProductInfo.Rows[i]["EfficiencyPercentage"].ToString();

                    COL = 8;
                    ROW = StartRow;
                    sheet[ROW, colDescription + 1].Text = dtOrderCostingProductInfo.Rows[i]["Description"].ToString();
                    ROW++;
                    sheet[ROW, colNoOfWS + 1].Text = dtOrderCostingProductInfo.Rows[i]["NoOfWorkstation"].ToString();
                    ROW++;
                    sheet[ROW, colWCTargetDay + 1].Text = dtOrderCostingProductInfo.Rows[i]["WorkCenterTargetPerDay"].ToString();

                    ROW = 17;
                    COL = 2;
                    StartRow = ROW;
                    sheet[ROW, colPrdAvlDays + 1].Text = dtOrderCostingProductInfo.Rows[i]["ProductionAvailableDays"].ToString();
                    ROW++;
                    sheet[ROW, colExcess + 1].Text = dtOrderCostingProductInfo.Rows[i]["ExcessShipmentPer"].ToString();
                    ROW++;
                    sheet[ROW, colCriticalLevel + 1].Text = dtOrderCostingProductInfo.Rows[i]["CriticalLevel"].ToString();
                    ROW++;
                    sheet[ROW, colPackingType + 1].Text = dtOrderCostingProductInfo.Rows[i]["PackingType"].ToString();
                    ROW++;
                    sheet[ROW, colTgtSelPrice + 1].Text = dtOrderCostingProductInfo.Rows[i]["TargetSellingPrice"].ToString();
                    ROW = StartRow;
                    COL = 5;
                    sheet[ROW, colSpecificTo + 1].Text = dtOrderCostingProductInfo.Rows[i]["SpecifyTo"].ToString();
                    ROW++;
                    sheet[ROW, colUOM + 1].Text = dtOrderCostingProductInfo.Rows[i]["UnitOfMeasurement"].ToString();
                    ROW++;
                    sheet[ROW, colPaymentDays + 1].Text = dtOrderCostingProductInfo.Rows[i]["PaymentDays"].ToString();
                    ROW++;
                    sheet[ROW, colOrderSize + 1].Text = dtOrderCostingProductInfo.Rows[i]["OrderSize"].ToString();

                    COL = 8;
                    ROW = StartRow;
                    sheet[ROW, colTargetCM + 1].Text = dtOrderCostingProductInfo.Rows[i]["TargetCM"].ToString();
                    ROW++;
                    sheet[ROW, colEstNoOfPagList + 1].Text = dtOrderCostingProductInfo.Rows[i]["EstNoOfPackingList"].ToString();
                    ROW++;
                    sheet[ROW, colRemarks + 1].Text = dtOrderCostingProductInfo.Rows[i]["Remarks"].ToString();
                    ROW++;
                    sheet[ROW, colCurrency + 1].Text = dtOrderCostingProductInfo.Rows[i]["Currency"].ToString();


                    //sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    //sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);



                }
                ROW = 7;
                sheet.Range[ROW, colProductMaster + 1, ROW, colProductMaster + 2].Merge();
                sheet.Range[ROW, colProdCategory + 1, ROW, colProdCategory + 2].Merge();
                sheet.Range[ROW, colProdSubCategory + 1, ROW, colProdSubCategory + 2].Merge();
                ROW++;   
                sheet.Range[ROW, colCostingStage + 1, ROW, colCostingStage + 2].Merge();
                sheet.Range[ROW, colCostingId + 1, ROW, colCostingId + 2].Merge();
                sheet.Range[ROW, colCostType + 1, ROW, colCostType + 2].Merge();

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


                DataTable dtCostingDetailInfo = _sqlRepository.GetDataTable(CostingDetailsql);

                ROW = 23;
                COL = 1;
                #region Costing Detail
                sheet[ROW, COL].Text = "Costing summary";
                sheet[ROW, COL].RowHeight = 15;
                sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, COL].CellStyle.Font.Size = 10;
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
                sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;

                ROW++;
                ROW++;



                sheet[ROW, COL].Text = "Sl No.";
                sheet[ROW, COL].ColumnWidth = 6;
                int colSlNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Costing Component";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCostingComponent = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Costing(A)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBuyerCosting = COL;
                COL++;
                sheet[ROW, COL].Text = "Quick Costing(B)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colQuickCosting= COL;
                COL++;
                sheet[ROW, COL].Text = "Pre Costing(C)";
                sheet[ROW, COL].ColumnWidth = 20;
                int colPreCosting= COL;
                COL++;
                sheet[ROW, COL].Text = "Proc. Costing(D)";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProcCosting = COL;
              
                int CostingDetailEndCol = COL;
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                //sheet.Range[ROW, 1, ROW, CostingDetailEndCol].CellStyle.Font.Color = ExcelKnownColors.White;
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
                    sheet[ROW, colQuickCosting].Number =clsStaticInfo.dbl( dtCostingDetailInfo.Rows[i]["CostingValue"].ToString());
                    sheet[ROW, colPreCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalGrossAmount"].ToString());
                    sheet[ROW, colProcCosting].Number = clsStaticInfo.dbl(dtCostingDetailInfo.Rows[i]["TotalProcurementGrossAmount"].ToString());
                    sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderInside(ExcelLineStyle.Hair);


                    ROW++;

                }
                sheet[ROW, 1].Text = "Total:";
                sheet.Range[ROW, 1].CellStyle.Font.Bold = true;


                sheet.Range[ROW, 1,ROW, 2].Merge();
                sheet.Range[ROW, colBuyerCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colBuyerCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colBuyerCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colQuickCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colQuickCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colQuickCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colPreCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colPreCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colPreCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, colProcCosting].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colProcCosting) + CostingDetailStartRow + ":" + reportUtility.GetColumnNameForXls(colProcCosting) + (ROW - 1) + ")";
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, CostingDetailEndCol].BorderInside(ExcelLineStyle.Hair);
                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[CostingDetailStartRow, 1, ROW, CostingDetailEndCol].CellStyle.Font.Size = 8f;
                sheet.Range[CostingDetailStartRow, colCostingComponent, ROW, CostingDetailEndCol].NumberFormat = clsStaticInfo.NumberFormat(2);

                #endregion

                ROW++;
                ROW++;
                COL = 1;
                if (preCosting == "1")
                {
                    sheet[ROW, COL].Text = "Pre Costing.";
                    sheet[ROW, COL].RowHeight = 20;
                    sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                    sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
                    sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
                    sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                }
                if (ProcurementCosting == "1")
                {
                    sheet[ROW, COL].Text = "Procurement Costing.";
                    sheet[ROW, COL].RowHeight = 20;
                    sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                    sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
                    sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
                    sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                }
                ROW++;
                int CostingComponentEndcol = 0;
                for (int i = 0; i < dtCostingDetailInfo.Rows.Count; i++)
                {

                    COL = 1;
                    dtOrderCostingComponent.DefaultView.RowFilter = "CostingComponentId='" + dtCostingDetailInfo.Rows[i]["CostingComponentId"].ToString() + "'";
                    sheet[ROW, COL].Text = dtCostingDetailInfo.Rows[i]["StandardName"].ToString() + " breakdown.";
                    sheet[ROW, COL].RowHeight = 15;
                    sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
                    sheet.Range[ROW, COL].CellStyle.Font.Size = 10;
                    sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
                    sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                    ROW++;
                    ROW++;

                    sheet[ROW, COL].Text = "Costing Item";
                    sheet[ROW, COL].ColumnWidth = 6;
                    int colCostingItem = COL;
                    COL+=3;

                    sheet[ROW, COL].Text = "Value";
                    sheet[ROW, COL].ColumnWidth = 10;
                    int colValue = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Type";
                    sheet[ROW, COL].ColumnWidth = 10;
                    int colType = COL;
                    COL+=2;
                    sheet[ROW, COL].Text = "Amount";
                    sheet[ROW, COL].ColumnWidth = 10;
                    int colAmount = COL;

                     CostingComponentEndcol = COL;
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Font.Bold = true;
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                    sheet.Range[ROW , colCostingItem, ROW, colCostingItem + 2].Merge();
                    sheet.Range[ROW , colType, ROW, colType + 1].Merge();
                    sheet.Range[ROW - 2, 1, ROW - 2, CostingComponentEndcol].Merge();
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                    int CostingComponentStartRow = ROW;
                    for (int j = 0; j < dtOrderCostingComponent.DefaultView.Count; j++)
                    {
                        //dsFromConsumptionByCosting.Tables[0].DefaultView[l]["InPutCostingItemId"].ToString()

                        sheet[ROW, colCostingItem].Text = dtOrderCostingComponent.DefaultView[j]["CostingItem"].ToString();
                        sheet[ROW, colType].Text = dtOrderCostingComponent.DefaultView[j]["ValueType"].ToString();
                        sheet[ROW, colValue].Number = clsStaticInfo.dbl(dtOrderCostingComponent.DefaultView[j]["Value"].ToString());
                        sheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtOrderCostingComponent.DefaultView[j]["TotalGrossAmount"].ToString());

                        sheet.Range[ROW, colCostingItem, ROW, colCostingItem + 2].Merge();
                        sheet.Range[ROW, colType, ROW, colType + 1].Merge();
                        sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                        ROW++;
                        
                    }
                    int CostingComponentEndRow = ROW-1;
                    sheet[ROW, 1].Text = "Total:";
                    sheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                    sheet.Range[ROW, colCostingItem, ROW, colType+1].Merge();
                    sheet.Range[ROW, colAmount].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colAmount) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(colAmount) + CostingComponentEndRow + ")";
                    sheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, CostingComponentEndcol].NumberFormat = clsStaticInfo.NumberFormat(4);

                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, CostingComponentEndcol].BorderInside(ExcelLineStyle.Hair);
                    ROW++;
                    ROW++;
                }


                sheet.Range[34, 1, 34, CostingComponentEndcol].Merge();




                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[7, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet.Range[34, 1,34, CostingComponentEndcol].CellStyle.Font.Size = 15;



                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility.PlantHeader(ref sheet, endCol, "Order Costing Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet[ROW, colProcCosting].HorizontalAlignment = ExcelHAlign.HAlignRight;


                string strFileName = "OrderCostingReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {
                throw;
            }
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

        private string OrderCostingProductDetailSQL(string OrderCostingId,string ProductMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @" select isnull(d.id,'New') isNewId, case when isnull(d.Id,'')<>'' THEN isnull(TEMPLATE.CostingComponentId,'DELETE') ELSE '' END AS isToBeDeleted,
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
                        ,CC.ProcurementCostingSavingsPercentage, CC.PreCostingSavingsPercentage
						 from hkp.CostingComponent CC
                        left outer join [dbo].[CostingTypeComponent] AS ctc  ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '"+ProductMasterId+@"')
                        left outer join OrderCostingDetailTemplate D on cc.id=d.CostingComponentId and d.OrderCostingMasterTemplateId='"+OrderCostingId+ @"'
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
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')) AS TEMPLATE 
					    on template.CostingComponentId=d.CostingComponentId


                        where   cc.Id IN (
                            select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')

					    UNION

					    select CostingComponentId from OrderCostingDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')='" + OrderCostingId + @"'

					--union

					--select CostingComponentId from CostingVersionDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')= '" + OrderCostingId + @"'
                    )  order by isnull(ctc.Sequence,999999),cc.Description";

        }

        private string OrderCostingComponentSQL(string OrderCostingId, string preCosting, string ProcurementCosting)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (preCosting == "1")
            {
                return @" SELECT  ci.Id,CC.CalculationMethod, ctc.Sequence AS ComponentSequence,ci.Sequence AS ItemSequnce, ci.CostingCategoryId, ci.CostingComponentId,ci.UserName CostingItem
,cc.CostingSegment,upper(isnull(itemval.ValueType,'FIXED')) AS ValueType,
                        isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount,isnull(itemval.Value,0) AS Value,isnull(itemval.Rate,0) AS Rate
						  from  OrderCostingDetailTemplate D 
						 INNER JOIN OrderCostingMasterTemplate AS cmt ON cmt.Id=d.OrderCostingMasterTemplateId
						 inner join hkp.CostingComponent CC on cc.id=d.CostingComponentId
						 INNER JOIN hkp.CostingItem AS ci ON ci.CostingComponentId=cc.Id
                         left outer join [dbo].[CostingTypeComponent] AS ctc  
                         ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster 
                                                                                  WHERE Id = cmt.ProductMasterId)

                         inner JOIN (SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.GrossAmount AS TotalGrossAmount FROM OrderPreCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"' 
                                            UNION ALL SELECT 'PERCENTAGE' AS ValueType, PC.Value,PC.Rate, i.Id,pc.Amount AS TotalGrossAmount FROM OrderPreCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + OrderCostingId + @"'	
                                            UNION ALL SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.[Value]  AS TotalGrossAmount FROM OrderPreCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,          pc.Amount AS TotalGrossAmount FROM OrderPreCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + OrderCostingId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM OrderPreCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + OrderCostingId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM OrderPreCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + OrderCostingId + @"'	
                                  )AS ITEMVAL ON  itemval.Id=ci.Id
                         WHERE d.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
                          order by ctc.Sequence,ci.Sequence";
            }
           else
            {
                return @"  SELECT ci.Id,CC.CalculationMethod, ctc.Sequence AS ComponentSequence,ci.Sequence AS ItemSequnce, ci.CostingCategoryId, ci.CostingComponentId,ci.UserName CostingItem
, cc.CostingSegment,upper(isnull(itemval.ValueType, 'FIXED')) AS ValueType,
                        isnull(itemval.TotalGrossAmount, 0) AS TotalGrossAmount, isnull(itemval.Value, 0) AS Value, isnull(itemval.Rate, 0) AS Rate

                          from OrderCostingDetailTemplate D

                         INNER JOIN OrderCostingMasterTemplate AS cmt ON cmt.Id = d.OrderCostingMasterTemplateId

                         inner join hkp.CostingComponent CC on cc.id = d.CostingComponentId

                         INNER JOIN hkp.CostingItem AS ci ON ci.CostingComponentId = cc.Id
                         left outer join[dbo].[CostingTypeComponent] AS ctc
                         ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster
                                                                                  WHERE Id = cmt.ProductMasterId)

                         inner JOIN(SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.GrossAmount AS TotalGrossAmount FROM OrderProcurementCostingDirectMaterial AS pc INNER JOIN HKP.CostingItem I on i.Id = PC.CostingItemId and pc.OrderCostingMasterTemplateId = '" + OrderCostingId + @"'
                                            UNION ALL SELECT 'PERCENTAGE' AS ValueType, PC.Value,PC.Rate, i.Id,pc.Amount AS TotalGrossAmount FROM OrderProcurementCostingDirectProcess AS pc INNER JOIN HKP.CostingItem I on i.Id = PC.CostingItemId and pc.OrderCostingMasterTemplateId = '" + OrderCostingId + @"'
                                            UNION ALL SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.[Value]  AS TotalGrossAmount FROM OrderProcurementCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id = PC.CostingItemId and pc.OrderCostingMasterTemplateId = '" + OrderCostingId + @"'
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate, i.Id,          pc.Amount AS TotalGrossAmount FROM OrderProcurementCostingSalesExpense AS pc INNER JOIN HKP.CostingItem I on i.Id = PC.CostingItemId and pc.OrderCostingMasterTemplateId = '" + OrderCostingId + @"'
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate, i.Id,           pc.Amount AS TotalGrossAmount FROM OrderProcurementCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id = PC.CostingItemId and pc.OrderCostingMasterTemplateId = '" + OrderCostingId + @"'
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate, i.Id,           pc.Amount AS TotalGrossAmount FROM OrderProcurementCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id = PC.CostingItemId and pc.OrderCostingMasterTemplateId = '" + OrderCostingId + @"'
                                  )AS ITEMVAL ON itemval.Id = ci.Id
                         WHERE d.OrderCostingMasterTemplateId = '" + OrderCostingId + @"'
                          order by ctc.Sequence,ci.Sequence";
            }
            

        }


    }
}
