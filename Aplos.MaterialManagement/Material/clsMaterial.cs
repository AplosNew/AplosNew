using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Library.MaterialManagement.Material
{
    public class clsMaterial
    {
        ISqlRepository _sqlRepository;
        public clsMaterial()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetCharacteristicsValueCboByCharacteristicsIdAfterSave(string materialMasterId, string characteristicsId, string valueAssignmentLevel, string MarkerMasterId)
        {
            try
            {
                var _sql = string.Empty;
                if (valueAssignmentLevel == ValueAssignmentEnum.Specific.ToString())

                    _sql = @"SELECT IsSelect = case when M.Id is null then Convert(bit, 'False')ELSE Convert(bit, 'True') END  ,CV.Id AS CharacteristicsValueId,CV.Code, CV.UserName AS [Text] 
                                ,Ratio = case when M.Id is null then null else M.Ratio end,M.Id,M.MarkerMasterId
                                FROM [HKP].[Characteristics] C
                            LEFT JOIN hkp.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id
							left join MarkerDetails M on M.CharacteristicsValueId=CV.Id and M.MarkerMasterId='" + MarkerMasterId + @"'
                            Where CV.MaterialMasterId='" + materialMasterId + "' AND CV.CharacteristicsId='" + characteristicsId + "' AND  C.ValueAssignmentLevel='" + valueAssignmentLevel + "'  Order by CV.Sequence";

                else

                    _sql = @"SELECT IsSelect = case when M.Id is null then Convert(bit, 'False')ELSE Convert(bit, 'True') END  ,CV.Id AS CharacteristicsValueId,CV.Code, CV.UserName AS [Text] 
                                ,Ratio = case when M.Id is null then null else M.Ratio end,M.Id,M.MarkerMasterId
                                FROM [HKP].[Characteristics] C
                            LEFT JOIN hkp.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id
							left join MarkerDetails M on M.CharacteristicsValueId=CV.Id and M.MarkerMasterId='" + MarkerMasterId + @"'
                            Where CV.CharacteristicsId='" + characteristicsId + "' AND  C.ValueAssignmentLevel='" + valueAssignmentLevel + "' AND  CV.SourceType='" + valueAssignmentLevel + "'  Order by CV.Sequence";
                return _sqlRepository.GetDataCollection(_sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetCharacteristicsValueCboByCharacteristicsId(string materialMasterId, string characteristicsId, string valueAssignmentLevel)
        {
            try
            {
                var _sql = string.Empty;
                if (valueAssignmentLevel == ValueAssignmentEnum.Specific.ToString())

                    _sql = @"SELECT IsSelect =Convert(bit, 'False') ,CV.Id AS CharacteristicsValueId,CV.Code, CV.UserName AS [Text] ,'' Ratio,null Id FROM [HKP].[Characteristics] C
                            LEFT JOIN hkp.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id
                            Where CV.MaterialMasterId='" + materialMasterId + "' AND CV.CharacteristicsId='" + characteristicsId + "' AND  C.ValueAssignmentLevel='" + valueAssignmentLevel + "'  Order by CV.Sequence";

                else

                    _sql = @"SELECT IsSelect =Convert(bit, 'False'),CV.Id AS CharacteristicsValueId,CV.Code, CV.UserName AS [Text] ,'' Ratio,null Id  FROM [HKP].[Characteristics] C
                            LEFT JOIN hkp.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id
                            Where CV.CharacteristicsId='" + characteristicsId + "' AND  C.ValueAssignmentLevel='" + valueAssignmentLevel + "' AND  CV.SourceType='" + valueAssignmentLevel + "' Order by CV.Sequence";
                return _sqlRepository.GetDataCollection(_sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getFiltersData()
        {
            try
            {
                var sql = @"Select PS.UserName POStatus,PO.Id PONo from TRN.ProductionOrder PO 
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> EntityList()
        {
            string sql = @"";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (identity.IsSysAdmin)
            {
                sql = @"SELECT distinct E.Id Value,E.PlantId,P.UserName AS PlantName,e.Code,e.UserName AS Text
                        FROM [ORG].[Entity] E
                            LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E.Id
                            LEFT JOIN org.Plant AS p ON p.Id=e.PlantId
                            LEFT JOIN org.Company AS c ON c.Id=e.CompanyId
                            WHERE  e.Id IN (
                        SELECT ept.EntityId FROM hkp.EntityProcessTag AS ept WHERE ept.ProcessId IN (SELECT pt.BaseProcessId FROM PlanningTypes AS pt)) AND E.[Active]=1 AND e.CompanyId='" + identity.CompanyId + @"'
                        ORDER BY e.Code";

                return _sqlRepository.GetDataCollection(sql, null);
            }

            sql = @"SELECT  distinct E2.Id Value,e2.PlantId,P.UserName AS PlantName,e2.Code,e2.UserName AS Text  FROM [SEC].[UserEntity] E
                        LEFT JOIN org.Entity AS e2 ON e2.Id=e.EntityId
                        LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E2.Id
                        LEFT JOIN org.Plant AS p ON p.Id=e.PlantId
                        LEFT JOIN org.Company AS c ON c.Id=e.CompanyId
                        WHERE E.UserId='" + identity.UserId + @"' AND e2.Id IN (
                        SELECT ept.EntityId FROM hkp.EntityProcessTag AS ept WHERE ept.ProcessId IN (SELECT pt.BaseProcessId FROM PlanningTypes AS pt)) AND E2.[Active]=1 ORDER BY E2.Code";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetSavedUnApprovedData()
        {
            try
            {
                string sql = @"SELECT M.*,E.EmployeeName ByWhom,EN.UserName Entity,MS.UserName MaterialStorage FROM [dbo].[MaterialIssueControlMaster] M
                            LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=M.ByWhomId
                            LEFT JOIN ORG.Entity EN ON EN.Id=M.EntityId
                            LEFT JOIN HKP.MaterialStorage MS ON MS.Id=M.MaterialStorageId";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetApprovedData(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false)
                    strkey = column + " like '%" + value + "%'";

                string sql = @"SELECT * FROM (SELECT DT.IssueId,M.*,E.EmployeeName ByWhom,E.EmployeeCode ByWhomEmployeeCode,EN.UserName Entity,MS.UserName MaterialStorage,ISNULL(D.TotalReqQty,0)TotalReqQty,ISNULL(ISU.IssuedQty,0)IssuedQty,Balance=D.TotalReqQty-ISNULL(ISU.IssuedQty,0),ISNULL(DT.AuthorizedByStatus,'')ApproveStatus,PS.UserName ProductionStatus
FROM [dbo].[MaterialIssueControlMaster] M
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=M.ByWhomId
LEFT JOIN ORG.Entity EN ON EN.Id=M.EntityId
LEFT JOIN TRN.ProductionOrder PO ON PO.Id=M.POId
LEFT JOIN HKP.ProductionStatus PS on PS.Id=PO.ProductionStatusId
LEFT JOIN HKP.MaterialStorage MS ON MS.Id=M.MaterialStorageId
LEFT JOIN(SELECT distinct M.Id IssueId,D.MaterialIssueControlMasterId,M.AuthorizedByStatus FROM TRN.IssueRequest IR
LEFT JOIN [dbo].[MaterialIssueControlDetail] D ON D.Id=IR.MaterialIssueControlDetailId
LEFT JOIN TRN.IssueRequestMaster M ON M.Id=IR.IssueRequestMasterId
) DT ON DT.MaterialIssueControlMasterId=M.Id
LEFT JOIN(Select SUM(TotalConsumption) TotalReqQty,MaterialIssueControlMasterId from [dbo].[MaterialIssueControlDetail] Group By MaterialIssueControlMasterId) D ON D.MaterialIssueControlMasterId=M.Id
LEFT JOIN(
SELECT isnull(sum(c.Qty), 0) IssuedQty,IR.IssueRequestMasterId
                                	FROM trn.InventoryIssue a
                                	LEFT JOIN trn.InventoryIssueDetail b ON b.InventoryIssueId = a.id
                                	LEFT JOIN trn.InventoryIssueHistory c ON c.InventoryIssueDetailId = b.Id
                                	LEFT JOIN trn.IssueRequest IR ON IR.Id = c.IssueRequestDetailId				
                                	GROUP BY IR.IssueRequestMasterId
) ISU ON ISU.IssueRequestMasterId=DT.IssueId
) AS TEMP WHERE " + strkey + " Order by AddedDate Desc";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSavedSODetailData(string masterId)
        {
            try
            {
                string sql = @"SELECT DISTINCT mo.MasterOrderNo,MIS.LineItemId,MIS.Id,ISNULL(so.Id,'') SOId,SO.CustomerPOId,CPO.PONumber
	                                ,mm.Id MaterialMasterId,mm.UserName MaterialMaster,mma.Id ArticleId,ISNULL(mma.StandardName, '') SOArticle
									,b.Id CustomerId,b.UserName Customer,mo.TotalQty MOQty,ISNULL(u.UserName, '') UOM,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP],ISNULL(mma.Id, '') ArticleId,mmc.CharCount,ISNULL(POD.ProductionOrderId, '') POId
                                    ,CBI.CostingBOQMasterId,moi.OrderCostingMasterTemplateId,B.UserName Buyer,PM.UserName AS ProductMasterName
									,PC.UserName AS ProductCategory,CEILING(SO.PlannedQty) PlannedQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
									,DMC.[Value] ItemMaterialCost,SDMC.[SOValue] SOMaterialCost,CMC.TotalGrossAmount CostingMaterialCost
									,ISNULL(QBOQ.BOQMaterialCost,0) BOQMaterialCost,SOTotalMaterailCost=CEILING(SO.PlannedQty)*SDMC.[SOValue]
									,CostingTotalMaterialCost=CMC.TotalGrossAmount*CEILING(SO.PlannedQty),BOQTotalCost=ISNULL(QBOQ.BOQMaterialCost,0)*CEILING(SO.PlannedQty)
									,TotalVarianceCostingVsSO=(CEILING(SO.PlannedQty)*SDMC.[SOValue])-(CMC.TotalGrossAmount*CEILING(SO.PlannedQty))
									,TotalVarianceCostingVsBOQ=ISNULL(QBOQ.BOQMaterialCost,0)*CEILING(SO.PlannedQty)-CMC.TotalGrossAmount*CEILING(SO.PlannedQty)
                                    ,MIS.PlanRate,MIS.PlantCost,MIS.TotalSOCostVsTotalPlanCost
                                FROM [dbo].[MaterialIssueControlSODetail] MIS 
								 LEFT JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=MIS.SOId
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN [dbo].[CostingBOQItems] CBI ON CBI.SalesOrderId=SO.Id
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
								left join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
								LEFT JOIN(SELECT distinct MI.Id,MC.UserName,MC.[Value] 
														FROM  dbo.MasterOrderItemCostingRate MC 
														LEFT JOIN dbo.OrderLineCostingItem OLC ON OLC.Id=MC.OrderLineCostingItemId
														LEFT JOIN TRN.MasterOrderItem MI ON MI.Id=MC.MasterOrderItemId
														WHERE OLC.SOItemName='DirectMaterialCost') DMC ON DMC.Id=MOI.Id
								LEFT JOIN(SELECT distinct MC.SalesOrderId,MC.UserName,MC.[SOValue] 
														FROM  dbo.SOCostingConfirmation MC 
														LEFT JOIN dbo.OrderLineCostingItem OLC ON OLC.Id=MC.OrderLineCostingItemId
														WHERE OLC.SOItemName='DirectMaterialCost') SDMC ON SDMC.SalesOrderId=so.Id
LEFT JOIN(SELECT pc.OrderCostingMasterTemplateId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM OrderProcurementCostingDirectMaterial AS pc  
INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial'
GROUP BY PC.OrderCostingMasterTemplateId) CMC ON CMC.OrderCostingMasterTemplateId=MOI.OrderCostingMasterTemplateId

LEFT JOIN (SELECT SUM((Q.MaterialCostPerUnit*Q.GrossConsumption))BOQMaterialCost,Q.MasterOrderItemId FROM [dbo].[QuickBOQ] Q
INNER JOIN HKP.CostingItem I on i.Id=Q.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial' GROUP BY Q.MasterOrderItemId) QBOQ ON QBOQ.MasterOrderItemId=moi.Id

                                WHERE MIS.MaterialIssueControlMasterId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetIssueRequestList(string masterId)
        {
            try
            {
                string sql = @"SELECT * FROM TRN.IssueRequest WHERE MaterialIssueControlDetailId IN(SELECT Id FROM MaterialIssueControlDetail Where MaterialIssueControlMasterId IN(SELECT Id from MaterialIssueControlMaster Where Id IN('" + masterId + "')))";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetIssueRequestBOQMapList(string masterId)
        {
            try
            {
                string sql = @"SELECT * FROM TRN.IssueRequestBOQMap WHERE IssueRequestDetailId IN(SELECT ID FROM TRN.IssueRequest WHERE MaterialIssueControlDetailId IN(SELECT ID FROM MaterialIssueControlDetail WHERE MaterialIssueControlMasterId IN(SELECT ID FROM MaterialIssueControlMaster WHERE Id IN('" + masterId + "'))))";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> GetSavedDetailDataToApprove(string masterId)
        {
            try
            {
                string sql = @"SELECT ROW_NUMBER() OVER(ORDER BY D.Id) SrNo,D.*,D.CostingItemId,I.UserName Item,A.StandardName QBOQArticle,A.Id ArticleId,M.Id MaterialMasterId
,M.UserName MaterialMaster,um.Code as UoM, um.Id as UoMId from dbo.MaterialIssueControlDetail D 
INNER JOIN HKP.CostingItem I on i.Id=D.CostingItemId
left join [SCS].[UnitOfMeasurement] um on um.Id = i.UnitOfMeasurementId
LEFT JOIN MST.MaterialMaster M ON M.Id=D.MaterialMasterId
LEFT JOIN MST.MaterialMasterArticle A ON A.Id=D.ArticleId
WHERE D.MaterialIssueControlMasterId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSavedDetailData(string masterId)
        {
            try
            {
                string sql = @"SELECT ROW_NUMBER() OVER(ORDER BY D.Id) SrNo
                ,D.Id MaterialIssueControlDetailId,D.MaterialIssueControlMasterId,D.CostingItemId,D.NetConsumptionPerUnit,D.ValueLoss,D.GrossConsumption,D.TotalConsumption,D.AdditionReduction
				,D.PlanConsumption,D.Rate,D.TotaPlanlAmount,ISNULL(IR.IssueQty,0) IssueQty,D.ArticleId,D.MaterialMasterId,D.StockRate,D.ActualIssueAmount,D.Remarks,D.AddedBy,D.AddedDate,D.AddedFromIP
				,I.UserName Item,A.StandardName QBOQArticle,A.Id ArticleId,M.Id MaterialMasterId
                ,M.UserName MaterialMaster,um.Code as UoM, um.Id as UoMId, BaseUoMFactor=case when M.BaseUOMId=i.UnitOfMeasurementId then 1 else 1 end
                ,B.UserName BudgetName,ACT.UserName ActivityName,BM.Id BudgetMasterId,BM.GLGeneralInfoId,ACT.Id ExpenseActivityId,M.MaterialGroupMasterId
                ,'' CostCenterName,''CostCenterId,D.Id,'' RequestedQty
                FROM dbo.MaterialIssueControlDetail D 
                INNER JOIN HKP.CostingItem I on i.Id=D.CostingItemId
                left join [SCS].[UnitOfMeasurement] um on um.Id = i.UnitOfMeasurementId
                LEFT JOIN MST.MaterialMaster M ON M.Id=D.MaterialMasterId
                LEFT JOIN MST.MaterialMasterArticle A ON A.Id=D.ArticleId
                LEFT JOIN HKP.MaterialGroupGL MGGL ON MGGL.Id=M.MaterialGroupMasterId
                LEFT JOIN MST.BudgetMaster BM ON BM.Id=MGGL.InventoryBudgetMasterId
                LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                LEFT JOIN HKP.Activity ACT ON ACT.Id=MGGL.InventoryActivityId
				LEFT JOIN (SELECT MaterialIssueControlDetailId, SUM(ISNULL(RequestedQty,0)) IssueQty,TransactionUoMId 
							FROM TRN.IssueRequest GROUP BY MaterialIssueControlDetailId,TransactionUoMId)IR ON IR.MaterialIssueControlDetailId=D.Id
                WHERE D.MaterialIssueControlMasterId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetList(string entityid, string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";


            string sql = @"select * from (SELECT  PO.Id,s.UserName AS ProductionStatus,so.SONo,so.BuyerRefNo,so.SODesc,SO.SOQuantity SOQty,ISNULL(PO.Qty,0) AS POQuantity
                        ,SO.PlanExFactoryDate ExFactoryDate,SO.DeliveryDate,SO.CommitmentDate
                       ,so.Material, so.Product,so.ProductCategory, so.Buyer, so.OwnRefNo, so.StyleNo, so.OwnStyleNo, So.MasterOrderId,so.Customer,so.article
						,PO.AddedDate
                            FROM [TRN].[ProductionOrder] AS PO                            
                           LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId, sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,Format(Min(so.CommitmentDate),'dd-MMM-yyyy') CommitmentDate,Format(Min(so.PlanExFactoryDate),'dd-MMM-yyyy') PlanExFactoryDate,
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
                            WHERE PO.entityid='" + entityid + @"' AND S.UserName<>'Closed') AS TEMP WHERE " + strkey + " ORDER BY AddedDate Desc";

            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetNotClosedPOList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";


            string sql = @"select * from (SELECT  PO.Id,s.UserName AS ProductionStatus,so.SONo,so.BuyerRefNo,so.SODesc,SO.SOQuantity SOQty,ISNULL(PO.Qty,0) AS POQuantity
                        ,SO.PlanExFactoryDate ExFactoryDate,SO.DeliveryDate,SO.CommitmentDate
                       ,so.Material, so.Product,so.ProductCategory, so.Buyer, so.OwnRefNo, so.StyleNo, so.OwnStyleNo, So.MasterOrderId,so.Customer,so.article
							,PO.AddedDate,PO.EntityId
                            FROM [TRN].[ProductionOrder] AS PO                            
                           LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId, sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,Format(Min(so.CommitmentDate),'dd-MMM-yyyy') CommitmentDate,Format(Min(so.PlanExFactoryDate),'dd-MMM-yyyy') PlanExFactoryDate,
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
                            WHERE  S.UserName<>'Closed') AS TEMP WHERE " + strkey + " ORDER BY AddedDate Desc";

            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetRunningPOList(string entityid, string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";


            string sql = @"select * from (SELECT  PO.Id,s.UserName AS ProductionStatus,so.SONo,so.BuyerRefNo,so.SODesc,SO.SOQuantity SOQty,ISNULL(PO.Qty,0) AS POQuantity
                        ,SO.PlanExFactoryDate ExFactoryDate,SO.DeliveryDate,SO.CommitmentDate
                       ,so.Material, so.Product,so.ProductCategory, so.Buyer, so.OwnRefNo, so.StyleNo, so.OwnStyleNo, So.MasterOrderId,so.Customer,so.article
						,PO.AddedDate,O.TotalOtherQty,T.TotalRequestedQty
                            FROM [TRN].[ProductionOrder] AS PO                            
                           LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId, sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,Format(Min(so.CommitmentDate),'dd-MMM-yyyy') CommitmentDate,Format(Min(so.PlanExFactoryDate),'dd-MMM-yyyy') PlanExFactoryDate,
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
							LEFT JOIN(Select SUM(D.OtherQty)TotalOtherQty,M.POId FROM dbo.InputConfirmationDetail D
LEFT JOIN dbo.InputConfirmationMaster M ON M.Id=D.InputConfirmationMasterId
GROUP BY M.POId)O ON O.POId=PO.Id
LEFT JOIN(
Select SUM(D.RequestedQty) TotalRequestedQty,M.ProductionOrderId FROM TRN.IssueRequest D
LEFT JOIN TRN.IssueRequestMaster M ON M.Id=D.IssueRequestMasterId
GROUP BY M.ProductionOrderId
) T ON T.ProductionOrderId=PO.Id
                            WHERE PO.entityid='" + entityid + @"' AND S.UserName='Running' AND (T.TotalRequestedQty-ISNULL(O.TotalOtherQty,0)>0)) AS TEMP WHERE " + strkey + " ORDER BY AddedDate Desc";

            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetSOItemList(string entityid, string ProductionOrderId)
        {
            string CmdText = @"SELECT DISTINCT mo.MasterOrderNo,moi.Id LineItemId,'' Id,Flag =Convert(bit, 'False')
	                                ,ISNULL(so.Id,'') SOId,SO.CustomerPOId,CPO.PONumber,mm.Id MaterialMasterId,mm.UserName MaterialMaster,mma.Id ArticleId
	                                ,ISNULL(mma.StandardName, '') SOArticle,b.Id CustomerId,b.UserName Customer,mo.TotalQty MOQty,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP],moi.OrderWastagePercentage [WastageP],ISNULL(mma.Id, '') ArticleId,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId,CBI.CostingBOQMasterId,moi.OrderCostingMasterTemplateId,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName,PC.UserName AS ProductCategory,CEILING(SO.PlannedQty) PlannedQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
									,(DMC.[Value]*R.ExchangeRate) ItemMaterialCost,(SDMC.[SOValue]*R.ExchangeRate) SOMaterialCost,CMC.TotalGrossAmount CostingMaterialCost
									,ISNULL(QBOQ.BOQMaterialCost,0) BOQMaterialCost,SOTotalMaterailCost=CEILING(SO.PlannedQty)*SDMC.[SOValue]
									,CostingTotalMaterialCost=CMC.TotalGrossAmount*CEILING(SO.PlannedQty),BOQTotalCost=ISNULL(QBOQ.BOQMaterialCost,0)*CEILING(SO.PlannedQty)
									,TotalVarianceCostingVsSO=(CEILING(SO.PlannedQty)*SDMC.[SOValue])-(CMC.TotalGrossAmount*CEILING(SO.PlannedQty))
									,TotalVarianceCostingVsBOQ=ISNULL(QBOQ.BOQMaterialCost,0)*CEILING(SO.PlannedQty)-CMC.TotalGrossAmount*CEILING(SO.PlannedQty)
                                    ,0 PlanRate,0 PlantCost,0 TotalSOCostVsTotalPlanCost
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN [dbo].[MasterOrderExchangeRates] R ON R.TransactionId=mo.Id
                                LEFT JOIN [dbo].[CostingBOQItems] CBI ON CBI.SalesOrderId=SO.Id
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
								left join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
								LEFT JOIN(SELECT distinct MI.Id,MC.UserName,MC.[Value] 
														FROM  dbo.MasterOrderItemCostingRate MC 
														LEFT JOIN dbo.OrderLineCostingItem OLC ON OLC.Id=MC.OrderLineCostingItemId
														LEFT JOIN TRN.MasterOrderItem MI ON MI.Id=MC.MasterOrderItemId
														WHERE OLC.SOItemName='DirectMaterialCost') DMC ON DMC.Id=MOI.Id
								LEFT JOIN(SELECT distinct MC.SalesOrderId,MC.UserName,MC.[SOValue] 
														FROM  dbo.SOCostingConfirmation MC 
														LEFT JOIN dbo.OrderLineCostingItem OLC ON OLC.Id=MC.OrderLineCostingItemId
														WHERE OLC.SOItemName='DirectMaterialCost') SDMC ON SDMC.SalesOrderId=so.Id
LEFT JOIN(SELECT pc.OrderCostingMasterTemplateId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM OrderProcurementCostingDirectMaterial AS pc  
INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial'
GROUP BY PC.OrderCostingMasterTemplateId) CMC ON CMC.OrderCostingMasterTemplateId=MOI.OrderCostingMasterTemplateId

LEFT JOIN (SELECT SUM((Q.MaterialCostPerUnit*Q.GrossConsumption))BOQMaterialCost,Q.MasterOrderItemId FROM [dbo].[QuickBOQ] Q
INNER JOIN HKP.CostingItem I on i.Id=Q.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial' GROUP BY Q.MasterOrderItemId) QBOQ ON QBOQ.MasterOrderItemId=moi.Id
 
   WHERE PO.EntityId = '" + entityid + @"' AND PS.UserName<>'Closed' AND PO.Id='" + ProductionOrderId + "'";


            return _sqlRepository.GetDataCollection(CmdText, null);
        }


        public IEnumerable<object> GetInputConfirmationSOItemList(string entityid, string ProductionOrderId,string masterId)
        {
            string CmdText = @"SELECT DISTINCT mo.MasterOrderNo,moi.Id LineItemId,ICSO.Id,Flag =Convert(bit, 'False')
	                                ,ISNULL(so.Id,'') SOId,SO.CustomerPOId,CPO.PONumber,mm.Id MaterialMasterId,mm.UserName MaterialMaster,mma.Id ArticleId
	                                ,ISNULL(mma.StandardName, '') SOArticle,b.Id CustomerId,b.UserName Customer,mo.TotalQty MOQty,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP],moi.OrderWastagePercentage [WastageP],ISNULL(mma.Id, '') ArticleId,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId,CBI.CostingBOQMasterId,moi.OrderCostingMasterTemplateId,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName,PC.UserName AS ProductCategory,CEILING(SO.PlannedQty) PlannedQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
									,(DMC.[Value]*R.ExchangeRate) ItemMaterialCost,(SDMC.[SOValue]*R.ExchangeRate) SOMaterialCost,CMC.TotalGrossAmount CostingMaterialCost
									,ISNULL(QBOQ.BOQMaterialCost,0) BOQMaterialCost,SOTotalMaterailCost=CEILING(SO.PlannedQty)*SDMC.[SOValue]
									,CostingTotalMaterialCost=CMC.TotalGrossAmount*CEILING(SO.PlannedQty),BOQTotalCost=ISNULL(QBOQ.BOQMaterialCost,0)*CEILING(SO.PlannedQty)
									,TotalVarianceCostingVsSO=(CEILING(SO.PlannedQty)*SDMC.[SOValue])-(CMC.TotalGrossAmount*CEILING(SO.PlannedQty))
									,TotalVarianceCostingVsBOQ=ISNULL(QBOQ.BOQMaterialCost,0)*CEILING(SO.PlannedQty)-CMC.TotalGrossAmount*CEILING(SO.PlannedQty)
                                    ,0 PlanRate,0 PlantCost,0 TotalSOCostVsTotalPlanCost
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN [dbo].[MasterOrderExchangeRates] R ON R.TransactionId=mo.Id
                                LEFT JOIN [dbo].[CostingBOQItems] CBI ON CBI.SalesOrderId=SO.Id
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
								left join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
								LEFT JOIN(SELECT distinct MI.Id,MC.UserName,MC.[Value] 
														FROM  dbo.MasterOrderItemCostingRate MC 
														LEFT JOIN dbo.OrderLineCostingItem OLC ON OLC.Id=MC.OrderLineCostingItemId
														LEFT JOIN TRN.MasterOrderItem MI ON MI.Id=MC.MasterOrderItemId
														WHERE OLC.SOItemName='DirectMaterialCost') DMC ON DMC.Id=MOI.Id
								LEFT JOIN(SELECT distinct MC.SalesOrderId,MC.UserName,MC.[SOValue] 
														FROM  dbo.SOCostingConfirmation MC 
														LEFT JOIN dbo.OrderLineCostingItem OLC ON OLC.Id=MC.OrderLineCostingItemId
														WHERE OLC.SOItemName='DirectMaterialCost') SDMC ON SDMC.SalesOrderId=so.Id
LEFT JOIN(SELECT pc.OrderCostingMasterTemplateId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM OrderProcurementCostingDirectMaterial AS pc  
INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial'
GROUP BY PC.OrderCostingMasterTemplateId) CMC ON CMC.OrderCostingMasterTemplateId=MOI.OrderCostingMasterTemplateId

LEFT JOIN (SELECT SUM((Q.MaterialCostPerUnit*Q.GrossConsumption))BOQMaterialCost,Q.MasterOrderItemId FROM [dbo].[QuickBOQ] Q
INNER JOIN HKP.CostingItem I on i.Id=Q.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial' GROUP BY Q.MasterOrderItemId) QBOQ ON QBOQ.MasterOrderItemId=moi.Id
LEFT JOIN dbo.InputConfirmationSODetail ICSO ON ICSO.SOId=so.Id AND ISNULL(ICSO.InputConfirmationMasterId,'')='"+ masterId + @"'
   WHERE PO.EntityId = '" + entityid + @"' AND PS.UserName<>'Closed' AND PO.Id='" + ProductionOrderId + "'";


            return _sqlRepository.GetDataCollection(CmdText, null);
        }


        public IEnumerable<object> GetMOIItemList(string entityid, string ProductionOrderId)
        {
            string CmdText = @"SELECT DISTINCT mo.MasterOrderNo,so.MasterOrderItemId
	                                ,ISNULL(so.Id,'') SOId
									,moi.OrderCostingMasterTemplateId
									,CBI.CostingBOQMasterId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN [dbo].[CostingBOQItems] CBI ON CBI.SalesOrderId=SO.Id
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PO.EntityId = '" + entityid + @"'	AND PS.UserName = 'Running' AND PO.Id='" + ProductionOrderId + "'";
            return _sqlRepository.GetDataCollection(CmdText, null);
        }

        public IEnumerable<object> GetCostingDataList(string LineItemId)
        {
            string CmdText = @"SELECT ROW_NUMBER() OVER(ORDER BY Q.Sequence) SrNo,NULL Id,I.Id CostingItemId,I.UserName Item,um.Code as UoM, um.Id as UoMId,Q.Consumption NetConsumptionPerUnit,Q.ValueLoss,Q.GrossConsumption,
 TotalConsumption=Q.GrossConsumption*SO.Qty,SO.Qty, 0 AdditionReduction,0 PlanConsumption
 ,Q.Rate,0 TotaPlanlAmount,A.StandardName QBOQArticle,A.Id ArticleId,M.Id MaterialMasterId,M.UserName MaterialMaster,ISNULL(SR.StockRate,0)StockRate,0 ActualIssueAmount, NULL Remarks,IM.Id InventoryMaterialId
 FROM OrderProcurementCostingDirectMaterial AS Q  
INNER JOIN HKP.CostingItem I on i.Id=Q.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial'
left join [SCS].[UnitOfMeasurement] um on um.Id = i.UnitOfMeasurementId
left join TRN.MasterOrderItem MOI ON MOI.OrderCostingMasterTemplateId=Q.OrderCostingMasterTemplateId
left join TRN.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id
left join MST.MaterialMasterArticle A ON A.Id=MOI.ArticleId
left join MST.MaterialMaster M ON M.Id=MOI.MaterialMasterId
LEFT JOIN [TRN].[InventoryMaterial] IM ON IM.MaterialMasterId=Q.MaterialMasterId AND IM.ArticleId=Q.ArticleId
LEFT JOIN (
Select StockRate= SUM(IRD.MaterialTranRate)/COUNT(IRD.Id),IM.MaterialMasterId,IM.ArticleId 
from TRN.InventoryReceiveDetail IRD 
JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
 AND (IRD.BaseQty-IRD.BaseIssueQty)>0
GROUP BY IM.MaterialMasterId,IM.ArticleId )SR ON SR.MaterialMasterId=M.Id AND SR.ArticleId=A.Id
Where MOI.Id " + LineItemId + "";
            return _sqlRepository.GetDataCollection(CmdText, null);
        }

        public IEnumerable<object> GetQBOQDataList(string LineItemId, string soId)
        {
            string CmdText = @"SELECT ROW_NUMBER() OVER(ORDER BY Q.Sequence) SrNo,NULL Id,I.Id CostingItemId,I.UserName Item,U.Code UoM,Q.UoMId,Q.NetConsumptionPerUnit,Q.ValueLossPercentage ValueLoss,Q.GrossConsumption
,Qty=(select SUM(Qty) from TRN.SalesOrder Where Id " + soId + @")
,TotalConsumption=Q.GrossConsumption*(select SUM(Qty) from TRN.SalesOrder Where Id " + soId + @"),
0 AdditionReduction,(Q.GrossConsumption*(select SUM(Qty) from TRN.SalesOrder Where Id " + soId + @")) PlanConsumption
,Q.MaterialCostPerUnit Rate,((Q.GrossConsumption*(select SUM(Qty) from TRN.SalesOrder Where Id " + soId + @"))*Q.MaterialCostPerUnit) TotaPlanlAmount
,A.StandardName QBOQArticle,A.Id ArticleId,M.Id MaterialMasterId
,M.UserName MaterialMaster,ISNULL(SR.StockRate,0)StockRate,0 ActualIssueAmount, NULL Remarks,IM.Id InventoryMaterialId,Q.Id BOMId
FROM [dbo].[QuickBOQ] Q
INNER JOIN HKP.CostingItem I on i.Id=Q.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial'
left join SCS.UnitOfMeasurement U on U.Id=Q.UoMId
left join MST.MaterialMasterArticle A ON A.Id=Q.ArticleId
left join MST.MaterialMaster M ON M.Id=Q.MaterialMasterId
LEFT JOIN [TRN].[InventoryMaterial] IM ON IM.MaterialMasterId=Q.MaterialMasterId AND IM.ArticleId=Q.ArticleId
LEFT JOIN (
Select StockRate= SUM(IRD.MaterialTranRate)/COUNT(IRD.Id),IM.MaterialMasterId,IM.ArticleId 
from TRN.InventoryReceiveDetail IRD 
JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
 AND (IRD.BaseQty-IRD.BaseIssueQty)>0
GROUP BY IM.MaterialMasterId,IM.ArticleId) SR ON SR.MaterialMasterId=M.Id AND SR.ArticleId=A.Id
Where Q.MasterOrderItemId " + LineItemId + "";
            return _sqlRepository.GetDataCollection(CmdText, null);
        }

        public DataTable loadIssueRequestMaster(string issueId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT 
                             EmIU.EmployeeName ReceivedBy
						,IsR.Id IssueId
						,REPLACE(CONVERT(VARCHAR(11),IsR.AddedDate, 113), ' ', '-') IssueDate
						,CheckedByName=CASE WHEN IsR.CheckedByStatus='Checked' Then eI.EmployeeName else '' END 
						,AuthorizedByName=CASE When IsR.AuthorizedByStatus='Approved'then eI1.EmployeeName else '' END
						--,AddedBy=CASE When IsR.CheckedByStatus='ForChecked' OR IsR.CheckedByStatus='Hold' OR IsR.CheckedByStatus='Reject' OR IsR.CheckedByStatus='Checked'then eI3.EmployeeName else ''  END 
						,PurOrCheckedStatus= CASE when IsR.CheckedByStatus='ForChecked' Then 'To be checked'
						when IsR.CheckedByStatus='Hold' Then 'Hold'
						when IsR.CheckedByStatus='Reject' Then 'Reject'
						when IsR.CheckedByStatus='Checked' Then 'Checked'
						else ''
						END
						,PurOrApprovedStatus= CASE 
						when IsR.AuthorizedByStatus='Reject' Then 'Reject For Approved'
						when IsR.AuthorizedByStatus='Hold' Then 'Hold For Approved'
						when IsR.AuthorizedByStatus='For Approval' Then 'To be Approval'
						when IsR.AuthorizedByStatus='Approved' Then 'Approved'
						else ''
						END
						,p.UserName ProcessName,po.SalesOrderId,FGColor=isnull(po.FGColor1,'')+','+isnull(po.FGColor2,'')+','+isnull(po.FGColor3,'')
						,'PONo: ' + ISNULL(IsR.ProductionOrderId,po.ProductionOrder) ProductionOrder,PreparedBy.EmployeeName AddedBy
						FROM TRN.IssueRequestMaster As IsR
						                LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IsR.CheckedBy
                                         LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IsR.AuthorizedBy
							             LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=IsR.AddedBy
										 Left Join EmployeeInformation EmIU on EmIU.SystemId=IsR.UpdatedBy
										  Left Join EmployeeInformation PreparedBy on PreparedBy.SystemId=IsR.Preparedby
										 left join trn.IssueRequestMasterProcessMap IRPmap On IRPmap.IssueRequestMasterId=IsR.Id
										 left join hkp.Process p On p.Id=IRPmap.ProcessId
										 LEFT JOIN(
							SELECT distinct PDAMAP.IssueRequestMasterId
								,SalesOrderId=STUFF((select distinct ','+xPDAMAP.SalesOrderId from
								trn.IssueRequestMaster xpo
								INNER JOin trn.IssueRequestMasterSalesOrderMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,FGColor1=STUFF((select distinct ','+FCV.UserName from
								trn.IssueRequestMaster xpo
								INNER JOin trn.IssueRequestSKUMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
								LEFT JOIN HKP.CharacteristicsValue AS FCV ON xPDAMAP.FirstCharacteristicsValueId=FCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS SCV ON xPDAMAP.SecondCharacteristicsValueId=SCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS TCV ON xPDAMAP.ThirdCharacteristicsValueId=TCV.Id
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

							   ,FGColor2=STUFF((select distinct ','+SCV.UserName from
								trn.IssueRequestMaster xpo
								INNER JOin trn.IssueRequestSKUMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
								LEFT JOIN HKP.CharacteristicsValue AS FCV ON xPDAMAP.FirstCharacteristicsValueId=FCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS SCV ON xPDAMAP.SecondCharacteristicsValueId=SCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS TCV ON xPDAMAP.ThirdCharacteristicsValueId=TCV.Id
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,FGColor3=STUFF((select distinct ','+TCV.UserName from
								trn.IssueRequestMaster xpo
								INNER JOin trn.IssueRequestSKUMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
								LEFT JOIN HKP.CharacteristicsValue AS FCV ON xPDAMAP.FirstCharacteristicsValueId=FCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS SCV ON xPDAMAP.SecondCharacteristicsValueId=SCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS TCV ON xPDAMAP.ThirdCharacteristicsValueId=TCV.Id
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ProductionOrder=STUFF((select distinct ','+PrOrderDetail.ProductionOrderId from
								trn.IssueRequestMaster xpo
								INNER JOin trn.IssueRequestMasterSalesOrderMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
								left join trn.ProductionOrderDetail PrOrderDetail ON PrOrderDetail.SalesOrderId=xPDAMAP.SalesOrderId
								
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								from  trn.IssueRequestMasterSalesOrderMap PDAMAP 							 
							  group by  PDAMAP.IssueRequestMasterId
							)PO ON PO.IssueRequestMasterId = IsR.Id
                      WHERE IsR.Id ='" + issueId + "'";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public DataTable loadIssueRequestDetail(string issueId)
        {
            string strSQL1;
            try
            {
                strSQL1 = @"DECLARE @Own VARCHAR(10)='Own',@Other VARCHAR(10)='Other';
                            Select 
                            ROW_NUMBER() OVER(ORDER BY IR.Id ASC) AS SiNo
                            ,IR.Id
                            ,CC.UserName AS CostCenterName
                            ,B.UserName ActivityName 
                            ,mm.UserName MaterialMasterName
                            ,ART.StandardName
                            ,ART.Code
                            ,IR.RequisitionId
                            ,IR.RequisitionDetailId
                            ,TUoM.UserName AS UOM
                            ,IR.RequestedQty
                            ,IR.RejectedQty
                            ,IR.RequestedQty+IR.RejectedQty AS Total

                            ,En.Username As EntityName
                            ,MRM.EntityId
                            --,Bu.Code
                            --,Bu.UserName
                            ,Us.FullName AddedBy
                            ,MRM.Id RequisitionNo
                            ,MRD.ArticleId
                            ,Dp.UserName DepartmentName
                            ,MGM.UserName MaterialMasterGroupName

                            ,MT.UserName MaterialType
                            ,MRD.FirstCharacteristicsId
                            ,FC.UserName AS FirstCharacteristics
                            ,MRD.FirstCharacteristicsValueId
                            ,FCV.UserName AS FirstCharacteristicsValue
                            ,MRD.SecondCharacteristicsId
                            ,SC.UserName AS SecondCharacteristics
                            ,MRD.SecondCharacteristicsValueId
                            ,SCV.UserName AS SecondCharacteristicsValue
                            ,MRD.ThirdCharacteristicsId
                            ,TC.UserName AS ThirdCharacteristics
                            ,MRD.ThirdCharacteristicsValueId
                            ,TCV.UserName AS ThirdCharacteristicsValue

                            ,IR.CostCenterId
                            ,IR.ExpenseActivityId
                            ,IR.BudgetMasterId
                            ,IR.GLGeneralInfoId 
                            ,OwnOtherQty=CASE
								WHEN IR.CostCenterId=IR.ExpenseActivityId THEN @Own						
								ELSE @Other
							
							END
,isnull(IGL1.UserName,'') AS CGL									
							,isnull(B1.UserName,'') AS CBUdget
							,isnull(IA1.UserName,'') AS GLBudgetActivity
                            --,IR.AddedBy
							--,CC.UserName AS CostCenterName
                            from trn.IssueRequest IR
                            left Join [TRN].[MaterialRequsitionDetails] As MRD on MRD.Id=IR.REquisitionDetailId
                            Left Join [TRN].[MaterialRequsitionMaster] As MRM On MRD.MaterialReqqusitionMasterId=MRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId

                            Left Join [ORG].[Entity] As En On MRM.EntityId=En.Id
                            Left Join [HKP].[Budget] As Bu On Bu.Id=MRD.ActivityId
                            Left JOIN MST.MaterialMaster AS MM ON IR.MaterialMasterId = MM.Id
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON IR.ArticleId = ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON IR.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IR.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IR.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IR.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IR.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IR.ThirdCharacteristicsValueId = TCV.Id
                             LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IR.TransactionUoMId = TUoM.Id
                            LEFT JOIN [SEC].[User] As Us On MRM.AddedBy=Us.UserId
                            LEFT JOIN dbo.EmployeeInformation As Em On Us.EmployeeId=Em.SystemId
                            LEFT JOIN [ORG].[Department] AS Dp On Dp.Id=Em.DepartmentId
                            LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                            LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IR.GLGeneralInfoId 
									LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IR.BudgetMasterId
									LEFT JOIN HKP.Activity IA1 ON IA1.Id=IR.ExpenseActivityId
									Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId

                      where IR.IssueRequestMasterId='" + issueId + "'";

                return _sqlRepository.GetDataTable(strSQL1);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public IEnumerable<object> GetTransactionData(Dictionary<string, string> parameters)
        {
            var str = @"SELECT D.Id MaterialIssueControlDetailId,D.MaterialIssueControlMasterId,MIC.POId,POD.SalesOrderId,D.CostingItemId,D.NetConsumptionPerUnit,D.ValueLoss,D.GrossConsumption,D.TotalConsumption,D.AdditionReduction,PS.UserName POStatus
                ,D.PlanConsumption,D.Rate,D.TotaPlanlAmount,ISNULL(IR.RequestedQty,0) RequestedQty,ISNULL(IR.IssueQty,0) IssueQty,Balance=ISNULL(IR.RequestedQty,0) -ISNULL(IR.IssueQty,0),D.PlanConsumption-ISNULL(IR.RequestedQty,0) StockQty,D.ArticleId,D.MaterialMasterId,D.StockRate,D.ActualIssueAmount,D.Remarks,D.AddedBy,D.AddedDate,D.AddedFromIP
                ,I.UserName Item,A.StandardName QBOQArticle,A.Id ArticleId,M.Id MaterialMasterId
                ,M.UserName MaterialMaster,um.Code as UoM, um.Id as UoMId, BaseUoMFactor=case when M.BaseUOMId=i.UnitOfMeasurementId then 1 else 1 end
                ,B.UserName BudgetName,ACT.UserName ActivityName,BM.Id BudgetMasterId,BM.GLGeneralInfoId,ACT.Id ExpenseActivityId,M.MaterialGroupMasterId
				,MM.UserName ItemMaterial,MMA.StandardName ItemArticle
                FROM dbo.MaterialIssueControlDetail D 
                INNER JOIN HKP.CostingItem I on i.Id=D.CostingItemId
                left join [SCS].[UnitOfMeasurement] um on um.Id = i.UnitOfMeasurementId
                LEFT JOIN MST.MaterialMaster M ON M.Id=D.MaterialMasterId
                LEFT JOIN MST.MaterialMasterArticle A ON A.Id=D.ArticleId
                LEFT JOIN HKP.MaterialGroupGL MGGL ON MGGL.Id=M.MaterialGroupMasterId
                LEFT JOIN MST.BudgetMaster BM ON BM.Id=MGGL.InventoryBudgetMasterId
                LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                LEFT JOIN HKP.Activity ACT ON ACT.Id=MGGL.InventoryActivityId
                LEFT JOIN (SELECT IR.MaterialIssueControlDetailId, SUM(ISNULL(IR.RequestedQty,0)) RequestedQty,SUM(ISNULL(I.Qty,0))IssueQty,IR.TransactionUoMId 
                            FROM TRN.IssueRequest IR
				LEFT JOIN  TRN.InventoryIssueHistory I ON I.IssueRequestDetailId=IR.Id
						Where I.IssueRequestDetailId<>'' AND IR.MaterialIssueControlDetailId<>''
						GROUP BY IR.MaterialIssueControlDetailId,IR.TransactionUoMId 
						)IR ON IR.MaterialIssueControlDetailId=D.Id
				LEFT JOIN MaterialIssueControlMaster MIC ON MIC.id=D.MaterialIssueControlMasterId
				JOIN TRN.ProductionOrderDetail POD ON POD.ProductionOrderId=MIC.POId
				JOIN TRN.ProductionOrder PO ON MIC.POId=PO.Id
				JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
				LEFT JOIN TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
				LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
				LEFT JOIN MST.MaterialMaster MM ON MM.Id=MOI.MaterialMasterId
				LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=MOI.ArticleId
				--Where POD.ProductionOrderId IN() AND PS.UserName IN ()
                Where POD.ProductionOrderId IN(" + parameters["PONo"] + @")  AND PS.UserName IN (" + parameters["POStatus"] + @")";
            return _sqlRepository.GetDataCollection(str);
        }

        public void GetTransactionReportSQL(string POId, string POStatus, out DataTable data)
        {
            try
            {
                string strSQL = @"SELECT D.Id MaterialIssueControlDetailId,D.MaterialIssueControlMasterId,MIC.POId,POD.SalesOrderId,D.CostingItemId,D.NetConsumptionPerUnit,D.ValueLoss,D.GrossConsumption,D.TotalConsumption,D.AdditionReduction
                ,D.PlanConsumption,D.Rate,D.TotaPlanlAmount,ISNULL(IR.RequestedQty,0) RequestedQty,ISNULL(IR.IssueQty,0) IssueQty,Balance=ISNULL(IR.RequestedQty,0) -ISNULL(IR.IssueQty,0),D.PlanConsumption-ISNULL(IR.RequestedQty,0) StockQty,D.ArticleId,D.MaterialMasterId,D.StockRate,D.ActualIssueAmount,D.Remarks,D.AddedBy,D.AddedDate,D.AddedFromIP
                ,I.UserName Item,A.StandardName QBOQArticle
                ,M.UserName MaterialMaster,um.Code as UoM, um.Id as UoMId, BaseUoMFactor=case when M.BaseUOMId=i.UnitOfMeasurementId then 1 else 1 end
                ,B.UserName BudgetName,ACT.UserName ActivityName,BM.Id BudgetMasterId,BM.GLGeneralInfoId,ACT.Id ExpenseActivityId,M.MaterialGroupMasterId
				,MM.UserName ItemMaterial,MMA.StandardName ItemArticle,PS.UserName POStatus
                FROM dbo.MaterialIssueControlDetail D 
                INNER JOIN HKP.CostingItem I on i.Id=D.CostingItemId
                left join [SCS].[UnitOfMeasurement] um on um.Id = i.UnitOfMeasurementId
                LEFT JOIN MST.MaterialMaster M ON M.Id=D.MaterialMasterId
                LEFT JOIN MST.MaterialMasterArticle A ON A.Id=D.ArticleId
                LEFT JOIN HKP.MaterialGroupGL MGGL ON MGGL.Id=M.MaterialGroupMasterId
                LEFT JOIN MST.BudgetMaster BM ON BM.Id=MGGL.InventoryBudgetMasterId
                LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                LEFT JOIN HKP.Activity ACT ON ACT.Id=MGGL.InventoryActivityId
                LEFT JOIN (SELECT IR.MaterialIssueControlDetailId, SUM(ISNULL(IR.RequestedQty,0)) RequestedQty,SUM(ISNULL(I.Qty,0))IssueQty,IR.TransactionUoMId 
                            FROM TRN.IssueRequest IR
				LEFT JOIN  TRN.InventoryIssueHistory I ON I.IssueRequestDetailId=IR.Id
						Where I.IssueRequestDetailId<>'' AND IR.MaterialIssueControlDetailId<>''
						GROUP BY IR.MaterialIssueControlDetailId,IR.TransactionUoMId 
						)IR ON IR.MaterialIssueControlDetailId=D.Id
				LEFT JOIN MaterialIssueControlMaster MIC ON MIC.id=D.MaterialIssueControlMasterId
				JOIN TRN.ProductionOrderDetail POD ON POD.ProductionOrderId=MIC.POId
				JOIN TRN.ProductionOrder PO ON MIC.POId=PO.Id
				JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
				LEFT JOIN TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
				LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
				LEFT JOIN MST.MaterialMaster MM ON MM.Id=MOI.MaterialMasterId
				LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=MOI.ArticleId
				Where POD.ProductionOrderId IN (" + POId + @")  AND PS.UserName IN (" + POStatus + @")";

                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }


        #region InputConfirmation

        public IEnumerable<object> GetIssueSlipDataByPOIdList(string ProductionOrderId)
        {
            string CmdText = @"Select ''Id,IR.IssueRequestMasterId IssueSlipId,IR.Id IssueSlipRowId,IR.CostCenterId,CC.UserName AS CostCenter,MM.UserName MaterialMaster
,IR.ArticleId,ART.StandardName Article,TUoM.Id UOMId,TUoM.Code AS UOM,IR.RequestedQty,ISNULL(IRH.ActualIssueQty,0) IssueQty,0 OtherQty,0 WasteQty,TotalQty=IR.RequestedQty,OT.UptodateOtherQty,OT.UptodateWasteQty
FROM TRN.IssueRequest IR 
LEFT JOIN TRN.IssueRequestMaster IRM ON IR.IssueRequestMasterId=IRM.Id
LEFT JOIN [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
Left JOIN MST.MaterialMaster AS MM ON IR.MaterialMasterId = MM.Id
LEFT JOIN MST.MaterialMasterArticle AS ART ON IR.ArticleId = ART.Id
LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IR.TransactionUoMId = TUoM.Id
LEFT JOIN (
SELECT SUM(Qty)ActualIssueQty, H.IssueRequestDetailId
FROM TRN.InventoryIssueHistory H
LEFT JOIN TRN.InventoryIssueDetail ISD ON ISD.Id=H.InventoryIssueDetailId
WHERE H.IssueRequestDetailId<>''
GROUP BY H.IssueRequestDetailId
) IRH ON IRH.IssueRequestDetailId=IR.Id
LEFT JOIN(
SELECT SUM(ID.OtherQty) UptodateOtherQty,SUM(ID.WasteQty)UptodateWasteQty,ID.IssueSlipRowId FROM dbo.InputConfirmationMaster IM
LEFT  JOIN  dbo.InputConfirmationDetail ID ON ID.InputConfirmationMasterId=IM.Id
Group BY ID.IssueSlipRowId
)OT ON OT.IssueSlipRowId=IR.Id
Where IRM.ProductionOrderId='" + ProductionOrderId + "'";
            return _sqlRepository.GetDataCollection(CmdText, null);
        }

        public IEnumerable<object> GetInventoryMaterialData(string plantId, string confirmdate)
        {
            try
            {
                string today = confirmdate;
                string sql = @"SELECT * FROM(
                         SELECT IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                     , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 0 else 1 END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 0 else 1 END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor,IRD.BaseUoMFactor GRNBaseUoMFactor
                        , round(IRD.MaterialTranRate,4) MaterialTranRate,  TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                       
						, round(ISNULL(IRD.TrnCurrencyBaseRate,0),4)+Round(ISNULL((case when ird.AdditionalChargesAmount>0 then ird.AdditionalChargesAmount/ird.BaseQty else 0 end),0),4) BaseRate
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4)+Round(ISNULL((case when ird.AdditionalChargesAmount>0 then ird.AdditionalChargesAmount/ird.BaseQty else 0 end) * ir.ToCurrencyRate,0),4) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(II.IssueQty, 0) StockQty
						, ISNULL(II.IssueQty,0) IssueQty, ISNULL(II.IssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty,ISNULL(IRD.InventoryTransferQty,0) InventoryTransferQty
						 ,((((((ISNULL(IRD.BaseQty,0) - ISNULL(II.IssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						, Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4)+Round(ISNULL((case when ird.AdditionalChargesAmount>0 then ird.AdditionalChargesAmount/ird.BaseQty else 0 end) * ir.ToCurrencyRate,0),4) BooksCurrencyBaseRate
						 ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4)+Round(ISNULL((case when ird.AdditionalChargesAmount>0 then ird.AdditionalChargesAmount/ird.BaseQty else 0 end),0),4) TrnCurrencyBaseRate
                        ,round(ISNULL(II.IssueAmount,0),4) TotalIssueAmount
                        , ISNULL(IRD.AdditionalChargesAmount,0) AdditionalChargesAmount
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,C.Id CountryId,C.UserName CountryName
                        ,0 SalesRate
						,0 TotalAmount
                        ,IM.MaterialMasterId
						,MM.UserName Material
						,MT.UserName MaterialType
						,IM.ArticleId
						,MMA.StandardName Article
						,IM.FirstCharacteristicsValueId
						,IM.SecondCharacteristicsValueId
						,IM.ThirdCharacteristicsValueId
						
						,IM.FirstCharacteristicsId
						,IM.SecondCharacteristicsId
						,IM.ThirdCharacteristicsId,IssueByUoM=CASE WHEN MM.IssueByUoM=0 THEN 'No' ELSE 'Yes' END
                        ,TrasactopmUomQty=(((ISNULL(IRD.BaseQty,0) - ISNULL(II.IssueQty, 0))*BaseUoMFactor)/BaseUoMFactor) 
						
						,'' IssueTransactionUoMId
						,'' IssueTransactionUoM,ird.MaterialStorageId,MS.UserName MaterialStorage,IRD.LotNumber
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					left join mst.MaterialMaster MM ON MM.Id=Im.MaterialMasterId
					left join mst.MaterialMasterArticle MMA ON MMA.Id=IM.ArticleId
                    LEFT JOIN mst.MaterialGroupMaster MGM ON MGM.Id=MM.MaterialGroupMasterId
					LEFT JOIN HKP.MaterialType MT ON MT.Id=MGM.MaterialTypeId
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
					LEFT JOIN (
									    select IID.InventoryMaterialId,IH.InventoryReceiveDetailId, IH.MaterialStorageId
                                        , Sum(ISNULL(IH.Qty,0)) IssueQty , Sum(ISNULL(IH.TotalMaterialBooksCurrencyAmount,0)) IssueAmount,IID.IsAsset
									    FROM TRN.InventoryIssueDetail IID  
									    LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									    LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									    WHERE II.PlantId='" + plantId + @"'   
									    GROUP BY IID.InventoryMaterialId,IID.IsAsset,IH.InventoryReceiveDetailId, IH.MaterialStorageId
									    ) II ON II.InventoryReceiveDetailId=IRD.Id and II.MaterialStorageId=IRD.MaterialStorageId 
                    left JOIN SCS.Country C On C.Id=IM.CountryId
                    LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE  IM.PlantId = '" + plantId + @"' 
                    
					AND IR.[Status]='Posting' AND IR.IsFOC=0
                   
					AND ISNULL(IM.FirstCharacteristicsValueId,'')='' AND  ISNULL(IM.SecondCharacteristicsValueId,'')=''
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='' AND ISNULL(IM.CountryId,'')='' 
                   
					AND IRD.BaseQty !=ISNULL(II.IssueQty,0)
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + today + @"' AS DATE) 

                    UNION ALL

                    SELECT IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                     , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 0 else 1 END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 0 else 1 END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor,IRD.BaseUoMFactor GRNBaseUoMFactor
                        , round(IRD.MaterialTranRate,4) MaterialTranRate,  TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                       
						, IRD.TrnCurrencyBaseRate BaseRate
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(II.IssueQty, 0) StockQty
						, ISNULL(II.IssueQty,0) IssueQty, ISNULL(II.IssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty,ISNULL(IRD.InventoryTransferQty,0) InventoryTransferQty
						 ,((((((ISNULL(IRD.BaseQty,0) - ISNULL(II.IssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BooksCurrencyBaseRate
						 ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4) TrnCurrencyBaseRate
                        ,round(ISNULL(II.IssueAmount,0),4) TotalIssueAmount
                        , ISNULL(IRD.AdditionalChargesAmount,0) AdditionalChargesAmount
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,C.Id CountryId,C.UserName CountryName--,null AS [Flag] 
                        ,0 SalesRate
						,0 TotalAmount
                        ,IM.MaterialMasterId
						,MM.UserName Material
						,MT.UserName MaterialType
						,IM.ArticleId
						,MMA.StandardName Article
						,IM.FirstCharacteristicsValueId
						,IM.SecondCharacteristicsValueId
						,IM.ThirdCharacteristicsValueId
						
						,IM.FirstCharacteristicsId
						,IM.SecondCharacteristicsId
						,IM.ThirdCharacteristicsId,IssueByUoM=CASE WHEN MM.IssueByUoM=0 THEN 'No' ELSE 'Yes' END
                        ,TrasactopmUomQty=(((ISNULL(IRD.BaseQty,0) - ISNULL(II.IssueQty, 0))*BaseUoMFactor)/BaseUoMFactor) 
						
						,'' IssueTransactionUoMId
						,'' IssueTransactionUoM,ird.MaterialStorageId,MS.UserName MaterialStorage,IRD.LotNumber
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					left join mst.MaterialMaster MM ON MM.Id=Im.MaterialMasterId
					left join mst.MaterialMasterArticle MMA ON MMA.Id=IM.ArticleId
                    LEFT JOIN mst.MaterialGroupMaster MGM ON MGM.Id=MM.MaterialGroupMasterId
					LEFT JOIN HKP.MaterialType MT ON MT.Id=MGM.MaterialTypeId
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
					LEFT JOIN (
									    select IID.InventoryMaterialId,IH.InventoryReceiveDetailId, IH.MaterialStorageId
                                        , Sum(ISNULL(IH.Qty,0)) IssueQty , Sum(ISNULL(IH.TotalMaterialBooksCurrencyAmount,0)) IssueAmount,IID.IsAsset
									    FROM TRN.InventoryIssueDetail IID  
									    LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									    LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									    WHERE II.PlantId='" + plantId + @"'   
									    GROUP BY IID.InventoryMaterialId,IID.IsAsset,IH.InventoryReceiveDetailId, IH.MaterialStorageId
									    ) II ON II.InventoryReceiveDetailId=IRD.Id and II.MaterialStorageId=IRD.MaterialStorageId 
                    left JOIN SCS.Country C On C.Id=IM.CountryId
                    LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE  IM.PlantId = '" + plantId + @"' 
                    
					AND IR.IsApproved=1 AND IR.IsFOC=1
                  
					AND ISNULL(IM.FirstCharacteristicsValueId,'')='' AND  ISNULL(IM.SecondCharacteristicsValueId,'')=''
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='' AND ISNULL(IM.CountryId,'')='' 
                   
					AND IRD.BaseQty !=ISNULL(II.IssueQty,0)
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + today + @"' AS DATE) 

					Union ALL
					SELECT IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                     , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 0 else 1 END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 0 else 1 END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor,IRD.BaseUoMFactor GRNBaseUoMFactor
                        , round(IRD.MaterialTranRate,4) MaterialTranRate,  TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                        ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4) BaseRate
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                       ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty,ISNULL(IRD.InventoryTransferQty,0) InventoryTransferQty
						 ,((((((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BooksCurrencyBaseRate
						 ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4) TrnCurrencyBaseRate
                         ,0 TotalIssueAmount
                        , ISNULL(IRD.AdditionalChargesAmount,0) AdditionalChargesAmount
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,C.Id CountryId,C.UserName CountryName--,null AS [Flag] 
                        ,0 SalesRate
						,0 TotalAmount
                       ,IM.MaterialMasterId
						,MM.UserName Material
						,MT.UserName MaterialType
						,IM.ArticleId
						,MMA.StandardName Article
						,IM.FirstCharacteristicsValueId
						,IM.SecondCharacteristicsValueId
						,IM.ThirdCharacteristicsValueId
						
						,IM.FirstCharacteristicsId
						,IM.SecondCharacteristicsId
						,IM.ThirdCharacteristicsId,IssueByUoM=CASE WHEN MM.IssueByUoM=0 THEN 'No' ELSE 'Yes' END
                        ,TrasactopmUomQty=(((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0))*BaseUoMFactor)/BaseUoMFactor) 
						
						,'' IssueTransactionUoMId
						,'' IssueTransactionUoM,ird.MaterialStorageId,MS.UserName MaterialStorage,IRD.LotNumber
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					left join mst.MaterialMaster MM ON MM.Id=Im.MaterialMasterId
					left join mst.MaterialMasterArticle MMA ON MMA.Id=IM.ArticleId
                    LEFT JOIN mst.MaterialGroupMaster MGM ON MGM.Id=MM.MaterialGroupMasterId
					LEFT JOIN HKP.MaterialType MT ON MT.Id=MGM.MaterialTypeId
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                    left JOIN SCS.Country C On C.Id=IM.CountryId
                    LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE  IM.PlantId = '" + plantId + @"' 
                    
					AND IR.[Status]='Posting' AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.GRNType='MaterialTransfer'
                   
					AND ISNULL(IM.FirstCharacteristicsValueId,'')='' AND  ISNULL(IM.SecondCharacteristicsValueId,'')=''
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='' AND ISNULL(IM.CountryId,'')='' 
                   
					AND IRD.BaseQty !=IRD.BaseIssueQty
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + today + @"' AS DATE)							

					Union ALL
					SELECT IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                     , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 0 else 1 END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 0 else 1 END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor,IRD.BaseUoMFactor GRNBaseUoMFactor
                        , round(IRD.MaterialTranRate,4) MaterialTranRate,  TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                        ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4) BaseRate
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                         ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty,ISNULL(IRD.InventoryTransferQty,0) InventoryTransferQty
						 ,((((((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BooksCurrencyBaseRate
						 ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4) TrnCurrencyBaseRate
                         ,0 TotalIssueAmount
                         , ISNULL(IRD.AdditionalChargesAmount,0) AdditionalChargesAmount
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,C.Id CountryId,C.UserName CountryName--,null AS [Flag] 
                        ,0 SalesRate
						,0 TotalAmount
                        ,IM.MaterialMasterId
						,MM.UserName Material
						,MT.UserName MaterialType
						,IM.ArticleId
						,MMA.StandardName Article
						,IM.FirstCharacteristicsValueId
						,IM.SecondCharacteristicsValueId
						,IM.ThirdCharacteristicsValueId
						
						,IM.FirstCharacteristicsId
						,IM.SecondCharacteristicsId
						,IM.ThirdCharacteristicsId,IssueByUoM=CASE WHEN MM.IssueByUoM=0 THEN 'No' ELSE 'Yes' END
                        ,TrasactopmUomQty=(((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0))*BaseUoMFactor)/BaseUoMFactor) 
						
						,'' IssueTransactionUoMId
						,'' IssueTransactionUoM,ird.MaterialStorageId,MS.UserName MaterialStorage,IRD.LotNumber
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					left join mst.MaterialMaster MM ON MM.Id=Im.MaterialMasterId
					left join mst.MaterialMasterArticle MMA ON MMA.Id=IM.ArticleId
                    LEFT JOIN mst.MaterialGroupMaster MGM ON MGM.Id=MM.MaterialGroupMasterId
					LEFT JOIN HKP.MaterialType MT ON MT.Id=MGM.MaterialTypeId
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                    left JOIN SCS.Country C On C.Id=IM.CountryId
                    LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE  IM.PlantId = '" + plantId + @"' 
                 
					AND IR.[Status] IS null And IR.IsApproved=1 AND IR.RequiredPosting=0 AND IR.GRNType='MaterialTransfer'
                   
					AND ISNULL(IM.FirstCharacteristicsValueId,'')='' AND  ISNULL(IM.SecondCharacteristicsValueId,'')=''
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='' AND ISNULL(IM.CountryId,'')='' 
                   
					AND IRD.BaseQty !=IRD.BaseIssueQty
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + today + @"' AS DATE) )x WHERE x.BalanceStock>0 ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetInputSavedData(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false)
                    strkey = column + " like '%" + value + "%'";

                string sql = @"SELECT * FROM (
SELECT M.*,FORMAT(M.ConfirmationDate,'dd-MMM-yyyy')CD,ER.EmployeeCode ResponsiblePersonEmployeeCode,ER.EmployeeName ResponsiblePerson,EC.EmployeeCode CheckedByEmployeeCode,EC.EmployeeName CheckedBy,EN.UserName Entity,WCM.UserName WorkCenterMaster,P.UserName Process 
FROM dbo.InputConfirmationMaster M
LEFT JOIN dbo.EmployeeInformation ER ON ER.SystemId=M.ResponsiblePersonId
LEFT JOIN dbo.EmployeeInformation EC ON EC.SystemId=M.CheckedById
LEFT JOIN ORG.Entity EN ON EN.Id=M.EntityId
LEFT JOIN SCS.WorkCenterMaster WCM ON WCM.Id=M.WorkCenterMasterId
LEFT JOIN HKP.Process P ON P.id=M.ProcessId
) AS TEMP WHERE " + strkey + " Order by AddedDate Desc";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSavedinputDetailData(string masterId)
        {
            try
            {
                string sql = @"SELECT D.*,A.StandardName Article,U.Code UOM,CC.UserName CostCenter,PendingBookedQty=RequestedQty - OtherQty FROM dbo.InputConfirmationDetail D
 LEFT JOIN MST.MaterialMasterArticle A ON A.Id=D.ArticleId
 LEFT JOIN SCS.UnitofMeasurement U ON U.Id=D.UOMId 
Left Join [ORG].[CostCenter] CC On CC.Id=D.CostCenterId
                WHERE D.InputConfirmationMasterId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetInputConfirmationAdditionalMaterialData(string masterId)
        {
            try
            {
                string sql = @"Select M.*,MMA.Code ArticleCode,MM.UserName Material from dbo.InputConfirmationAdditionalMaterial M
LEFT JOIN MST.MaterialMaster MM  ON MM.Id=M.MaterialMasterId
LEFT JOIN MST.MaterialMasterArticle MMA  ON MMA.Id=M.ArticleId
                WHERE M.InputConfirmationMasterId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetFirstProcessCbo(string ProductionOrderId)
        {
            try
            {
                string sql = @"SELECT B.Id Value, B.UserName Text FROM TRN.ProductionOrderProcessSet A
LEFT JOIN HKP.Process B ON B.Id=A.ProcessId
Where A.ProductionOrderId='" + ProductionOrderId + "' AND A.Sequence=1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetMaterialIssueReportData(string masterId, out DataTable dtOrder)
        {
            try
            {
                string strSql = string.Empty;
                strSql = @"SELECT ROW_NUMBER() OVER(ORDER BY D.Id) SrNo,MS.PlanPercentage,FORMAT(MS.AddedDate,'dd-MMM-yyyy')AddedDate
                ,D.ValueLoss,D.GrossConsumption*100 GrossConsumption,D.TotalConsumption
				,D.PlanConsumption,ISNULL(IR.IssueQty,0) IssueQty
				,ISNULL(IDRM.Qty,0)ActualIssue,Balance=(ISNULL(IR.IssueQty,0) -ISNULL(IDRM.Qty,0))
				,D.ArticleId,D.MaterialMasterId,D.StockRate,D.ActualIssueAmount,D.Remarks
				,I.UserName Item,A.StandardName QBOQArticle
                ,M.UserName MaterialMaster,um.Code as UoM,IRM.CheckedByStatus,IRM.AuthorizedByStatus,MS.POId,IRM.Id IssueSlipId,CC.UserName CostCenter
				,P.UserName Customer,PL.Code,MSO.SOQty,PT.UserName PackingType
                ,Shade=STUFF((select distinct ','+PLA.AttributeValue from ProductLibraryAttribute PLA                                               
							                                where PLA.ProductLibraryId=PL.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                ,MS.AddedBy,CB.EmployeeName CheckedBy,AP.EmployeeName AuthorizedBy,AR.StandardName Article
                FROM dbo.MaterialIssueControlDetail D 
				LEFT JOIN dbo.MaterialIssueControlMaster MS ON MS.Id=D.MaterialIssueControlMasterId
				LEFT JOIN dbo.MaterialIssueControlSODetail MSO ON MSO.MaterialIssueControlMasterId=D.MaterialIssueControlMasterId
				LEFT JOIN TRN.SalesOrder SO ON SO.Id=MSO.SOId
				LEFT JOIN HKP.PackingType PT ON PT.Id=SO.PackingTypeId
				LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=MSO.LineItemId
				LEFT JOIN dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId
				LEFT JOIN HKP.Party P ON P.Id=MSO.CustomerId
				LEFT Join [ORG].[CostCenter] CC On CC.Id=MS.CostCenterId
                LEFT JOIN HKP.CostingItem I on i.Id=D.CostingItemId
                LEFT join [SCS].[UnitOfMeasurement] um on um.Id = i.UnitOfMeasurementId
                LEFT JOIN MST.MaterialMaster M ON M.Id=D.MaterialMasterId
                LEFT JOIN MST.MaterialMasterArticle A ON A.Id=D.ArticleId
				LEFT JOIN (SELECT MaterialIssueControlDetailId, SUM(ISNULL(RequestedQty,0)) IssueQty,TransactionUoMId,IssueRequestMasterId,Id
							FROM TRN.IssueRequest GROUP BY MaterialIssueControlDetailId,TransactionUoMId,IssueRequestMasterId,Id)IR ON IR.MaterialIssueControlDetailId=D.Id
				LEFT JOIN TRN.IssueRequestMaster IRM ON IRM.Id=IR.IssueRequestMasterId	
                LEFT JOIN dbo.EmployeeInformation CB ON CB.SystemId=IRM.CheckedBy
				LEFT JOIN dbo.EmployeeInformation AP ON AP.SystemId=IRM.AuthorizedBy
				 LEFT JOIN (
                                	SELECT aa.Id,sum(cc.Qty) Qty
                                	FROM trn.IssueRequest aa
                                	LEFT JOIN trn.IssueRequestMaster dd ON dd.id = aa.IssueRequestMasterId
                                	LEFT JOIN [TRN].[IssueRequestBOQMap] bb ON bb.IssueRequestDetailId = aa.id
                                	LEFT JOIN [TRN].[IssueDetailAndIssueRequestMap] cc ON cc.IssueRequestBOQMapId = bb.Id
                                	WHERE cc.IssueRequestBOQMapId IS NOT NULL
                                	GROUP BY aa.Id
                                	) IDRM ON IDRM.Id = IR.id
                LEFT JOIN TRN.MasterOrderItem SMOI ON SMOI.Id=SO.MasterOrderItemId
                 LEFT JOIN [MST].[MaterialMasterArticle] AR ON AR.Id=SMOI.ArticleId 
                WHERE D.MaterialIssueControlMasterId='" + masterId + "'";

                dtOrder = _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                
            }
        }//End Function


        public void GetMaterialIssueCheckApproveReportData(string masterId, out DataTable dtOrder)
        {
            try
            {
                string strSql = string.Empty;
                strSql = @"SELECT ROW_NUMBER() OVER(ORDER BY D.Id) SrNo,MS.PlanPercentage,FORMAT(MS.AddedDate,'dd-MMM-yyyy')AddedDate
                ,D.ValueLoss,D.GrossConsumption*100 GrossConsumption,D.TotalConsumption
				,D.PlanConsumption,ISNULL(IR.IssueQty,0) IssueQty
				,ISNULL(IDRM.Qty,0)ActualIssue,Balance=(ISNULL(IR.IssueQty,0) -ISNULL(IDRM.Qty,0))
				,D.ArticleId,D.MaterialMasterId,D.StockRate,D.ActualIssueAmount,D.Remarks
				,I.UserName Item,A.StandardName QBOQArticle
                ,M.UserName MaterialMaster,um.Code as UoM,IRM.CheckedByStatus,IRM.AuthorizedByStatus,MS.POId,IRM.Id IssueSlipId,CC.UserName CostCenter
				,P.UserName Customer,PL.Code,MSO.SOQty,PT.UserName PackingType
                ,Shade=STUFF((select distinct ','+PLA.AttributeValue from ProductLibraryAttribute PLA                                               
							                                where PLA.ProductLibraryId=PL.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                ,MS.AddedBy,CB.EmployeeName CheckedBy,AP.EmployeeName AuthorizedBy
                FROM dbo.MaterialIssueControlDetail D 
				LEFT JOIN dbo.MaterialIssueControlMaster MS ON MS.Id=D.MaterialIssueControlMasterId
				LEFT JOIN dbo.MaterialIssueControlSODetail MSO ON MSO.MaterialIssueControlMasterId=D.MaterialIssueControlMasterId
				LEFT JOIN TRN.SalesOrder SO ON SO.Id=MSO.SOId
				LEFT JOIN HKP.PackingType PT ON PT.Id=SO.PackingTypeId
				LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=MSO.LineItemId
				LEFT JOIN dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId
				LEFT JOIN HKP.Party P ON P.Id=MSO.CustomerId
				LEFT Join [ORG].[CostCenter] CC On CC.Id=MS.CostCenterId
                LEFT JOIN HKP.CostingItem I on i.Id=D.CostingItemId
                LEFT join [SCS].[UnitOfMeasurement] um on um.Id = i.UnitOfMeasurementId
                LEFT JOIN MST.MaterialMaster M ON M.Id=D.MaterialMasterId
                LEFT JOIN MST.MaterialMasterArticle A ON A.Id=D.ArticleId
				LEFT JOIN (SELECT MaterialIssueControlDetailId, SUM(ISNULL(RequestedQty,0)) IssueQty,TransactionUoMId,IssueRequestMasterId,Id
							FROM TRN.IssueRequest GROUP BY MaterialIssueControlDetailId,TransactionUoMId,IssueRequestMasterId,Id)IR ON IR.MaterialIssueControlDetailId=D.Id
				LEFT JOIN TRN.IssueRequestMaster IRM ON IRM.Id=IR.IssueRequestMasterId	
                LEFT JOIN dbo.EmployeeInformation CB ON CB.SystemId=IRM.CheckedBy
				LEFT JOIN dbo.EmployeeInformation AP ON AP.SystemId=IRM.AuthorizedBy
				 LEFT JOIN (
                                	SELECT aa.Id,sum(cc.Qty) Qty
                                	FROM trn.IssueRequest aa
                                	LEFT JOIN trn.IssueRequestMaster dd ON dd.id = aa.IssueRequestMasterId
                                	LEFT JOIN [TRN].[IssueRequestBOQMap] bb ON bb.IssueRequestDetailId = aa.id
                                	LEFT JOIN [TRN].[IssueDetailAndIssueRequestMap] cc ON cc.IssueRequestBOQMapId = bb.Id
                                	WHERE cc.IssueRequestBOQMapId IS NOT NULL
                                	GROUP BY aa.Id
                                	) IDRM ON IDRM.Id = IR.id
                WHERE D.MaterialIssueControlMasterId=(Select distinct MO.MaterialIssueControlMasterId from trn.IssueRequest A
LEFT JOIN dbo.MaterialIssueControlDetail MO ON MO.Id=A.MaterialIssueControlDetailId
Where  IssueRequestMasterId='" + masterId + "')";

                dtOrder = _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//End Function
        #endregion




    }

}
