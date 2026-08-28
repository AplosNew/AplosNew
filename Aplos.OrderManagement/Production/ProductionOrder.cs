using Library.Crosscutting.Security;
using Library.Data.Sql;
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

namespace Library.OrderManagement.Production
{
    public class ProductionOrder
    {

        SqlRepository _sqlRepository;
        public ProductionOrder()
        {
            _sqlRepository = new SqlRepository();
        }

        public string ProductionOrderList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT 
case when PO.PlantId='" + identity.PlantId + @"' AND PO.PlantId=EN.PlantId then 'OWN' else 
case when PO.PlantId='" + identity.PlantId + @"' and EN.PlantId<>PO.PlantId then 'OUT' ELSE
case when PO.PlantId<>'" + identity.PlantId + @"' AND EN.PlantId='" + identity.PlantId + @"' THEN 'IN' ELSE '' END END END AS Owner,
PO.*,UsedInPB=CAST(CASE WHEN m.productionorderid IS NOT NULL THEN 1 ELSE 0 END AS BIT),isnull(po.Remarks,'') AS ProductionRemarks,isnull(s.UserName,'') AS ProductionStatus, isnull(EN.UserName,'') AS EntityName, 
FORMAT(PO.AddedDate,'dd-MMM-yyyy') CreationDate,
                                        isnull(PS.UserName,'') AS ProductionStatusName,PB.Id BulletinTemplateId,SO.*
                                FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
LEFT JOIN TRN.ProductionSummary M ON m.productionorderid=PO.Id
                                AND m.Id=(SELECT TOP 1 ID FROM TRN.ProductionSummary EII WHERE EII.productionorderid=PO.Id ORDER BY EII.AddedDate DESC )
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId, sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,
                                                    MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											 
					                                BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													,Material=STUFF((select distinct ', '+mm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                     ,Article=STUFF((select distinct ', '+mm.StandardName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													,Product=STUFF((select distinct ', '+Pm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													,ProductCategory=STUFF((select distinct ', '+pc.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
from trn.ProductionOrderDetail AS pod JOIN  trn.SalesOrder SO ON pod.SalesOrderId=so.Id group by pod.ProductionOrderId
) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            LEFT OUTER JOIN [TRN].[ProductionBulletinTemplate] AS PB ON PB.ProductionOrderId=po.Id";

        }

    
        public string SalesOrderListForCostingBOQ(string CustomerId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT ROW_NUMBER() OVER (ORDER BY SO.MasterOrderItemId) AS RN,0 AS Selected,CanSelect=CASE WHEN ISNULL(SO.CostingBOQMasterId,'')='' THEN 1 ELSE 0 END
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,MOI.OrderCostingMasterTemplateId
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , DEST.UserName AS DestinationName, SHP.UserName AS ShipmentModeName
	                            , PO.PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description,
	                            moi.BuyerReferenceNo BuyerItemNo,
	                            moi.OwnReferenceNo OwnItemNo,  mo.BuyerReferenceNo BuyerOrderNo,
	                            mo.OwnReferenceNo OwnOrderNo
	                            , Flag = CAST(0 AS BIT)
								,ItemList=STUFF((SELECT distinct ','+cix.UserName FROM CostingBOQ AS cbx
													JOIN hkp.CostingItem AS cix ON cix.Id=cbx.CostingItemId
                                    where cbx.SalesOrderId=so.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,BOMList=STUFF((SELECT distinct ','+cbx.CostingBOQMasterId FROM TRN.SalesOrder AS cbx
                                    where cbx.Id=so.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
,Approved=CASE WHEN OCMT.isPreCostingApproved=0 THEN 'Yes' WHEN OCMT.isQuickCostingApproved=1 THEN 'Yes' WHEN OCMT.isProcurementCostingApproved=1 THEN 'Yes' ELSE 'No' END,OCMT.CostingStage
                       FROM [TRN].[SalesOrder] AS SO 
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       LEFT JOIN dbo.OrderCostingMasterTemplate OCMT ON OCMT.Id=MOI.OrderCostingMasterTemplateId
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       join hkp.OrderCategory AS oc on OC.Id=SO.OrderCategoryId and OC.UserName IN ('Confirmed','To Confirm') AND SO.CostingBOQMasterId IS NULL
			where MO.PartyId='" + CustomerId + @"' AND (mo.OrderStatusId<>'Closed' AND so.OrderStatusId NOT IN ('Closed','Cancelled'))";

        }
        public string SalesOrderListForExistingProcess(string SalesOrderIds, string OrderProcurementCostingDirectMaterialId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,0 AS Selected
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,so.OrderCostingMasterTemplateId
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , DEST.UserName AS DestinationName, SHP.UserName AS ShipmentModeName
	                            , PO.PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description,
	                            moi.BuyerReferenceNo BuyerItemNo,
	                            moi.OwnReferenceNo OwnItemNo,  mo.BuyerReferenceNo BuyerOrderNo,
	                            mo.OwnReferenceNo OwnOrderNo
	                            , Flag = CAST(0 AS BIT)
								,ItemList=STUFF((SELECT distinct ','+cix.UserName FROM CostingBOQ AS cbx
													JOIN hkp.CostingItem AS cix ON cix.Id=cbx.CostingItemId
                                    where cbx.SalesOrderId=so.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,BOMList=STUFF((SELECT distinct ','+cbx.CostingBOQMasterId FROM CostingBOQSalesOrder AS cbx
                                    where cbx.SalesOrderId=so.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                       FROM [TRN].[SalesOrder] AS SO 
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
			  where SO.Id IN (SELECT distinct SalesOrderId
			  FROM CostingBOQItems WHERE SalesOrderId IN (" + SalesOrderIds + @") AND OrderProcurementCostingDirectMaterialId='" + OrderProcurementCostingDirectMaterialId + @"')";

        }
        public string GetExistingSalesOrderList(string BOMMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT ROW_NUMBER() OVER (ORDER BY SO.MasterOrderItemId) AS RN,0 AS Selected
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,so.OrderCostingMasterTemplateId
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , DEST.UserName AS DestinationName, SHP.UserName AS ShipmentModeName
	                            , PO.PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description,
	                            moi.BuyerReferenceNo BuyerItemNo,
	                            moi.OwnReferenceNo OwnItemNo,  mo.BuyerReferenceNo BuyerOrderNo,
	                            mo.OwnReferenceNo OwnOrderNo
	                            , Flag = CAST(0 AS BIT)
								,ItemList=STUFF((SELECT distinct ','+cix.UserName FROM CostingBOQ AS cbx
													JOIN hkp.CostingItem AS cix ON cix.Id=cbx.CostingItemId
                                    where cbx.SalesOrderId=so.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,BOMList=STUFF((SELECT distinct ','+cbx.CostingBOQMasterId FROM [TRN].[SalesOrder] AS cbx
                                    where cbx.Id=so.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),OCMT.CostingStage
                       FROM [TRN].[SalesOrder] AS SO 
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       LEFT JOIN dbo.OrderCostingMasterTemplate OCMT ON OCMT.Id=MOI.OrderCostingMasterTemplateId
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
			  where SO.CostingBOQMasterId IN ('" + BOMMasterId + "')";

        }
        public string GetExistingSalesOrderListForReport(string BOMMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,0 AS Selected
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,so.OrderCostingMasterTemplateId
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , DEST.UserName AS DestinationName, SHP.UserName AS ShipmentModeName
	                            , PO.PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description,
	                            moi.BuyerReferenceNo BuyerItemNo,
	                            moi.OwnReferenceNo OwnItemNo,  mo.BuyerReferenceNo BuyerOrderNo,
	                            mo.OwnReferenceNo OwnOrderNo
	                            , Flag = CAST(0 AS BIT)
								,ItemList=STUFF((SELECT distinct ','+cix.UserName FROM CostingBOQ AS cbx
													JOIN hkp.CostingItem AS cix ON cix.Id=cbx.CostingItemId
                                    where cbx.SalesOrderId=so.Id and CBX.CostingBOQMasterId='" + BOMMasterId + @"' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,BOMList=STUFF((SELECT distinct ','+cbx.CostingBOQMasterId FROM CostingBOQSalesOrder AS cbx
                                    where cbx.SalesOrderId=so.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,BOMRemarks=STUFF((SELECT distinct ','+cbx.Remarks FROM CostingBOQMaster AS cbx
								INNER JOIN CostingBOQSalesOrder SX ON sx.CostingBOQMasterId=cbx.Id
                                    where sx.SalesOrderId=so.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                       FROM [TRN].[SalesOrder] AS SO 
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
			  where SO.Id IN (SELECT SalesOrderId
			  FROM CostingBOQSalesOrder WHERE CostingBOQMasterId='" + BOMMasterId + "')";

        }
        public string SalesOrderListForCostingBOQMaster(string CostingMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,0 AS Selected
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,so.OrderCostingMasterTemplateId
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , DEST.UserName AS DestinationName, SHP.UserName AS ShipmentModeName
	                            , PO.PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description,
	                            moi.BuyerReferenceNo BuyerItemNo,
	                            moi.OwnReferenceNo OwnItemNo,  mo.BuyerReferenceNo BuyerOrderNo,
	                            mo.OwnReferenceNo OwnOrderNo
	                            , Flag = CAST(0 AS BIT)
                       FROM [TRN].[SalesOrder] AS SO 
					   join CostingBOQSalesOrder BSO on SO.SalesOrderId=SO.Id
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
			where BSO.CostingBOQMasterId='" + CostingMasterId + @"'";

        }


        public string GetProductionHistorySql(string ProductionOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT ps.Id,FORMAT(ps.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,PLN.UserName AS Plant, E.UserName Entity ,ISNULL(fp.UserName,fs.UserName) AS FromLocation,fw.UserName AS FromWorkCenter,bp.UserName AS ProductionHour,
                               ISNULL(tp.UserName,ts.UserName) AS ToLocation,Tw.UserName AS ToWorkCenter,ps.ProductionGrade,ps.Quantity,ps.AddedBy,FORMAT(ps.AddedDate,'dd-MMM-yyyy') AS AddedDate,ps.LotNumber
                                FROM trn.ProductionSummary AS ps
                                LEFT JOIN hkp.ProductionBookingPeriod AS BP ON bp.Id=ps.ProductionBookingPeriodId
                                LEFT JOIN hkp.Process AS Fp ON Fp.id=ps.ProcessId
                                LEFT JOIN hkp.SFGInventory AS FS ON fs.id=ps.FromSFGInventoryId
                                LEFT JOIN hkp.Process AS SFGF ON SFGF.id=FS.ProcessId
                                LEFT JOIN scs.WorkCenterMaster AS Fw ON fw.Id=ps.WorkCenterMasterId
								LEFT JOIN ORG.Entity E on E.Id=ps.EntityId
								LEFT JOIN ORG.Plant PLN on PLN.Id=E.PlantId


                                LEFT JOIN hkp.Process AS Tp ON Tp.id=ps.ToProcessId
                                LEFT JOIN hkp.SFGInventory AS TS ON TS.id=ps.ToSFGInventoryId
                                LEFT JOIN hkp.Process AS SFGT ON SFGT.id=ts.ProcessId
                                LEFT JOIN scs.WorkCenterMaster AS Tw ON Tw.Id=ps.ToWorkCenterMasterId

                                WHERE ps.ProductionOrderId='" + ProductionOrderId + @"'
                                ORDER BY ISNULL(fp.Sequence,ISNULL(SFGF.Sequence,0)+ISNULL(SFGF.Sequence,0)*0.05),ps.ProductionDate,fw.Sequence,BP.Sequence ";

        }

        public IWorkbook GetProductionHistory(string ProductionOrderId)
        {

            #region declare

            ReportUtility oru = new ReportUtility();


            DataTable dtMainData = new DataTable();
            DataTable dtSFGinventory = new DataTable();

            StringCollection strColPrId = new StringCollection();



            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();

            #endregion
            try
            {

                DataTable dtData = _sqlRepository.GetDataTable(GetProductionHistorySql(ProductionOrderId));
                if (dtData.Rows.Count == 0)
                    throw new Exception("No production data found");

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;

                IWorkbook workbook = application.Workbooks.Create(1);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int ROW = 1, COL = 1;
                int endCol = 1;


                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];



                ROW = 5;


                string Key = "";
                int Serial = 0;
                int colSL = 0, colTransactionId = 0, colProductionDate = 0, colProductionHour = 0, colEntity = 0, colLotNumber = 0, colPlant = 0, colFromLocation = 0, colFromWorkCenter = 0, colToLocation = 0, colToWorkCenter = 0, colProductionGrade = 0, colQuantity = 0, colAddedBy = 0, colAddedDate = 0;

                int startRow = ROW;
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    string tempKey = dtData.Rows[i]["FromLocation"].ToString() + "-" + dtData.Rows[i]["ToLocation"].ToString();
                    if (Key != tempKey)
                    {
                        //do sum
                        if (i > 0)
                        {
                            sheet1[ROW, 1].Text = "Total";
                            sheet1.Range[ROW, 1, ROW, colQuantity - 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[ROW, 1, ROW, colQuantity].CellStyle.Font.Bold = true;
                            sheet1[ROW, colQuantity].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colQuantity) + startRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colQuantity) + (ROW - 1).ToString() + ")";
                            sheet1.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                            sheet1.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;

                            ROW++;
                        }

                        Serial = 0;
                        COL = 1; endCol = 1;
                        ROW++;

                        sheet1[ROW, 1].Text = "From:";
                        sheet1[ROW, 3].Text = dtData.Rows[i]["FromLocation"].ToString();

                        sheet1[ROW, 5].Text = "To:";
                        sheet1[ROW, 6].Text = dtData.Rows[i]["ToLocation"].ToString();

                        sheet1.Range[ROW, 1, ROW, 6].CellStyle.Font.Bold = true;
                        sheet1.Range[ROW, 1, ROW, 6].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;

                        ROW++;

                        #region ------------------Column Header------------------
                        colSL = COL;
                        sheet1.Range[ROW, COL].Text = "SL#";
                        sheet1.Range[ROW, COL].ColumnWidth = 6;
                        COL++;
                        colTransactionId = COL;
                        sheet1.Range[ROW, COL].Text = "Transaction Id";
                        sheet1.Range[ROW, COL].ColumnWidth = 12;
                        COL++;
                        colProductionDate = COL;
                        sheet1.Range[ROW, COL].Text = "Production Date";
                        sheet1.Range[ROW, COL].ColumnWidth = 12;
                        COL++;
                        colProductionHour = COL;
                        sheet1.Range[ROW, COL].Text = "Production Hour";
                        sheet1.Range[ROW, COL].ColumnWidth = 12;
                        COL++;
                        colPlant = COL;
                        sheet1.Range[ROW, COL].Text = "Plant";
                        sheet1.Range[ROW, COL].ColumnWidth = 12;
                        COL++;
                        colEntity = COL;
                        sheet1.Range[ROW, COL].Text = "Entity";
                        sheet1.Range[ROW, COL].ColumnWidth = 12;
                        COL++;
                        colLotNumber = COL;
                        sheet1.Range[ROW, COL].Text = "Lot Number";
                        sheet1.Range[ROW, COL].ColumnWidth = 12;
                        COL++;
                        colFromLocation = COL;
                        sheet1.Range[ROW, COL].Text = "From Location";
                        sheet1.Range[ROW, COL].ColumnWidth = 15;
                        COL++;
                        colFromWorkCenter = COL;
                        sheet1.Range[ROW, COL].Text = "From WorkCenter";
                        sheet1.Range[ROW, COL].ColumnWidth = 15;

                        COL++;
                        colToLocation = COL;
                        sheet1.Range[ROW, COL].Text = "To Location";
                        sheet1.Range[ROW, COL].ColumnWidth = 15;
                        COL++;
                        colToWorkCenter = COL;
                        sheet1.Range[ROW, COL].Text = "To WorkCenter";
                        sheet1.Range[ROW, COL].ColumnWidth = 15;

                        COL++;
                        colProductionGrade = COL;
                        sheet1.Range[ROW, COL].Text = "Grade";
                        sheet1.Range[ROW, COL].ColumnWidth = 10;


                        COL++;
                        colQuantity = COL;
                        sheet1.Range[ROW, COL].Text = "Quantity";
                        sheet1.Range[ROW, COL].ColumnWidth = 10;
                        sheet1.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

                        COL++;
                        colAddedBy = COL;
                        sheet1.Range[ROW, COL].Text = "Added By";
                        sheet1.Range[ROW, COL].ColumnWidth = 15;

                        COL++;
                        colAddedDate = COL;
                        sheet1.Range[ROW, COL].Text = "Added Date";
                        sheet1.Range[ROW, COL].ColumnWidth = 12;


                        endCol = COL;
                        sheet1.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[ROW, 1, ROW, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                        sheet1.Range[ROW, 1, ROW, endCol].WrapText = true;
                        sheet1.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                        sheet1.Range[ROW, 1, ROW, endCol].RowHeight = 23;
                        #endregion ------------------Column Header------------------

                        ROW++;
                        startRow = ROW;

                    }
                    Serial++;
                    sheet1[ROW, colSL].Number = Serial;
                    sheet1[ROW, colTransactionId].Text = dtData.Rows[i]["Id"].ToString();
                    sheet1[ROW, colProductionDate].Text = dtData.Rows[i]["ProductionDate"].ToString();
                    sheet1[ROW, colProductionHour].Text = dtData.Rows[i]["ProductionHour"].ToString();
                    sheet1[ROW, colEntity].Text = dtData.Rows[i]["Entity"].ToString();
                    sheet1[ROW, colLotNumber].Text = dtData.Rows[i]["LotNumber"].ToString();
                    sheet1[ROW, colPlant].Text = dtData.Rows[i]["Plant"].ToString();
                    sheet1[ROW, colFromLocation].Text = dtData.Rows[i]["FromLocation"].ToString();
                    sheet1[ROW, colFromWorkCenter].Text = dtData.Rows[i]["FromWorkCenter"].ToString();
                    sheet1[ROW, colToLocation].Text = dtData.Rows[i]["ToLocation"].ToString();
                    sheet1[ROW, colToWorkCenter].Text = dtData.Rows[i]["ToWorkCenter"].ToString();
                    sheet1[ROW, colProductionGrade].Text = dtData.Rows[i]["ProductionGrade"].ToString();
                    sheet1[ROW, colQuantity].Number = clsStaticInfo.dbl(dtData.Rows[i]["Quantity"].ToString());
                    sheet1[ROW, colAddedBy].Text = dtData.Rows[i]["AddedBy"].ToString();
                    sheet1[ROW, colAddedDate].Text = dtData.Rows[i]["AddedDate"].ToString();

                    if (dtData.Rows[i]["ProductionGrade"].ToString().ToUpper() != "A")
                        sheet1[ROW, colProductionGrade].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet1.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    ROW++;

                    Key = tempKey;

                }


                sheet1[ROW, 1].Text = "Total";
                sheet1.Range[ROW, 1, ROW, colQuantity - 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[ROW, 1, ROW, colQuantity].CellStyle.Font.Bold = true;
                sheet1[ROW, colQuantity].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colQuantity) + startRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colQuantity) + (ROW - 1).ToString() + ")";
                sheet1.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;

                ROW = 1;
                COL = 3;


                string FactoryName = string.Empty;

                string FactoryAddress = string.Empty;



                #region Freeze Panes

                sheet1.IsDisplayZeros = false;


                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.NumberFormat = clsStaticInfo.NumberFormat(0);

                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet1, endCol, "Production History PRNo-" + ProductionOrderId, identity.PlantId);
                reportUtility.PageSetup(ref sheet1, 6, ExcelPageOrientation.Landscape);
                sheet1.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;



                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$8";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + "" + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Production Data";
                #endregion Page Setup    
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPOLotControlData(string poId, string entityId)
        {
            try
            {

                var sql = @"SELECT DISTINCT ISNULL(LP.Id,P.Id) ProcessId,LC.Id,ISNULL(LP.UserName,P.UserName) Process
,EP.ProductionBookingLevel,PS.ProductionOrderId
,MasterOrderItemId=CASE WHEN (EP.ProductionBookingLevel='MasterOrderItem' OR EP.ProductionBookingLevel='SalesOrder') THEN ISNULL(LC.MasterOrderItemId,MOI.Id) ELSE NULL END
,SalesOrderId=CASE WHEN (EP.ProductionBookingLevel='MasterOrderItem' OR EP.ProductionBookingLevel='ProductionOrder'  OR EP.ProductionBookingLevel IS NULL) THEN NULL ELSE ISNULL(LC.SalesOrderId,SO.Id) END
,LotNo=FORMAT(GETDATE(), 'yy')+''+FORMAT(GETDATE(), 'MM')+''+PO.Id
,UserLotNo=FORMAT(GETDATE(), 'yy')+''+FORMAT(GETDATE(), 'MM')+''+PO.Id

,LotQty=CASE WHEN EP.ProductionBookingLevel='MasterOrderItem' THEN MOI.TotalQty 
		 WHEN EP.ProductionBookingLevel='SalesOrder' THEN SO.Qty 
		ELSE PO.Qty END

,OrderQty=CASE WHEN EP.ProductionBookingLevel='MasterOrderItem' THEN 
					(Select SUM(TotalQty)Qty From TRN.MasterOrderItem A
					LEFT JOIN TRN.SalesOrder B ON B.MasterOrderItemId=A.Id
					LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
					Where C.ProductionOrderId=PO.Id
					Group By C.ProductionOrderId)

		 WHEN EP.ProductionBookingLevel='SalesOrder' THEN 
				(Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=PO.Id
				Group By C.ProductionOrderId)

				ELSE (Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=PO.Id
				Group By C.ProductionOrderId) END

,PlanQty=(Select SUM((isnull(B.qty, 0) * (1 + (isnull(A.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(A.OrderWastagePercentage, 0)))) From TRN.MasterOrderItem A
					LEFT JOIN TRN.SalesOrder B ON B.MasterOrderItemId=A.Id
					LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
					Where C.ProductionOrderId=PO.Id
					Group By C.ProductionOrderId)

		
,SchedulePercentage=CONVERT(decimal(18,2), PO.Qty/(
CASE WHEN EP.ProductionBookingLevel='MasterOrderItem' THEN 
					(Select SUM(TotalQty)Qty From TRN.MasterOrderItem A
					LEFT JOIN TRN.SalesOrder B ON B.MasterOrderItemId=A.Id
					LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
					Where C.ProductionOrderId=PO.Id
					Group By C.ProductionOrderId)

		 WHEN EP.ProductionBookingLevel='SalesOrder' THEN 
				(Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=PO.Id
				Group By C.ProductionOrderId)

				ELSE (Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=PO.Id
				Group By C.ProductionOrderId) END
))

,ProcessPlanQty=CONVERT(decimal(18,2),((CASE WHEN EP.ProductionBookingLevel='MasterOrderItem' THEN 
					(Select SUM(TotalQty)Qty From TRN.MasterOrderItem A
					LEFT JOIN TRN.SalesOrder B ON B.MasterOrderItemId=A.Id
					LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
					Where C.ProductionOrderId=PO.Id
					Group By C.ProductionOrderId)

		 WHEN EP.ProductionBookingLevel='SalesOrder' THEN 
				(Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=PO.Id
				Group By C.ProductionOrderId)

				ELSE (Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=PO.Id
				Group By C.ProductionOrderId) END)*(CONVERT(decimal(18,2), PO.Qty/(
CASE WHEN EP.ProductionBookingLevel='MasterOrderItem' THEN 
					(Select SUM(TotalQty)Qty From TRN.MasterOrderItem A
					LEFT JOIN TRN.SalesOrder B ON B.MasterOrderItemId=A.Id
					LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
					Where C.ProductionOrderId=PO.Id
					Group By C.ProductionOrderId)

		 WHEN EP.ProductionBookingLevel='SalesOrder' THEN 
				(Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=PO.Id
				Group By C.ProductionOrderId)

				ELSE (Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=PO.Id
				Group By C.ProductionOrderId) END
)))/(100-(PS.Qty-100))*100))

,LC.Remark,RANK() OVER(PARTITION BY moi.Id ORDER BY moi.Id DESC) Rank,A.StandardName LotArticle
FROM TRN.ProductionOrderProcessSet PS
LEFT JOIN [dbo].[ProductionOrderLotControl] LC ON LC.ProductionOrderId=PS.ProductionOrderId AND PS.ProcessId=LC.ProcessId
LEFT JOIN HKP.Process P ON P.Id=PS.ProcessId
LEFT JOIN HKP.Process LP ON LP.Id=LC.ProcessId
LEFT JOIN [HKP].[EntityProcessTag] EP ON EP.ProcessId=PS.ProcessId AND EP.EntityId='" + entityId + @"'
LEFT JOIN TRN.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderDetail POD ON POD.ProductionOrderId=PO.Id
LEFT JOIN TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
LEFT JOIN TRN.[MasterOrderItem] MOI ON SO.MasterOrderItemId=moi.Id
LEFT JOIN MST.MaterialMasterArticle A ON A.Id=MOI.ArticleId
Where PS.ProductionOrderId='" + poId + "' Order By P.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPOLotControlSettingData(string entityId, string PoId)
        {
            try
            {

                var sql = @"SELECT DISTINCT L.[Id], L.[SeqNo], L.[EntityId], L.[LotNo], L.[UserLotNo], L.[ProductionBookingLevel], L.[ProductionOrderId]
, L.[MasterOrderItemId], L.[SalesOrderId], L.[ProcessId], L.[LotQty], L.[OrderQty], L.[PlanQty], L.[SchedulePercentage]
, L.[ProcessPlanQty], L.[Sufix], L.[Remark],P.UserName Process
,LotArticle=ISNULL(A.StandardName
,STUFF((SELECT distinct ','+IA.StandardName FROM TRN.ProductionOrderDetail PD
LEFT JOIN TRN.SalesOrder S ON S.Id=PD.SalesOrderId
LEFT JOIN TRN.[MasterOrderItem] MI ON S.MasterOrderItemId=mi.Id
LEFT JOIN MST.MaterialMasterArticle IA ON IA.Id=MI.ArticleId
 where PD.ProductionOrderId=L.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
,ISNULL(L.IsDefault,0)IsDefault ,CAST(ROW_NUMBER() OVER(ORDER BY P.UserName) AS INT) Serial 
, UsedInPS=ISNULL((Select top(1) CAST(CASE WHEN Id IS NULL THEN 0 ELSE 1 END AS bit) from TRN.ProductionSummary Where ProductionOrderId=L.ProductionOrderId AND ProcessId=L.ProcessId ORDER BY AddedDate DESC),0)
FROM [dbo].[ProductionOrderLotControl] L
LEFT JOIN HKP.Process P ON P.Id=L.ProcessId
LEFT JOIN TRN.[MasterOrderItem] MOI ON L.MasterOrderItemId=moi.Id
LEFT JOIN MST.MaterialMasterArticle A ON A.Id=MOI.ArticleId
Where L.ProductionOrderId='" + PoId + "' AND L.EntityId='" + entityId + "' Order By P.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetPOLotControlSettingsData(string entityId, string PoId, string userLotNo)
        {
            try
            {

                var sql = @"Select LCS.Id,P.UserName Process,P.Id ProcessId,EntityId='" + entityId + @"',B.* ,LotQty=B.Qty
,OrderQty=CASE WHEN B.ProductionBookingLevel='MasterOrderItem' THEN 
					(Select SUM(TotalQty)Qty From TRN.MasterOrderItem A
					LEFT JOIN TRN.SalesOrder B ON B.MasterOrderItemId=A.Id
					LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
					Where C.ProductionOrderId=" + PoId + @"
					Group By C.ProductionOrderId)

		 WHEN B.ProductionBookingLevel='SalesOrder' THEN 
				(Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=" + PoId + @"
				Group By C.ProductionOrderId)

				ELSE (Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=" + PoId + @"
				Group By C.ProductionOrderId) END

,PlanQty=(Select SUM((isnull(B.qty, 0) * (1 + (isnull(A.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(A.OrderWastagePercentage, 0)))) From TRN.MasterOrderItem A
					LEFT JOIN TRN.SalesOrder B ON B.MasterOrderItemId=A.Id
					LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
					Where C.ProductionOrderId=" + PoId + @"
					Group By C.ProductionOrderId)

		
,SchedulePercentage=CONVERT(decimal(18,2), B.Qty/(
CASE WHEN B.ProductionBookingLevel='MasterOrderItem' THEN 
					(Select SUM(TotalQty)Qty From TRN.MasterOrderItem A
					LEFT JOIN TRN.SalesOrder B ON B.MasterOrderItemId=A.Id
					LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
					Where C.ProductionOrderId=" + PoId + @"
					Group By C.ProductionOrderId)

		 WHEN B.ProductionBookingLevel='SalesOrder' THEN 
				(Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=" + PoId + @"
				Group By C.ProductionOrderId)

				ELSE (Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=" + PoId + @"
				Group By C.ProductionOrderId) END
))

,ProcessPlanQty=CONVERT(decimal(18,2),((CASE WHEN B.ProductionBookingLevel='MasterOrderItem' THEN 
					(Select SUM(TotalQty)Qty From TRN.MasterOrderItem A
					LEFT JOIN TRN.SalesOrder B ON B.MasterOrderItemId=A.Id
					LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
					Where C.ProductionOrderId=" + PoId + @"
					Group By C.ProductionOrderId)

		 WHEN B.ProductionBookingLevel='SalesOrder' THEN 
				(Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=" + PoId + @"
				Group By C.ProductionOrderId)

				ELSE (Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=" + PoId + @"
				Group By C.ProductionOrderId) END)*(CONVERT(decimal(18,2), B.Qty/(
CASE WHEN B.ProductionBookingLevel='MasterOrderItem' THEN 
					(Select SUM(TotalQty)Qty From TRN.MasterOrderItem A
					LEFT JOIN TRN.SalesOrder B ON B.MasterOrderItemId=A.Id
					LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
					Where C.ProductionOrderId=" + PoId + @"
					Group By C.ProductionOrderId)

		 WHEN B.ProductionBookingLevel='SalesOrder' THEN 
				(Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=" + PoId + @"
				Group By C.ProductionOrderId)

				ELSE (Select SUM(Qty)Qty From TRN.SalesOrder B 
				LEFT JOIN TRN.ProductionOrderDetail C ON C.SalesOrderId=B.Id
				Where C.ProductionOrderId=" + PoId + @"
				Group By C.ProductionOrderId) END
)))/(100-(PS.Qty-100))*100))
,UserLotNo=CASE WHEN B.ProductionBookingLevel='ProductionOrder' THEN CAST('" + userLotNo + @"' AS varchar(50))
				WHEN B.ProductionBookingLevel='MasterOrderItem' THEN CAST(B.SeqNo AS varchar(10)) +'-'+CAST('" + userLotNo + @"' AS varchar(50))
				WHEN B.ProductionBookingLevel='SalesOrder' THEN 'S'+CAST(B.SeqNo AS varchar(10)) +'-'+CAST('" + userLotNo + @"' AS varchar(50)) ELSE NULL END
,LotNo=CASE WHEN B.ProductionBookingLevel='ProductionOrder' THEN CAST('" + userLotNo + @"' AS varchar(50))
				WHEN B.ProductionBookingLevel='MasterOrderItem' THEN CAST(B.SeqNo AS varchar(10)) +'-'+CAST('" + userLotNo + @"' AS varchar(50))
				WHEN B.ProductionBookingLevel='SalesOrder' THEN 'S'+CAST(B.SeqNo AS varchar(10)) +'-'+CAST('" + userLotNo + @"' AS varchar(50)) ELSE NULL END


,NULL Sufix
From(
Select A.*,ProductionBookingLevel=CASE WHEN (A.MasterOrderItemId IS NULL AND A.SalesOrderId IS NULL) THEN 'ProductionOrder'   
					WHEN (A.MasterOrderItemId IS NOT NULL AND A.SalesOrderId IS NULL) THEN 'MasterOrderItem'   
					WHEN (A.MasterOrderItemId IS NULL AND A.SalesOrderId IS  NOT NULL) THEN 'SalesOrder' ELSE NULL END

from (
Select Id ProductionOrderId,NULL MasterOrderItemId,NULL SalesOrderId,Qty
,SeqNo=ROW_NUMBER() OVER(ORDER BY Id DESC) from TRN.ProductionOrder Where Id=" + PoId + @"
UNION
select POD.ProductionOrderId,SO.MasterOrderItemId,NULL SalesOrderId,MOI.TotalQty Qty
,SeqNo=ROW_NUMBER() OVER (ORDER BY POD.ProductionOrderId,SO.MasterOrderItemId)
from TRN.ProductionOrderDetail POD
LEFT JOIN TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
Where  POD.ProductionOrderId=" + PoId + @"
UNION
SELECT POD.ProductionOrderId,NULL MasterOrderItemId,SO.Id SalesOrderId,SO.Qty
,SeqNo=ROW_NUMBER() OVER (ORDER BY POD.ProductionOrderId)
from TRN.ProductionOrderDetail POD
LEFT JOIN TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
Where  POD.ProductionOrderId=" + PoId + @")A
)B
LEFT JOIN TRN.ProductionOrderProcessSet PS on PS.ProductionOrderId=" + PoId + @" and B.ProductionBookingLevel=PS.ProductionBookingLevel
LEFT JOIN HKP.Process P ON P.Id=PS.ProcessId
LEFT JOIN HKP.EntityProcessTag T ON T.EntityId=" + entityId + @" AND P.Id=T.ProcessId
LEFT JOIN dbo.LotControlSetting LCS ON LCS.EntityId='" + entityId + @"' AND LCS.ProcessId=P.Id AND LCS.ProductionOrderId=" + PoId + @" 
AND LCS.MasterOrderItemId=B.MasterOrderItemId AND LCS.SalesOrderId=B.SalesOrderId AND LCS.ProductionBookingLevel=B.ProductionBookingLevel
Where P.Id IS NOT NULL";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPOData(string poId)
        {
            try
            {

                var sql = @"select * from (SELECT distinct so.SONos,so.Customer,so.Article,so.ArticleId,so.StyleNo, so.OwnStyleNo, so.Product,
                            PO.Id POId,s.UserName AS POStatus,SO.SOQuantity SOQty,ISNULL(PO.Qty,0) AS POQuantity, So.LineItemId,SO.SOStatus
                            FROM [TRN].[ProductionOrder] AS PO  JOIN TRN.ProductionOrderProcessSet POP ON POP.ProductionOrderId=PO.Id                          
                            LEFT OUTER  JOIN (select pod.ProductionOrderId, sum(so.Qty) AS SOQuantity,
                                                    LineItemId=STUFF((select distinct ','+XMOI.Id from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONos=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
													SOStatus=STUFF((select distinct ','+OS.UserName from 
								                                 HKP.OrderStatus OS 
								                                INNER JOIN trn.SalesOrder AS sox on OS.Id=SOX.OrderStatusId
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                                   
                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													
                                                     ,Article=STUFF((select distinct ', '+mm.StandardName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                    ,ArticleId=STUFF((select distinct ', '+mm.Id from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													,Product=STUFF((select distinct ', '+Pm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													
                            from trn.ProductionOrderDetail AS pod JOIN  trn.SalesOrder SO ON pod.SalesOrderId=so.Id group by pod.ProductionOrderId
                            ) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE PO.Id='" + poId + @"'
							) AS TEMP";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> _GetSKUData(string poId, bool SKU1, bool SKU2, bool Both)
        {
            try
            {
                string sql = "";
                if (SKU1)
                {
                    sql = @"Select ''Id,D.ProductionOrderId,FC.CharacteristicsValueId SKU1Id,CV.UserName Color,SUM(Qty)Qty From [TRN].[FirstCharacteristics] FC 
LEFT JOIN HKP.CharacteristicsValue CV ON CV.Id=FC.CharacteristicsValueId
left join TRN.ProductionOrderDetail D ON D.SalesOrderId=FC.SalesOrderId
Where D.ProductionOrderId='" + poId + @"'
Group By  D.ProductionOrderId,FC.CharacteristicsValueId,CV.UserName";

                }
                if (SKU2)
                {
                    sql = @"Select ''Id,D.ProductionOrderId,SC.CharacteristicsValueId SKU2Id,SCV.UserName Size,SUM(Qty)Qty From TRN.[SecondCharacteristics] SC
LEFT JOIN HKP.CharacteristicsValue SCV ON SCV.Id=SC.CharacteristicsValueId
left join TRN.ProductionOrderDetail D ON D.SalesOrderId=SC.SalesOrderId
Where D.ProductionOrderId='" + poId + @"'
Group By  D.ProductionOrderId,SC.CharacteristicsValueId,SCV.UserName";
                }
                if (SKU1 == true && SKU2 == true || Both == true)
                {
                    sql = @"Select ''Id,D.ProductionOrderId,FC.CharacteristicsValueId SKU1Id,SC.CharacteristicsValueId SKU2Id,FCV.UserName Color,SCV.UserName Size,SUM(SC.Qty)Qty 
From TRN.[SecondCharacteristics] SC
LEFT JOIN [TRN].[FirstCharacteristics] FC ON FC.Id=SC.FirstCharacteristicsId
LEFT JOIN HKP.CharacteristicsValue FCV ON FCV.Id=FC.CharacteristicsValueId
LEFT JOIN HKP.CharacteristicsValue SCV ON SCV.Id=SC.CharacteristicsValueId
left join TRN.ProductionOrderDetail D ON D.SalesOrderId=SC.SalesOrderId
Where D.ProductionOrderId='" + poId + @"'
Group By  D.ProductionOrderId,FC.CharacteristicsValueId,SC.CharacteristicsValueId,FCV.UserName ,SCV.UserName";
                }
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSKUData(string poId, bool SKU1, bool SKU2, bool Both)
        {
            try
            {
                string sql = "";
                if (SKU1 && !SKU2 && !Both)
                {
                    sql = @"Select ''ID,D.ProductionOrderId,FC.CharacteristicsValueId SKU1Id,CV.UserName SKUColor,SUM(Qty)Qty From [TRN].[FirstCharacteristics] FC 
LEFT JOIN HKP.CharacteristicsValue CV ON CV.Id=FC.CharacteristicsValueId
left join TRN.ProductionOrderDetail D ON D.SalesOrderId=FC.SalesOrderId
Where D.ProductionOrderId='" + poId + @"'
Group By  D.ProductionOrderId,FC.CharacteristicsValueId,CV.UserName";

                }
                else if (SKU2 && !SKU1 && !Both)
                {
                    sql = @"Select ''ID,D.ProductionOrderId,SC.CharacteristicsValueId SKU2Id,SCV.UserName SKUSize,SUM(Qty)Qty From TRN.[SecondCharacteristics] SC
LEFT JOIN HKP.CharacteristicsValue SCV ON SCV.Id=SC.CharacteristicsValueId
left join TRN.ProductionOrderDetail D ON D.SalesOrderId=SC.SalesOrderId
Where D.ProductionOrderId='" + poId + @"'
Group By  D.ProductionOrderId,SC.CharacteristicsValueId,SCV.UserName";
                }
                else
                {
                    sql = @"Select ''ID,D.ProductionOrderId,FC.CharacteristicsValueId SKU1Id,SC.CharacteristicsValueId SKU2Id,FCV.UserName SKUColor,SCV.UserName SKUSize,SUM(SC.Qty)Qty 
From TRN.[SecondCharacteristics] SC
LEFT JOIN [TRN].[FirstCharacteristics] FC ON FC.Id=SC.FirstCharacteristicsId
LEFT JOIN HKP.CharacteristicsValue FCV ON FCV.Id=FC.CharacteristicsValueId
LEFT JOIN HKP.CharacteristicsValue SCV ON SCV.Id=SC.CharacteristicsValueId
left join TRN.ProductionOrderDetail D ON D.SalesOrderId=SC.SalesOrderId
Where D.ProductionOrderId='" + poId + @"'
Group By  D.ProductionOrderId,FC.CharacteristicsValueId,SC.CharacteristicsValueId,FCV.UserName ,SCV.UserName";
                }


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex) { throw ex; }
        }


        public IEnumerable<object> GetSavedSKUData(string poId)
        {
            try
            {
                // 1. Check if data already saved for this Production Order
                string checkSql = @"SELECT * FROM dbo.ProductionOrderSchedulingParametersType2 WHERE ProductionOrderId = '" + poId + @"'";
                DataTable savedCount = _sqlRepository.GetDataTable(checkSql); // adjust to your repo's scalar method



                string sql = "";
                if (savedCount.Rows.Count > 0)
                {

                    if (Convert.ToBoolean(savedCount.Rows[0]["SKU1"].ToString()) && !Convert.ToBoolean(savedCount.Rows[0]["SKU2"].ToString()) && !Convert.ToBoolean(savedCount.Rows[0]["Both"].ToString()))
                    {
                        sql = @"SELECT D.ID,D.ProductionOrderID,D.NoOfWorkStation,D.Efficiency,D.SPT,D.PlanWorkingHoursPerDay,D.FirstDayOutPut,D.PlanTargetPerHour,D.IncrementValue,D.IncrementType,D.DayToReachTheTarget
,FORMAT(D.LSD,'dd-MMM-yyyy')LSD,FORMAT(D.CommitmentDate,'dd-MMM-yyyy')CommitmentDate,D.ProductionPriority,D.TargetPerHour,D.TargetPerDay,D.MinimumLineDays,D.RequiredLineDays
,D.RequiredNoOfLines,D.AllocatedLines,D.Color,D.Qty,D.Qty PlanQty,FORMAT(D.MainRawMaterialInhouseDate,'dd-MMM-yyyy')MainRawMaterialInhouseDate
,FORMAT(D.OtherRawMaterialInhouseDate,'dd-MMM-yyyy')OtherRawMaterialInhouseDate,D.WCPreferenceType,D.PlanningStatus,D.RunningOrderBlockSize,D.ConsiderHourFromWorkCenter,D.ConsiderWorkStationsFromWorkCenter,D.WorkCenterGroupId
,D.AddedBy,D.AddedDate,D.AddedFromIP,D.UpdatedBy,D.UpdatedDate,D.UpdatedFromIP,D.AdjustableQty,D.SKU1,D.SKU2,D.Both,D.SKU1Id,D.SKU2Id,CV.UserName SKUColor, WG.UserName WorkCenterGroup,D.MaximumAllowedWorkCenter,D.PlanPercentage,D.ProductionStatusId
                     FROM dbo.ProductionOrderSchedulingParametersType2  D
                     LEFT JOIN HKP.CharacteristicsValue CV ON CV.Id = D.SKU1Id
                     LEFT JOIN HKP.WorkCenterGroup WG ON WG.Id=D.WorkCenterGroupId
                     WHERE D.ProductionOrderId = '" + poId + @"'";
                    }
                    else if (Convert.ToBoolean(savedCount.Rows[0]["SKU2"].ToString()) && !Convert.ToBoolean(savedCount.Rows[0]["SKU1"].ToString()) && !Convert.ToBoolean(savedCount.Rows[0]["Both"].ToString()))
                    {
                        sql = @"Select D.ID,D.ProductionOrderID,D.NoOfWorkStation,D.Efficiency,D.SPT,D.PlanWorkingHoursPerDay,D.FirstDayOutPut,D.PlanTargetPerHour,D.IncrementValue,D.IncrementType,D.DayToReachTheTarget
,FORMAT(D.LSD,'dd-MMM-yyyy')LSD,FORMAT(D.CommitmentDate,'dd-MMM-yyyy')CommitmentDate,D.ProductionPriority,D.TargetPerHour,D.TargetPerDay,D.MinimumLineDays,D.RequiredLineDays
,D.RequiredNoOfLines,D.AllocatedLines,D.Color,D.Qty,D.Qty PlanQty,FORMAT(D.MainRawMaterialInhouseDate,'dd-MMM-yyyy')MainRawMaterialInhouseDate
,FORMAT(D.OtherRawMaterialInhouseDate,'dd-MMM-yyyy')OtherRawMaterialInhouseDate,D.WCPreferenceType,D.PlanningStatus,D.RunningOrderBlockSize,D.ConsiderHourFromWorkCenter,D.ConsiderWorkStationsFromWorkCenter,D.WorkCenterGroupId
,D.AddedBy,D.AddedDate,D.AddedFromIP,D.UpdatedBy,D.UpdatedDate,D.UpdatedFromIP,D.AdjustableQty,D.SKU1,D.SKU2,D.Both,D.SKU1Id,D.SKU2Id, CV.UserName SKUSize, WG.UserName WorkCenterGroup,D.MaximumAllowedWorkCenter,D.PlanPercentage,D.ProductionStatusId
                     FROM dbo.ProductionOrderSchedulingParametersType2  D
                     LEFT JOIN HKP.CharacteristicsValue CV ON CV.Id = D.SKU2Id
                     LEFT JOIN HKP.WorkCenterGroup WG ON WG.Id=D.WorkCenterGroupId
                     WHERE D.ProductionOrderId = '" + poId + @"'";
                    }
                    else
                    {
                        sql = @"SELECT D.ID,D.ProductionOrderID,D.NoOfWorkStation,D.Efficiency,D.SPT,D.PlanWorkingHoursPerDay,D.FirstDayOutPut,D.PlanTargetPerHour,D.IncrementValue,D.IncrementType,D.DayToReachTheTarget
,FORMAT(D.LSD,'dd-MMM-yyyy')LSD,FORMAT(D.CommitmentDate,'dd-MMM-yyyy')CommitmentDate,D.ProductionPriority,D.TargetPerHour,D.TargetPerDay,D.MinimumLineDays,D.RequiredLineDays
,D.RequiredNoOfLines,D.AllocatedLines,D.Color,D.Qty,D.Qty PlanQty,FORMAT(D.MainRawMaterialInhouseDate,'dd-MMM-yyyy')MainRawMaterialInhouseDate
,FORMAT(D.OtherRawMaterialInhouseDate,'dd-MMM-yyyy')OtherRawMaterialInhouseDate,D.WCPreferenceType,D.PlanningStatus,D.RunningOrderBlockSize,D.ConsiderHourFromWorkCenter,D.ConsiderWorkStationsFromWorkCenter,D.WorkCenterGroupId
,D.AddedBy,D.AddedDate,D.AddedFromIP,D.UpdatedBy,D.UpdatedDate,D.UpdatedFromIP,D.AdjustableQty,D.SKU1,D.SKU2,D.Both,D.SKU1Id,D.SKU2Id, WG.UserName WorkCenterGroup,FCV.UserName SKUColor, SCV.UserName SKUSize,D.MaximumAllowedWorkCenter,D.PlanPercentage,D.ProductionStatusId
                     FROM dbo.ProductionOrderSchedulingParametersType2  D
                     LEFT JOIN HKP.CharacteristicsValue FCV ON FCV.Id = D.SKU1Id
                     LEFT JOIN HKP.CharacteristicsValue SCV ON SCV.Id = D.SKU2Id
                     LEFT JOIN HKP.WorkCenterGroup WG ON WG.Id=D.WorkCenterGroupId
                     WHERE D.ProductionOrderId = '" + poId + @"'";
                    }
                }
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex) { throw ex; }
        }


        public IEnumerable<object> GetWorkCenterGroup()
        {
            try
            {
                string sql = @"select WG.* from hkp.WorkCenterGroup WG";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSalesOrderFilterData()
        {
            try
            {
                string sql = @"select Po.Id POId,SO.Id SOId,P.Id PartyId, P.UserName Customer from TRN.ProductionOrder PO
LEFT JOIN TRN.ProductionOrderDetail PD ON PD.ProductionOrderId=PO.Id
LEFT JOIN TRN.SalesOrder SO ON SO.Id=PD.SalesOrderId
LEFT JOIN TRN.MasterOrderItem I ON I.Id=SO.MasterOrderItemId
LEFT JOIN TRN.MasterOrder M ON M.Id=I.MasterOrderId
LEFT JOIN HKP.Party P ON P.Id=M.PartyId
Where SO.OrderStatusId NOT IN('Closed,Cancelled')";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPackingSKUData(string soId,string packetRegistrationTypeId)
        {
            try
            {

                string sql = @"SELECT PR.Id,SC.SalesOrderId,FC.CharacteristicsValueId AS SKU1Id,SC.CharacteristicsValueId AS SKU2Id,FCV.UserName AS SKUColor,SCV.UserName AS SKUSize,
    CEILING((SUM(SC.Qty) * CM.PlanPercentage / 100.0) + SUM(SC.Qty)) AS NoOfUnit,SUM(SC.Qty) AS Qty,
    UnitPerPack = ISNULL(PR.UnitPerPack,PT.NoOfUnitPerPack),PR.BarCode,PR.QRCode,PR.RFID,PR.Remark,
    NoOfPack =CEILING(ISNULL(PR.NoOfPack,((SUM(SC.Qty) * CM.PlanPercentage / 100.0)+ SUM(SC.Qty))/ ISNULL(PR.UnitPerPack,PT.NoOfUnitPerPack)))
    ,LineItemReference = COALESCE(PR.LineItemReference,CM.LineItemReference,MOI.BuyerReferenceNo)
FROM TRN.SecondCharacteristics SC
LEFT JOIN TRN.FirstCharacteristics FC ON FC.Id = SC.FirstCharacteristicsId
LEFT JOIN HKP.CharacteristicsValue FCV ON FCV.Id = FC.CharacteristicsValueId
LEFT JOIN HKP.CharacteristicsValue SCV ON SCV.Id = SC.CharacteristicsValueId
LEFT JOIN TRN.ProductionOrderDetail D ON D.SalesOrderId = SC.SalesOrderId
LEFT JOIN dbo.PacketRegistration PR ON PR.SalesOrderId = SC.SalesOrderId AND FC.CharacteristicsValueId = PR.SKU1Id AND SC.CharacteristicsValueId = PR.SKU2Id
    AND PR.PacketRegistrationTypeId = '" + packetRegistrationTypeId + @"'   -- IMPORTANT
LEFT JOIN dbo.PacketRegistrationType PT ON PT.Id = '" + packetRegistrationTypeId + @"'
LEFT JOIN dbo.PacketRegistrationMaster CM ON CM.Id = PT.PacketRegistrationMasterId
LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=(Select MasterOrderItemId From TRN.SalesOrder Where Id " + soId + @")
WHERE SC.SalesOrderId " + soId + @"
GROUP BY PR.Id,SC.SalesOrderId,FC.CharacteristicsValueId,SC.CharacteristicsValueId,FCV.UserName,SCV.UserName,PR.UnitPerPack,PT.NoOfUnitPerPack,PR.BarCode,PR.QRCode,PR.RFID,PR.Remark,CM.PlanPercentage,PR.NoOfPack,PR.LineItemReference,CM.LineItemReference,MOI.BuyerReferenceNo
HAVING SUM(SC.Qty) <> 0;";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex) { throw ex; }
        }


        public IEnumerable<object> GetCartonList(string masterId)
        {
            try
            {
                string sql = @"WITH CartonData AS
(
    SELECT 
        M.UserName AS PackingName,P.UserName AS Customer,D.SalesOrderId AS SOId,FCV.UserName AS Color,SCV.UserName AS Size,CG.CartonNo,CG.NoOfPcs,        
        ROW_NUMBER() OVER (PARTITION BY M.UserName,P.UserName,D.SalesOrderId,FCV.UserName,SCV.UserName ORDER BY CAST(CG.CartonNo AS INT)) AS RowNo
    FROM dbo.PacketRegistrationMaster M
    LEFT JOIN dbo.PacketRegistrationType T ON T.PacketRegistrationMasterId = M.Id
    LEFT JOIN dbo.PacketRegistrationDetail D ON D.PacketRegistrationMasterId = M.Id
    LEFT JOIN TRN.SalesOrder S ON S.Id = D.SalesOrderId
    LEFT JOIN TRN.MasterOrderItem MI ON MI.Id = S.MasterOrderItemId
    LEFT JOIN TRN.MasterOrder MO ON MO.Id = MI.MasterOrderId
    LEFT JOIN HKP.Party P ON P.Id = MO.PartyId
    LEFT JOIN dbo.PacketRegistration R ON R.PacketRegistrationTypeId = T.Id AND R.SalesOrderId = D.SalesOrderId
    LEFT JOIN dbo.CartonGeneration CG ON CG.PacketRegistrationId = R.Id
    LEFT JOIN HKP.CharacteristicsValue FCV ON FCV.Id = R.SKU1Id
    LEFT JOIN HKP.CharacteristicsValue SCV ON SCV.Id = R.SKU2Id
    WHERE M.StatusType IN ('Running','Active') AND CG.PacketRegistrationId = '" + masterId + @"'
)
SELECT PackingName,Customer,SOId,Color,Size,
    STRING_AGG(CAST(CartonNo AS VARCHAR(MAX)),',') WITHIN GROUP (ORDER BY CAST(CartonNo AS INT)) AS CartonNo, SUM(NoOfPcs) AS NoOfPcs
FROM CartonData
GROUP BY PackingName,Customer,SOId,Color,Size,((RowNo - 1) / 10)
ORDER BY MIN(RowNo);";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetQRCodeData(string masterId)
        {
            try
            {
                string sql = @"SELECT M.UserName AS PackingName,P.UserName AS Customer,D.SalesOrderId AS SOId,FCV.UserName AS Color,SCV.UserName AS Size,CG.NoOfPcs,CG.Id CartonNo        
    FROM dbo.PacketRegistrationMaster M
    LEFT JOIN dbo.PacketRegistrationType T ON T.PacketRegistrationMasterId = M.Id
    LEFT JOIN dbo.PacketRegistrationDetail D ON D.PacketRegistrationMasterId = M.Id
    LEFT JOIN TRN.SalesOrder S ON S.Id = D.SalesOrderId
    LEFT JOIN TRN.MasterOrderItem MI ON MI.Id = S.MasterOrderItemId
    LEFT JOIN TRN.MasterOrder MO ON MO.Id = MI.MasterOrderId
    LEFT JOIN HKP.Party P ON P.Id = MO.PartyId
    LEFT JOIN dbo.PacketRegistration R ON R.PacketRegistrationTypeId = T.Id AND R.SalesOrderId = D.SalesOrderId
    LEFT JOIN dbo.CartonGeneration CG ON CG.PacketRegistrationId = R.Id
    LEFT JOIN HKP.CharacteristicsValue FCV ON FCV.Id = R.SKU1Id
    LEFT JOIN HKP.CharacteristicsValue SCV ON SCV.Id = R.SKU2Id
    WHERE M.StatusType IN ('Running','Active') AND CG.PacketRegistrationId = '"+ masterId +@"' 
    Order By CG.CartonNo";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }


}
