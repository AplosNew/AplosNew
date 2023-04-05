using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.OrderManagement.Production;
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

namespace Library.Planning.PlanningType1
{

    public class ProductionDashboard
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        #region Constructor
        public ProductionDashboard()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }
        #endregion Constructor

        public void ProfitabilityReport(string PlantId, string EntityId, string date)
        {
            try
            {
                string sql = GetProfitability(PlantId, EntityId, date);
                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Profitability Report";

                DataTable dtProfitability = _sqlRepository.GetDataTable(sql);



                int ROW = 6;
                int COL = 1;


                sheet[ROW, COL].Text = "Sl No.";
                sheet[ROW, COL].ColumnWidth = 4;
                int colSlNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 9;
                int colEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "Work Center";
                sheet[ROW, COL].ColumnWidth = 10;
                int colWorkCenter = COL;
                COL++;
                sheet[ROW, COL].Text = "DailyFixedCost";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 13;
                int colDailyFixedCost = COL;

                COL++;
                sheet[ROW, COL].Text = "Additional Cost / Hour";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAdditionalCost = COL;
                COL++;
                sheet[ROW, COL].Text = "WC Hour/Day";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colWcHour = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Hour";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPlanHour = COL;
                COL++;
                sheet[ROW, COL].Text = "Total WC Cost";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalWC = COL;
                COL++;
                sheet[ROW, COL].Text = "Tgt Qty";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTgtQty = COL;
                COL++;
                sheet[ROW, COL].Text = "CM-Target";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colCMTarget = COL;
                COL++;
                sheet[ROW, COL].Text = "Revenue on Target";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colRevenueonTarget = COL;

                COL++;
                sheet[ROW, COL].Text = "Prod Qty";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProdQty = COL;
                COL++;
                sheet[ROW, COL].Text = "CM-Production";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colCMProduction = COL;
                COL++;
                sheet[ROW, COL].Text = "Revenue on Production";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colRevenueOnProduction = COL;
                COL++;
                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCurrency = COL;

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                int StartRow = ROW; //row 20
                for (int i = 0; i < dtProfitability.Rows.Count; i++)
                {
                    sheet[ROW, colSlNo].Number = (i + 1);

                    sheet[ROW, colEntity].Text = dtProfitability.Rows[i]["Entity"].ToString();
                    sheet[ROW, colWorkCenter].Text = dtProfitability.Rows[i]["Workcenter"].ToString();
                    sheet[ROW, colDailyFixedCost].Number = clsStaticInfo.dbl(dtProfitability.Rows[i]["DailyFixedCost"].ToString());
                    sheet[ROW, colAdditionalCost].Number = clsStaticInfo.dbl(dtProfitability.Rows[i]["AdditionalCostPerHour"].ToString());
                    sheet[ROW, colWcHour].Number = clsStaticInfo.dbl(dtProfitability.Rows[i]["StandardTimePerDay"].ToString());
                    sheet[ROW, colPlanHour].Number = clsStaticInfo.dbl(dtProfitability.Rows[i]["PlanHour"].ToString());
                    sheet[ROW, colTotalWC].Number = clsStaticInfo.dbl(dtProfitability.Rows[i]["TotalWCCost"].ToString());
                    sheet[ROW, colTgtQty].Number = clsStaticInfo.dbl(dtProfitability.Rows[i]["TargetQuantity"].ToString());
                    sheet[ROW, colCMTarget].Number = clsStaticInfo.dbl(dtProfitability.Rows[i]["CMTarget"].ToString());
                    sheet[ROW, colRevenueonTarget].Number = clsStaticInfo.dbl(dtProfitability.Rows[i]["RevenueOnTarget"].ToString());
                    sheet[ROW, colProdQty].Number = clsStaticInfo.dbl(dtProfitability.Rows[i]["ProductionQuantity"].ToString());
                    sheet[ROW, colCMProduction].Number = clsStaticInfo.dbl(dtProfitability.Rows[i]["CMProduction"].ToString());
                    sheet[ROW, colRevenueOnProduction].Number = clsStaticInfo.dbl(dtProfitability.Rows[i]["RevenueOnProduction"].ToString());
                    sheet[ROW, colCurrency].Text = dtProfitability.Rows[i]["ReportCurrency"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }
                sheet.Range[StartRow, colDailyFixedCost, ROW, colDailyFixedCost].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colAdditionalCost, ROW, colAdditionalCost].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colTotalWC, ROW, colTotalWC].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colCMTarget, ROW, colCMTarget].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colRevenueonTarget, ROW, colRevenueonTarget].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colCMProduction, ROW, colCMProduction].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colRevenueOnProduction, ROW, colRevenueOnProduction].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colWcHour, ROW, colWcHour].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[StartRow, colPlanHour, ROW, colPlanHour].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[StartRow, colTgtQty, ROW, colTgtQty].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[StartRow, colProdQty, ROW, colProdQty].NumberFormat = clsStaticInfo.NumberFormat(0);

                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();

                sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colSlNo, ROW, colSlNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;


                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Profitability", PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "Profitability Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetWorkcenterWisePlan(string EntityId, string date)
        {
            return @"SELECT ppt.WorkCenterMasterId,W.UserName, w.Sequence,SUM(ppt.Quantity) AS Quantity FROM ProductionPlanningType1 AS ppt 
                    INNER JOIN scs.WorkCenterMaster AS w ON ppt.WorkCenterMasterId=w.Id
                    WHERE ppt.ProductionDate='" + Convert.ToDateTime(date).ToString("dd-MMM-yyyy") + "' and w.EntityId='" + EntityId + @"'
                    group by  ppt.WorkCenterMasterId,W.UserName, w.Sequence
                    ORDER BY w.Sequence";
        }
        public string GetProfitability(string PlantId, string EntityId, string date)
        {
            string Filter = "";
            if (string.IsNullOrEmpty(EntityId) == false && EntityId.ToUpper() != "NULL")
                Filter = " AND wcm.EntityId='" + EntityId + "'";

            return @"select *,
CASE WHEN k.RevenueOnTarget>k.TotalWCCost THEN k.RevenueOnTarget ELSE
	CASE WHEN k.RevenueOnProduction>k.RevenueOnTarget THEN k.RevenueOnProduction ELSE k.TotalWCCost END END AS MaxValue
	
 from (SELECT wcm.Sequence,C.Code AS ReportCurrency,wcm.Id,wcm.PlantId, wcm.EntityId, wcm.UserName AS Workcenter ,ENT.UserName AS Entity,
 isnull(trg.CM,0) AS CMTarget,isnull(PRD.CM,0) AS CMProduction, trg.TargetQuantity,PRD.ProductionQuantity,
isnull(rer.ExchangeRate,1)*wcm.DailyFixedCost AS DailyFixedCost,
isnull(rer.ExchangeRate,1)* wcm.VariableCost AS AdditionalCostPerHour,
wcm.StandardTimePerDay,wcm.MaxTimePerDay,TRG.PlanHour,
                            CASE WHEN TRG.PlanHour-wcm.StandardTimePerDay>0 
                            THEN wcm.VariableCost*(TRG.PlanHour-wcm.StandardTimePerDay) ELSE 0 END AS AdditionalCost,
                          Convert(decimal(18,0),(CASE WHEN TRG.PlanHour-wcm.StandardTimePerDay>0 
THEN wcm.VariableCost*(TRG.PlanHour-wcm.StandardTimePerDay) ELSE 0 END+wcm.DailyFixedCost)*isnull(rer.ExchangeRate,1)) AS TotalWCCost,
                            isnull(trg.RevenueOnTarget,0) AS RevenueOnTarget,isnull(prd.RevenueOnProduction,0) AS RevenueOnProduction
                              FROM scs.WorkCenterMaster AS wcm
								LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=wcm.CurrencyId AND rer.PlantId='" + PlantId + @"'
								LEFT JOIN ReportExchangeRates AS rec ON rec.FromCurrencyId=rec.ToCurrencyId AND rec.PlantId='" + PlantId + @"'
								LEFT JOIN scs.Currency AS c ON c.Id=rec.FromCurrencyId
                                left join org.Entity ENT on ENT.Id=WCM.EntityId
                              LEFT JOIN (
                              SELECT dpt.WorkCenterMasterID,max(dpt.TotalHourPlanning) AS PlanHour,SUM(dpt.Quantity*CM)/NULLIF(SUM(dpt.Quantity),0) AS CM,SUM(dpt.Quantity) AS TargetQuantity,
                             Convert(decimal(18,0),sum(ord.CM*dpt.Quantity)) AS RevenueOnTarget
                                FROM trn.DailyProductionTarget AS dpt
                                 left outer join (
                                                        select POD.ProductionOrderId,
                                                        SUM(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty,
                                                        min(so.DeliveryDate) AS FirstDeliveryDate,
                                                        max(so.DeliveryDate) AS LastDeliveryDate,
                                                        MAX(so.CommitmentDate) AS ProductionCompletionDate,
                                                        SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate*SO.Qty ELSE  so.Rate * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1)* so.Qty END)/SUM(so.Qty) AS FOB,
														SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM*SO.Qty ELSE  so.CM * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1)* so.Qty END)/SUM(so.Qty) AS CM,
							                            sum(SO.Qty) AS OrderQty 
                                                           from trn.ProductionOrderDetail POD 
                                                        left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                                                        left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                        LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                        left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                                                        left JOIN org.Company AS c ON c.Id=mo.CompanyId
                                                        left outer join MasterOrderExchangeRates RT on RT.TransactionId=MO.Id
								                     	LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=C.BaseCurrencyId AND rer.PlantId='" + PlantId + @"'
                                                        LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId='" + PlantId + @"'
                                                        join trn.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
														LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
														
                                                  
                                                        group by POD.ProductionOrderId
                                                        ) AS ORD on ord.ProductionOrderID=dpt.ProductionOrderId 
                              WHERE dpt.TargetDate='" + date + @"'
                              GROUP BY dpt.WorkCenterMasterID) AS TRG ON trg.WorkCenterMasterID=wcm.Id
  
                              LEFT JOIN ( SELECT dpt.WorkCenterMasterID,SUM(dpt.Quantity*CM)/NULLIF(SUM(dpt.Quantity),0) AS CM,SUM(dpt.Quantity) AS ProductionQuantity,
                              Convert(decimal(18,0),SUM( ord.CM*dpt.Quantity)) AS RevenueOnProduction
                                FROM trn.ProductionSummary AS dpt
                                 left outer join (
                                                        select POD.ProductionOrderId,
                                                        SUM(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty,
                                                        min(so.DeliveryDate) AS FirstDeliveryDate,
                                                        max(so.DeliveryDate) AS LastDeliveryDate,
                                                        MAX(so.CommitmentDate) AS ProductionCompletionDate,
	                                                    SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate*SO.Qty ELSE  so.Rate * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1)* so.Qty END)/SUM(so.Qty) AS FOB,
														SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM*SO.Qty ELSE  so.CM * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1)* so.Qty END)/SUM(so.Qty) AS CM,
												                                                      
                                                        sum(SO.Qty) AS OrderQty 
                                                           from trn.ProductionOrderDetail POD 
                                                        left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                                                        left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                        --LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                        left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                                                        left JOIN org.Company AS c ON c.Id=mo.CompanyId
                                                        left outer join MasterOrderExchangeRates RT on RT.TransactionId=MO.Id
								                     	LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=C.BaseCurrencyId AND rer.PlantId='" + PlantId + @"'
                                                        LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId='" + PlantId + @"'
                                                        join trn.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
														LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
														
                                                  
                                                        group by POD.ProductionOrderId
                                                        ) AS ORD on ord.ProductionOrderID=dpt.ProductionOrderId 
                              WHERE dpt.ProductionDate='" + date + @"'
                               GROUP BY  dpt.WorkCenterMasterID
                              ) AS PRD ON prd.WorkCenterMasterID=wcm.Id
  
  
                            WHERE wcm.PlantId='" + PlantId + @"' AND wcm.ProcessId=(SELECT TOP 1 pt.BaseProcessId
                                                                        FROM PlanningTypes AS pt)

                            " + Filter + @"
                            ) AS K

                        ORDER BY k.Sequence";
        }
        public string GetProcessWiseProduction(string PlantId, string EntityId, string date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            PackingData det = new PackingData();
            IdentityParameter para = new IdentityParameter
            {
                CompanyGroupId = identity.CompanyGroupId,
                CompanyId = identity.CompanyId,
                PlantId = identity.PlantId,
                AddedBy = identity.Name,
                AddedDate = DateTime.Now,
                AddedFromIP = identity.IPAddress,
                UpdatedBy = identity.Name,
                UpdatedDate = DateTime.Now,
                UpdatedFromIP = identity.IPAddress
            };
            var sqlProcess = @"SELECT Id PurposeId FROM HKP.MaterialMovementPurpose Where ProcessId=(Select Id from HKP.Process Where UserName='Packing')";
            DataTable dtProcess = _sqlRepository.GetDataTable(sqlProcess);
            if (dtProcess.Rows.Count>0)
            {
                string PurposeId = dtProcess.Rows[0]["PurposeId"].ToString();
                det.SaveScandataToBooking(date, date, PurposeId, para); 
            }

            if (string.IsNullOrEmpty(EntityId) || EntityId.ToUpper() == "NULL")
            {
                return @"SELECT  p.Sequence, p.Id,p.UserName,ISNULL(k.Quantity,0) as Quantity  FROM HKP.Process P LEFT JOIN ( SELECT p.Id,isnull(SUM(ps.Quantity),0) AS Quantity
                          FROM hkp.Process AS p
                        LEFT JOIN trn.ProductionSummary AS ps ON ps.ProcessId=p.Id AND ps.ProductionDate='" + date + @"' AND  ps.EntityId IN (SELECT Id From org.Entity AS e  WHERE e.CompanyId='" + identity.CompanyId + @"')
                        join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                        LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            

                        WHERE PS.PlantId='" + PlantId + @"' AND p.Id IN (SELECT ept.ProcessId
                                         FROM hkp.EntityProcessTag AS ept JOIN org.Entity AS e ON e.Id=ept.EntityId WHERE e.CompanyId='" + identity.CompanyId + @"')
                 
                        GROUP BY p.Id) AS K ON K.Id=P.Id
                        WHERE P.IsProductionProcess=1 AND P.Active=1 AND p.Id IN (SELECT ept.ProcessId
                                         FROM hkp.EntityProcessTag AS ept JOIN org.Entity AS e ON e.Id=ept.EntityId WHERE e.CompanyId='" + identity.CompanyId + @"')
                        ORDER BY p.Sequence";
            }
            else
            {
                return @"SELECT  p.Sequence, p.Id,p.UserName,ISNULL(k.Quantity,0) as Quantity FROM HKP.Process P LEFT JOIN (SELECT  p.Id,isnull(SUM(ps.Quantity),0) AS Quantity
                          FROM hkp.Process AS p
                        LEFT JOIN trn.ProductionSummary AS ps ON ps.ProcessId=p.Id AND ps.ProductionDate='" + date + @"' AND EntityId='" + EntityId + @"'
                        join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                        LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId

                        WHERE p.Id IN (SELECT ept.ProcessId
                                         FROM hkp.EntityProcessTag AS ept  WHERE ept.EntityId='" + EntityId + @"')
                 
                        GROUP BY p.Id) AS K ON K.Id=P.Id
                        WHERE P.IsProductionProcess=1 AND P.Active=1 AND p.Id IN (SELECT ept.ProcessId
                                         FROM hkp.EntityProcessTag AS ept  WHERE ept.EntityId='" + EntityId + @"')
                        ORDER BY p.Sequence";
            }
        }
        private string CommonColumns()
        {

            return @" MasterOrderId =STUFF((select distinct ','+XMOI.Id from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ps.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
                                                    BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ps.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ps.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ps.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ps.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ps.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ps.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where ps.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where ps.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                   ";
        }

        public string GetProductionOrderWiseProduction(string EntityId, string ProcessId, string date)
        {

            string _commonColumns = CommonColumns();


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (string.IsNullOrEmpty(EntityId) || EntityId.ToUpper() == "NULL")
            {
                return @"SELECT p.Sequence,e.UserName AS Entity,ps.ProductionOrderId, p.Id,p.UserName AS Process,isnull(SUM(ps.Quantity),0) AS Quantity," + _commonColumns + @"
                          FROM hkp.Process AS p
                        LEFT JOIN trn.ProductionSummary AS ps ON ps.ProcessId=p.Id AND ps.ProductionDate='" + date + @"' AND  ps.EntityId IN (SELECT Id From org.Entity AS e  WHERE e.CompanyId='" + identity.CompanyId + @"')
                        join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                        LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId                         
                        LEFT JOIN org.Entity AS e ON e.Id=ps.EntityId
                        WHERE p.Id IN (SELECT ept.ProcessId
                                         FROM hkp.EntityProcessTag AS ept JOIN org.Entity AS e ON e.Id=ept.EntityId WHERE e.CompanyId='" + identity.CompanyId + @"')
                 
                        AND PS.ProcessId='" + ProcessId + @"'
                        GROUP BY ps.ProductionOrderId,p.Sequence,p.Id,p.UserName,e.UserName
                        ORDER BY e.UserName, p.Sequence,ps.ProductionOrderId";
            }
            else
            {
                return @"SELECT p.Sequence,e.UserName AS Entity,ps.ProductionOrderId, p.Id,p.UserName AS Process,isnull(SUM(ps.Quantity),0) AS Quantity," + _commonColumns + @"
                          FROM hkp.Process AS p
                        LEFT JOIN trn.ProductionSummary AS ps ON ps.ProcessId=p.Id AND ps.ProductionDate='" + date + @"' AND EntityId='" + EntityId + @"'
                        join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                        LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId                         
                        LEFT JOIN org.Entity AS e ON e.Id=ps.EntityId
                        WHERE p.Id IN (SELECT ept.ProcessId
                                         FROM hkp.EntityProcessTag AS ept  WHERE ept.EntityId='" + EntityId + @"')
                        
                        AND PS.ProcessId='" + ProcessId + @"'
                        GROUP BY ps.ProductionOrderId,p.Sequence,p.Id,p.UserName,e.UserName
                        ORDER BY e.UserName,p.Sequence,ps.ProductionOrderId";
            }
        }
        public DataTable GetWorkCenterWiseWIP(string PlantId, string EntityId, DataTable dt)
        {

            if (string.IsNullOrEmpty(EntityId) == false && EntityId != "null")
            {
                dt.DefaultView.RowFilter = "EntityId='" + EntityId + @"'";
                dt = dt.DefaultView.ToTable();

            }

            if (dt.Rows.Count == 0)
                return dt;

            dt = dt.AsEnumerable().GroupBy(x => new
            {
                EntityId = x["EntityId"],
                Entity = x["Entity"],
                WorkCenterMasterId = x["WorkCenterMasterId"],
                WorkCenter = x["WorkCenter"],
                Capacity = x["Capacity"],
                //AlignedWithPlan = x["AlignedWithPlan"],
            })
                                          .Select(x =>
                                          {
                                              DataRow row = dt.NewRow();
                                              row["EntityId"] = x.Key.EntityId;
                                              row["Entity"] = x.Key.Entity;
                                              row["WorkCenterMasterId"] = x.Key.WorkCenterMasterId;
                                              row["WorkCenter"] = x.Key.WorkCenter;
                                              row["Capacity"] = x.Key.Capacity;
                                              row["AlignedWithPlan"] = x.Min(r => (decimal)OTSBD.clsStaticInfo.dbl(r["AlignedWithPlan"]));
                                              row["MaxValue"] = (decimal)x.Key.Capacity > x.Max(r => (decimal)OTSBD.clsStaticInfo.dbl(r["WIP"])) ? x.Key.Capacity : x.Max(r => (decimal)OTSBD.clsStaticInfo.dbl(r["WIP"]));

                                              row["InQuantity"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["InQuantity"]));
                                              row["OutQuantity"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["OutQuantity"]));
                                              row["KillQuantity"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["KillQuantity"]));
                                              row["WIP"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["WIP"]));
                                              row["InQuantityToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["InQuantityToday"]));
                                              row["OutQuantityToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["OutQuantityToday"]));
                                              row["KillQuantityToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["KillQuantityToday"]));
                                              return row;
                                          }
                                          ).CopyToDataTable();



            //dt.DefaultView.RowFilter = "isnull(InQuantityToday,0)>0 OR isnull(OutQuantityToday,0)>0 OR isnull(KillQuantityToday,0)>0";
            //dt = dt.DefaultView.ToTable();
            return dt;

        }
        public DataTable GetWorkCenterWiseWIPForGraph(string PlantId, string EntityId, string ProcessId, DataTable dt)
        {

            DataTable _dtWC = _sqlRepository.GetDataTable(@" SELECT E.UserName AS Entity,P.UserName AS Plant, WCM.* FROM scs.WorkCenterMaster AS wcm 
                                                             JOIN org.Entity AS e ON e.Id=wcm.EntityId
                                                             JOIN org.Plant AS p ON p.Id=e.PlantId where WCM.plantid='" + PlantId + @"' AND WCM.ProcessId='" + ProcessId + @"' ORDER BY WCM.Sequence");




            if (string.IsNullOrEmpty(EntityId) == false && EntityId != "null")
            {
                dt.DefaultView.RowFilter = "EntityId='" + EntityId + @"'";
                dt = dt.DefaultView.ToTable();

                _dtWC.DefaultView.RowFilter = "EntityId='" + EntityId + @"'";
                _dtWC = _dtWC.DefaultView.ToTable();
            }

            for (int i = 0; i < _dtWC.Rows.Count; i++)
            {
                dt.DefaultView.RowFilter = "WorkCenterMasterId='" + _dtWC.Rows[i]["Id"].ToString() + @"'";
                if (dt.DefaultView.Count == 0)
                {

                    DataRow row = dt.NewRow();
                    row["EntityId"] = _dtWC.Rows[i]["EntityId"].ToString();
                    row["Entity"] = _dtWC.Rows[i]["Entity"].ToString();
                    row["WorkCenterMasterId"] = _dtWC.Rows[i]["Id"].ToString();
                    row["WorkCenter"] = _dtWC.Rows[i]["UserName"].ToString();
                    row["AlignedWithPlan"] = true;
                    row["WorkCenterSequence"] = _dtWC.Rows[i]["Sequence"].ToString();

                    row["Capacity"] = _dtWC.Rows[i]["Capacity"];
                    row["MaxValue"] = _dtWC.Rows[i]["Capacity"];
                    row["InQuantity"] = 0;
                    row["OutQuantity"] = 0;
                    row["KillQuantity"] = 0;
                    row["WIP"] = 0;
                    row["InQuantityToday"] = 0;
                    row["OutQuantityToday"] = 0;
                    row["KillQuantityToday"] = 0;
                    dt.Rows.Add(row);
                }
            }

            dt.DefaultView.RowFilter = null;
            if (dt.Rows.Count == 0)
                return dt;

            dt.DefaultView.Sort = "WorkCenterSequence ASC";
            dt = dt.DefaultView.ToTable();


            dt = dt.AsEnumerable().GroupBy(x => new
            {
                EntityId = x["EntityId"],
                Entity = x["Entity"],
                WorkCenterMasterId = x["WorkCenterMasterId"],
                WorkCenter = x["WorkCenter"],
                Capacity = x["Capacity"],
                //AlignedWithPlan = x["AlignedWithPlan"],
            })
                                          .Select(x =>
                                          {
                                              DataRow row = dt.NewRow();
                                              row["EntityId"] = x.Key.EntityId;
                                              row["Entity"] = x.Key.Entity;
                                              row["WorkCenterMasterId"] = x.Key.WorkCenterMasterId;
                                              row["WorkCenter"] = x.Key.WorkCenter;
                                              row["Capacity"] = x.Key.Capacity;
                                              row["AlignedWithPlan"] = x.Min(r => (decimal)OTSBD.clsStaticInfo.dbl(r["AlignedWithPlan"]));
                                              row["MaxValue"] = (decimal)x.Key.Capacity > x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["WIP"])) ? x.Key.Capacity : x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["WIP"]));

                                              row["InQuantity"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["InQuantity"]));
                                              row["OutQuantity"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["OutQuantity"]));
                                              row["KillQuantity"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["KillQuantity"]));
                                              row["WIP"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["WIP"]));
                                              row["InQuantityToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["InQuantityToday"]));
                                              row["OutQuantityToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["OutQuantityToday"]));
                                              row["KillQuantityToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["KillQuantityToday"]));
                                              return row;
                                          }
                                          ).CopyToDataTable();





            return dt;

        }
        public DataTable GetPRWiseWIP(string EntityId, string WorkCenterMasterId, DataTable dt)
        {
            if (string.IsNullOrEmpty(EntityId) == false && EntityId != "null")
            {
                dt.DefaultView.RowFilter = "EntityId='" + EntityId + @"'";
                dt = dt.DefaultView.ToTable();
            }

            //dt.DefaultView.RowFilter = "";
            //dt = dt.DefaultView.ToTable();
            dt.DefaultView.RowFilter = "WorkCenterMasterId='" + WorkCenterMasterId + @"' AND ( isnull(InQuantityToday,0)>0 OR isnull(OutQuantityToday,0)>0 OR isnull(KillQuantityToday,0)>0)";
            dt = dt.DefaultView.ToTable();
            return dt;

        }

        public List<Dictionary<string, object>> GetInWC(string FDUD, string EntityId, string WorkCenterMasterId, string ProcessId, string date)
        {
            if (FDUD == "FD")
                FDUD = "=";
            else
                FDUD = "<=";

            string sql = @"SELECT ps.ProductionOrderId,FORMAT(PS.ProductionDate,'dd-MMM-yyyy') AS ProductionDate, 
                            ISNULL(s.UserName,wcm.UserName) AS [FromLocation],ISNULL(sTo.UserName,wcmTo.UserName) AS [ToLocation],sum(ps.Quantity) AS Quantity," + CommonColumns() + @"
                              FROM trn.ProductionSummary AS ps
                            LEFT JOIN scs.WorkCenterMaster AS wcm  ON wcm.Id=ps.WorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS s ON s.Id=ps.FromSFGInventoryId
                            LEFT JOIN hkp.Process AS p ON p.Id=ps.ProcessId

                            LEFT JOIN scs.WorkCenterMaster AS wcmTO  ON wcmTO.Id=ps.ToWorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS sTO ON sTO.Id=ps.ToSFGInventoryId
                            LEFT JOIN hkp.Process AS pTo ON pTo.Id=ps.ToProcessId

                            join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS SS ON SS.Id=po.ProductionStatusId
                            WHERE (isnull(SS.StandardName,'')<>'Closed'  OR convert(date,po.ClosingDate)>CONVERT(DATE,'" + date + @"')) AND ps.EntityId='" + EntityId + @"' AND isnull(ps.ToWorkCenterMasterId,'')='" + WorkCenterMasterId + @"' AND ps.toProcessId='" + ProcessId + @"'
                             AND  ps.ProductionDate" + FDUD + @"'" + date + @"' AND ps.ProductionGrade='A'
                            GROUP BY PS.ProductionDate,ps.ProductionOrderId,s.UserName,wcm.UserName,sTo.UserName,wcmTo.UserName ORDER BY PS.ProductionDate DESC";

            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetOutWC(string FDUD, string EntityId, string WorkCenterMasterId, string ProcessId, string date)
        {
            if (FDUD == "FD")
                FDUD = "=";
            else
                FDUD = "<=";

            string sql = @"SELECT ps.ProductionOrderId,FORMAT(PS.ProductionDate,'dd-MMM-yyyy') AS ProductionDate, 
                    ISNULL(s.UserName,wcm.UserName) AS [ToLocation], ISNULL(sFrom.UserName,wcmFrom.UserName) AS [FromLocation],sum(ps.Quantity) AS Quantity," + CommonColumns() + @"
                              FROM trn.ProductionSummary AS ps
                            LEFT JOIN scs.WorkCenterMaster AS wcm  ON wcm.Id=ps.ToWorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS s ON s.Id=ps.ToSFGInventoryId
                            LEFT JOIN hkp.Process AS p ON p.Id=ps.ToProcessId

                            LEFT JOIN scs.WorkCenterMaster AS wcmFrom  ON wcmFrom.Id=ps.WorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS sFrom ON sFrom.Id=ps.FromSFGInventoryId
                            LEFT JOIN hkp.Process AS pFrom ON pFrom.Id=ps.ProcessId

                            join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS SS ON SS.Id=po.ProductionStatusId
                            WHERE  (isnull(SS.StandardName,'')<>'Closed'  OR convert(date,po.ClosingDate)>CONVERT(DATE,'" + date + @"')) AND  ps.EntityId='" + EntityId + @"' AND isnull(ps.WorkCenterMasterId,'')='" + WorkCenterMasterId + @"' AND ps.ProcessId='" + ProcessId + @"'
                             AND ps.ProductionDate" + FDUD + @"'" + date + @"' AND ps.ProductionGrade='A'
                            GROUP BY PS.ProductionDate,ps.ProductionOrderId,s.UserName,wcm.UserName,sFrom.UserName,wcmFrom.UserName ORDER BY PS.ProductionDate DESC";

            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetKillWC(string FDUD, string EntityId, string WorkCenterMasterId, string ProcessId, string date)
        {
            if (FDUD == "FD")
                FDUD = "=";
            else
                FDUD = "<=";

            string sql = @"select PS.ProductionDate,PS.ProductionOrderId, PS.FromLocation AS [FromLocation],'' AS ToLocation, sum(PS.Quantity) AS Quantity," + CommonColumns() + @"
                          from (SELECT FORMAT(PS.ProductionDate,'dd-MMM-yyyy') AS ProductionDate, ps.ProductionOrderId,ISNULL(s.UserName,wcm.UserName) AS FromLocation,ps.Quantity
                          FROM trn.ProductionSummary AS ps
                        LEFT JOIN scs.WorkCenterMaster AS wcm  ON wcm.Id=ps.ToWorkCenterMasterId
                        LEFT JOIN hkp.SFGInventory AS s ON s.Id=ps.ToSFGInventoryId
                        LEFT JOIN hkp.Process AS p ON p.Id=ps.ToProcessId
                        join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                        LEFT OUTER JOIN hkp.ProductionStatus AS SS ON SS.Id=po.ProductionStatusId
                        WHERE  (isnull(SS.StandardName,'')<>'Closed'  OR convert(date,po.ClosingDate)>CONVERT(DATE,'" + date + @"')) AND ps.EntityId='" + EntityId + @"' AND isnull(ps.WorkCenterMasterId,'')='" + WorkCenterMasterId + @"' AND ps.ProcessId='" + ProcessId + @"'
                         AND ps.ProductionDate" + FDUD + @"'" + date + @"' AND ps.ProductionGrade<>'A'
                        UNION ALL
                        SELECT FORMAT(q.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,q.ProductionOrderId,wcm.UserName AS FromLocation,isnull(q.DefectiveQty,0) AS  KillQuantity
                        FROM trn.Quality AS q
                        JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=q.WorkCenterMasterID
                        join trn.ProductionOrder PO ON PO.Id=Q.ProductionOrderId
                        LEFT OUTER JOIN hkp.ProductionStatus AS SS ON SS.Id=po.ProductionStatusId
                        WHERE  (isnull(SS.StandardName,'')<>'Closed'  OR convert(date,po.ClosingDate)>CONVERT(DATE,'" + date + @"')) AND wcm.ProcessId='" + ProcessId + @"' AND isnull(q.WorkCenterMasterID,'')='" + WorkCenterMasterId + @"' AND convert(date,Q.ProductionDate)" + FDUD + @" convert(date,'" + date + @"')
                        ) AS PS GROUP BY PS.ProductionDate,PS.ProductionOrderId, PS.FromLocation ORDER BY CONVERT(date,PS.ProductionDate) DESC";

            return _sqlRepository.GetDataCollection(sql);
        }



        public List<Dictionary<string, object>> GetInPO(string FDUD, string ProductionOrderId, string ProcessId, string date)
        {
            if (FDUD == "FD")
                FDUD = "=";
            else
                FDUD = "<=";

            string sql = @"SELECT FORMAT(PS.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,ps.ProductionOrderId,
                            pbp.UserName AS ProductionPeriod,pbp.Sequence,ISNULL(s.Sequence,wcm.Sequence) AS WCSequence,

                            ISNULL(s.UserName,wcm.UserName)  AS [FromLocation],ISNULL(sTo.UserName,wcmTo.UserName) AS [ToLocation],sum(ps.Quantity) AS Quantity
                                FROM trn.ProductionSummary AS ps
                            LEFT JOIN scs.WorkCenterMaster AS wcm  ON wcm.Id=ps.WorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS s ON s.Id=ps.FromSFGInventoryId
                            LEFT JOIN hkp.Process AS p ON p.Id=ps.ProcessId
                            LEFT JOIN hkp.ProductionBookingPeriod AS pbp ON pbp.Id=ps.ProductionBookingPeriodId

                            LEFT JOIN scs.WorkCenterMaster AS wcmTO  ON wcmTO.Id=ps.ToWorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS sTO ON sTO.Id=ps.ToSFGInventoryId
                            LEFT JOIN hkp.Process AS pTo ON pTo.Id=ps.ToProcessId

                            join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS SS ON SS.Id=po.ProductionStatusId
                            WHERE isnull(isnull(ps.ToWorkCenterMasterId,ps.ToSFGInventoryId),'')<>'' AND ps.ToProcessId='" + ProcessId + "' AND ps.ProductionOrderId='" + ProductionOrderId + @"'
                            AND ps.ProductionDate" + FDUD + @"'" + date + @"' AND ps.ProductionGrade='A'
                            GROUP BY s.Sequence,wcm.Sequence, pbp.UserName,pbp.Sequence, PS.ProductionDate,ps.ProductionOrderId,s.UserName,wcm.UserName,sTo.UserName,wcmTo.UserName 
                            ORDER BY PS.ProductionDate DESC,s.Sequence,wcm.Sequence,pbp.Sequence";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetOutPO(string FDUD, string ProductionOrderId, string ProcessId, string date)
        {
            if (FDUD == "FD")
                FDUD = "=";
            else
                FDUD = "<=";
            string sql = @"SELECT FORMAT(PS.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,ps.ProductionOrderId, 
                        pbp.UserName AS ProductionPeriod,pbp.Sequence,ISNULL(sFrom.Sequence,wcmFrom.Sequence) AS WCSequence,

                            ISNULL(s.UserName,wcm.UserName) AS [ToLocation], ISNULL(sFrom.UserName,wcmFrom.UserName) AS [FromLocation],sum(ps.Quantity) AS Quantity
                              FROM trn.ProductionSummary AS ps
                            LEFT JOIN scs.WorkCenterMaster AS wcm  ON wcm.Id=ps.ToWorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS s ON s.Id=ps.ToSFGInventoryId
                            LEFT JOIN hkp.Process AS p ON p.Id=ps.ToProcessId

                            LEFT JOIN scs.WorkCenterMaster AS wcmFrom  ON wcmFrom.Id=ps.WorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS sFrom ON sFrom.Id=ps.FromSFGInventoryId
                            LEFT JOIN hkp.Process AS pFrom ON pFrom.Id=ps.ProcessId
                            LEFT JOIN hkp.ProductionBookingPeriod AS pbp ON pbp.Id=ps.ProductionBookingPeriodId

                            join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS SS ON SS.Id=po.ProductionStatusId
                            WHERE isnull(isnull(ps.WorkCenterMasterId,ps.FromSFGInventoryId),'')<>'' AND ps.ProcessId='" + ProcessId + "' AND ps.ProductionOrderId='" + ProductionOrderId + @"'
                             AND ps.ProductionDate" + FDUD + @"'" + date + @"' AND ps.ProductionGrade='A'
                            GROUP BY pbp.UserName,pbp.Sequence,sFrom.Sequence,wcmFrom.Sequence,PS.ProductionDate,ps.ProductionOrderId,s.UserName,wcm.UserName,sFrom.UserName,wcmFrom.UserName 
                            ORDER BY PS.ProductionDate DESC,pbp.Sequence,pbp.UserName,sFrom.Sequence,wcmFrom.Sequence";

            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetKillPO(string FDUD, string ProductionOrderId, string ProcessId, string date)
        {
            if (FDUD == "FD")
                FDUD = "=";
            else
                FDUD = "<=";

            string sql = @"select  pbp.UserName AS ProductionPeriod,  PS.ProductionDate,PS.ProductionOrderId, PS.FromLocation AS [FromLocation],'' AS ToLocation, sum(PS.Quantity) AS Quantity
                          from (SELECT FORMAT(PS.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,ps.ProductionOrderId,ISNULL(s.UserName,wcm.UserName) AS FromLocation,ps.Quantity
                          FROM trn.ProductionSummary AS ps
                        LEFT JOIN hkp.ProductionBookingPeriod AS pbp ON pbp.Id=ps.ProductionBookingPeriodId
                        LEFT JOIN scs.WorkCenterMaster AS wcm  ON wcm.Id=ps.ToWorkCenterMasterId
                        LEFT JOIN hkp.SFGInventory AS s ON s.Id=ps.ToSFGInventoryId
                        LEFT JOIN hkp.Process AS p ON p.Id=ps.ToProcessId
                        join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                        LEFT OUTER JOIN hkp.ProductionStatus AS SS ON SS.Id=po.ProductionStatusId
                        WHERE isnull(isnull(ps.WorkCenterMasterId,ps.FromSFGInventoryId),'')<>'' AND ps.ProcessId='" + ProcessId + "' AND ps.ProductionOrderId='" + ProductionOrderId + @"'
                         AND ps.ProductionDate" + FDUD + @"'" + date + @"' AND ps.ProductionGrade<>'A'
                        UNION ALL
                        SELECT  '' AS ProductionPeriod,FORMAT(q.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,q.ProductionOrderId,wcm.UserName AS FromLocation,isnull(q.DefectiveQty,0) AS  KillQuantity
                        FROM trn.Quality AS q
                        JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=q.WorkCenterMasterID
                        join trn.ProductionOrder PO ON PO.Id=Q.ProductionOrderId
                        LEFT OUTER JOIN hkp.ProductionStatus AS SS ON SS.Id=po.ProductionStatusId
                        WHERE  q.ProductionOrderId='" + ProductionOrderId + @"' AND isnull(wcm.ProcessId,'')='" + ProcessId + @"' AND convert(date,Q.ProductionDate)" + FDUD + @" convert(date,'" + date + @"')
                        ) AS PS GROUP BY PS.ProductionDate,PS.ProductionOrderId, PS.FromLocation ORDER BY CONVERT(DATE,PS.ProductionDate) DESC";

            return _sqlRepository.GetDataCollection(sql);
        }


        public List<Dictionary<string, object>> GetInWCPO(string FDUD, string ProductionOrderId, string WorkCenterMasterId, string date)
        {
            if (FDUD == "FD")
                FDUD = "=";
            else
                FDUD = "<=";

            string sql = @"SELECT FORMAT(PS.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,ps.ProductionOrderId,
ISNULL(s.UserName,wcm.UserName)  AS [FromLocation],ISNULL(sTo.UserName,wcmTo.UserName) AS [ToLocation],sum(ps.Quantity) AS Quantity
                                FROM trn.ProductionSummary AS ps
                            LEFT JOIN scs.WorkCenterMaster AS wcm  ON wcm.Id=ps.WorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS s ON s.Id=ps.FromSFGInventoryId
                            LEFT JOIN hkp.Process AS p ON p.Id=ps.ProcessId

                            LEFT JOIN scs.WorkCenterMaster AS wcmTO  ON wcmTO.Id=ps.ToWorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS sTO ON sTO.Id=ps.ToSFGInventoryId
                            LEFT JOIN hkp.Process AS pTo ON pTo.Id=ps.ToProcessId

                            join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS SS ON SS.Id=po.ProductionStatusId
                            WHERE ps.ToWorkCenterMasterId='" + WorkCenterMasterId + "' AND ps.ProductionOrderId='" + ProductionOrderId + @"'
                            AND ps.ProductionDate" + FDUD + @"'" + date + @"' AND ps.ProductionGrade='A'
                            GROUP BY PS.ProductionDate,ps.ProductionOrderId,s.UserName,wcm.UserName,sTo.UserName,wcmTo.UserName ORDER BY PS.ProductionDate DESC";

            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetOutWCPO(string FDUD, string ProductionOrderId, string WorkCenterMasterId, string date)
        {
            if (FDUD == "FD")
                FDUD = "=";
            else
                FDUD = "<=";
            string sql = @"SELECT FORMAT(PS.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,ps.ProductionOrderId, 
                    ISNULL(s.UserName,wcm.UserName) AS [ToLocation], ISNULL(sFrom.UserName,wcmFrom.UserName) AS [FromLocation],sum(ps.Quantity) AS Quantity
                              FROM trn.ProductionSummary AS ps
                            LEFT JOIN scs.WorkCenterMaster AS wcm  ON wcm.Id=ps.ToWorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS s ON s.Id=ps.ToSFGInventoryId
                            LEFT JOIN hkp.Process AS p ON p.Id=ps.ToProcessId

                            LEFT JOIN scs.WorkCenterMaster AS wcmFrom  ON wcmFrom.Id=ps.WorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS sFrom ON sFrom.Id=ps.FromSFGInventoryId
                            LEFT JOIN hkp.Process AS pFrom ON pFrom.Id=ps.ProcessId
                            join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS SS ON SS.Id=po.ProductionStatusId
                            WHERE ps.WorkCenterMasterId='" + WorkCenterMasterId + "' AND ps.ProductionOrderId='" + ProductionOrderId + @"'
                             AND ps.ProductionDate" + FDUD + @"'" + date + @"' AND ps.ProductionGrade='A'
                            GROUP BY PS.ProductionDate,ps.ProductionOrderId,s.UserName,wcm.UserName,sFrom.UserName,wcmFrom.UserName ORDER BY PS.ProductionDate DESC";

            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetKillWCPO(string FDUD, string ProductionOrderId, string WorkCenterMasterId, string date)
        {
            if (FDUD == "FD")
                FDUD = "=";
            else
                FDUD = "<=";

            string sql = @"select PS.ProductionDate,PS.ProductionOrderId, PS.FromLocation AS [FromLocation],'' AS ToLocation, sum(PS.Quantity) AS Quantity
                          from (SELECT FORMAT(PS.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,ps.ProductionOrderId,ISNULL(s.UserName,wcm.UserName) AS FromLocation,ps.Quantity
                          FROM trn.ProductionSummary AS ps
                        LEFT JOIN scs.WorkCenterMaster AS wcm  ON wcm.Id=ps.ToWorkCenterMasterId
                        LEFT JOIN hkp.SFGInventory AS s ON s.Id=ps.ToSFGInventoryId
                        LEFT JOIN hkp.Process AS p ON p.Id=ps.ToProcessId
                        join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                        LEFT OUTER JOIN hkp.ProductionStatus AS SS ON SS.Id=po.ProductionStatusId
                        WHERE ps.WorkCenterMasterId='" + WorkCenterMasterId + "' AND ps.ProductionOrderId='" + ProductionOrderId + @"'
                         AND ps.ProductionDate" + FDUD + @"'" + date + @"' AND ps.ProductionGrade<>'A'
                        UNION ALL
                        SELECT FORMAT(q.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,q.ProductionOrderId,wcm.UserName AS FromLocation,isnull(q.DefectiveQty,0) AS  KillQuantity
                        FROM trn.Quality AS q
                        JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=q.WorkCenterMasterID
                        join trn.ProductionOrder PO ON PO.Id=Q.ProductionOrderId
                        LEFT OUTER JOIN hkp.ProductionStatus AS SS ON SS.Id=po.ProductionStatusId
                        WHERE  q.ProductionOrderId='" + ProductionOrderId + @"' AND q.WorkCenterMasterID='" + WorkCenterMasterId + @"' AND convert(date,Q.ProductionDate)" + FDUD + @" convert(date,'" + date + @"')
                        ) AS PS GROUP BY PS.ProductionDate,PS.ProductionOrderId, PS.FromLocation ORDER BY CONVERT(DATE,PS.ProductionDate) DESC";

            return _sqlRepository.GetDataCollection(sql);
        }

        #region Production Booking WIP 
        public List<Dictionary<string, object>> GetWIPInWC(string FDUD, string EntityId, string WorkCenterMasterId, string ProcessId, string date)
        {
            if (FDUD == "FD")
                FDUD = "=";
            else
                FDUD = "<=";

            string sql = @"SELECT ps.ProductionOrderId,FORMAT(PS.ProductionDate,'dd-MMM-yyyy') AS ProductionDate, 
                            ISNULL(s.UserName,wcm.UserName) AS [FromLocation],ISNULL(sTo.UserName,wcmTo.UserName) AS [ToLocation],sum(ps.Quantity) AS Quantity," + CommonColumns() + @"
                              FROM trn.ProductionSummary AS ps
                            LEFT JOIN scs.WorkCenterMaster AS wcm  ON wcm.Id=ps.WorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS s ON s.Id=ps.FromSFGInventoryId
                            LEFT JOIN hkp.Process AS p ON p.Id=ps.ProcessId

                            LEFT JOIN scs.WorkCenterMaster AS wcmTO  ON wcmTO.Id=ps.ToWorkCenterMasterId
                            LEFT JOIN hkp.SFGInventory AS sTO ON sTO.Id=ps.ToSFGInventoryId
                            LEFT JOIN hkp.Process AS pTo ON pTo.Id=ps.ToProcessId

                            join trn.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS SS ON SS.Id=po.ProductionStatusId
                            WHERE (isnull(SS.StandardName,'')<>'Closed'  OR convert(date,po.ClosingDate)>CONVERT(DATE,'" + date + @"')) AND ps.EntityId='" + EntityId + @"' AND isnull(ps.ToWorkCenterMasterId,'')='" + WorkCenterMasterId + @"' AND ps.toProcessId='" + ProcessId + @"'
                             AND  ps.ProductionDate" + FDUD + @"'" + date + @"' AND ps.ProductionGrade='A'
                            GROUP BY PS.ProductionDate,ps.ProductionOrderId,s.UserName,wcm.UserName,sTo.UserName,wcmTo.UserName ORDER BY PS.ProductionDate DESC";

            return _sqlRepository.GetDataCollection(sql);
        }
        #endregion

        #region Hourly Production Display
        public class EntityHourlyProductionInfo
        {
            public string GridId { get; set; }
            public string EntityId { get; set; }
            public string EntityName { get; set; }
            public object Data = null;
        };
        public List<EntityHourlyProductionInfo> HourlyProductionBookingPeriod(string Date, string PlantId, string ProcessId, string EntityId)
        {
            List<EntityHourlyProductionInfo> info = new List<EntityHourlyProductionInfo>();
            try
            {
                var entity = "";
                //Date = "05-Aug-2021";
                if (!string.IsNullOrEmpty(EntityId))
                {
                    entity = "and ps.EntityId = '" + EntityId + "'";
                }
                var sql = @"select wc.UserName WorkCenterMasterName,WC.EntityId,E.UserName AS Entity,pb.UserName ProductionBookingPeriodName ,sum(ps.Quantity)Quantity,pb.Id ProductionBookingPeriodId,wc.Id WorkCenterMasterId
                            From HKP.ProductionBookingPeriod pb
                            left join TRN.ProductionSummary ps on ps.ProductionBookingPeriodId = pb.Id
                            left join SCS.WorkCenterMaster wc on wc.Id = ps.WorkCenterMasterId
                            left join org.Entity E on e.Id=wc.EntityId
                            where ps.ProductionDate = '" + Date + "' and ps.PlantId = '" + PlantId + "' and ps.ProcessId = '" + ProcessId + "' " + entity + @"
                            group by pb.UserName, wc.UserName, pb.Id, wc.Id, wc.Sequence, pb.Sequence,WC.EntityId,E.UserName
                            order by E.UserName,wc.Sequence,pb.Sequence  ";

                DataTable dt = _sqlRepository.GetDataTable(sql);

                DataTable dtAllPeriod = _sqlRepository.GetDataTable("Select * from HKP.ProductionBookingPeriod ORDER BY [Sequence]");

                DataTable dtPivot = new DataTable("Temp");
                dtPivot.Columns.Add("EntityId");
                dtPivot.Columns.Add("Entity");
                dtPivot.Columns.Add("WorkCenterMasterId");
                dtPivot.Columns.Add("WC Name");
                dtPivot.Columns.Add("Total", typeof(double));
                foreach (DataRow item in dtAllPeriod.Rows)
                    dtPivot.Columns.Add(item["UserName"].ToString(), typeof(double));


                string WCId = "";
                DataRow dr = null;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (WCId != dt.Rows[i]["WorkCenterMasterId"].ToString())
                    {
                        dr = dtPivot.NewRow();
                        dr["WorkCenterMasterId"] = dt.Rows[i]["WorkCenterMasterId"].ToString();
                        dr["EntityId"] = dt.Rows[i]["EntityId"].ToString();
                        dr["Entity"] = dt.Rows[i]["Entity"].ToString();
                        dr["WC Name"] = dt.Rows[i]["WorkCenterMasterName"].ToString();
                        dtPivot.Rows.Add(dr);

                        dr = dtPivot.Rows[dtPivot.Rows.Count - 1];
                    }

                    dr[dt.Rows[i]["ProductionBookingPeriodName"].ToString()] = clsStaticInfo.dbl(dt.Rows[i]["Quantity"].ToString());
                    WCId = dt.Rows[i]["WorkCenterMasterId"].ToString();
                }

                for (int i = 0; i < dtPivot.Rows.Count; i++)
                {
                    double total = 0;
                    for (int COL = 5; COL < dtPivot.Columns.Count; COL++)
                        total += clsStaticInfo.dbl(dtPivot.Rows[i][COL].ToString());

                    dtPivot.Rows[i]["Total"] = total;
                }


                // Dictionary<string, object> dicData = new Dictionary<string, object>();


                StringCollection strCol = new StringCollection();
                for (int i = 0; i < dtPivot.Rows.Count; i++)
                {
                    if (strCol.Contains(dtPivot.Rows[i]["EntityId"].ToString()))
                        continue;
                    strCol.Add(dtPivot.Rows[i]["EntityId"].ToString());

                    dtPivot.DefaultView.RowFilter = "EntityId='" + dtPivot.Rows[i]["EntityId"].ToString() + "'";
                    DataTable dtTemp = dtPivot.DefaultView.ToTable();

                    //dicData.Add(dtPivot.Rows[i]["Entity"].ToString(), Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtTemp));

                    info.Add(new EntityHourlyProductionInfo { GridId = "GridHourly" + dtPivot.Rows[i]["EntityId"].ToString(), EntityId = dtPivot.Rows[i]["EntityId"].ToString(), EntityName = dtPivot.Rows[i]["Entity"].ToString(), Data = Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtTemp) });
                }
                return info;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #endregion

    }
}
