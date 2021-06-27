using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Planning.OrderManagement
{

    public class Bulletin
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        #region Constructor
        public Bulletin()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }
        #endregion Constructor



        private string BulletinSql(string entityid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @" 

SELECT PLN.UserName AS Plant,E.UserName AS Entity,
              po.Id AS ProductionOrderID,ps.UserName AS ProductionOrderStatus,
                  BT.Id BulletinTemplateId, BT.BulletinName,BM.ProcessId,P.UserName AS BulletinProcessName,
                         BM.RequiredStdTarget, BM.PlannedHoursPerDay, BM.MaxNoOfWS,ProductionBulletinTemplateId,
                        sk.Code AS SkillCode,
                         
                         BMD.Sequence,OPRN.UserName AS Operation, BMD.OperationVariationId,OV.UserName AS OperationVariation,
                         BMD.SkillId,sk.UserName AS Skill, BMD.MachineVarientId,MV.StandardName AS MachineVarient,
                         BMD.FGZoneId,fz.UserName AS FGZone, BMD.FGComponentId,FGC.UserName AS FGComponen, BMD.AdditionalSPT,
                         BMD.TotalSPT, BMD.AllotedWorkstation, BMD.AllotedManpower,
                         BMD.AttachmentId, ATC.UserName AS Attachment, BMD.GaugeFolderId,GF.UserName AS GaugeFolder,
                         BMD.OperationConsumptionId, OCO.UserName AS OperationConsumption, BMD.OperationTypeId,OT.UserName AS OperationType,
                         BMD.Frequency, BMD.Remark, BMD.OperationCategoryId,OPC.UserName AS OperationCategory,
                         BMD.QualityLevel,BMD.OperationGroup,
                         
                               BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path('') ), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path('') ), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path('') ), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path('') ), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path('') ), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path('') ), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path('') ), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path('') ), 1, 1, ''),
			                                                    
                             StyleGroup=STUFF((select distinct ','+xmoi.ProductionGrouping from 
	                                                                             trn.SalesOrder XSO 
		                                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                   
			                                                                                where PO.Id=Xpod.ProductionOrderId	for xml path('') ), 1, 1, ''),
			                                                    
                            MasterOrderNo=STUFF((select distinct ','+XMO.MasterOrderNo from 
	                                                                                trn.SalesOrder XSO 
		                                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                                               left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                                                             where PO.Id=Xpod.ProductionOrderId	for xml path('') ), 1, 1, '')
			                                             
                                                              
                      

                            from trn.ProductionOrder PO
                            
                      JOIN trn.ProductionBulletinTemplate AS BT ON BT.ProductionOrderId=po.Id
                             LEFT OUTER JOIN trn.ProductionBulletinTemplateMaster AS BM ON bm.ProductionBulletinTemplateId=BT.Id
                             LEFT OUTER JOIN trn.ProductionBulletinTemplateDetail AS BMD ON BMD.ProductionBulletinTemplateMasterId=BM.Id
							 LEFT OUTER JOIN hkp.Process AS p ON p.Id=bm.ProcessId
							 
							LEFT OUTER JOIN HKP.Attachment AS ATC ON ATC.Id=BMD.AttachmentId
							LEFT OUTER JOIN HKP.FGComponent AS FGC ON FGC.Id=BMD.FGComponentId
							LEFT OUTER JOIN HKP.FGZone AS FZ  ON FZ.Id=BMD.FGZoneId
							LEFT OUTER JOIN HKP.GaugeFolder AS GF ON GF.Id=BMD.GaugeFolderId
							LEFT OUTER JOIN HKP.OperationCategory AS OPC ON OPC.Id=BMD.OperationCategoryId
							LEFT OUTER JOIN HKP.OperationConsumption AS OCO ON OCO.Id=BMD.OperationConsumptionId
							LEFT OUTER JOIN HKP.OperationType AS OT ON OT.Id=BMD.OperationTypeId
							LEFT OUTER JOIN MST.OperationVariation AS OV ON OV.Id=BMD.OperationVariationId
							LEFT OUTER JOIN MST.Operation AS OPRN ON OPRN.Id=OV.OperationId
							LEFT OUTER JOIN hkp.Skill AS sk ON sk.Id=BMD.SkillId
							LEFT OUTER JOIN mst.MaterialMasterArticle AS MV ON MV.Id=BMD.MachineVarientId

							LEFT JOIN mst.OperationMaster AS om ON om.Id=bmd.OperationMasterId   
							
	
							
                            left outer join org.Entity E  on e.Id=PO.EntityID
                            LEFT OUTER JOIN org.Unit AS u ON u.Id=e.UnitId
                            left outer join org.Plant PLN on pln.Id=PO.PlantId
							LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
         WHERE ISNULL(ps.UserName,'')<>'Closed'  and po.EntityId in (" + entityid + @")          
ORDER BY PLN.Sequence,e.UserName,po.Id,P.Sequence,bmd.Sequence"
;
        }

        public void BulletinReport(string entityid)
        {
            try
            {

                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");

                string sql = BulletinSql(entityid);

                //Instantiate the Excel application object
                DataTable dtBulletin = _sqlRepository.GetDataTable(sql);
                if (dtBulletin.Rows.Count == 0)
                    throw new Exception("No data found");
                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Bulletin Report";


                int ROW = 6;
                int COL = 1;


                sheet[ROW, COL].Text = "Plant";

                sheet[ROW, COL].ColumnWidth = 10;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";

                sheet[ROW, COL].ColumnWidth = 10;
                int colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order Status";
                sheet[ROW, COL].ColumnWidth = 14;
                int colProductionOrderStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Buyer Ref No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBuyerRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Ref No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOwnRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Item No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Item No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOwnStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "SO No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSONo = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Description";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSODesc = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Item Group";
                sheet[ROW, COL].ColumnWidth = 10;
                int colStyleGroup = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Bulletin Id";
                sheet[ROW, COL].ColumnWidth = 8;
                int colBulletinId = COL;
                COL++;
                sheet[ROW, COL].Text = "Bulletin Name";
                sheet[ROW, COL].ColumnWidth = 15;
                int colBulletinName = COL;
                COL++;
                sheet[ROW, COL].Text = "Bulletin Process Name";
                sheet[ROW, COL].ColumnWidth = 14;
                int colBulletinProcessName = COL;
                COL++;
                sheet[ROW, COL].Text = "Required Std Target";
                sheet[ROW, COL].ColumnWidth = 10;
                int colRequiredStdTarget = COL;
                COL++;
                sheet[ROW, COL].Text = "Planned Hours Per Day";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlannedHoursPerDay = COL;
                COL++;
                sheet[ROW, COL].Text = "Max No Of WS";
                sheet[ROW, COL].ColumnWidth = 10;
                int colMaxNoOfWS = COL;
                COL++;
                sheet[ROW, COL].Text = "Operation Sequence";
                sheet[ROW, COL].ColumnWidth = 8;
                int colSequence = COL;
                COL++;
                sheet[ROW, COL].Text = "Operation";
                sheet[ROW, COL].ColumnWidth = 14;
                int colOperation = COL;
                COL++;
                sheet[ROW, COL].Text = "Operation Variation";
                sheet[ROW, COL].ColumnWidth = 14;
                int colOperationVariation = COL;
                COL++;
                sheet[ROW, COL].Text = "Skill Code";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSkillCode = COL;
                COL++;
                sheet[ROW, COL].Text = "Skill";
                sheet[ROW, COL].ColumnWidth = 14;
                int colSkill = COL;
                COL++;
                sheet[ROW, COL].Text = "Machine Varient";
                sheet[ROW, COL].ColumnWidth = 10;
                int colMachineVarient = COL;

                COL++;
                sheet[ROW, COL].Text = "FG Zone";
                sheet[ROW, COL].ColumnWidth = 10;
                int colFGZone = COL;
                COL++;
                sheet[ROW, COL].Text = "FG Component";
                sheet[ROW, COL].ColumnWidth = 10;
                int colFGComponen = COL;
                COL++;
                sheet[ROW, COL].Text = "Additional SPT";

                sheet[ROW, COL].ColumnWidth = 10;
                int colAdditionalSPT = COL;
                COL++;
                sheet[ROW, COL].Text = "Total SPT";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTotalSPT = COL;
                COL++;
                sheet[ROW, COL].Text = "Alloted Work station";
                sheet[ROW, COL].ColumnWidth = 9;
                int colAllotedWorkstation = COL;
                COL++;
                sheet[ROW, COL].Text = "Alloted Man Power";
                sheet[ROW, COL].ColumnWidth = 9;
                int colAllotedManpower = COL;
                COL++;
                sheet[ROW, COL].Text = "Attachment";
                sheet[ROW, COL].ColumnWidth = 10;
                int colAttachment = COL;
                COL++;
                sheet[ROW, COL].Text = "Gauge Folder";
                sheet[ROW, COL].ColumnWidth = 10;
                int colGaugeFolder = COL;
                COL++;
                sheet[ROW, COL].Text = "Operation Consumption";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOperationConsumption = COL;
                COL++;
                sheet[ROW, COL].Text = "Operation Type";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOperationType = COL;
                COL++;
                sheet[ROW, COL].Text = "Frequency";
                sheet[ROW, COL].ColumnWidth = 10;
                int colFrequency = COL;
                COL++;
                sheet[ROW, COL].Text = "Remark";
                sheet[ROW, COL].ColumnWidth = 10;
                int colRemark = COL;
                COL++;
                sheet[ROW, COL].Text = "Operation Category";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOperationCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Quality Level";
                sheet[ROW, COL].ColumnWidth = 10;
                int colQualityLevel = COL;
                COL++;
                sheet[ROW, COL].Text = "Operation Group";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOperationGroup = COL;

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                int StartRow = ROW; //row 20
                for (int i = 0; i < dtBulletin.Rows.Count; i++)
                {

                    sheet[ROW, colPlant].Text = dtBulletin.Rows[i]["Plant"].ToString();
                    sheet[ROW, colEntity].Text = dtBulletin.Rows[i]["Entity"].ToString();

                    sheet[ROW, colProductionOrderNo].Text = dtBulletin.Rows[i]["ProductionOrderID"].ToString();
                    sheet[ROW, colProductionOrderStatus].Text = dtBulletin.Rows[i]["ProductionOrderStatus"].ToString();
                    sheet[ROW, colBulletinId].Text = dtBulletin.Rows[i]["ProductionBulletinTemplateId"].ToString();
                    sheet[ROW, colBulletinName].Text = dtBulletin.Rows[i]["BulletinName"].ToString();
                    sheet[ROW, colBulletinProcessName].Text = dtBulletin.Rows[i]["BulletinProcessName"].ToString();
                    sheet[ROW, colRequiredStdTarget].Number = clsStaticInfo.dbl(dtBulletin.Rows[i]["RequiredStdTarget"].ToString());
                    sheet[ROW, colRequiredStdTarget].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colPlannedHoursPerDay].Number = clsStaticInfo.dbl(dtBulletin.Rows[i]["PlannedHoursPerDay"].ToString());
                    sheet[ROW, colPlannedHoursPerDay].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colMaxNoOfWS].Number = clsStaticInfo.dbl(dtBulletin.Rows[i]["MaxNoOfWS"].ToString());
                    sheet[ROW, colMaxNoOfWS].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colSequence].Number = clsStaticInfo.dbl(dtBulletin.Rows[i]["Sequence"].ToString());
                    sheet[ROW, colSequence].NumberFormat = "#,##0;(#,##0)";
                    sheet[ROW, colOperationVariation].Text = dtBulletin.Rows[i]["OperationVariation"].ToString();
                    sheet[ROW, colOperation].Text = dtBulletin.Rows[i]["Operation"].ToString();
                    sheet[ROW, colSkill].Text = dtBulletin.Rows[i]["Skill"].ToString();
                    sheet[ROW, colSkillCode].Text = dtBulletin.Rows[i]["SkillCode"].ToString();
                    sheet[ROW, colMachineVarient].Text = dtBulletin.Rows[i]["MachineVarient"].ToString();
                    sheet[ROW, colFGZone].Text = dtBulletin.Rows[i]["FGZone"].ToString();
                    sheet[ROW, colFGComponen].Text = dtBulletin.Rows[i]["FGComponen"].ToString();
                    sheet[ROW, colAdditionalSPT].Number = clsStaticInfo.dbl(dtBulletin.Rows[i]["AdditionalSPT"].ToString());
                    sheet[ROW, colAdditionalSPT].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colTotalSPT].Number = clsStaticInfo.dbl(dtBulletin.Rows[i]["TotalSPT"].ToString());
                    sheet[ROW, colTotalSPT].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colAllotedWorkstation].Number = clsStaticInfo.dbl(dtBulletin.Rows[i]["AllotedWorkstation"].ToString());
                    sheet[ROW, colAllotedWorkstation].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colAllotedManpower].Number = clsStaticInfo.dbl(dtBulletin.Rows[i]["AllotedManpower"].ToString());
                    sheet[ROW, colAllotedManpower].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colAttachment].Text = dtBulletin.Rows[i]["Attachment"].ToString();
                    sheet[ROW, colGaugeFolder].Text = dtBulletin.Rows[i]["GaugeFolder"].ToString();
                    sheet[ROW, colOperationConsumption].Text = dtBulletin.Rows[i]["OperationConsumption"].ToString();
                    sheet[ROW, colOperationType].Text = dtBulletin.Rows[i]["OperationType"].ToString();
                    sheet[ROW, colFrequency].Number = clsStaticInfo.dbl(dtBulletin.Rows[i]["Frequency"].ToString());
                    sheet[ROW, colFrequency].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colRemark].Text = dtBulletin.Rows[i]["Remark"].ToString();
                    sheet[ROW, colOperationCategory].Text = dtBulletin.Rows[i]["OperationCategory"].ToString();
                    sheet[ROW, colOperationGroup].Text = dtBulletin.Rows[i]["OperationGroup"].ToString();
                    sheet[ROW, colBuyerRefNo].Text = dtBulletin.Rows[i]["BuyerRefNo"].ToString();
                    sheet[ROW, colOwnRefNo].Text = dtBulletin.Rows[i]["OwnRefNo"].ToString();
                    sheet[ROW, colStyleNo].Text = dtBulletin.Rows[i]["StyleNo"].ToString();
                    sheet[ROW, colSONo].Text = dtBulletin.Rows[i]["SONo"].ToString();
                    sheet[ROW, colSODesc].Text = dtBulletin.Rows[i]["SODesc"].ToString();
                    sheet[ROW, colBuyer].Text = dtBulletin.Rows[i]["buyer"].ToString();
                    sheet[ROW, colCustomer].Text = dtBulletin.Rows[i]["Customer"].ToString();
                    sheet[ROW, colStyleGroup].Text = dtBulletin.Rows[i]["StyleGroup"].ToString();
                    sheet[ROW, colMasterOrderNo].Text = dtBulletin.Rows[i]["MasterOrderNo"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }

                //sheet.Range[StartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colPOValue, ROW, colPOValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colAcceptanceValue, ROW, colAcceptanceValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colGRNValue, ROW, colGRNValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Bulletin Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "BulletinReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string ProductionInfoSql(string entityid, string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            return @"  



select ORD.CM,
  wc.UserName AS  [LineNo],  ps.ProcessId ,ps.ProductionOrderId ,ps.Quantity,''WorkHour ,dr.DaysRun,
  buyer=STUFF((select distinct ','+XB.UserName from 
	                               trn.SalesOrder XSO 
		                               JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                               left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                               left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                               left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                           where ps.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
Item=STUFF((select distinct ','+XMM.UserName from 
	                               trn.SalesOrder XSO 
		                               JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                               left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                               left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                               left outer join MST.MaterialMaster XMM on XMM.Id=XMOI.MaterialMasterId
			                           where ps.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
									   
									  p.UserName AS Process,
									  
									  tr.TotalHour AS PlannedHoursPerDay,
									  TR.Manpower AS WithMachine,
									  0 AS WithoutMachine,
									  TR.SMV as SPT,
									  TR.Manpower AllotedManpower,TR.Manpower RequiredManPower,
									  TR.Manpower AllotedWorkstation,
									 TR.Quantity AS TargetPerDay,
									 TR.Quantity/tr.TotalHour AS  RequiredStdTarget
from(
select 
p.ProcessId,p.ProductionOrderId,p.WorkCenterMasterId,sum(p.Quantity) Quantity
 from  TRN.ProductionSummary as p 
  JOIN trn.ProductionOrderProcessSet AS Ps ON ps.ProductionOrderId=p.ProductionOrderId  AND ps.IsBaseProcess=1 and ps.ProcessId=p.ProcessId
where p.ProductionDate='" + Date + @"' 
and p.ProductionGrade='A'
group by p.WorkCenterMasterId,p.ProcessId,p.ProductionOrderId
) ps
LEFT JOIN (SELECT p.ProcessId,p.ProductionOrderId,p.WorkCenterMasterId,COUNT(DISTINCT p.ProductionDate) AS DaysRun  
           from  TRN.ProductionSummary as p 
           JOIN trn.ProductionOrderProcessSet AS Ps ON ps.ProductionOrderId=p.ProductionOrderId  AND ps.IsBaseProcess=1 and ps.ProcessId=p.ProcessId
           WHERE p.ProductionDate<='" + Date + @"'
           GROUP BY  p.ProcessId,p.ProductionOrderId,p.WorkCenterMasterId

) AS DR ON dr.ProcessId=ps.ProcessId
           AND dr.ProductionOrderId=ps.ProductionOrderId AND dr.WorkCenterMasterId=ps.WorkCenterMasterId
LEFT JOIN trn.DailyProductionTarget AS TR ON tr.ProductionOrderId=ps.ProductionOrderId AND tr.WorkCenterMasterID=ps.WorkCenterMasterId AND tr.TargetDate='" + Date + @"'
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
								                     	LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=C.BaseCurrencyId AND rer.PlantId='" + identity.PlantId + @"'
                                                        LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId='" + identity.PlantId + @"'
                                                        join trn.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
														LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
														 WHERE (isnull(s.StandardName,'')<>'Closed' OR convert(date,po.ClosingDate)>CONVERT(DATE,'" + Date + @"')) 
                                                  
                                                        group by POD.ProductionOrderId
                                                        ) AS ORD on ord.ProductionOrderID=ps.ProductionOrderId 

left join scs.WorkCenterMaster as wc on wc.id=ps.WorkCenterMasterId 
left join hkp.Process p on p.id=ps.ProcessId
where wc.EntityId IN (" + entityid + @") 
order by wc.Sequence


";

            return @"  

select 
  wc.UserName AS  [LineNo],  ps.ProcessId ,ps.ProductionOrderId ,ps.Quantity,''WorkHour ,
  buyer=STUFF((select distinct ','+XB.UserName from 
	                               trn.SalesOrder XSO 
		                               JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                               left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                               left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                               left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                           where ps.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
Item=STUFF((select distinct ','+XMM.UserName from 
	                               trn.SalesOrder XSO 
		                               JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                               left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                               left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                               left outer join MST.MaterialMaster XMM on XMM.Id=XMOI.MaterialMasterId
			                           where ps.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
									   
									  p.UserName AS Process,bulletin.PlannedHoursPerDay,bulletin.WithMachine,bulletin.WithoutMachine,bulletin.TotalSPT as SPT,
									   bulletin.AllotedManpower,bulletin.RequiredManPower,bulletin.AllotedWorkstation,bulletin.OperationTargetPerHr,
									   bulletin.RequiredStdTarget
from(
select 
p.ProcessId,p.ProductionOrderId,p.WorkCenterMasterId,sum(p.Quantity) Quantity
 from  TRN.ProductionSummary as p 
where --p.ProcessId='20204' and
 p.ProductionDate='" + Date + @"' 
and p.ProductionGrade='A'
group by p.WorkCenterMasterId,p.ProcessId,p.ProductionOrderId
) ps
left join (
			select
			pbt.ProductionOrderId ,pbt.Id ProductionBulletinTemplateId,pbtm.ProcessId,
			pbtm.PlannedHoursPerDay ,pbtm.RequiredStdTarget ,
			WithMachine = SUM(case when  isnull(pbtd.MachineVarientId,'')<>'' then AllotedManpower else 0 end),
			WithoutMachine = SUM(case when  isnull(pbtd.MachineVarientId,'')='' then AllotedManpower else 0 end),
			sum (pbtd.TotalSPT) TotalSPT
			,sum(pbtd.AllotedManpower)AllotedManpower,sum(pbtd.RequiredManPower) RequiredManPower,sum(pbtd.AllotedWorkstation) AllotedWorkstation
			,sum(pbtd.OperationTargetPerHr) OperationTargetPerHr
			from trn.ProductionBulletinTemplate as pbt
			left join trn.ProductionBulletinTemplateMaster pbtm on pbtm.ProductionBulletinTemplateId=pbt.Id
			left join trn.ProductionBulletinTemplateDetail pbtd on pbtd.ProductionBulletinTemplateMasterId=pbtm.Id
			
			group by pbt.Id,pbt.ProductionOrderId,pbtm.ProcessId ,pbtm.PlannedHoursPerDay,pbtm.RequiredStdTarget

		) as bulletin on bulletin.ProductionOrderId=ps.ProductionOrderId AND bulletin.ProcessId=ps.ProcessId

left join scs.WorkCenterMaster as wc on wc.id=ps.WorkCenterMasterId 
left join hkp.Process p on p.id=ps.ProcessId
where wc.EntityId IN (" + entityid + @") 
order by wc.Sequence

"
;
        }
        public void ProductionEfficiencyReport(string PlantId, string entityid, string Date)
        {
            try
            {

                if (string.IsNullOrEmpty(entityid) || entityid.ToUpper() == "NULL")
                {
                    DataTable dtEntityList = _sqlRepository.GetDataTable("select Id from org.Entity where PlantId='" + PlantId + "'");
                    entityid = "''";
                    for (int i = 0; i < dtEntityList.Rows.Count; i++)
                        entityid += ",'" + dtEntityList.Rows[i]["Id"].ToString() + "'";
                }
                else
                {
                    entityid = "'" + entityid + "'";
                }

                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");

                string sql = ProductionInfoSql(entityid, Date);

                //Instantiate the Excel application object
                DataTable dtProductionInfo = _sqlRepository.GetDataTable(sql);
                if (dtProductionInfo.Rows.Count == 0)
                    throw new Exception("No data found");
                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Production Information Report";


                int ROW = 6;
                int COL = 1;


                sheet[ROW, COL].Text = "Line No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLineNo = COL;
                sheet.Range[ROW, colLineNo, ROW + 1, colLineNo].Merge();
                COL++;

                sheet[ROW, COL].Text = "Production Order Id";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProductionOrderID = COL;
                sheet.Range[ROW, colProductionOrderID, ROW + 1, colProductionOrderID].Merge();

                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 15;
                int colBuyer = COL;
                sheet.Range[ROW, colBuyer, ROW + 1, colBuyer].Merge();

                COL++;
                sheet[ROW, COL].Text = "Item Name";
                sheet[ROW, COL].ColumnWidth = 15;
                int colItem = COL;
                sheet.Range[ROW, colItem, ROW + 1, colItem].Merge();

                COL++;
                sheet[ROW, COL].Text = "SPT";
                sheet[ROW, COL].ColumnWidth = 7;
                int colSPT = COL;
                sheet.Range[ROW, colSPT, ROW + 1, colSPT].Merge();

                COL++;

                sheet[ROW, COL].Text = "Days Run";
                sheet[ROW, COL].ColumnWidth = 10;
                int colDaysRun = COL;
                sheet.Range[ROW, colDaysRun, ROW + 1, colDaysRun].Merge();

                COL++;
                sheet[ROW, COL].Text = "TGT/hr";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTGThr = COL;
                sheet.Range[ROW, colTGThr, ROW + 1, colTGThr].Merge();
                COL++;

                sheet[ROW, COL].Text = "Work Hour";
                sheet[ROW, COL].ColumnWidth = 10;
                int colWorkHour = COL;
                sheet.Range[ROW, colWorkHour, ROW + 1, colWorkHour].Merge();
                COL++;

                sheet[ROW + 1, COL].Text = "RUN M/C";
                sheet[ROW + 1, COL].ColumnWidth = 10;
                int colRUNmc = COL;
                COL++;
                sheet[ROW + 1, COL].Text = "Hel.";
                sheet[ROW + 1, COL].ColumnWidth = 10;
                int colHel = COL;

                COL++;
                sheet[ROW + 1, COL].Text = "Total";
                sheet[ROW + 1, COL].ColumnWidth = 10;
                int colTotalMP = COL;

                sheet[ROW, colRUNmc].Text = "Man Power";
                sheet.Range[ROW, colRUNmc, ROW, colTotalMP].Merge();

                COL++;
                sheet[ROW, COL].Text = "TGT EFF %";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTGTEFF = COL;
                sheet.Range[ROW, colTGTEFF, ROW + 1, colTGTEFF].Merge();

                COL++;
                sheet[ROW, COL].Text = "TGT/Day";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTGTDAY = COL;
                sheet.Range[ROW, colTGTDAY, ROW + 1, colTGTDAY].Merge();

                COL++;
                sheet[ROW + 1, COL].Text = "Total";
                sheet[ROW + 1, COL].ColumnWidth = 10;
                int colTotalP = COL;

                COL++;
                sheet[ROW + 1, COL].Text = "G.Hour";
                sheet[ROW + 1, COL].ColumnWidth = 10;
                int colGhour = COL;

                COL++;
                sheet[ROW + 1, COL].Text = "OT Hour";
                sheet[ROW + 1, COL].ColumnWidth = 8;
                int colOThour = COL;

                sheet[ROW, colTotalP].Text = "Production";
                sheet.Range[ROW, colTotalP, ROW, colOThour].Merge();
                COL++;
                sheet[ROW, COL].Text = "Avg./hr";
                sheet[ROW, COL].ColumnWidth = 15;
                int colAvgHr = COL;
                sheet.Range[ROW, colAvgHr, ROW + 1, colAvgHr].Merge();

                COL++;
                sheet[ROW, COL].Text = "Target Achievement";
                sheet[ROW, COL].ColumnWidth = 14;
                int colTargetAchievement = COL;
                sheet.Range[ROW, colTargetAchievement, ROW + 1, colTargetAchievement].Merge();

                COL++;
                sheet[ROW, COL].Text = "Variance";
                sheet[ROW, COL].ColumnWidth = 10;
                int colVariance = COL;
                sheet.Range[ROW, colVariance, ROW + 1, colVariance].Merge();
                COL++;
                sheet[ROW, COL].Text = "Produce min";
                sheet[ROW, COL].ColumnWidth = 10;
                int colProduceMin = COL;
                sheet.Range[ROW, colProduceMin, ROW + 1, colProduceMin].Merge();
                COL++;
                sheet[ROW, COL].Text = "Available min.";
                sheet[ROW, COL].ColumnWidth = 10;
                int colAvailableMin = COL;
                sheet.Range[ROW, colAvailableMin, ROW + 1, colAvailableMin].Merge();
                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].ColumnWidth = 6;
                int colCM = COL;
                sheet.Range[ROW, colCM, ROW + 1, colCM].Merge();
                COL++;
                sheet[ROW, COL].Text = "CM Target";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCMTarget = COL;
                sheet.Range[ROW, colCMTarget, ROW + 1, colCMTarget].Merge();
                COL++;
                sheet[ROW, COL].Text = "CM Earned";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCMEarned = COL;
                sheet.Range[ROW, colCMEarned, ROW + 1, colCMEarned].Merge();
                COL++;
                sheet[ROW, COL].Text = "CM Spend";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCMSpend = COL;
                sheet.Range[ROW, colCMSpend, ROW + 1, colCMSpend].Merge();
                COL++;
                sheet[ROW, COL].Text = "CM Margin";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCMMargin = COL;
                sheet.Range[ROW, colCMMargin, ROW + 1, colCMMargin].Merge();

                COL++;
                sheet[ROW, COL].Text = "Efficiency %";
                sheet[ROW, COL].ColumnWidth = 10;
                int colEfficiency = COL;
                sheet.Range[ROW, colEfficiency, ROW + 1, colEfficiency].Merge();

                COL++;
                sheet[ROW, COL].Text = "Remarks/Problems";
                sheet[ROW, COL].ColumnWidth = 10;
                int colRemarksProblemms = COL;
                sheet.Range[ROW, colRemarksProblemms, ROW + 1, colRemarksProblemms].Merge();

                COL++;
                sheet[ROW + 1, COL].Text = "Target";
                sheet[ROW + 1, COL].ColumnWidth = 8;
                int colTarget = COL;

                COL++;
                sheet[ROW + 1, COL].Text = "Actual";
                sheet[ROW + 1, COL].ColumnWidth = 14;
                int colActual = COL;

                COL++;
                sheet[ROW + 1, COL].Text = "Achievement";
                sheet[ROW + 1, COL].ColumnWidth = 14;
                int colAchievement = COL;

                COL++;
                sheet[ROW + 1, COL].Text = "Remarks";
                sheet[ROW + 1, COL].ColumnWidth = 10;
                int colRemarks = COL;

                sheet[ROW, colTarget].Text = "FINISHING";
                sheet.Range[ROW, colTarget, ROW, colRemarks].Merge();

                int endCol = COL;

                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW += 2;

                int StartRow = ROW; //row 20
                for (int i = 0; i < dtProductionInfo.Rows.Count; i++)
                {
                    sheet[ROW, colLineNo].Text = dtProductionInfo.Rows[i]["LineNo"].ToString();
                    sheet[ROW, colProductionOrderID].Text = dtProductionInfo.Rows[i]["ProductionOrderId"].ToString();
                    sheet[ROW, colBuyer].Text = dtProductionInfo.Rows[i]["buyer"].ToString();
                    sheet[ROW, colItem].Text = dtProductionInfo.Rows[i]["Item"].ToString();
                    sheet[ROW, colSPT].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["SPT"].ToString());
                    sheet[ROW, colWorkHour].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["PlannedHoursPerDay"].ToString());
                    sheet[ROW, colRUNmc].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["WithMachine"].ToString());
                    sheet[ROW, colHel].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["WithoutMachine"].ToString());
                    sheet[ROW, colTotalMP].Formula = clsStaticInfo.GetxlsCol(colRUNmc) + ROW.ToString() + "+" + clsStaticInfo.GetxlsCol(colHel) + ROW.ToString();
                    sheet[ROW, colDaysRun].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["DaysRun"].ToString());

                    sheet[ROW, colTGThr].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["RequiredStdTarget"].ToString());
                    

                    sheet[ROW, colTGTEFF].Formula = "if(and(" + clsStaticInfo.GetxlsCol(colTotalMP) + ROW.ToString() + ">0," + clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString() + ">0," + clsStaticInfo.GetxlsCol(colSPT) + ROW.ToString() + @">0)," 
                        + clsStaticInfo.GetxlsCol(colTGTDAY) + ROW.ToString() + "/(" + clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString()
                        + "*60*" + clsStaticInfo.GetxlsCol(colTotalMP) + ROW.ToString() + "/" + clsStaticInfo.GetxlsCol(colSPT) + ROW.ToString() + ")*100,0)";

                    sheet[ROW, colTGTDAY].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["TargetPerDay"].ToString());
                    sheet[ROW, colTotalP].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["Quantity"].ToString());
                    // //sheet[ROW, colTotalP].NumberFormat = "#,##0.00;(#,##0.00)";
                    // sheet[ROW, colGhour].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["MaxNoOfWS"].ToString());
                    // sheet[ROW, colOThour].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["MaxNoOfWS"].ToString());
                    //// sheet[ROW, colOThour].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colAvgHr].Formula = "IF(" + clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString() + ">0," + clsStaticInfo.GetxlsCol(colTotalP) + ROW.ToString() + "/" + clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString() + ",0)";
                    // sheet[ROW, colTargetAchievement].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["Sequence"].ToString());

                    sheet[ROW, colTargetAchievement].Formula = "IF(" + clsStaticInfo.GetxlsCol(colTGTDAY) + ROW.ToString() + ">0," + clsStaticInfo.GetxlsCol(colTotalP) + ROW.ToString() + "/" + clsStaticInfo.GetxlsCol(colTGTDAY) + ROW.ToString() + "*" + 100 + ",0)";
                    sheet[ROW, colVariance].Formula = clsStaticInfo.GetxlsCol(colTotalP) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colTGTDAY) + ROW.ToString();

                    sheet[ROW, colProduceMin].Formula = clsStaticInfo.GetxlsCol(colTotalP) + ROW.ToString() + "*" + clsStaticInfo.GetxlsCol(colSPT) + ROW.ToString();
                    sheet[ROW, colAvailableMin].Formula = clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString() + "*" + clsStaticInfo.GetxlsCol(colTotalMP) + ROW.ToString() + "*" + 60;
                    sheet[ROW, colCM].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["CM"].ToString());
                    sheet[ROW, colCMTarget].Formula = clsStaticInfo.GetxlsCol(colCM) + ROW.ToString() + "*" + clsStaticInfo.GetxlsCol(colTGThr) + ROW.ToString() + "*" + clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString();
                    sheet[ROW, colCMEarned].Formula = clsStaticInfo.GetxlsCol(colCM) + ROW.ToString() + "*" + clsStaticInfo.GetxlsCol(colTotalP) + ROW.ToString();
                    sheet[ROW, colCMSpend].Formula = "IF(" + clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString() + @">0," + (25 + "*" + clsStaticInfo.GetxlsCol(colRUNmc) + ROW.ToString()) + "/" + 11 + "*" + clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString() + @",0)";
                    sheet[ROW, colCMMargin].Formula = clsStaticInfo.GetxlsCol(colCMEarned) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colCMSpend) + ROW.ToString();

                    sheet[ROW, colEfficiency].Formula = "IF(" + clsStaticInfo.GetxlsCol(colAvailableMin) + ROW.ToString() + ">0," + clsStaticInfo.GetxlsCol(colTotalP) + ROW.ToString() + "*" + clsStaticInfo.GetxlsCol(colSPT) + ROW.ToString() + "/" + clsStaticInfo.GetxlsCol(colAvailableMin) + ROW.ToString() + "*100" + ",0)";
                    // sheet[ROW, colRemarksProblemms].Text = dtProductionInfo.Rows[i]["Skill"].ToString();
                    // sheet[ROW, colTarget].Text = dtProductionInfo.Rows[i]["SkillCode"].ToString();
                    // sheet[ROW, colActual].Text = dtProductionInfo.Rows[i]["MachineVarient"].ToString();
                    // sheet[ROW, colAchievement].Text = dtProductionInfo.Rows[i]["FGZone"].ToString();
                    // sheet[ROW, colRemarks].Text = dtProductionInfo.Rows[i]["FGComponen"].ToString();



                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }

                sheet.Range[StartRow, colSPT, ROW, colSPT].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colTGThr, ROW, colTGThr].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colWorkHour, ROW, colWorkHour].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colRUNmc, ROW, colRUNmc].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colHel, ROW, colHel].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colTotalMP, ROW, colTotalMP].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colTargetAchievement, ROW, colTargetAchievement].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colProduceMin, ROW, colProduceMin].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colAvailableMin, ROW, colAvailableMin].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colCM, ROW, colCM].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colCMTarget, ROW, colCMTarget].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colCMSpend, ROW, colCMSpend].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colCMEarned, ROW, colCMEarned].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colCMMargin, ROW, colCMMargin].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colEfficiency, ROW, colEfficiency].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colTGTEFF, ROW, colTGTEFF].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();



                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Production Information Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[6, 1, 7, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[6, 1, 7, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                string strFileName = "Production Information Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }


    }
}
