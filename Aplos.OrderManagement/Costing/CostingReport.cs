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


        public void Report()
        {
            try
            {

                string sql = QuickBOQSql();
                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Quick BOQ";

                DataTable dtQuickBOQReport = _sqlRepository.GetDataTable(sql);



                int ROW = 6;
                int COL = 1;


                sheet[ROW, COL].Text = "Sl No.";

                sheet[ROW, COL].ColumnWidth = 6;
                int colSlNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Customer";

                sheet[ROW, COL].ColumnWidth = 20;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order No.";

                sheet[ROW, COL].ColumnWidth = 10;
                int colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "MasterOrder Creation Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colMasterOrderCreationDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order Item Id";
                sheet[ROW, COL].ColumnWidth = 20;
                int colMasterOrderItemId = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Item Description	";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colBuyerItemDescription = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Reference No.";
                sheet[ROW, COL].ColumnWidth = 5;
                int colBuyerReferenceNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Reference No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOwnReferenceNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "FG Material";
                sheet[ROW, COL].ColumnWidth = 10;
                int colMaterialMaster = COL;
                COL++;
                sheet[ROW, COL].Text = "FG Article";
                sheet[ROW, COL].ColumnWidth = 15;
                int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Item Qty";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colItemQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Grouping";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionGrouping = COL;

                COL++;
                sheet[ROW, COL].Text = "SO Id";
               
                sheet[ROW, COL].ColumnWidth = 10;
                int colSOId = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Delivery Date";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colSODeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Raw Material";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBOQMaterialMaster = COL;
                COL++;
                sheet[ROW, COL].Text = "Raw Article";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBOQArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Costing Item";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCostingItem = COL;
                COL++;
                sheet[ROW, COL].Text = "UOM";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCode = COL;
                COL++;
                sheet[ROW, COL].Text = "Material Cost Per Unit";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colMaterialCostPerUnit = COL;
                COL++;
                sheet[ROW, COL].Text = "Net Consumption Per Unit";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colNetConsumptionPerUnit = COL;
                COL++;
                sheet[ROW, COL].Text = "Value Loss Percentage";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colValueLossPercentage = COL;
                COL++;
                sheet[ROW, COL].Text = "Gross Consumption";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
               
                int colGrossConsumption = COL;



                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                int StartRow = ROW; //row 20
                for (int i = 0; i < dtQuickBOQReport.Rows.Count; i++)
                {


                    sheet[ROW, colSlNo].Number = (i + 1);

                    sheet[ROW, colCustomer].Text = dtQuickBOQReport.Rows[i]["Customer"].ToString();
                    sheet[ROW, colMasterOrderNo].Text = dtQuickBOQReport.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, colMasterOrderCreationDate].Text = dtQuickBOQReport.Rows[i]["MasterOrderCreationDate"].ToString();
                    sheet[ROW, colMasterOrderItemId].Text = dtQuickBOQReport.Rows[i]["MasterOrderItemId"].ToString();
                    sheet[ROW, colBuyerItemDescription].Text = dtQuickBOQReport.Rows[i]["BuyerItemDescription"].ToString();
                    sheet[ROW, colBuyerReferenceNo].Text = dtQuickBOQReport.Rows[i]["BuyerReferenceNo"].ToString();
                    sheet[ROW, colOwnReferenceNo].Text = dtQuickBOQReport.Rows[i]["OwnReferenceNo"].ToString();
                    sheet[ROW, colProduct].Text = dtQuickBOQReport.Rows[i]["Product"].ToString();
                    sheet[ROW, colMaterialMaster].Text = dtQuickBOQReport.Rows[i]["MaterialMaster"].ToString();
                    sheet[ROW, colArticle].Text = dtQuickBOQReport.Rows[i]["Article"].ToString();
                    sheet[ROW, colItemQty].Number = clsStaticInfo.dbl(dtQuickBOQReport.Rows[i]["ItemQty"].ToString());
                    sheet[ROW, colProductionGrouping].Text = dtQuickBOQReport.Rows[i]["ProductionGrouping"].ToString();
                    sheet[ROW, colSOId].Text = dtQuickBOQReport.Rows[i]["SOId"].ToString();
                    sheet[ROW, colSODeliveryDate].Text = dtQuickBOQReport.Rows[i]["SODeliveryDate"].ToString();
                    sheet[ROW, colQty].Number =clsStaticInfo.dbl( dtQuickBOQReport.Rows[i]["Qty"].ToString());
                    sheet[ROW, colBOQMaterialMaster].Text = dtQuickBOQReport.Rows[i]["BOQMaterialMaster"].ToString();
                    sheet[ROW, colBOQArticle].Text = dtQuickBOQReport.Rows[i]["BOQArticle"].ToString();
                    sheet[ROW, colCostingItem].Text = dtQuickBOQReport.Rows[i]["CostingItem"].ToString();
                    sheet[ROW, colCode].Text = dtQuickBOQReport.Rows[i]["Code"].ToString();
                    sheet[ROW, colMaterialCostPerUnit].Number =clsStaticInfo.dbl( dtQuickBOQReport.Rows[i]["MaterialCostPerUnit"].ToString());
                    sheet[ROW, colNetConsumptionPerUnit].Number = clsStaticInfo.dbl(dtQuickBOQReport.Rows[i]["NetConsumptionPerUnit"].ToString());
                    sheet[ROW, colValueLossPercentage].Number = clsStaticInfo.dbl(dtQuickBOQReport.Rows[i]["ValueLossPercentage"].ToString());
                    sheet[ROW, colGrossConsumption].Number = clsStaticInfo.dbl(dtQuickBOQReport.Rows[i]["GrossConsumption"].ToString());



                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }
                sheet.Range[StartRow, colMaterialCostPerUnit,  ROW, endCol].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colNetConsumptionPerUnit, ROW, endCol].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colValueLossPercentage, ROW, endCol].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colGrossConsumption, ROW,endCol ].NumberFormat = clsStaticInfo.NumberFormat(2);
            
                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();

                

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Quick BOQ Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "QuickBOQ.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }



        private string QuickBOQSql()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @" 
SELECT P.UserName Customer,MO.MasterOrderNo,FORMAT(MO.AddedDate,'dd-MMM-yyyy') MasterOrderCreationDate,MOI.Id MasterOrderItemId
,MOI.BuyerItemDescription,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,PM.UserName Product,MM.UserName MaterialMaster, ART.StandardName Article
,MOI.TotalQty ItemQty,MOI.ProductionGrouping,SO.Id SOId, FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') SODeliveryDate,SO.Qty
,BMM.UserName BOQMaterialMaster, BART.StandardName BOQArticle,CI.UserName CostingItem,uom.Code,BOQ.MaterialCostPerUnit,
BOQ.NetConsumptionPerUnit,BOQ.ValueLossPercentage,BOQ.GrossConsumption
FROM TRN.MasterOrderItem MOI
LEFT JOIN TRN.MasterOrder MO ON MO.Id=MOI.MasterOrderId
LEFT JOIN HKP.Party P ON P.Id=MO.PartyId
JOIN MST.MaterialMaster AS MM ON MOI.MaterialMasterId=MM.Id
LEFT JOIN MST.MaterialMasterArticle AS ART ON MOI.ArticleId=ART.Id
LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId= MM.Id
LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id

JOIN dbo.QuickBOQ AS BOQ ON BOQ.MasterOrderItemId=MOI.Id
LEFT JOIN MST.MaterialMaster AS BMM ON BOQ.MaterialMasterId=BMM.Id
LEFT JOIN MST.MaterialMasterArticle AS BART ON BOQ.ArticleId=BART.Id
LEFT JOIN [HKP].[CostingItem] CI ON CI.Id=BOQ.CostingItemId
LEFT JOIN [SCS].[UnitOfMeasurement] uom ON Uom.Id=BOQ.UoMId
LEFT JOIN TRN.SalesOrder SO ON MOI.Id=SO.MasterOrderItemId
WHERE MO.OrderStatusId='Active' 
 
ORDER BY MO.Id,MOI.Id,SO.Id

";

        }



    }
}
