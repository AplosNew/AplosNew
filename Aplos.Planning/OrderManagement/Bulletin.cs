using ConnectionManager;
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

        private string DailyProductionInfoSql(string PlantId, string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return @"select WCM.EntityId,E.UserName EntityName, WCM.UserName Line ,WCM.PlanEfficiency
							,DPT.ManPowerWithMachine,DPT.ManPowerWithHand,DPT.Manpower
							,WCM.id WorkCenterMasterId
							,TodayTGT=(DPT.Manpower*60*WCM.PlanEfficiency*DPT.TotalHour)/DPT.SMV
							,DPT.ManpowerBulletin
						    ,DPT.SMV,DPT.Quantity,DPT.TotalHour,DPT.TargetDate
						    ,PO.id PRNo,MM.Id MaterialMasterId
							,AllocatedQty=	case when ISNULL(S.Qty,0)>0 then S.Qty else PO.Qty end
							,PS.Quantity PreviousDayQCpass,S.PlanWorkingHoursPerDay,S.TargetPerHour,
						     case when isnull(PRO.IsFirst,0)=0 THEN prsum.InQuantity- prsum.OutQuantity- prsum.KillQuantity ELSE NULL END AS WIP,
                             SO.CM,
                                             BuyerOrderRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
															
											 StyleDescription =STUFF((select distinct ','+MOI.BuyerItemDescription from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                             OwnOrderRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
											                         	 trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

					                          
                                                 
                                             Buyer=STUFF((select distinct ','+XB.UserName from 
														 			  trn.SalesOrder XSO 
														 	JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
														 	left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
														 	left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
														 	left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
														  where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											NextBuyer=(
														select top 1 XB.UserName AS NextBuyer from 
														             trn.DailyProductionTarget TX 
														join trn.ProductionOrderDetail PODX ON PODX.ProductionOrderId=TX.ProductionOrderId and podX.Id=(select TOP 1 Id from TRN.ProductionOrderDetail DX where DX.ProductionOrderId=TX.ProductionOrderId)
														join trn.SalesOrder SOX ON SOX.Id=PODX.SalesOrderId
														join trn.MasterOrderItem MOIX ON MOIX.Id=soX.MasterOrderItemId
														join trn.MasterOrder XMO on Xmo.Id=MOIX.MasterOrderId
														join [HKP].Buyer XB on XB.Id=XMO.BuyerId
														where tx.TargetDate>'" + Date + @"'
														and tx.WorkCenterMasterID=wcm.Id  
														and XB.Id<>MO.BuyerId
												      )

                                from SCS.WorkCenterMaster WCM 
								left join hkp.Process PRO on PRO.Id=WCM.ProcessId
                                left outer join TRN.DailyProductionTarget DPT on dpt.WorkCenterMasterID=WCM.Id  and  DPT.TargetDate='" + Date + @"'
                                left outer join TRN.ProductionOrder PO on PO.Id=DPT.ProductionOrderId  
                                left join trn.ProductionOrderDetail POD ON POD.ProductionOrderId=po.Id and pod.Id=(select TOP 1 Id from TRN.ProductionOrderDetail D where D.ProductionOrderId=PO.Id)
                                left join(select id,MasterOrderItemId, sum(Qty*Rate)/sum(Qty) as CM  from  trn.SalesOrder  
								group by id,MasterOrderItemId								
								) SO on SO.Id=POD.SalesOrderId
                                left join trn.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
                                left join trn.MasterOrder MO ON MO.Id=MOI.MasterOrderId

                                left join mst.MaterialMaster MM ON MM.Id=MOI.MaterialMasterId
                                left join mst.MaterialMasterArticle MMA ON MMA.Id=MOI.ArticleId
								left outer join ORG.Entity E on E.Id=WCM.EntityId
								left outer join ProductionOrderSchedulingParametersType1 S on s.ProductionOrderID=po.Id
								left outer join(select 
										p.ProcessId,p.ProductionOrderId,
										p.WorkCenterMasterId,sum(p.Quantity) Quantity
										from  TRN.ProductionSummary as p 
										JOIN trn.ProductionOrderProcessSet AS Ps ON ps.ProductionOrderId=p.ProductionOrderId  AND ps.IsBaseProcess=1 and ps.ProcessId=p.ProcessId
										where p.ProductionDate<'" + Date + @"' 
										and p.ProductionGrade='A'
										group by p.WorkCenterMasterId,p.ProcessId,p.ProductionOrderId
										)PS on PS.WorkCenterMasterId=WCM.id and ps.ProductionOrderId=PO.Id
								left outer join								                            (	
                                select ProductionOrderId,WorkCenterMasterId,SUM(InQuantity) AS InQuantity,SUM(OutQuantity) AS OutQuantity,SUM(KillQuantity) AS KillQuantity   from 
			                        (SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS InQuantity,0 AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE convert(date,ps.ProductionDate)<=convert(date,'" + Date + @"') 

			                         union all 
			 
			                         SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE convert(date,ps.ProductionDate)<=convert(date,'" + Date + @"') 

			                          union all 
			 
			                         SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END  AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE convert(date,ps.ProductionDate)<=convert(date,'" + Date + @"') 
                                    
                                    union all 
			 
			                         SELECT q.ProductionOrderId,q.WorkCenterMasterID,0 AS InQuantity,0 AS OutQuantity,isnull(q.DefectiveQty,0) AS  KillQuantity
                                      FROM trn.Quality AS q
                                      JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=q.WorkCenterMasterID
			                         WHERE  convert(date,Q.ProductionDate)<=convert(date,'" + Date + @"') 
			                ) AS K group by ProductionOrderId,WorkCenterMasterId) prSum ON WCM.Id = prSum.WorkCenterMasterId And PO.Id=prSum.ProductionOrderId

						  where WCM.PlantId='" + PlantId + @"' 
						  order by E.Id,WCM.Sequence";
        }
        private string ProductionInfoSql(string entityid, string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            return @"select ORD.CM,wc.Id as WorkCenterMasterId,
  wc.UserName AS  [LineNo],  wc.ProcessId ,tr.ProductionOrderId ,ps.Quantity,''WorkHour ,dr.DaysRun,
  CASE WHEN ISNULL(ex.MachineCostPerHour,0)*ISNULL(tr.TotalHour,0) <ex.MinFixedCost THEN ex.MinFixedCost ELSE CASE WHEN ISNULL(ex.MachineCostPerHour,0)*ISNULL(tr.TotalHour,0)>EX.MaxFixedCost THEN ex.MaxFixedCost ELSE ISNULL(ex.MachineCostPerHour,0)*ISNULL(tr.TotalHour,0) END END AS MachineCostPerDay,
  buyer=STUFF((select distinct ','+XB.UserName from 
	                               trn.SalesOrder XSO 
		                               JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                               left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                               left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                               left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                           where prod.ProductionOrderId =Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
Item=STUFF((select distinct ','+XMM.UserName from 
	                               trn.SalesOrder XSO 
		                               JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                               left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                               left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                               left outer join MST.MaterialMaster XMM on XMM.Id=XMOI.MaterialMasterId
			                           where prod.ProductionOrderId =Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
	BuyerItemNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
                                   trn.MasterOrderItem XMOI 	  
								        INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								        INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id 
										where prod.ProductionOrderId =podx.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
  
  
									   
									  p.UserName AS Process,
									  
									  tr.TotalHour AS PlannedHoursPerDay,
									  TR.Manpower AS WithMachine,
									  0 AS WithoutMachine,
									  TR.SMV as SPT,
									  TR.Manpower AllotedManpower,TR.Manpower RequiredManPower,
									  TR.Manpower AllotedWorkstation,
									 TR.Quantity AS TargetPerDay,
									 TR.Quantity/tr.TotalHour AS  RequiredStdTarget
     FROM scs.WorkCenterMaster as wc 
      JOIN (
     	SELECT p.ProcessId,p.ProductionOrderId,p.WorkCenterMasterId
     	  FROM trn.ProductionSummary AS p   
     	 JOIN trn.ProductionOrderProcessSet AS PROCC ON PROCC.ProductionOrderId=p.ProductionOrderId  AND PROCC.IsBaseProcess=1 and PROCC.ProcessId=p.ProcessId     
      where p.ProductionDate='" + Date + @"' 
		UNION 
		 SELECT  wcm.ProcessId,p.ProductionOrderId,WCM.Id FROM trn.DailyProductionTarget AS P
		 JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=P.WorkCenterMasterID
     	 JOIN trn.ProductionOrderProcessSet AS PROCC ON PROCC.ProductionOrderId=p.ProductionOrderId  AND PROCC.IsBaseProcess=1 and PROCC.ProcessId=wcm.ProcessId     
        where p.TargetDate='" + Date + @"' 
     ) AS PROD ON prod.ProcessId=wc.ProcessId AND wc.Id=prod.WorkCenterMasterId
                left join    trn.DailyProductionTarget AS TR on wc.id=TR.WorkCenterMasterId AND tr.ProductionOrderId=PROD.ProductionOrderId AND TR.TargetDate='" + Date + @"'
                left JOIN trn.ProductionOrderProcessSet AS PROCC ON PROCC.ProductionOrderId=PROD.ProductionOrderId  AND PROCC.IsBaseProcess=1 and PROCC.ProcessId=WC.ProcessId     
             LEFT JOIN (
                            select 
                            p.ProcessId,p.ProductionOrderId,p.WorkCenterMasterId,sum(p.Quantity) Quantity
                            from  TRN.ProductionSummary as p 
                            JOIN trn.ProductionOrderProcessSet AS Ps ON ps.ProductionOrderId=p.ProductionOrderId  AND ps.IsBaseProcess=1 and ps.ProcessId=p.ProcessId
                            where p.ProductionDate='" + Date + @"' 
            and p.ProductionGrade='A'
            group by p.WorkCenterMasterId,p.ProcessId,p.ProductionOrderId
            ) ps  ON PROD.ProductionOrderId=ps.ProductionOrderId AND wc.Id=ps.WorkCenterMasterId AND ps.ProcessId=wc.ProcessId
            LEFT JOIN (SELECT p.ProcessId,p.ProductionOrderId,p.WorkCenterMasterId,COUNT(DISTINCT p.ProductionDate) AS DaysRun  
                       from  TRN.ProductionSummary as p 
                       JOIN trn.ProductionOrderProcessSet AS Ps ON ps.ProductionOrderId=p.ProductionOrderId  AND ps.IsBaseProcess=1 and ps.ProcessId=p.ProcessId
                       WHERE p.ProductionDate<='" + Date + @"'
                       GROUP BY  p.ProcessId,p.ProductionOrderId,p.WorkCenterMasterId

            ) AS DR ON dr.ProcessId=wc.ProcessId
                         AND dr.ProductionOrderId=PROD.ProductionOrderId AND dr.WorkCenterMasterId=wc.Id
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
                                                        ) AS ORD on ord.ProductionOrderID=PROD.ProductionOrderId

            LEFT JOIN (SELECT ec.EntityId,ec.GeneralWorkingHourPerDay,
			            (CASE WHEN same.FromCurrencyId=ec.CurrencyId THEN ec.MachineCostPerHour ELSE ec.MachineCostPerHour*rer.ExchangeRate END) AS MachineCostPerHour,
			            CASE WHEN same.FromCurrencyId=ec.CurrencyId THEN (CASE WHEN ISNULL(ec.MinFixedCost,0)=0 AND ISNULL(ec.MaxFixedCost,0)>0 THEN ec.MaxFixedCost ELSE ec.MinFixedCost END)  ELSE (CASE WHEN ISNULL(ec.MinFixedCost,0)=0 AND ISNULL(ec.MaxFixedCost,0)>0 THEN ec.MaxFixedCost ELSE ec.MinFixedCost END) *rer.ExchangeRate END AS MinFixedCost,
			            CASE WHEN same.FromCurrencyId=ec.CurrencyId THEN (CASE WHEN ISNULL(ec.MinFixedCost,0)>0 AND ISNULL(ec.MaxFixedCost,0)=0 THEN ec.MinFixedCost ELSE ec.MaxFixedCost END)  ELSE (CASE WHEN ISNULL(ec.MinFixedCost,0)>0 AND ISNULL(ec.MaxFixedCost,0)=0 THEN ec.MinFixedCost ELSE ec.MaxFixedCost END)*rer.ExchangeRate END AS MaxFixedCost
			           FROM EntityConfig AS ec 
			            LEFT JOIN org.Entity AS e ON e.Id=ec.EntityId
			            JOIN EntityConfig AS X ON ec.Id=x.Id and x.Id=(SELECT TOP 1 Id FROM EntityConfig AS ecx WHERE ecx.EntityId=ec.EntityId)
			            LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=EC.CurrencyId AND rer.PlantId=e.PlantId
			            LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=e.PlantId
			            WHERE ec.EntityId IN (" + entityid + @") ) AS EX ON ex.EntityId=wc.EntityId
            left join hkp.Process p on p.id=wc.ProcessId
            where wc.EntityId IN (" + entityid + @") 
            order by wc.Sequence


            ";
        }
        public void ProductionEfficiencyReport(string PlantId, string entityid, string Date)
        {
            try
            {
                DataSet dsHour;
                DataSet dsProHour;
                DataTable dtHour;
                DataView dvHour;
                Dictionary<string, int> Hour = new Dictionary<string, int>();

                GetProductionBookingPeriod(out dsHour);
                dtHour = dsHour.Tables[0];
                dvHour = new DataView();
                dvHour.Table = dsHour.Tables[0];

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
                GetProductionHour(entityid, Date, out dsProHour);
                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(2);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Production Information Report (" + Convert.ToDateTime(Date).ToString("dd-MMM-yyyy") + @")";


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
                sheet[ROW, COL].Text = "Buyer Item Ref.";
                sheet[ROW, COL].ColumnWidth = 15;
                int colBuyerItemNo = COL;
                sheet.Range[ROW, colBuyerItemNo, ROW + 1, colBuyerItemNo].Merge();

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

                for (int i = 0; i < dvHour.Count; i++)
                {
                    COL++;
                    int colHour = COL;
                    sheet[ROW + 1, COL].Text = dvHour[i]["UserName"].ToString();
                    sheet[ROW + 1, COL].ColumnWidth = 10;
                    Hour.Add(dvHour[i]["Id"].ToString(), colHour);
                }

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
                    sheet[ROW, colBuyerItemNo].Text = dtProductionInfo.Rows[i]["BuyerItemNo"].ToString();
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
                     sheet[ROW, colAvgHr].NumberFormat = "#,##0.00;(#,##0.00)";
                    // sheet[ROW, colTargetAchievement].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["Sequence"].ToString());

                    sheet[ROW, colTargetAchievement].Formula = "IF(" + clsStaticInfo.GetxlsCol(colTGTDAY) + ROW.ToString() + ">0," + clsStaticInfo.GetxlsCol(colTotalP) + ROW.ToString() + "/" + clsStaticInfo.GetxlsCol(colTGTDAY) + ROW.ToString() + "*" + 100 + ",0)";
                    sheet[ROW, colVariance].Formula = clsStaticInfo.GetxlsCol(colTotalP) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colTGTDAY) + ROW.ToString();

                    sheet[ROW, colProduceMin].Formula = clsStaticInfo.GetxlsCol(colTotalP) + ROW.ToString() + "*" + clsStaticInfo.GetxlsCol(colSPT) + ROW.ToString();
                    sheet[ROW, colAvailableMin].Formula = clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString() + "*" + clsStaticInfo.GetxlsCol(colTotalMP) + ROW.ToString() + "*" + 60;
                    sheet[ROW, colCM].Number = clsStaticInfo.dbl(dtProductionInfo.Rows[i]["CM"].ToString());
                    sheet[ROW, colCMTarget].Formula = clsStaticInfo.GetxlsCol(colCM) + ROW.ToString() + "*" + clsStaticInfo.GetxlsCol(colTGThr) + ROW.ToString() + "*" + clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString();
                    sheet[ROW, colCMEarned].Formula = clsStaticInfo.GetxlsCol(colCM) + ROW.ToString() + "*" + clsStaticInfo.GetxlsCol(colTotalP) + ROW.ToString();
                    //sheet[ROW, colCMSpend].Formula = "IF(" + clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString() + @">0," + (clsStaticInfo.dbl(dtProductionInfo.Rows[i]["MachineCostPerDay"].ToString()).ToString() + "*" + clsStaticInfo.GetxlsCol(colRUNmc) + ROW.ToString()) + "/" + 11 + "*" + clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString() + @",0)";
                    sheet[ROW, colCMSpend].Formula = "IF(" + clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString() + @">0," + (clsStaticInfo.dbl(dtProductionInfo.Rows[i]["MachineCostPerDay"].ToString()).ToString() + "*" + clsStaticInfo.GetxlsCol(colRUNmc) + ROW.ToString()) + @",0)";
                    sheet[ROW, colCMMargin].Formula = clsStaticInfo.GetxlsCol(colCMEarned) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colCMSpend) + ROW.ToString();

                    sheet[ROW, colEfficiency].Formula = "IF(" + clsStaticInfo.GetxlsCol(colAvailableMin) + ROW.ToString() + ">0," + clsStaticInfo.GetxlsCol(colTotalP) + ROW.ToString() + "*" + clsStaticInfo.GetxlsCol(colSPT) + ROW.ToString() + "/" + clsStaticInfo.GetxlsCol(colAvailableMin) + ROW.ToString() + "*100" + ",0)";
                    // sheet[ROW, colRemarksProblemms].Text = dtProductionInfo.Rows[i]["Skill"].ToString();MachineCostPerDay
                    // sheet[ROW, colTarget].Text = dtProductionInfo.Rows[i]["SkillCode"].ToString();
                    // sheet[ROW, colActual].Text = dtProductionInfo.Rows[i]["MachineVarient"].ToString();
                    // sheet[ROW, colAchievement].Text = dtProductionInfo.Rows[i]["FGZone"].ToString();
                    // sheet[ROW, colRemarks].Text = dtProductionInfo.Rows[i]["FGComponen"].ToString();

                    dsProHour.Tables[0].DefaultView.RowFilter = "ProductionOrderId= '" + dtProductionInfo.Rows[i]["ProductionOrderId"].ToString() + "' and WorkCenterMasterId='" + dtProductionInfo.Rows[i]["WorkCenterMasterId"].ToString() + "'  ";

                    for (int d = 0; d < dsProHour.Tables[0].DefaultView.Count; d++)
                    {
                        if (Hour.ContainsKey(dsProHour.Tables[0].DefaultView[d]["ProductionBookingPeriodId"].ToString()) == false)
                            continue;

                        sheet[ROW, Hour[dsProHour.Tables[0].DefaultView[d]["ProductionBookingPeriodId"].ToString()]].Number = clsStaticInfo.dbl(dsProHour.Tables[0].DefaultView[d]["Quantity"].ToString());
                    }

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


                #region Sheet 2
                sheet = workbook.Worksheets[1];

                sql = DailyProductionInfoSql(PlantId, Date);
                //Instantiate the Excel application object
                DataTable dtDailyProduction = _sqlRepository.GetDataTable(sql);
                if (dtProductionInfo.Rows.Count == 0)
                    throw new Exception("No data found");

                sheet.Name = "Daily Production Report";

                ROW = 7;
                COL = 1;
                string EntityId = "";
                List<string> Entity = new List<string>();
                List<string> EntityName = new List<string>();
                int count = 0;
                for (int i = 0; i < dtDailyProduction.Rows.Count; i++)
                {
                    if (dtDailyProduction.Rows[i]["EntityId"].ToString() != EntityId)
                    {
                        count++;
                        Entity.Add(dtDailyProduction.Rows[i]["EntityId"].ToString());
                        EntityName.Add(dtDailyProduction.Rows[i]["EntityName"].ToString());
                    }
                    EntityId = dtDailyProduction.Rows[i]["EntityId"].ToString();
                }
                EntityId = dtDailyProduction.Rows[0]["EntityId"].ToString();
                for (int C = 0; C < count; C++)
                {
                    EntityId = Entity[C].ToString();

                    if (Entity[C].ToString() == EntityId)
                    {
                        COL = 1;

                        sheet[ROW, COL].Text = "Entity:";
                        int colEntity = COL;
                        colEntity++;
                        sheet[ROW, colEntity].Text = EntityName[C].ToString();
                        sheet.Range[ROW, colEntity, ROW, colEntity + 1].Merge();
                        sheet.Range[ROW, 1, ROW, colEntity + 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                        sheet.Range[ROW, 1, ROW, colEntity + 1].CellStyle.Font.Color = ExcelKnownColors.White;

                        sheet[ROW, colEntity + 2].Text = "Date:";
                        int colDate = COL;
                        colDate = colEntity + 2;
                        colDate++;
                        sheet[ROW, colDate].Text = Date;
                        sheet.Range[ROW, colDate - 1, ROW, colDate].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                        sheet.Range[ROW, colDate - 1, ROW, colDate].CellStyle.Font.Color = ExcelKnownColors.White;

                        ROW++;
                        sheet[ROW, COL].Text = "Line No.";
                        sheet[ROW, COL].ColumnWidth = 15;
                        int colLineNo2 = COL;
                        sheet.Range[ROW, colLineNo2, ROW + 1, colLineNo2].Merge();
                        COL++;

                        sheet[ROW + 1, COL].Text = "Running";
                        sheet[ROW + 1, COL].ColumnWidth = 15;
                        sheet[ROW + 1, COL].CellStyle.Font.Bold = true;

                        int colRunning = COL;
                        COL++;
                        sheet[ROW + 1, COL].Text = "Next";
                        sheet[ROW + 1, COL].ColumnWidth = 15;
                        sheet[ROW + 1, COL].CellStyle.Font.Bold = true;

                        int colNext = COL;

                        sheet[ROW, COL].Text = "Buyer";
                        sheet.Range[ROW, colRunning, ROW, colNext].Merge();
                        sheet.Range[ROW, colRunning].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                        COL++;
                        sheet[ROW, COL].Text = "Style Name";
                        sheet[ROW, COL].ColumnWidth = 15;
                        int colStyleName = COL;
                        sheet.Range[ROW, colStyleName, ROW + 1, colStyleName].Merge();

                        COL++;
                        sheet[ROW, COL].Text = "Allocated Quantity";
                        sheet[ROW, COL].ColumnWidth = 15;
                        int colAllocatedQty = COL;
                        sheet.Range[ROW, colAllocatedQty, ROW + 1, colAllocatedQty].Merge();

                        COL++;
                        sheet[ROW, COL].Text = "Style Description";
                        sheet[ROW, COL].ColumnWidth = 20;
                        int colStyleDescription = COL;
                        sheet.Range[ROW, colStyleDescription, ROW + 1, colStyleDescription].Merge();

                        COL++;
                        sheet[ROW, COL].Text = "Hourly Target";
                        sheet[ROW, COL].ColumnWidth = 13;
                        int colHourlyTarget = COL;
                        sheet.Range[ROW, colHourlyTarget, ROW + 1, colHourlyTarget].Merge();

                        COL++;

                        sheet[ROW, COL].Text = "SPT";
                        sheet[ROW, COL].ColumnWidth = 10;
                        int colSPT2 = COL;
                        sheet.Range[ROW, colSPT2, ROW + 1, colSPT2].Merge();

                        COL++;
                        sheet[ROW, COL].Text = "CM";
                        sheet[ROW, COL].ColumnWidth = 10;
                        int colCM2 = COL;
                        sheet.Range[ROW, colCM2, ROW + 1, colCM2].Merge();

                        COL++;

                        sheet[ROW + 1, COL].Text = "OP";
                        sheet[ROW + 1, COL].ColumnWidth = 10;
                        sheet[ROW + 1, COL].CellStyle.Font.Bold = true;
                        int colOP = COL;
                        COL++;

                        sheet[ROW + 1, COL].Text = " Asst. Op";
                        sheet[ROW + 1, COL].ColumnWidth = 10;
                        sheet[ROW + 1, COL].CellStyle.Font.Bold = true;
                        int colAsstOP = COL;
                        COL++;

                        sheet[ROW + 1, COL].Text = "Total";
                        sheet[ROW + 1, COL].ColumnWidth = 10;
                        sheet[ROW + 1, COL].CellStyle.Font.Bold = true;
                        int colTotal = COL;
                        sheet[ROW, COL].Text = "Required";
                        sheet.Range[ROW, colOP, ROW, colTotal].Merge();
                        sheet[ROW, colOP].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                        COL++;
                        sheet[ROW + 1, COL].Text = "OP";
                        sheet[ROW + 1, COL].ColumnWidth = 10;
                        sheet[ROW + 1, COL].CellStyle.Font.Bold = true;
                        int colAOP = COL;
                        COL++;

                        sheet[ROW + 1, COL].Text = " Asst. Op";
                        sheet[ROW + 1, COL].ColumnWidth = 10;
                        sheet[ROW + 1, COL].CellStyle.Font.Bold = true;
                        int colAAsstOP = COL;
                        COL++;

                        sheet[ROW + 1, COL].Text = "Total";
                        sheet[ROW + 1, COL].ColumnWidth = 10;
                        sheet[ROW + 1, COL].CellStyle.Font.Bold = true;
                        int colATotal = COL;
                        sheet[ROW, COL].Text = "Allocated";
                        sheet.Range[ROW, colAOP, ROW, colATotal].Merge();
                        sheet[ROW, colAOP].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                        COL++;
                        sheet[ROW, COL].Text = "Prvs. day Q.C Pass";
                        sheet[ROW, COL].ColumnWidth = 20;
                        int colPrvsdauQCPass = COL;
                        sheet.Range[ROW, colPrvsdauQCPass, ROW + 1, colPrvsdauQCPass].Merge();
                        COL++;

                        sheet[ROW, COL].Text = "Today TGT";
                        sheet[ROW, COL].ColumnWidth = 10;
                        int colTodayTGT = COL;
                        sheet.Range[ROW, colTodayTGT, ROW + 1, colTodayTGT].Merge();
                        COL++;

                        sheet[ROW, COL].Text = "WIP";
                        sheet[ROW, COL].ColumnWidth = 10;
                        int colWIP = COL;
                        sheet.Range[ROW, colWIP, ROW + 1, colWIP].Merge();
                        COL++;
                        sheet[ROW, COL].Text = "Expc. Effi.";
                        sheet[ROW, COL].ColumnWidth = 10;
                        int colExpcEffi = COL;
                        sheet.Range[ROW, colExpcEffi, ROW + 1, colExpcEffi].Merge();
                        COL++;

                        sheet[ROW, COL].Text = "Today Work Hour";
                        sheet[ROW, COL].ColumnWidth = 10;
                        int colTodayWorkHour = COL;
                        sheet.Range[ROW, colTodayWorkHour, ROW + 1, colTodayWorkHour].Merge();
                        COL++;

                        sheet[ROW, COL].Text = "Running Day No.";
                        sheet[ROW, COL].ColumnWidth = 10;
                        int colRunningDayNo = COL;
                        sheet.Range[ROW, colRunningDayNo, ROW + 1, colRunningDayNo].Merge();


                        endCol = COL;

                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                        ROW += 2;


                        StartRow = ROW; //row 20
                        for (int j = 0; j < dtDailyProduction.Rows.Count; j++)
                        {
                            //EntityId = dtDailyProduction.Rows[j]["EntityId"].ToString();

                            if (dtDailyProduction.Rows[j]["EntityId"].ToString() == EntityId)
                            {

                                sheet[ROW, colLineNo2].Text = dtDailyProduction.Rows[j]["Line"].ToString();
                                sheet[ROW, colRunning].Text = dtDailyProduction.Rows[j]["Buyer"].ToString();
                                sheet[ROW, colNext].Text = dtDailyProduction.Rows[j]["NextBuyer"].ToString();
                                sheet[ROW, colStyleName].Text = dtDailyProduction.Rows[j]["BuyerOrderRefNo"].ToString();
                                sheet[ROW, colAllocatedQty].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[j]["AllocatedQty"].ToString());
                                sheet[ROW, colStyleDescription].Text = dtDailyProduction.Rows[j]["StyleDescription"].ToString();
                                sheet[ROW, colSPT2].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[j]["SMV"].ToString());
                                sheet[ROW, colCM2].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[j]["CM"].ToString());
                                //sheet[ROW, colOP].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[i]["WithoutMachine"].ToString());
                                //sheet[ROW, colAsstOP].Formula = clsStaticInfo.GetxlsCol(colRUNmc) + ROW.ToString() + "+" + clsStaticInfo.GetxlsCol(colHel) + ROW.ToString();
                                sheet[ROW, colTotal].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[j]["ManpowerBulletin"].ToString());
                                sheet[ROW, colAOP].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[j]["ManPowerWithHand"].ToString());
                                sheet[ROW, colAAsstOP].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[j]["ManPowerWithMachine"].ToString());
                                sheet[ROW, colATotal].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[j]["Manpower"].ToString());

                                sheet[ROW, colPrvsdauQCPass].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[j]["PreviousDayQCpass"].ToString());
                                sheet[ROW, colTodayTGT].Formula = "IF(" + clsStaticInfo.GetxlsCol(colSPT2) + ROW.ToString() + ">0," + (clsStaticInfo.GetxlsCol(colATotal) + ROW.ToString() + "*" + 60 + "*" + clsStaticInfo.GetxlsCol(colExpcEffi) + ROW.ToString() + "*" + clsStaticInfo.GetxlsCol(colTodayWorkHour) + ROW.ToString()) + "/" + clsStaticInfo.GetxlsCol(colSPT2) + ROW.ToString() + ",0)";
                                sheet[ROW, colHourlyTarget].Formula = "IF(" + clsStaticInfo.GetxlsCol(colTodayWorkHour) + ROW.ToString() + ">0," + clsStaticInfo.GetxlsCol(colTodayTGT) + ROW.ToString() + "/" + clsStaticInfo.GetxlsCol(colTodayWorkHour) + ROW.ToString() + ",0)";

                                sheet[ROW, colWIP].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[j]["WIP"].ToString());
                                sheet[ROW, colExpcEffi].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[j]["PlanEfficiency"].ToString());
                                sheet[ROW, colTodayWorkHour].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[j]["TotalHour"].ToString());
                                //sheet[ROW, colRunningDayNo].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[j]["DaysRunning"].ToString());
                                //  sheet[ROW, colRunningDayNo].Formula = clsStaticInfo.GetxlsCol(colRUNmc) + ROW.ToString() + "+" + clsStaticInfo.GetxlsCol(colHel) + ROW.ToString();

                                //sheet[ROW, colTGTEFF].Formula = "if(and(" + clsStaticInfo.GetxlsCol(colTotalMP) + ROW.ToString() + ">0," + clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString() + ">0," + clsStaticInfo.GetxlsCol(colSPT) + ROW.ToString() + @">0),"
                                //    + clsStaticInfo.GetxlsCol(colTGTDAY) + ROW.ToString() + "/(" + clsStaticInfo.GetxlsCol(colWorkHour) + ROW.ToString()
                                //    + "*60*" + clsStaticInfo.GetxlsCol(colTotalMP) + ROW.ToString() + "/" + clsStaticInfo.GetxlsCol(colSPT) + ROW.ToString() + ")*100,0)";

                                //sheet[ROW, colTGTDAY].Number = clsStaticInfo.dbl(dtDailyProduction.Rows[i]["TargetPerDay"].ToString());

                                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                                ROW++;
                            }

                        }
                        sheet[ROW, 1].Text = "Total";
                        sheet[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                        //sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);



                        sheet[ROW, colHourlyTarget].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colHourlyTarget) + StartRow + ":" + clsStaticInfo.GetxlsCol(colHourlyTarget) + (ROW - 1) + ")";
                        sheet[ROW, colHourlyTarget].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colOP].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colOP) + StartRow + ":" + clsStaticInfo.GetxlsCol(colOP) + (ROW - 1) + ")";
                        sheet[ROW, colOP].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colTodayWorkHour].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colTodayWorkHour) + StartRow + ":" + clsStaticInfo.GetxlsCol(colTodayWorkHour) + (ROW - 1) + ")";
                        sheet[ROW, colTodayWorkHour].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colTotal].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colTotal) + StartRow + ":" + clsStaticInfo.GetxlsCol(colTotal) + (ROW - 1) + ")";
                        sheet[ROW, colTotal].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colAOP].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colAOP) + StartRow + ":" + clsStaticInfo.GetxlsCol(colAOP) + (ROW - 1) + ")";
                        sheet[ROW, colAOP].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colATotal].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colATotal) + StartRow + ":" + clsStaticInfo.GetxlsCol(colATotal) + (ROW - 1) + ")";
                        sheet[ROW, colATotal].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colPrvsdauQCPass].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colPrvsdauQCPass) + StartRow + ":" + clsStaticInfo.GetxlsCol(colPrvsdauQCPass) + (ROW - 1) + ")";
                        sheet[ROW, colPrvsdauQCPass].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colTodayTGT].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colTodayTGT) + StartRow + ":" + clsStaticInfo.GetxlsCol(colTodayTGT) + (ROW - 1) + ")";
                        sheet[ROW, colTodayTGT].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colSPT2].Formula = "AVERAGE(" + clsStaticInfo.GetxlsCol(colSPT2) + StartRow + ":" + clsStaticInfo.GetxlsCol(colSPT2) + (ROW - 1) + ")";
                        sheet[ROW, colSPT2].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colExpcEffi].Formula = "AVERAGE(" + clsStaticInfo.GetxlsCol(colExpcEffi) + StartRow + ":" + clsStaticInfo.GetxlsCol(colExpcEffi) + (ROW - 1) + ")";
                        sheet[ROW, colExpcEffi].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colTodayWorkHour].Formula = "AVERAGE(" + clsStaticInfo.GetxlsCol(colTodayWorkHour) + StartRow + ":" + clsStaticInfo.GetxlsCol(colTodayWorkHour) + (ROW - 1) + ")";
                        sheet[ROW, colTodayWorkHour].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet.Range[StartRow, colAllocatedQty, ROW, colAllocatedQty].NumberFormat = clsStaticInfo.NumberFormat();
                        sheet.Range[StartRow, colHourlyTarget, ROW, colHourlyTarget].NumberFormat = clsStaticInfo.NumberFormat();
                        sheet.Range[StartRow, colSPT2, ROW, colSPT2].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[StartRow, colCM2, ROW, colCM2].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[StartRow, colOP, ROW, colOP].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[StartRow, colAsstOP, ROW, colAsstOP].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[StartRow, colTotal, ROW, colTotal].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[StartRow, colAOP, ROW, colAOP].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[StartRow, colAsstOP, ROW, colAsstOP].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[StartRow, colATotal, ROW, colATotal].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[StartRow, colPrvsdauQCPass, ROW, colPrvsdauQCPass].NumberFormat = clsStaticInfo.NumberFormat();
                        sheet.Range[StartRow, colWIP, ROW, colWIP].NumberFormat = clsStaticInfo.NumberFormat();
                        sheet.Range[StartRow, colTodayWorkHour, ROW, colTodayWorkHour].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[StartRow, colRunningDayNo, ROW, colRunningDayNo].NumberFormat = clsStaticInfo.NumberFormat();
                        sheet.Range[StartRow, colTodayTGT, ROW, colTodayTGT].NumberFormat = clsStaticInfo.NumberFormat();
                        sheet.Range[StartRow, colExpcEffi, ROW, colExpcEffi].NumberFormat = clsStaticInfo.NumberFormat(2);

                    }
                    ROW++;
                    ROW++;
                    ROW++;
                }

                DataTable dtBuyerSummary = dtDailyProduction.AsEnumerable().GroupBy(x => new
                {
                    Buyer = x["Buyer"]
                })
                                     .Select(x =>
                                     {
                                         DataRow row = dtDailyProduction.NewRow();
                                         row["Buyer"] = x.Key.Buyer;
                                         row["TodayTGT"] = x.Sum(r => clsStaticInfo.dbl(r["TodayTGT"].ToString()));
                                         return row;
                                     }
                ).CopyToDataTable();


                //column
                COL = 1;
                int StartOfSummaryRow = ROW;
                sheet[ROW, COL].Text = "Buyer Status";
                int colBuyerStatus = COL;
                sheet.Range[ROW, colBuyerStatus, ROW, colBuyerStatus + 2].Merge();
                sheet[ROW, colBuyerStatus].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet[ROW, colBuyerStatus].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet[ROW, colBuyerStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                ROW++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colBSBuyer = COL;
                COL++;

                sheet[ROW, COL].Text = "Running Lines";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colBSRunningLine = COL;
                COL++;

                sheet[ROW, COL].Text = "Target";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colBSTarget = COL;
                endCol = COL;

                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;
                int BSStartRow = ROW;
                for (int i = 0; i < dtBuyerSummary.Rows.Count; i++)
                {
                    dtDailyProduction.DefaultView.RowFilter = "Buyer='" + dtBuyerSummary.Rows[i]["Buyer"].ToString() + "'";

                    sheet[ROW, colBSBuyer].Text = dtBuyerSummary.Rows[i]["Buyer"].ToString();
                    sheet[ROW, colBSRunningLine].Number = dtDailyProduction.DefaultView.ToTable(true, "WorkCenterMasterId").Rows.Count;
                    sheet[ROW, colBSTarget].Number = OTSBD.clsStaticInfo.dbl(dtBuyerSummary.Rows[i]["TodayTGT"].ToString());
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }
                sheet[ROW, 1].Text = "Total";
                sheet[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet[ROW, colBSRunningLine].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBSRunningLine) + BSStartRow + ":" + clsStaticInfo.GetxlsCol(colBSRunningLine) + (ROW - 1) + ")";
                sheet[ROW, colBSRunningLine].NumberFormat = "#,##0.00;(#,##0.)";
                sheet[ROW, colBSTarget].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBSTarget) + BSStartRow + ":" + clsStaticInfo.GetxlsCol(colBSTarget) + (ROW - 1) + ")";
                sheet[ROW, colBSTarget].NumberFormat = "#,##0.00;(#,##0.)";
                sheet.Range[BSStartRow, colBSRunningLine, ROW, colBSRunningLine].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[BSStartRow, colBSTarget, ROW, colBSTarget].NumberFormat = clsStaticInfo.NumberFormat();

                //second summary report

                ROW = StartOfSummaryRow;

                DataTable dtEntitySummary = dtDailyProduction.AsEnumerable().GroupBy(y => new
                {
                    EntityName = y["EntityName"]
                })
                      .Select(y =>
                      {
                          DataRow row = dtDailyProduction.NewRow();
                          row["EntityName"] = y.Key.EntityName;
                          row["TodayTGT"] = y.Sum(r => clsStaticInfo.dbl(r["TodayTGT"].ToString()));
                          row["PreviousDayQCpass"] = y.Sum(r => clsStaticInfo.dbl(r["PreviousDayQCpass"].ToString()));
                          row["PlanEfficiency"] = y.Average(r => clsStaticInfo.dbl(r["PlanEfficiency"].ToString()));
                          row["TotalHour"] = y.Average(r => clsStaticInfo.dbl(r["TotalHour"].ToString()));
                          row["ManPowerWithHand"] = y.Sum(r => clsStaticInfo.dbl(r["ManPowerWithHand"].ToString()));
                          row["ManPowerWithMachine"] = y.Sum(r => clsStaticInfo.dbl(r["ManPowerWithMachine"].ToString()));

                          return row;
                      }
 ).CopyToDataTable();


                //column
                COL = endCol + 2;
                int StartCOl = endCol + 2;
                sheet[ROW, COL].Text = "SUMMARY";
                int colSUMMARY = COL;
                sheet.Range[ROW, colSUMMARY, ROW, colSUMMARY + 6].Merge();
                sheet[ROW, colSUMMARY].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet[ROW, colSUMMARY].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet[ROW, colSUMMARY].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                ROW++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colSEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "Today Target";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colSTodayTarget = COL;
                COL++;

                sheet[ROW, COL].Text = "Previous Day Actual";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colSPreviousDayActual = COL;
                COL++;

                sheet[ROW, COL].Text = "Target Efficiency";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colSTargetEfficiency = COL;
                COL++;

                sheet[ROW, COL].Text = "Working Hour";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colSWorkingHour = COL;
                COL++;

                sheet[ROW, COL].Text = "Operator";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colSOP = COL;
                COL++;

                sheet[ROW, COL].Text = " Assistant Operator";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colSHP = COL;
                endCol = COL;

                sheet.Range[ROW, StartCOl, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, StartCOl, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, StartCOl, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, StartCOl, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;
                int SummaryStartRow = ROW;

                for (int i = 0; i < dtEntitySummary.Rows.Count; i++)
                {
                    dtDailyProduction.DefaultView.RowFilter = "EntityName='" + dtEntitySummary.Rows[i]["EntityName"].ToString() + "'";

                    sheet[ROW, colSEntity].Text = dtEntitySummary.Rows[i]["EntityName"].ToString();
                    //sheet[ROW, colSTodayTarget].Number = dtDailyProduction.DefaultView.ToTable(true, "WorkCenterMasterId").Rows.Count;
                    sheet[ROW, colSTodayTarget].Number = OTSBD.clsStaticInfo.dbl(dtEntitySummary.Rows[i]["TodayTGT"].ToString());
                    sheet[ROW, colSPreviousDayActual].Number = OTSBD.clsStaticInfo.dbl(dtEntitySummary.Rows[i]["PreviousDayQCpass"].ToString());
                    sheet[ROW, colSTargetEfficiency].Number = OTSBD.clsStaticInfo.dbl(dtEntitySummary.Rows[i]["PlanEfficiency"].ToString());
                    sheet[ROW, colSWorkingHour].Number = OTSBD.clsStaticInfo.dbl(dtEntitySummary.Rows[i]["TotalHour"].ToString());
                    sheet[ROW, colSOP].Number = OTSBD.clsStaticInfo.dbl(dtEntitySummary.Rows[i]["ManPowerWithHand"].ToString());
                    sheet[ROW, colSHP].Number = OTSBD.clsStaticInfo.dbl(dtEntitySummary.Rows[i]["ManPowerWithMachine"].ToString());

                    sheet.Range[ROW, StartCOl, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, StartCOl, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }
                sheet[ROW, StartCOl].Text = "Total";
                sheet[ROW, StartCOl].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, StartCOl, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, StartCOl, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, StartCOl, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet[ROW, colSTodayTarget].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colSTodayTarget) + SummaryStartRow + ":" + clsStaticInfo.GetxlsCol(colSTodayTarget) + (ROW - 1) + ")";
                sheet[ROW, colSTodayTarget].NumberFormat = "#,##0.00;(#,##0)";
                sheet[ROW, colSPreviousDayActual].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colSPreviousDayActual) + SummaryStartRow + ":" + clsStaticInfo.GetxlsCol(colSPreviousDayActual) + (ROW - 1) + ")";
                sheet[ROW, colSPreviousDayActual].NumberFormat = "#,##0.00;(#,##0)";
                sheet[ROW, colSTargetEfficiency].Formula = "AVERAGE(" + clsStaticInfo.GetxlsCol(colSTargetEfficiency) + SummaryStartRow + ":" + clsStaticInfo.GetxlsCol(colSTargetEfficiency) + (ROW - 1) + ")";
                sheet[ROW, colSTargetEfficiency].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, colSWorkingHour].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colSWorkingHour) + SummaryStartRow + ":" + clsStaticInfo.GetxlsCol(colSWorkingHour) + (ROW - 1) + ")";
                sheet[ROW, colSWorkingHour].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, colSOP].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colSOP) + SummaryStartRow + ":" + clsStaticInfo.GetxlsCol(colSOP) + (ROW - 1) + ")";
                sheet[ROW, colSOP].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, colSHP].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colSHP) + SummaryStartRow + ":" + clsStaticInfo.GetxlsCol(colSHP) + (ROW - 1) + ")";
                sheet[ROW, colSHP].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet.Range[SummaryStartRow, colSTodayTarget, ROW, colSTodayTarget].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[SummaryStartRow, colSPreviousDayActual, ROW, colSPreviousDayActual].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[SummaryStartRow, colSTargetEfficiency, ROW, colSTargetEfficiency].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[SummaryStartRow, colSWorkingHour, ROW, colSWorkingHour].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[SummaryStartRow, colSOP, ROW, colSOP].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[SummaryStartRow, colSHP, ROW, colSHP].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 10f;

                //sheet["A" + StartRow.ToString()].FreezePanes();
                identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Daily Production Info. Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[6, 1, 7, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, colSHP, ROW, endCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                string strFileName = "Production Information Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();

                #endregion


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void GetProductionBookingPeriod(out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"select * From HKP.ProductionBookingPeriod order by Sequence";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        public void GetProductionHour(string entityid, string Date, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"select wc.Id as WorkCenterMasterId, ps.ProcessId ,ps.ProductionOrderId,ps.ProductionBookingPeriodId ,ps.Quantity
                            from(
                            select 
                            p.ProcessId,p.ProductionBookingPeriodId,p.ProductionOrderId,p.WorkCenterMasterId,sum(p.Quantity) Quantity
                             from  TRN.ProductionSummary as p 
                            where --p.ProcessId='20204' and
                             p.ProductionDate='" + Date + @"'
                            and p.ProductionGrade='A'
                            group by p.WorkCenterMasterId,p.ProductionBookingPeriodId,p.ProcessId,p.ProductionOrderId
                            ) ps
                            join scs.WorkCenterMaster as wc on wc.id=ps.WorkCenterMasterId 
                            left join hkp.Process p on p.id=ps.ProcessId 
                            where wc.EntityId IN (" + entityid + @")
                            order by wc.Sequence";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

    }
}
