using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Library.Planning.PlanningType1
{
    public class PlanningType1Scheduler
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        #region Constructor
        public PlanningType1Scheduler()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }
        #endregion Constructor
        public string GetAllWorkcenterWisePlanningSummary(string EntityId)
        {

            string sql = @"SELECT wcm.Id,p.UserName AS Plant,e.UserName AS Entity,wcm.Code AS WorkCenterCode,wcm.UserName AS WorkCenter,
							format(ed.StartDate,'dd-MMM-yyyy') AS WorkCenterStartDate,format(ed.EndDate,'dd-MMM-yyyy') AS WorkCenterEndDate,
		                    format(pl.PlanningStartDate,'dd-MMM-yyyy') AS PlanningStartDate,
			                    format(pl.PlanningEndDate,'dd-MMM-yyyy') AS PlanningEndDate
                      FROM scs.WorkCenterMaster AS wcm
					LEFT JOIN scs.WorkCenterMasterEffectiveDate ED ON ed.WorkCenterMasterId=wcm.Id
															AND ed.WorkCenterMasterId=(SELECT TOP 1 ED.WorkCenterMasterId FROM scs.WorkCenterMasterEffectiveDate dd WHERE dd.WorkCenterMasterId=wcm.Id ORDER BY dd.StartDate DESC )
              
                    INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                    INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                    LEFT OUTER JOIN (
                    SELECT ppt.WorkCenterMasterId,MIN(ppt.ProductionDate) AS PlanningStartDate,MAX(ppt.ProductionDate) AS PlanningEndDate FROM ProductionPlanningType2 AS ppt 
                    WHERE ppt.EntityID='" + EntityId + @"'	
                    GROUP BY ppt.WorkCenterMasterId
                    ) AS PL ON pl.WorkCenterMasterId=wcm.Id

                    WHERE wcm.EntityId='" + EntityId + @"' AND wcm.ProcessId IN(SELECT pt.BaseProcessId FROM PlanningTypes AS pt WHERE pt.PlanningType='PlanningType1')
                    ORDER BY wcm.Sequence
                    ";

            return sql;
        }

		public string GetAllWorkcenterWisePlanningType2Summary(string EntityId)
		{

			string sql = @"SELECT wcm.Id,p.UserName AS Plant,e.UserName AS Entity,wcm.Code AS WorkCenterCode,wcm.UserName AS WorkCenter,
							format(ed.StartDate,'dd-MMM-yyyy') AS WorkCenterStartDate,format(ed.EndDate,'dd-MMM-yyyy') AS WorkCenterEndDate,
		                    format(pl.PlanningStartDate,'dd-MMM-yyyy') AS PlanningStartDate,
			                    format(pl.PlanningEndDate,'dd-MMM-yyyy') AS PlanningEndDate
                      FROM scs.WorkCenterMaster AS wcm
					LEFT JOIN scs.WorkCenterMasterEffectiveDate ED ON ed.WorkCenterMasterId=wcm.Id
															AND ed.WorkCenterMasterId=(SELECT TOP 1 ED.WorkCenterMasterId FROM scs.WorkCenterMasterEffectiveDate dd WHERE dd.WorkCenterMasterId=wcm.Id ORDER BY dd.StartDate DESC )
              
                    INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                    INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                    LEFT OUTER JOIN (
                    SELECT ppt.WorkCenterMasterId,MIN(ppt.ProductionDate) AS PlanningStartDate,MAX(ppt.ProductionDate) AS PlanningEndDate FROM ProductionPlanningType1 AS ppt 
                    WHERE ppt.EntityID='" + EntityId + @"'	
                    GROUP BY ppt.WorkCenterMasterId
                    ) AS PL ON pl.WorkCenterMasterId=wcm.Id

                    WHERE wcm.EntityId='" + EntityId + @"' AND wcm.ProcessId IN(SELECT pt.BaseProcessId FROM PlanningTypes AS pt WHERE pt.PlanningType='PlanningType2')
                    ORDER BY wcm.Sequence
                    ";

			return sql;
		}

		public string GetSingleWorkcenterWisePlanningSummary(string WorkCenterId)
        {

            string sql = @"SELECT ppt.ProductionOrderID,ps.UserName AS ProductionStatus,t1.ProductionPriority,
format(min(t1.LSD),'dd-MMM-yyyy') AS LSD,
format(min(ppt.ProductionDate),'dd-MMM-yyyy') AS PlanningStartDate,
format(MAX(ppt.ProductionDate),'dd-MMM-yyyy') AS PlanningEndDate,po.PlannedQty,

 MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											 
					                                BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where ppt.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where ppt.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			                    
			                     FROM ProductionPlanningType1 AS ppt
								 JOIN trn.ProductionOrder AS po ON po.Id=ppt.ProductionOrderID
								 JOIN hkp.ProductionStatus ps ON ps.Id=po.ProductionStatusId
								 JOIN ProductionOrderSchedulingParametersType1 AS T1 ON t1.ProductionOrderID=po.Id


WHERE ppt.WorkCenterMasterId='" + WorkCenterId + @"'
GROUP BY ps.UserName,t1.ProductionPriority,t1.LSD, ppt.ProductionOrderID,po.PlannedQty
ORDER BY min(ppt.ProductionDate) ASC
";

            return sql;
        }

		public string GetSingleWorkcenterWisePlanningType2Summary(string WorkCenterId)
		{

			string sql = @"SELECT ppt.ProductionOrderID,ps.UserName AS ProductionStatus,t1.ProductionPriority,
format(min(t1.LSD),'dd-MMM-yyyy') AS LSD,
format(min(ppt.ProductionDate),'dd-MMM-yyyy') AS PlanningStartDate,
format(MAX(ppt.ProductionDate),'dd-MMM-yyyy') AS PlanningEndDate,po.PlannedQty,

 MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											 
					                                BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where ppt.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where ppt.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			                    
			                     FROM ProductionPlanningType2 AS ppt
								 JOIN trn.ProductionOrderType2 AS po ON po.Id=ppt.ProductionOrderID
								 JOIN hkp.ProductionStatus ps ON ps.Id=po.ProductionStatusId
								 JOIN ProductionOrderSchedulingParametersType2 AS T1 ON t1.ProductionOrderID=po.Id


WHERE ppt.WorkCenterMasterId='" + WorkCenterId + @"'
GROUP BY ps.UserName,t1.ProductionPriority,t1.LSD, ppt.ProductionOrderID,po.PlannedQty
ORDER BY min(ppt.ProductionDate) ASC
";

			return sql;
		}

		public string GetSingleWorkcenterWiseTargetSummaryByDate(string WorkCenterId, string Date)
        {

            string sql = @"SELECT ppt.ProductionOrderID,PBT.Id BulletinId,PBT.BulletinName,ps.UserName AS ProductionStatus,t1.ProductionPriority,
format(min(t1.LSD),'dd-MMM-yyyy') AS LSD,SUM(ppt.Quantity) AS TargetQty,
format(min(ppt.TargetDate),'dd-MMM-yyyy') AS PlanningStartDate,
format(MAX(ppt.TargetDate),'dd-MMM-yyyy') AS PlanningEndDate,po.PlannedQty,

 MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											 
					                                BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where ppt.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where ppt.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			   			          FROM trn.DailyProductionTarget AS ppt
								 JOIN trn.ProductionOrder AS po ON po.Id=ppt.ProductionOrderID
								 left join trn.ProductionBulletinTemplate PBT on PBT.ProductionOrderId=po.Id
								 JOIN hkp.ProductionStatus ps ON ps.Id=po.ProductionStatusId
								 JOIN ProductionOrderSchedulingParametersType1 AS T1 ON t1.ProductionOrderID=po.Id


WHERE ppt.WorkCenterMasterId='" + WorkCenterId + @"' AND PPT.TargetDate='" + Date + @"'
GROUP BY ps.UserName,t1.ProductionPriority,t1.LSD, ppt.ProductionOrderID,po.PlannedQty,PBT.Id,PBT.BulletinName
ORDER BY min(ppt.TargetDate) ASC
";

            return sql;
        }
        public string GetSameDayPlanningSummary(string RowId, string ProductionOrderId)
        {

            string sql = @"SELECT x.Id,x.Quantity, ppt.ProductionOrderID,ps.UserName AS ProductionStatus,t1.ProductionPriority,
format(min(t1.LSD),'dd-MMM-yyyy') AS LSD,
format(min(ppt.ProductionDate),'dd-MMM-yyyy') AS PlanningStartDate,
format(MAX(ppt.ProductionDate),'dd-MMM-yyyy') AS PlanningEndDate,po.PlannedQty,

 MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											 
					                                BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ppt.ProductionOrderID	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where ppt.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where ppt.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			                    
			                     FROM ProductionPlanningType1 AS ppt
									LEFT JOIN ProductionPlanningType1 AS X ON x.ProductionOrderID=ppt.ProductionOrderID
			                     AND x.ID=(SELECT top 1 Id FROM ProductionPlanningType1 AS M WHERE m.ProductionOrderID=ppt.ProductionOrderID
			                     AND m.WorkCenterMasterId=ppt.WorkCenterMasterId AND m.ProductionDate=(SELECT ProductionDate FROM ProductionPlanningType1  WHERE ID='" + RowId + @"'))
							
								 JOIN trn.ProductionOrder AS po ON po.Id=ppt.ProductionOrderID
								 JOIN hkp.ProductionStatus ps ON ps.Id=po.ProductionStatusId
								 JOIN ProductionOrderSchedulingParametersType1 AS T1 ON t1.ProductionOrderID=po.Id


WHERE ppt.WorkCenterMasterId=(SELECT pp.WorkCenterMasterId
                                FROM ProductionPlanningType1 AS pp WHERE pp.ID='" + RowId + @"')
                               
AND ppt.ProductionOrderID IN (
	
	SELECT  ProductionOrderID
	  FROM ProductionPlanningType1 WHERE ProductionDate=(SELECT ProductionDate FROM ProductionPlanningType1  WHERE ID='" + RowId + @"')
	  AND  WorkCenterMasterId=(SELECT WorkCenterMasterId FROM ProductionPlanningType1  WHERE ID='" + RowId + @"')
	  AND  ProductionOrderID<>(SELECT ProductionOrderID FROM ProductionPlanningType1  WHERE ID='" + RowId + @"')
	  )
GROUP BY x.Id,x.Quantity, ps.UserName,t1.ProductionPriority,t1.LSD, ppt.ProductionOrderID,po.PlannedQty
ORDER BY min(ppt.ProductionDate) ASC
";

            return sql;
        }

        public string GetProductionReference(string productionOrderId)
        {

            string sql = @"   SELECT  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
                        ISNULL(PO.Qty,0) AS POQuantity,ISNULL(PO.PlannedQty,0) AS SOQuantity,ISNULL(SO.Qty,0) AS SavedQuantity,t1.Color,FORMAT(t1.LSD,'dd-MMM-yyyy') AS LSD,FORMAT(t1.CommitmentDate,'dd-MMM-yyyy') AS CommitmentDate,t1.TargetPerDay
                                ,T1.ProductionPriority,PRODPR.ProductionQtyAtPR 
                                   ,ISNULL(CASE WHEN ISNULL(T1.Qty,0)>0 THEN T1.Qty ELSE PO.PlannedQty END,0)-(ISNULL(PRODPR.ProductionQtyAtPR,0)-ISNULL(PRDQ.ProductionBookedQty,0)) AS ToBePlanQty,
                                  			buyer=STUFF((select distinct ','+XB.UserName from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			where po.id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
	Customer=STUFF((select distinct ','+XB.UserName from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			left outer join [HKP].Party XB on XB.Id=XMO.PartyId
			where po.id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
 
 FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            INNER JOIN ProductionOrderSchedulingParametersType1 t1 ON t1.ProductionOrderID=po.Id
                            LEFT OUTER  JOIN (SELECT pod.ProductionOrderId,SUM(so.Qty) AS Qty
                                                FROM trn.SalesOrder AS so
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id GROUP BY pod.ProductionOrderId) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            
                            LEFT OUTER JOIN (
												SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
											FROM  trn.ProductionSummary S 
											--WHERE  CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PRODPR ON  PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
							 left outer join (SELECT pod.ProductionOrderId,
                                sum(isnull(so.ProductionBookedQty,0)) ProductionBookedQty
                                 FROM trn.SalesOrder AS so
                                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id

                                GROUP BY pod.ProductionOrderId
                            ) AS PRDQ ON PRDQ.ProductionOrderId=T1.ProductionOrderId
                            WHERE PO.id='" + productionOrderId + "'";

            return sql;
        }

		#region PriorityUpdate

		public DataTable getCurrentPriority(string Entity)
        {
            try
            {
				var str = @"Select po.Id as ProductionId, pt.ProductionPriority , ps.StandardName as Status
							from ProductionOrderSchedulingParametersType1 AS pt
							left join trn.ProductionOrder po on po.Id = pt.ProductionOrderID
							left join hkp.ProductionStatus ps on ps.Id = po.ProductionStatusId
							where ps.StandardName in ('Active','Running') and po.EntityId = '"+Entity+@"'
							";
				return _sqlRepository.GetDataTable(str);
            }
			catch(Exception e)
            {
				throw e;
            }
        }

		public void SaveFileList(List<Dictionary<string, object>> data)
		{
			try
			{
				ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
				var sqlx = "Select * From ProductionOrderSchedulingParametersType1";
				objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

				for (int i = 0; i<data.Count;i++ )
                {
					dsRef.Tables[0].DefaultView.RowFilter = @"ProductionOrderID='"+data[i]["ProductionId"].ToString() +"'";
					DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
					if(dr["ProductionPriority"].ToString() != data[i]["ProductionPriority"].ToString())
                    {
						dr.BeginEdit();
						dr["ProductionPriority"] = clsStaticInfo.dbl(data[i]["ProductionPriority"].ToString());
						dr["UpdatedDate"] = Convert.ToDateTime(DateTime.Now);
						dr.EndEdit();
					}

				}

				clsStaticInfo obj = new clsStaticInfo();
				obj.SaveDataSets(dsRef);
			}
			catch (Exception e)
			{
				throw e;
			}
		}
		#endregion PriorityUpdate

	}
}
