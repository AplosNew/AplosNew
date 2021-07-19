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
        public void OrderCostingReport(string OrderCostingId)
        {
            try
            {
                string sql = OrderCostingProductInfoSQL(OrderCostingId);
                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Order Costing Report";

                DataTable dtOrderCostingProductInfo = _sqlRepository.GetDataTable(sql);

                int ROW = 6;
                int COL = 1;

                #region Header
                sheet[ROW, COL].Text = "Product Information";
                sheet[ROW, COL].RowHeight = 15;
                sheet.Range[ROW,COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW,COL].CellStyle.Font.Size = 10;
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                //sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
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
                sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                //sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
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
                //sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                //sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                //sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
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
                    sheet[ROW, colUOM + 1].Text = dtOrderCostingProductInfo.Rows[i]["UOM"].ToString();
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


                //sheet.Range[StartRow, colMaterialCostPerUnit, ROW, endCol].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colNetConsumptionPerUnit, ROW, endCol].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colValueLossPercentage, ROW, endCol].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colGrossConsumption, ROW, endCol].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[7, 1, 21, endCol].CellStyle.Font.Size = 8f;

                //sheet["A" + StartRow.ToString()].FreezePanes();



                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Order Costing Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

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
								,c.Code Currency
							from OrderCostingMasterTemplate qcm 
							left outer join SCS.Currency c on c.Id=qcm.CurrencyId
                            left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT JOIN [TRN].[ProductMasterEfficency] EFF ON eff.ProductMasterId=qcm.ProductMasterId AND EfficencyName='Costing'  
							LEFT OUTER JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType
                            WHERE QCM.ID='" + OrderCostingId + @"'";

        }



    }
}
