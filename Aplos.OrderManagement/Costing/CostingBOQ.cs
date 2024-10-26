using ConnectionManager.DAL;
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
    public class CostingBOQ
    {
        CustomIdentity identity;
        SqlRepository _sqlRepository;
        public CostingBOQ()
        {
            _sqlRepository = new SqlRepository();
            identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        }

        public List<Dictionary<string, object>> GetAllCostingDirectMaterial(List<Dictionary<string, object>> SelectedSalesOrderIds, string SalesOrderId, string CostingBOQMasterId)
        {
            //           string sql = @"SELECT 
            //                           convert(bit,CASE WHEN ISNULL(itm.Id,'')='' THEN 0 ELSE 1 END) AS Selected,
            //                           convert(bit,CASE WHEN ISNULL(PRE.Id,'')='' THEN 0 ELSE 1 END) AS AlreadyTaken,ci.Sequence,
            //                            cm.Id, cm.CostingItemId, cm.Consumption,uom.UserName AS UOM,CM.GrossConsumption,CM.GrossAmount,
            //                           ci.UserName AS ItemDescription,
            //                                  CM.[Description], CM.SourcingType, CM.Remarks,ei.EmployeeName AS ResponsiblePerson,
            //                                  mm.UserName AS Material,mma.StandardName AS Article
            //                             FROM OrderProcurementCostingDirectMaterial AS CM
            //                           INNER JOIN trn.SalesOrder AS so ON so.OrderCostingMasterTemplateId=cm.OrderCostingMasterTemplateId AND so.Id='" + SalesOrderId + @"'
            //                           INNER JOIN hkp.CostingItem AS ci ON ci.Id=cm.CostingItemId
            //                           LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=cm.MaterialMasterId
            //                           LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=cm.ArticleId
            //                           LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=cm.ResponsiblePersonId
            //                           LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=ci.UnitOfMeasurementId
            //                           LEFT JOIN CostingBOQItems ITM ON itm.OrderProcurementCostingDirectMaterialId=cm.Id AND isnull(ITM.CostingBOQMasterId,'')='" + CostingBOQMasterId + @"'
            //                           LEFT JOIN CostingBOQItems PRE ON pre.OrderProcurementCostingDirectMaterialId=cm.Id AND isnull(pre.CostingBOQMasterId,'')<>'" + CostingBOQMasterId + @"'
            //Order By PRE.Id, ci.Sequence";
            string costingStage = "";
            string soIds = "''";
            string sql = "";
            for (int i = 0; i < SelectedSalesOrderIds.Count; i++)
                soIds += ",'" + SelectedSalesOrderIds[i]["SalesOrderId"].ToString() + "'";

            costingStage = SelectedSalesOrderIds[0]["CostingStage"].ToString();

            sql = @"SELECT 
                            convert(bit,CASE WHEN isnull(cb.Id,'')='' THEN 0 ELSE 1 END,0) AS Saved, convert(bit,CASE WHEN isnull(cb.Id,'')='' THEN 0 ELSE 1 END,0) AS Selected,cm.OrderCostingMasterTemplateId,
                            ci.Sequence,isnull(tr.SOCount,0) SOCount,
                             cm.Id, cm.CostingItemId, cm.Consumption,uom.UserName AS UOM,CM.GrossConsumption,CM.GrossAmount,
                            ci.UserName AS ItemDescription,
                                   CM.[Description], CM.SourcingType, CM.Remarks,ei.EmployeeName AS ResponsiblePerson,
                                   mm.UserName AS Material,mma.StandardName AS Article,CM.BOQCriteria,CM.POCriteria
                              FROM OrderProcurementCostingDirectMaterial AS CM
	                         INNER JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=cm.OrderCostingMasterTemplateId
                            INNER JOIN trn.SalesOrder AS so ON  so.Id='" + SalesOrderId + @"' --and so.MasterOrderItemId=moi.id
                            INNER JOIN hkp.CostingItem AS ci ON ci.Id=cm.CostingItemId
                            LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=cm.MaterialMasterId
                            LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=cm.ArticleId
                            LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=cm.ResponsiblePersonId
                            LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=ci.UnitOfMeasurementId
                            LEFT JOIN ( SELECT cb.OrderProcurementCostingDirectMaterialId,COUNT(DISTINCT cb.SalesOrderId) AS SOCount
									   FROM CostingBOQItems AS cb
									 WHERE cb.SalesOrderId IN (" + soIds + @")
									 GROUP BY cb.OrderProcurementCostingDirectMaterialId) AS TR ON tr.OrderProcurementCostingDirectMaterialId=cm.Id
                            LEFT JOIN CostingBOQItems AS cb ON CB.ID=(select TOP 1 Id from CostingBOQItems WHERE cb.OrderProcurementCostingDirectMaterialId=CM.Id AND cb.SalesOrderId=SO.Id AND cb.CostingBOQMasterId='" + CostingBOQMasterId + @"')
 
                            --LEFT JOIN CostingBOQItems ITM ON itm.OrderProcurementCostingDirectMaterialId=cm.Id AND isnull(ITM.CostingBOQMasterId,'')=''
                            --LEFT JOIN CostingBOQItems PRE ON pre.OrderProcurementCostingDirectMaterialId=cm.Id AND isnull(pre.CostingBOQMasterId,'')<>''
                        Order By  ci.Sequence ";

            if (costingStage== "ProcurementCosting")
            {
                sql = @"SELECT 
                            convert(bit,CASE WHEN isnull(cb.SOCount,0)=0 THEN 0 ELSE 1 END,0) AS Saved, convert(bit,CASE WHEN isnull(cb.SOCount,0)=0 THEN 0 ELSE 1 END,0) AS Selected,cm.OrderCostingMasterTemplateId,
                            ci.Sequence,isnull(tr.SOCount,0) SOCount,
                             cm.Id, cm.CostingItemId, cm.Consumption,uom.UserName AS UOM,CM.GrossConsumption,CM.GrossAmount,
                            ci.UserName AS ItemDescription,
                                   CM.[Description], CM.SourcingType, CM.Remarks,ei.EmployeeName AS ResponsiblePerson,
                                   mm.UserName AS Material,mma.StandardName AS Article,CM.BOQCriteria,CM.POCriteria
                              FROM OrderProcurementCostingDirectMaterial AS CM
                         INNER JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=cm.OrderCostingMasterTemplateId
                            INNER JOIN trn.SalesOrder AS so ON  so.Id='" + SalesOrderId + @"' and so.MasterOrderItemId=moi.id
                               INNER JOIN hkp.CostingItem AS ci ON ci.Id=cm.CostingItemId
                            LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=cm.MaterialMasterId
                            LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=cm.ArticleId
                            LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=cm.ResponsiblePersonId
                            LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=ci.UnitOfMeasurementId
                            LEFT JOIN ( SELECT cb.OrderProcurementCostingDirectMaterialId,COUNT(DISTINCT cb.SalesOrderId) AS SOCount
									   FROM CostingBOQItems AS cb
									 WHERE cb.SalesOrderId IN (" + soIds + @")
									 GROUP BY cb.OrderProcurementCostingDirectMaterialId) AS TR ON tr.OrderProcurementCostingDirectMaterialId=cm.Id
									 
							LEFT JOIN ( SELECT cb.OrderProcurementCostingDirectMaterialId,COUNT(DISTINCT cb.SalesOrderId) AS SOCount
									   FROM CostingBOQItems AS cb
									 WHERE cb.SalesOrderId IN (" + soIds + @") AND cb.CostingBOQMasterId='" + CostingBOQMasterId + @"'
									 GROUP BY cb.OrderProcurementCostingDirectMaterialId) AS CB ON CB.OrderProcurementCostingDirectMaterialId=cm.Id
                          
                      Order BY  CASE WHEN convert(bit,CASE WHEN isnull(cb.SOCount,0)=0 THEN 0 ELSE 1 END,0)=0 AND isnull(tr.SOCount,0)=" + SelectedSalesOrderIds.Count + @" THEN 1 ELSE 0 END,ci.Sequence ";
            }
            else
            {
                sql = @"SELECT 
                            convert(bit,CASE WHEN isnull(cb.SOCount,0)=0 THEN 0 ELSE 1 END,0) AS Saved, convert(bit,CASE WHEN isnull(cb.SOCount,0)=0 THEN 0 ELSE 1 END,0) AS Selected,cm.OrderCostingMasterTemplateId,
                            ci.Sequence,isnull(tr.SOCount,0) SOCount,
                             cm.Id, cm.CostingItemId, cm.Consumption,uom.UserName AS UOM,CM.GrossConsumption,CM.GrossAmount,
                            ci.UserName AS ItemDescription,
                                   CM.[Description], CM.SourcingType, CM.Remarks,ei.EmployeeName AS ResponsiblePerson,
                                   mm.UserName AS Material,mma.StandardName AS Article,CM.BOQCriteria,CM.POCriteria
                              FROM OrderPreCostingDirectMaterial AS CM
                         INNER JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=cm.OrderCostingMasterTemplateId
                            INNER JOIN trn.SalesOrder AS so ON  so.Id='" + SalesOrderId + @"' and so.MasterOrderItemId=moi.id
                               INNER JOIN hkp.CostingItem AS ci ON ci.Id=cm.CostingItemId
                            LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=cm.MaterialMasterId
                            LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=cm.ArticleId
                            LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=cm.ResponsiblePersonId
                            LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=ci.UnitOfMeasurementId
                            LEFT JOIN ( SELECT cb.OrderProcurementCostingDirectMaterialId,COUNT(DISTINCT cb.SalesOrderId) AS SOCount
									   FROM CostingBOQItems AS cb
									 WHERE cb.SalesOrderId IN (" + soIds + @")
									 GROUP BY cb.OrderProcurementCostingDirectMaterialId) AS TR ON tr.OrderProcurementCostingDirectMaterialId=cm.Id
									 
							LEFT JOIN ( SELECT cb.OrderProcurementCostingDirectMaterialId,COUNT(DISTINCT cb.SalesOrderId) AS SOCount
									   FROM CostingBOQItems AS cb
									 WHERE cb.SalesOrderId IN (" + soIds + @") AND cb.CostingBOQMasterId='" + CostingBOQMasterId + @"'
									 GROUP BY cb.OrderProcurementCostingDirectMaterialId) AS CB ON CB.OrderProcurementCostingDirectMaterialId=cm.Id
                          
                      Order BY  CASE WHEN convert(bit,CASE WHEN isnull(cb.SOCount,0)=0 THEN 0 ELSE 1 END,0)=0 AND isnull(tr.SOCount,0)=" + SelectedSalesOrderIds.Count + @" THEN 1 ELSE 0 END,ci.Sequence ";
            }

            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetAllCostingDirectMaterial(string CostingBOQMasterId)
        {

            string sql = @"SELECT distinct cb.CostingItemId,ci.Sequence,CI.UserName AS CostingItem,BI.BOMMaterialRefNo,cb.MaterialMasterId, cb.ArticleId,mm.Code AS MaterialCode,mm.UserName AS Material,
                                    mma.Code AS ArticleCode,mma.StandardName AS Article,cb.VendorId,p.UserName AS Vendor, null UsedBOQ
                                   -- ,UsedBOQ=(Select top(1) CAST(CASE WHEN Id IS NULL THEN 0 ELSE 1 END AS bit) from [TRN].[POBOQMAP] Where BOQDetailId=cb.Id)
                                      FROM BOQ AS cb 
                                    LEFT JOIN CostingBOQItems AS BI ON bi.CostingItemId=cb.CostingItemId AND bi.CostingBOQMasterId='" + CostingBOQMasterId + @"'
                                    LEFT JOIN hkp.CostingItem AS ci ON ci.Id=cb.CostingItemId
                                    LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=cb.MaterialMasterId
                                    LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=cb.ArticleId
                                    LEFT JOIN hkp.Party AS p ON cb.VendorId=p.Id
                                    WHERE cb.CostingBOQMasterId='" + CostingBOQMasterId + @"'
                                    ORDER BY ci.Sequence";


            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetAllCostingDirectMaterialForQuantityEdit(string CostingBOQMasterId)
        {

            string sql = @"SELECT distinct BOQ.Id, convert(bit,isnull(mm.WithSKU,0)) AS WithSKU,BOQ.CostingItemId,boq.SalesOrderId,boq.FGFirstCharacteristicsValueId,boq.FGSecondCharacteristicsValueId,cv1.UserName AS SKU1
									,cv2.UserName AS SKU2,BOQ.IncompleteMaterial,cb.AddedBy AS PreparedBy,FORMAT(cb.AddedDate,'dd-MMM-yyyy') AS CostingDate,
									ci.Sequence,ci.UserName AS CostingItem,mm.UserName AS Material,mma.StandardName AS Article,BOQ.ItemRefNo,p.UserName AS Vendor,
									mm.Code AS MaterialCode,mma.Code AS ArticleCode,emp.EmployeeName AS ResponsiblePerson,boq.BOMQty,boq.RequiredQty,boq.BOMQty-boq.RequiredQty AS BalanceToPurchase
									,(R.Rate+isnull(boq.UpDownCharge,0))*boq.RequiredQty AS BOMAmount,R.Rate,isnull(boq.UpDownCharge,0)UpDownCharge,BOQ.BOQCriteria,c.Code AS Currency,
									BOQ.RMDescription,BOQ.RMCustomerSpec,BOQ.RMVendorSpec,BOQ.SKUDesc,ci.Id CostingItemId,uom.UserName AS UOM
									,(R.Rate+isnull(boq.UpDownCharge,0))*BOQ.BOMQty AS PlanAmount,R.Rate*BOQ.BOMQty AS BOMAmount ,BOQ.OwnReferenceNo,BOQ.Remark,
									SKUDescConcat= ISNULL(BOQ.SKUDesc,CONCAT(boq.SalesOrderId,' ',d.UserName,' ',cv1.UserName,' ',cv2.UserName)),
									CriteriaDetail= ISNULL(BOQ.SKUDesc,CONCAT(boq.SalesOrderId,' ',d.UserName,' ',cv1.UserName,' ',cv2.UserName)),
									BOQ.FileName,BOQ.FileOriginalName,BOQ.Extension,BOQ.POCriteria
									,SONumber=STUFF((select distinct ','+XSO.Id 
																			 from trn.SalesOrder XSO
																			 LEFT JOIN dbo.CostingBOQMaster CBM ON CBM.Id=XSO.CostingBOQMasterId
																			 where CBM.Id=boq.CostingBOQMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,Destination=STUFF((select distinct ','+d.UserName 
																			 from mst.Destination D
																			 LEFT JOIN trn.SalesOrder XSO ON XSO.DestinationId=D.Id
																			 LEFT JOIN dbo.CostingBOQMaster CBM ON CBM.Id=XSO.CostingBOQMasterId
																			 where CBM.Id=boq.CostingBOQMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                    FROM BOQ
                                    LEFT JOIN CostingBOQMaster AS cb ON cb.Id=boq.CostingBOQMasterId
									LEFT JOIN CostingBOQItems CBI ON CBI.CostingItemId=BOQ.CostingItemId AND CBI.CostingBOQMasterId=boq.CostingBOQMasterId
									LEFT JOIN dbo.OrderProcurementCostingDirectMaterial R ON R.Id=CBI.OrderProcurementCostingDirectMaterialId AND R.CostingItemId=boq.CostingItemId
                                    LEFT JOIN hkp.Party AS p ON p.Id=boq.VendorId
                                    LEFT JOIN employeeinformation emp ON emp.SystemId=cb.EmployeeSystemId

                                    LEFT JOIN trn.SalesOrder AS so ON so.Id=boq.SalesOrderId
                                    LEFT JOIN mst.Destination AS d ON d.Id=so.DestinationId
                                    LEFT JOIN hkp.CostingItem AS ci ON ci.Id=boq.CostingItemId
                                    LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=boq.MaterialMasterId
                                    LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=boq.ArticleId
                                    LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=boq.UoMId
                                    LEFT JOIN scs.Currency AS c ON c.Id=boq.CurrencyId

                                    LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=boq.FGFirstCharacteristicsValueId
                                    LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=boq.FGSecondCharacteristicsValueId
                                    WHERE BOQ.CostingBOQMasterId='" + CostingBOQMasterId + @"' 
                                    
                                    ORDER BY ci.Sequence";

            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetAllBOQCosting(string CostingBOQMasterId)
        {

            string sql = @"SELECT convert(bit,isnull(BOQ.RequiredQtyApproved,0)) RequiredQtyApproved,convert(bit,isnull(mm.WithSKU,0)) AS WithSKU,BOQ.CostingItemId,boq.SalesOrderId,d.UserName AS Destination,
                            cv1.UserName AS SKU1,cv2.UserName AS SKU2,BOQ.IncompleteMaterial,cb.AddedBy AS PreparedBy,FORMAT(cb.AddedDate,'dd-MMM-yyyy') AS CostingDate,
                                    BOQ.Id, ci.Sequence,ci.UserName AS CostingItem,mm.UserName AS Material,mma.StandardName AS Article,BOQ.ItemRefNo,p.UserName AS Vendor,
                                    mm.Code AS MaterialCode,mma.Code AS ArticleCode,emp.EmployeeName AS ResponsiblePerson,
                                    boq.BOMQty,boq.RequiredQty,boq.BOMQty-boq.RequiredQty AS BalanceToPurchase,uom.UserName AS UOM,boq.rate*boq.RequiredQty AS BOMAmount,BOQ.BOQCriteria,c.Code AS Currency,
                                    BOQ.RMDescription,BOQ.RMCustomerSpec,BOQ.RMVendorSpec,BOQ.SKUDesc,ci.Id CostingItemId,
  boq.Rate*BOQ.RequiredQty AS PlanAmount,boq.Rate*BOQ.BOMQty AS BOMAmount ,BOQ.OwnReferenceNo,BOQ.Remark,
  SKUDescConcat= ISNULL(BOQ.SKUDesc,CONCAT(boq.SalesOrderId,' ',d.UserName,' ',cv1.UserName,' ',cv2.UserName)),
 CriteriaDetail= ISNULL(BOQ.SKUDesc,CONCAT(boq.SalesOrderId,' ',d.UserName,' ',cv1.UserName,' ',cv2.UserName)),
 BOQ.FileName,BOQ.FileOriginalName,BOQ.Extension,BOQ.POCriteria,BOQ.[Status],BOQ.Reason
,SONumber=STUFF((select distinct ','+XSO.Id 
                                         from   trn.SalesOrder XSO 	                                    
							             where XSO.Id=boq.SalesOrderId     	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                    FROM BOQ
                                    LEFT JOIN CostingBOQMaster AS cb ON cb.Id=boq.CostingBOQMasterId
                                    LEFT JOIN hkp.Party AS p ON p.Id=boq.VendorId
                                    LEFT JOIN employeeinformation emp ON emp.SystemId=cb.EmployeeSystemId

                                    LEFT JOIN trn.SalesOrder AS so ON so.Id=boq.SalesOrderId
                                    LEFT JOIN mst.Destination AS d ON d.Id=so.DestinationId
                                    LEFT JOIN hkp.CostingItem AS ci ON ci.Id=boq.CostingItemId
                                    LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=boq.MaterialMasterId
                                    LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=boq.ArticleId
                                    LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=boq.UoMId
                                    LEFT JOIN scs.Currency AS c ON c.Id=boq.CurrencyId

                                    LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=boq.FGFirstCharacteristicsValueId
                                    LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=boq.FGSecondCharacteristicsValueId
                                    WHERE BOQ.CostingBOQMasterId='" + CostingBOQMasterId + @"' 
                                    
                                    ORDER BY ci.Sequence";

            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetCustomerList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (
                                     SELECT P.Id,p.UserName AS Customer,mo.NoOfMasterOrder,moi.NoOfMasterOrderItem,so.NoOfMasterSalesOrder,
                                            so.MissingSKUBreakdown, so.MissingCostingAttachment, so.CostItemNos,
                                            so.ItemWithoutBOQ, so.OverdueSOList, so.DueSOList
                                    FROM hkp.Party AS p
                                    JOIN [HKP].[CompanyParty] AS COMP ON COMP.PartyId=P.Id AND COMP.PartyType='Customer' AND (COMP.PlantId='" + identity.PlantId + @"' OR isnull(COMP.PlantId,'')='')
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=COMP.PartyAccountGroupId
                                    LEFT JOIN (SELECT mo.PartyId,COUNT(*) AS NoOfMasterOrder FROM trn.MasterOrder AS mo GROUP BY mo.PartyId) AS MO ON mo.PartyId=p.Id
                                    LEFT JOIN (
	                                    SELECT mo.PartyId,COUNT(*) AS NoOfMasterOrderItem FROM trn.MasterOrder AS mo
	                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id GROUP BY mo.PartyId) AS MOI ON MOI.PartyId=p.Id

	                                 JOIN (
		                               SELECT mo.PartyId,COUNT(DISTINCT so.Id) AS NoOfMasterSalesOrder,
                                      SUM(CASE WHEN isnull(fc.Id,'')='' THEN 1 ELSE 0 END) AS MissingSKUBreakdown,
                                     SUM(CASE WHEN isnull(moi.OrderCostingMasterTemplateId,'')='' THEN 1 ELSE 0 END) AS MissingCostingAttachment,
                                     SUM(itm.CostItemNos) AS CostItemNos,SUM(itmw.ItemWithoutBOQ) AS ItemWithoutBOQ,sum(ovd.OverdueSOList) AS OverdueSOList,sum(du.DueSOList)DueSOList
  
                                       FROM trn.MasterOrder AS mo
		                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
		                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.id
                                        join hkp.OrderCategory AS oc on OC.Id=SO.OrderCategoryId and OC.UserName IN ('Confirmed','To Confirm')
		                                LEFT JOIN trn.FirstCharacteristics AS fc ON fc.SalesOrderId=so.Id AND fc.Id=(SELECT TOP 1 Id FROM trn.FirstCharacteristics WHERE SalesOrderId=so.Id)
		                              
		                                LEFT JOIN (SELECT COUNT(cb.CostingItemId) CostItemNos,cb.OrderCostingMasterTemplateId
		                                             FROM OrderProcurementCostingDirectMaterial AS cb GROUP BY cb.OrderCostingMasterTemplateId) AS ITM ON itm.OrderCostingMasterTemplateId=moi.OrderCostingMasterTemplateId
		                                
		                                 LEFT JOIN (SELECT SUM(CASE WHEN isnull(cb2.Id,'')='' THEN 1 ELSE 0 END) ItemWithoutBOQ,cb.OrderCostingMasterTemplateId
		                                             FROM OrderProcurementCostingDirectMaterial AS cb 
		                                            LEFT JOIN CostingBOQItems AS cb2 ON cb2.OrderProcurementCostingDirectMaterialId=cb.Id
		                                            GROUP BY cb.OrderCostingMasterTemplateId) AS ITMW ON ITMW.OrderCostingMasterTemplateId=moi.OrderCostingMasterTemplateId
		                                            
		                                 LEFT JOIN (SELECT COUNT(DISTINCT so.Id) AS OverdueSOList,so.Id
														FROM trn.SalesOrder AS so
	                                                    INNER JOIN trn.MasterOrderItem AS moi ON so.MasterOrderItemId=moi.id
														JOIN OrderProcurementCostingDirectMaterial J ON j.OrderCostingMasterTemplateId=moi.OrderCostingMasterTemplateId
														LEFT JOIN CostingBOQItems AS cb ON cb.OrderProcurementCostingDirectMaterialId=j.Id
														WHERE DATEDIFF(DAY,GETDATE(),CASE WHEN j.DependentDate='SOCreationDate' THEN so.AddedDate ELSE CASE WHEN  j.DependentDate='SODeliveryDate' THEN so.DeliveryDate ELSE GETDATE() END END)<0
														GROUP BY so.Id
		                                 ) OVD ON ovd.Id=so.Id
		                                 
		                                        LEFT JOIN (SELECT COUNT(DISTINCT so.Id) AS DueSOList,so.Id
														FROM trn.SalesOrder AS so
	                                                    INNER JOIN trn.MasterOrderItem AS moi ON so.MasterOrderItemId=moi.id
														JOIN OrderProcurementCostingDirectMaterial J ON j.OrderCostingMasterTemplateId=moi.OrderCostingMasterTemplateId
														LEFT JOIN CostingBOQItems AS cb ON cb.OrderProcurementCostingDirectMaterialId=j.Id
														WHERE DATEDIFF(DAY,GETDATE(),CASE WHEN j.DependentDate='SOCreationDate' THEN so.AddedDate ELSE CASE WHEN  j.DependentDate='SODeliveryDate' THEN so.DeliveryDate ELSE GETDATE() END END)>0
														GROUP BY so.Id
		                                 ) DU ON DU.Id=so.Id
		                WHERE so.OrderStatusId='Active'
		                       GROUP BY mo.PartyId) AS SO ON SO.PartyId=p.Id
                                    WHERE p.Id IN (SELECT MO.PartyId FROM trn.MasterOrder MO WHERE mo.plantId='" + identity.PlantId + @"')
                            ) AS TEMP WHERE 1=1 AND " + strkey + @" ";


            return _sqlRepository.GetDataCollection(sql, null);

        }
        public List<Dictionary<string, object>> GetEditList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 1000 * from (
                                  SELECT cost.Id, cost.Remarks,ei.EmployeeName,p.UserName AS CustomerName,COST.CustomerId,
                                    ei.SystemId AS EmployeeSystemId,cost.UserName,FORMAT(cost.AddedDate,'dd-MMM-yyyy') BOMCreationDate,cost.AddedDate,
                                    ItemList=STUFF((SELECT distinct ','+  XCI.UserName
                                     from CostingBOQItems XITM
                                     INNER JOIN OrderProcurementCostingDirectMaterial AS XD ON Xd.Id=Xitm.OrderProcurementCostingDirectMaterialId
                                     INNER JOIN hkp.CostingItem AS Xci ON Xci.Id=Xd.CostingItemId
			                         where XITM.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                    
                                    ,SalesOrderId=STUFF((SELECT distinct ','+  XITM.Id
                                    from trn.SalesOrder AS XITM
                                    where XITM.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                    ,Description=STUFF((SELECT distinct ','+  XITM.Description
                                    from trn.SalesOrder AS XITM
                                    where XITM.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                    ,BuyerItemNo=STUFF((SELECT distinct ','+  moi.BuyerReferenceNo
                                    from trn.SalesOrder AS XITM
                                    JOIN trn.MasterOrderItem AS moi ON moi.Id=XITM.MasterOrderItemId
                                    where XITM.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                    ,OwnItemNo=STUFF((SELECT distinct ','+  moi.OwnReferenceNo
                                    from trn.SalesOrder AS XITM
                                    JOIN trn.MasterOrderItem AS moi ON moi.Id=XITM.MasterOrderItemId
                                    where XITM.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


                                    ,MasterOrderId=STUFF((SELECT distinct ','+  moi.MasterOrderId
                                    from trn.SalesOrder AS XITM
                                    JOIN trn.MasterOrderItem AS moi ON moi.Id=XITM.MasterOrderItemId
                                    where XITM.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                    ,OwnOrderNo=STUFF((SELECT distinct ','+  mo.OwnReferenceNo
                                    from trn.SalesOrder AS XITM
                                    JOIN trn.MasterOrderItem AS moi ON moi.Id=XITM.MasterOrderItemId
                                    JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
                                    where XITM.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                    ,BuyerOrderNo=STUFF((SELECT distinct ','+  mo.BuyerReferenceNo
                                    from trn.SalesOrder AS XITM
                                    JOIN trn.MasterOrderItem AS moi ON moi.Id=XITM.MasterOrderItemId
                                    JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
                                    where XITM.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                     FROM CostingBOQMaster AS cost
                                    LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=cost.EmployeeSystemId
                                    LEFT JOIN hkp.Party AS p ON p.Id=cost.CustomerId
                            ) AS TEMP WHERE 1=1 AND " + strkey + @" ORDER BY convert(datetime,AddedDate) DESC";


            return _sqlRepository.GetDataCollection(sql, null);

        }


        public Dictionary<string, object> Save(Dictionary<string, object> MasterData, List<Dictionary<string, object>> SalesOrderData, List<Dictionary<string, object>> ItemData)
        {
            try
            {
                string costingStage = "";
                if (MasterData == null)
                    throw new Exception("Please select parameters");

                if (SalesOrderData == null || SalesOrderData.Count == 0)
                    throw new Exception("Please select sales order");

                if (ItemData == null || ItemData.Count == 0)
                    throw new Exception("Please select at least one item");

                if (MasterData["UserName"] == null)
                    throw new Exception("User name is required.");
                //if (string.IsNullOrEmpty(MasterData["UserName"].ToString()))
                //    throw new Exception("Please add user name.");

                string SOIds = "''";
                for (int i = 0; i < SalesOrderData.Count; i++)
                    SOIds += ",'" + SalesOrderData[i]["SalesOrderId"].ToString() + "'";

                string CostingItemIds = "''";
                for (int i = 0; i < ItemData.Count; i++)
                    CostingItemIds += ",'" + ItemData[i]["CostingItemId"].ToString() + "'";


                costingStage = SalesOrderData[0]["CostingStage"].ToString();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.clsConnectionManager ConManager = new ConnectionManager.clsConnectionManager();
                ConManager.getDataSet("select * from CostingBOQMaster where Id='" + MasterData["Id"] + "'", out DataSet dsMaster);
                string sql = @"select TOP(1)* from CostingBOQMaster where Id<>'" + MasterData["Id"] + "' and UserName='" + MasterData["UserName"] + "' ";

                ConManager.getDataSet(sql, out DataSet dsValid);
                string _masterId = "";
                if (dsValid.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("User Name Already Exist.");
                }
                #region BOQ MASTER


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenHRID(DateTime.Now.ToShortDateString(), "Costing BOQ Master", out _masterId);
                    _masterId = System.DateTime.Now.ToString("yyyy").Substring(2, 2) + _masterId;
                    MasterData["Id"] = _masterId;
                    AddNewRow(dsMaster.Tables[0], MasterData);
                }
                else
                {
                    _masterId = MasterData["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], MasterData);
                }
                #endregion BOQ MASTER


                //#region SO

                ConManager.getDataSet(@"select * from TRN.SalesOrder where Id IN (" + SOIds + ")", out DataSet dsSOItems);

                for (int s = 0; s < dsSOItems.Tables[0].Rows.Count; s++)
                {
                    DataView dv = new DataView(dsSOItems.Tables[0]);
                    dv.RowFilter = "Id='" + dsSOItems.Tables[0].Rows[s]["Id"] + "'";

                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();

                        drmo["CostingBOQMasterId"] = _masterId;
                        drmo["UpdatedBy"] = identity.Name;
                        drmo["UpdatedDate"] = DateTime.Now.ToString();
                        drmo["UpdatedFromIP"] = identity.IPAddress;

                        drmo.EndEdit();

                    }
                }

                // drop table CostingBOQSalesOrder

                //ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter("select * from CostingBOQSalesOrder where CostingBOQMasterId='" + MasterData["Id"].ToString() + "'", out DataSet dsSOItems, false, "1");
                string _childId = "";
                //for (int i = 0; i < dsSOItems.Tables[0].Rows.Count; i++)
                //{
                //    var k = SalesOrderData.Where(w => w["SalesOrderId"].ToString() == dsSOItems.Tables[0].Rows[i]["SalesOrderId"].ToString()).ToList();
                //    if (k.Count == 0)
                //        dsSOItems.Tables[0].Rows[i].Delete();
                //}

                //for (int i = 0; i < SalesOrderData.Count; i++)
                //{
                //    dsSOItems.Tables[0].DefaultView.RowFilter = "SalesOrderId='" + SalesOrderData[i]["SalesOrderId"].ToString() + "'";
                //    if (dsSOItems.Tables[0].DefaultView.Count == 0)
                //    {
                //        if (_childId == "")
                //        {
                //            bplib.clsGenID genid = new bplib.clsGenID();
                //            genid.GenHRID(DateTime.Now.ToShortDateString(), "Costing BOQ Sales Order", out _childId);
                //            _childId = System.DateTime.Now.ToString("yyyy").Substring(2, 2) + _childId;
                //        }

                //        DataRow dr = dsSOItems.Tables[0].NewRow();

                //        dr["Id"] = _childId + "-" + (i + 1).ToString();
                //        dr["CostingBOQMasterId"] = _masterId;
                //        dr["SalesOrderId"] = SalesOrderData[i]["SalesOrderId"].ToString();

                //        dr["AddedBy"] = identity.Name;
                //        dr["AddedDate"] = System.DateTime.Now.ToString();
                //        dr["AddedFromIP"] = identity.IPAddress;
                //        dr["UpdatedBy"] = identity.Name;
                //        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                //        dr["UpdatedFromIP"] = identity.IPAddress;


                //        dsSOItems.Tables[0].Rows.Add(dr);
                //    }
                //}
                //#endregion SO

                #region BOQ Items
                ConManager.getDataSet("select * from CostingBOQItems where SalesOrderId IN (" + SOIds + ")", out DataSet dsItems);
                _childId = "";
                //for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                //{
                //    var k = ItemData.Where(w => w["Id"].ToString() == dsItems.Tables[0].Rows[i]["OrderProcurementCostingDirectMaterialId"].ToString()).ToList();
                //    if (k.Count == 0)
                //        dsItems.Tables[0].Rows[i].Delete();
                //}
                for (int SO = 0; SO < SalesOrderData.Count; SO++)
                {

                    for (int i = 0; i < ItemData.Count; i++)
                    {
                        dsItems.Tables[0].DefaultView.RowFilter = "SalesOrderId='" + SalesOrderData[SO]["SalesOrderId"].ToString() + "' AND OrderProcurementCostingDirectMaterialId='" + ItemData[i]["Id"].ToString() + "'";
                        if (dsItems.Tables[0].DefaultView.Count == 0)
                        {
                            if (_childId == "")
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenHRID(DateTime.Now.ToShortDateString(), "Costing BOQ Items", out _childId);
                                _childId = System.DateTime.Now.ToString("yyyy").Substring(2, 2) + _childId;
                            }

                            DataRow dr = dsItems.Tables[0].NewRow();

                            dr["Id"] = _childId + "-" + SO.ToString() + "-" + (i + 1).ToString();
                            dr["SalesOrderId"] = SalesOrderData[SO]["SalesOrderId"].ToString();
                            dr["OrderProcurementCostingDirectMaterialId"] = ItemData[i]["Id"].ToString();
                            dr["CostingItemId"] = ItemData[i]["CostingItemId"].ToString();
                            dr["CostingBOQMasterId"] = _masterId;
                            dr["BOMMaterialRefNo"] = _masterId + "/" + ItemData[i]["CostingItemId"].ToString();


                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;


                            dsItems.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dsItems.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();



                            dr.EndEdit();


                        }
                    }
                }
                #endregion BOQ Items
                BOQ(_masterId, SOIds, CostingItemIds, ItemData[0]["OrderCostingMasterTemplateId"].ToString(), costingStage, out DataSet dsBOQ, out DataSet dsBOQCompact);

                //for (int i = 0; i < dsItems.Tables[0].DefaultView.Count; i++)
                //{
                //    dsBOQCompact.Tables[0].DefaultView.RowFilter = "CostingItemId='"+ dsItems.Tables[0].DefaultView[i]["CostingItemId"].ToString() + "'";
                //    for (int j = 0; j < dsBOQCompact.Tables[0].DefaultView.Count; j++)
                //    {
                //        dsBOQCompact.Tables[0].DefaultView[j]["ItemRefNo"] = dsItems.Tables[0].DefaultView[i]["BOMMaterialRefNo"].ToString() + "/" + (j + 1).ToString();
                //    }
                //}

                Library.General.Conversions.UOMConversion conversion = new Library.General.Conversions.UOMConversion();

                dsBOQCompact.Tables[0].DefaultView.RowFilter = null;
                dsBOQCompact.Tables[0].DefaultView.Sort = "Sequence ASC";
                for (int j = 0; j < dsBOQCompact.Tables[0].DefaultView.Count; j++)
                {
                    string BaseUOM = conversion.GetMaterialUOMByCategory(dsBOQCompact.Tables[0].DefaultView[j]["MaterialMasterId"].ToString(), General.Conversions.UOMConversion.UOMCategory.BaseUOMId);
                    string POUoMId = conversion.GetMaterialUOMByCategory(dsBOQCompact.Tables[0].DefaultView[j]["MaterialMasterId"].ToString(), General.Conversions.UOMConversion.UOMCategory.PurchaseOrderUOMId);

                    dsBOQCompact.Tables[0].DefaultView[j]["BaseUoMId"] = bplib.clsWebLib.RetValidLen(BaseUOM);
                    dsBOQCompact.Tables[0].DefaultView[j]["POUoMId"] = bplib.clsWebLib.RetValidLen(POUoMId);

                    dsBOQCompact.Tables[0].DefaultView[j]["BOMQtyBase"] = conversion.Convert(
                        dsBOQCompact.Tables[0].DefaultView[j]["MaterialMasterId"].ToString(),
                        dsBOQCompact.Tables[0].DefaultView[j]["UoMId"].ToString(),
                        BaseUOM,
                        clsStaticInfo.dbl(dsBOQCompact.Tables[0].DefaultView[j]["BOMQty"].ToString()));

                    dsBOQCompact.Tables[0].DefaultView[j]["RequiredQtyBase"] = dsBOQCompact.Tables[0].DefaultView[j]["BOMQtyBase"];

                    dsBOQCompact.Tables[0].DefaultView[j]["RequiredQtyPO"] = conversion.Convert(
                        dsBOQCompact.Tables[0].DefaultView[j]["MaterialMasterId"].ToString(),
                        dsBOQCompact.Tables[0].DefaultView[j]["UoMId"].ToString(),
                        POUoMId,
                        clsStaticInfo.dbl(dsBOQCompact.Tables[0].DefaultView[j]["BOMQty"].ToString()));


                    double POUOMFactor = conversion.Convert(
                        dsBOQCompact.Tables[0].DefaultView[j]["MaterialMasterId"].ToString(),
                        dsBOQCompact.Tables[0].DefaultView[j]["UoMId"].ToString(),
                        POUoMId,
                        1);

                    //if (POUOMFactor > 0 )
                    //    dsBOQCompact.Tables[0].DefaultView[j]["Rate"] = clsStaticInfo.dbl(dsBOQCompact.Tables[0].DefaultView[j]["Rate"]) / POUOMFactor;

                    dsBOQCompact.Tables[0].DefaultView[j]["ItemRefNo"] = _masterId + "/" + (j + 1).ToString();
                }
                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsItems, dsBOQ, dsBOQCompact, dsSOItems);

            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {

            }
            return MasterData;
        }

       

        public void Delete(string Id)
        {

            try
            {
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                objCon.ExecuteNonQueryWrapper(@"delete from CostingBOQItems where CostingBOQMasterId='" + Id + "'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"delete from CostingBOQSalesOrder where CostingBOQMasterId='" + Id + "'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"delete from CostingBOQMaster where Id='" + Id + "'", true, "1");

                objCon.CommitTransaction();

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }




            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }

        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }


        private void BOQQuery(string CostingBOQMasterId, string SalesOrderIds, string ProcurementCostingItemIds, string CostingTemplateId, string costingStage, out DataTable dt, out Dictionary<string, DataRow> dicNewBOQ)
        {
            try
            {
                string _sql = "";
                //if (costingStage == "PreCosting")
                //{
                //    _sql = @"SELECT so.Id AS SalesOrderId,ocs.CostingItemId,ocs.MaterialMasterId,ocs.ArticleId,ocs.VendorId,so.DestinationId,ci.Sequence,
                //                OCS.Id AS OrderProcurementCostingDirectMaterialId,OCS.BOQCriteria,OCS.POCriteria,
                //                fc.CharacteristicsValueId AS FGFirstCharacteristicsValueId,sc.CharacteristicsValueId FGSecondCharacteristicsValueId,tc.CharacteristicsValueId FGThirdCharacteristicsValueId,
                //                ocs.GrossConsumption,ci.UnitOfMeasurementId AS UoMId, ocs.GrossAmount,cmt.CurrencyId,
                //                CASE WHEN isnull(tc.Id,'')<>'' THEN tc.Qty ELSE 
                //                CASE WHEN ISNULL(sc.Id,'')<>'' THEN sc.Qty ELSE
                //                CASE WHEN ISNULL(fc.Id,'')<>'' THEN fc.Qty ELSE so.Qty END END END AS OrderQty
   
                //                   from trn.SalesOrder so
                //                                            LEFT JOIN OrderPreCostingDirectMaterial OCS ON 1=1
                //                                            JOIN OrderCostingMasterTemplate AS cmt ON cmt.Id=ocs.OrderCostingMasterTemplateId
                //                                            JOIN hkp.CostingItem AS ci ON ci.Id=OCS.CostingItemId

                //                                            LEFT JOIN trn.FirstCharacteristics AS fc ON fc.SalesOrderId=so.Id
                //                                            LEFT JOIN trn.SecondCharacteristics AS sc ON sc.FirstCharacteristicsId=fc.Id AND sc.SalesOrderId=so.Id
                //                                            LEFT JOIN trn.ThirdCharacteristics AS tc ON tc.SecondCharacteristicsId=sc.Id AND tc.SalesOrderId=so.Id

                       
							         //                       LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=fc.CharacteristicsValueId
							         //                       LEFT JOIN hkp.Characteristics AS c1 ON c1.Id=cv1.CharacteristicsId
							
							         //                       LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=sc.CharacteristicsValueId
							         //                       LEFT JOIN hkp.Characteristics AS c2 ON c2.Id=cv2.CharacteristicsId
							
							         //                       LEFT JOIN hkp.CharacteristicsValue AS cv3 ON cv3.Id=tc.CharacteristicsValueId
							         //                       LEFT JOIN hkp.Characteristics AS c3 ON c3.Id=cv3.CharacteristicsId
							
							
                //                    WHERE so.Id IN (" + SalesOrderIds + @") AND Ci.Id  IN (" + ProcurementCostingItemIds + @") AND ocs.OrderCostingMasterTemplateId='" + CostingTemplateId + @"'
                //                          and isnull(CONCAT(SO.Id,'-',OCS.Id),'') NOT IN (select isnull(CONCAT(SalesOrderId,'-',OrderProcurementCostingDirectMaterialId),'') AS Id from CostingBOQItems where CostingBOQMasterId<>'" + CostingBOQMasterId + @"')
                //                    ORDER BY so.Id,fc.CharacteristicsValueId,sc.CharacteristicsValueId,tc.CharacteristicsValueId,ci.Id";
                //}
                //else
                //{
                    _sql = @"SELECT so.Id AS SalesOrderId,ocs.CostingItemId,ocs.MaterialMasterId,ocs.ArticleId,ocs.VendorId,so.DestinationId,ci.Sequence,
                                OCS.Id AS OrderProcurementCostingDirectMaterialId,OCS.BOQCriteria,OCS.POCriteria,
                                fc.CharacteristicsValueId AS FGFirstCharacteristicsValueId,sc.CharacteristicsValueId FGSecondCharacteristicsValueId,tc.CharacteristicsValueId FGThirdCharacteristicsValueId,
                                ocs.GrossConsumption,ci.UnitOfMeasurementId AS UoMId, ocs.GrossAmount,cmt.CurrencyId,
                                CASE WHEN isnull(tc.Id,'')<>'' THEN tc.Qty ELSE 
                                CASE WHEN ISNULL(sc.Id,'')<>'' THEN sc.Qty ELSE
                                CASE WHEN ISNULL(fc.Id,'')<>'' THEN fc.Qty ELSE so.Qty END END END AS OrderQty
   
                                   from trn.SalesOrder so
                                                            LEFT JOIN OrderProcurementCostingDirectMaterial OCS ON 1=1
                                                            JOIN OrderCostingMasterTemplate AS cmt ON cmt.Id=ocs.OrderCostingMasterTemplateId
                                                            JOIN hkp.CostingItem AS ci ON ci.Id=OCS.CostingItemId

                                                            LEFT JOIN trn.FirstCharacteristics AS fc ON fc.SalesOrderId=so.Id
                                                            LEFT JOIN trn.SecondCharacteristics AS sc ON sc.FirstCharacteristicsId=fc.Id AND sc.SalesOrderId=so.Id
                                                            LEFT JOIN trn.ThirdCharacteristics AS tc ON tc.SecondCharacteristicsId=sc.Id AND tc.SalesOrderId=so.Id

                       
							                                LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=fc.CharacteristicsValueId
							                                LEFT JOIN hkp.Characteristics AS c1 ON c1.Id=cv1.CharacteristicsId
							
							                                LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=sc.CharacteristicsValueId
							                                LEFT JOIN hkp.Characteristics AS c2 ON c2.Id=cv2.CharacteristicsId
							
							                                LEFT JOIN hkp.CharacteristicsValue AS cv3 ON cv3.Id=tc.CharacteristicsValueId
							                                LEFT JOIN hkp.Characteristics AS c3 ON c3.Id=cv3.CharacteristicsId
							
							
                                    WHERE so.Id IN (" + SalesOrderIds + @") AND Ci.Id  IN (" + ProcurementCostingItemIds + @") AND ocs.OrderCostingMasterTemplateId='" + CostingTemplateId + @"'
                                          and isnull(CONCAT(SO.Id,'-',OCS.Id),'') NOT IN (select isnull(CONCAT(SalesOrderId,'-',OrderProcurementCostingDirectMaterialId),'') AS Id from CostingBOQItems where CostingBOQMasterId<>'" + CostingBOQMasterId + @"')
                                    ORDER BY so.Id,fc.CharacteristicsValueId,sc.CharacteristicsValueId,tc.CharacteristicsValueId,ci.Id";
                //}


                dt = _sqlRepository.GetDataTable(_sql);
                dt.Columns.Add("BOMQty");
                dt.Columns.Add("RequiredQty");
                dt.Columns.Add("BOMAmount");
                dt.Columns.Add("MasterOrderItemId");

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["BOMQty"] = OTSBD.clsStaticInfo.dbl(dt.Rows[i]["OrderQty"].ToString()) * OTSBD.clsStaticInfo.dbl(dt.Rows[i]["GrossConsumption"].ToString());
                    dt.Rows[i]["RequiredQty"] = OTSBD.clsStaticInfo.dbl(dt.Rows[i]["OrderQty"].ToString()) * OTSBD.clsStaticInfo.dbl(dt.Rows[i]["GrossConsumption"].ToString());
                    dt.Rows[i]["BOMAmount"] = OTSBD.clsStaticInfo.dbl(dt.Rows[i]["OrderQty"].ToString()) * OTSBD.clsStaticInfo.dbl(dt.Rows[i]["GrossAmount"].ToString());
                }

                dicNewBOQ = new Dictionary<string, DataRow>();
                string KEY = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    KEY = BOMComparingString(dt.Rows[i]);
                    dicNewBOQ.Add(KEY, dt.Rows[i]);
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private string BOMComparingString(DataRow dr)
        {

            string[] columns = {
                    "SalesOrderId",
                    "MaterialMasterId",
                    "ArticleId",
                    "CostingItemId",
                    //"DestinationId",
                    "FGFirstCharacteristicsValueId",
                    "FGSecondCharacteristicsValueId",
                    "FGThirdCharacteristicsValueId" };

            string key = "";
            for (int i = 0; i < columns.Length; i++)
            {
                key += "-" + dr[columns[i]].ToString().Trim();
            }

            return key;
        }

        private string GetPK(string TableName)
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), TableName, out sID);
            return sID;
        }

        private void CopyRow(DataRow drSource, DataRow drDestination)
        {

            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = bplib.clsWebLib.RetValidLen(drSource[drSource.Table.Columns[COL].ColumnName].ToString());

                }
                catch (Exception ex)
                {
                }
                try
                {
                    drDestination["AddedBy"] = identity.Name;
                    drDestination["AddedDate"] = DateTime.Now;
                    drDestination["AddedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedBy"] = identity.Name;
                    drDestination["UpdatedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedDate"] = DateTime.Now;

                }
                catch (Exception ex)
                {
                }
            }

        }

        private void EditRow(DataRow drSource, DataRow drDestination)
        {

            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {

                    if (drSource.Table.Columns[COL].ColumnName.ToUpper() == "ID"
                       || drSource.Table.Columns[COL].ColumnName.ToUpper() == "ADDEDBY"
                            || drSource.Table.Columns[COL].ColumnName.ToUpper() == "ADDEDDATE"
                        || drSource.Table.Columns[COL].ColumnName.ToUpper() == "ADDEDFROMIP")
                        continue;

                    drDestination[drSource.Table.Columns[COL].ColumnName] = bplib.clsWebLib.RetValidLen(drSource[drSource.Table.Columns[COL].ColumnName].ToString());

                }
                catch (Exception ex)
                {
                }

            }

            try
            {


                drDestination["UpdatedBy"] = identity.Name;
                drDestination["UpdatedFromIP"] = identity.IPAddress;
                drDestination["UpdatedDate"] = DateTime.Now;

            }
            catch (Exception ex)
            {
            }

        }

        public void BOQ(string CostingBOQMasterId, string SalesOrderIds, string CostingItemIds, string CostingTemplateId,string costingStage, out DataSet dsExistingBOQ, out DataSet CompactBOQData)
        {
            DataSet dsMISO;
            BOQQuery(CostingBOQMasterId, SalesOrderIds, CostingItemIds, CostingTemplateId, costingStage, out DataTable dtNewBOQ, out Dictionary<string, DataRow> dicNewData);

            ConnectionManager.clsConnectionManager ConManager = new ConnectionManager.clsConnectionManager();
            ConManager.getDataSet("select * from CostingBOQ where SalesOrderId IN (" + SalesOrderIds + ") AND CostingItemId IN (" + CostingItemIds + ")", out dsExistingBOQ);
            ConManager.getDataSet(@"select distinct MasterOrderItemId,Id from TRN.SalesOrder where Id IN (" + SalesOrderIds + ")", out dsMISO);


            dsExistingBOQ.Tables[0].Columns.Add("MasterOrderItemId");
            string KEY = "";
            Dictionary<string, DataRow> dicExistingData = new Dictionary<string, DataRow>();
            for (int i = 0; i < dsExistingBOQ.Tables[0].Rows.Count; i++)
            {


                KEY = BOMComparingString(dsExistingBOQ.Tables[0].Rows[i]);
                dicExistingData.Add(KEY, dsExistingBOQ.Tables[0].Rows[i]);
            }

            //delete unused data
            for (int i = 0; i < dsExistingBOQ.Tables[0].Rows.Count; i++)
            {
                KEY = BOMComparingString(dsExistingBOQ.Tables[0].Rows[i]);
                if (dicNewData.ContainsKey(KEY) == false)
                    dsExistingBOQ.Tables[0].Rows[i].Delete();

                continue;



            }
            string _id = "";
            for (int i = 0; i < dtNewBOQ.Rows.Count; i++)
            {
                KEY = BOMComparingString(dtNewBOQ.Rows[i]);
                if (dicExistingData.ContainsKey(KEY) == false)
                {
                    if (_id == "")
                        _id = GetPK("CostingBOQ");

                    DataRow dr = dsExistingBOQ.Tables[0].NewRow();
                    CopyRow(dtNewBOQ.Rows[i], dr);
                    dr["Id"] = _id + "-" + (i + 1).ToString();
                    dr["CostingBOQMasterId"] = CostingBOQMasterId;

                    //dsMISO.Tables[0].DefaultView.RowFilter = "Id='" + dr["SalesOrderId"] + "'";
                    dr["MasterOrderItemId"] = dsMISO.Tables[0].DefaultView[0].Row["MasterOrderItemId"].ToString();

                    dsExistingBOQ.Tables[0].Rows.Add(dr);

                    dicExistingData.Add(KEY, dr);
                }
                else
                {
                    DataRow dr = dicExistingData[KEY];
                    dr.BeginEdit();
                    dr["CostingBOQMasterId"] = CostingBOQMasterId;

                    dr["OrderQty"] = dtNewBOQ.Rows[i]["OrderQty"];
                    dr["BOMQty"] = dtNewBOQ.Rows[i]["BOMQty"];
                    dr["RequiredQty"] = dtNewBOQ.Rows[i]["RequiredQty"];
                    dr["BOMAmount"] = dtNewBOQ.Rows[i]["BOMAmount"];
                    dr["MasterOrderItemId"] = dsMISO.Tables[0].DefaultView[0].Row["MasterOrderItemId"].ToString();

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr.EndEdit();

                }
            }


            CompactBOQData = CompactBOQWithCriteria(CostingBOQMasterId, dsExistingBOQ.Tables[0], SalesOrderIds, CostingItemIds);

        }

        public DataSet CompactBOQWithCriteria(string CostingBOQMasterId, DataTable dtSourceBOQ, string SalesOrderIds, string CostingItemIds)
        {
            //Id CostingBOQMasterId  SalesOrderId CostingItemId   DestinationId MaterialMasterId    ArticleId ,BOMCriteria,
            //OrderProcurementCostingDirectMaterialId ItemRefNo UoMId   UoMIdBase CurrencyId  CurrencyIdBase VendorId    
            //SKU1 SKU2    SKU3 OrderQty    BOMQty RequiredQty BOMAmount Sequence POCriteria   AddedBy AddedDate   AddedFromIP UpdatedBy   UpdatedDate UpdatedFromIP

            ConnectionManager.clsConnectionManager ConManager = new ConnectionManager.clsConnectionManager();
            ConManager.getDataSet("select * from BOQ where CostingBOQMasterId='" + CostingBOQMasterId + "' AND Id NOT IN (SELECT BOQDetailId FROM TRN.POBOQMAP)", out DataSet dsExistingBOQ);


            ConManager = new ConnectionManager.clsConnectionManager();
            //ConManager.getDataSet(@"SELECT distinct CB.CostingItemId,MM.GrossAmount
            //                       FROM CostingBOQItems AS cb  
            //                     JOIN OrderProcurementCostingDirectMaterial MM ON MM.Id=CB.OrderProcurementCostingDirectMaterialId
            //                    WHERE cb.CostingBOQMasterId='" + CostingBOQMasterId + @"'", out DataSet dtRateFromCosting);

            ConManager.getDataSet(@"SELECT MM.CostingItemId,MM.GrossAmount,SO.Id
                                FROM OrderProcurementCostingDirectMaterial MM 
                                LEFT JOIN CostingBOQItems AS cb   ON MM.Id=CB.OrderProcurementCostingDirectMaterialId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=MM.OrderCostingMasterTemplateId
                                INNER JOIN trn.SalesOrder AS so ON  so.MasterOrderItemId=moi.id
                                WHERE MM.CostingItemId IN (" + CostingItemIds + ") AND SO.Id IN (" + SalesOrderIds + ")", out DataSet dtRateFromCosting);

            string _id = GetPK("BOMMasterAttachmentWithItem");
            int Index = 0;
            #region SKU1SKU2
            dtSourceBOQ.DefaultView.RowFilter = "BOQCriteria='" + BOQCriteria.SKU1SKU2 + "'";
            DataTable dtTemp = dtSourceBOQ.DefaultView.ToTable();
            DataTable dtBOQ = dtTemp.Clone();

            if (dtTemp.Rows.Count > 0)
            {
                dtBOQ = dtTemp.AsEnumerable().GroupBy(x => new
                {
                    SalesOrderId = DBNull.Value,
                    DestinationId = DBNull.Value,
                    MasterOrderItemId = x["MasterOrderItemId"],
                    FGFirstCharacteristicsValueId = x["FGFirstCharacteristicsValueId"],
                    FGSecondCharacteristicsValueId = x["FGSecondCharacteristicsValueId"],
                    FGThirdCharacteristicsValueId = DBNull.Value,
                    CostingItemId = x["CostingItemId"],
                    MaterialMasterId = x["MaterialMasterId"],
                    ArticleId = x["ArticleId"],
                    UoMId = x["UoMId"],
                    CurrencyId = x["CurrencyId"],
                    VendorId = x["VendorId"],
                    BOQCriteria = x["BOQCriteria"],
                    POCriteria = x["POCriteria"],
                    Sequence = x["Sequence"]
                })
                                      .Select(x =>
                                      {
                                          DataRow row = dtTemp.NewRow();
                                          row["SalesOrderId"] = DBNull.Value; row["DestinationId"] = DBNull.Value; row["FGFirstCharacteristicsValueId"] = x.Key.FGFirstCharacteristicsValueId; row["FGSecondCharacteristicsValueId"] = x.Key.FGSecondCharacteristicsValueId; row["FGThirdCharacteristicsValueId"] = DBNull.Value; row["MasterOrderItemId"] = x.Key.MasterOrderItemId;
                                          row["CostingItemId"] = x.Key.CostingItemId; row["Sequence"] = x.Key.Sequence; row["MaterialMasterId"] = x.Key.MaterialMasterId; row["ArticleId"] = x.Key.ArticleId; row["UoMId"] = x.Key.UoMId; row["CurrencyId"] = x.Key.CurrencyId; row["VendorId"] = x.Key.VendorId; row["BOQCriteria"] = x.Key.BOQCriteria; row["POCriteria"] = x.Key.POCriteria;
                                          row["OrderQty"] = x.Sum(r => (decimal)r["OrderQty"]); row["BOMQty"] = x.Sum(r => (decimal)r["BOMQty"]); row["RequiredQty"] = x.Sum(r => (decimal)r["RequiredQty"]); row["BOMAmount"] = x.Sum(r => (decimal)r["BOMAmount"]);
                                          return row;
                                      }
                                      ).CopyToDataTable();


                for (int i = 0; i < dtBOQ.Rows.Count; i++)
                {
                    dsExistingBOQ.Tables[0].DefaultView.RowFilter = "CostingItemId='" + dtBOQ.Rows[i]["CostingItemId"].ToString() + "' AND isnull(FGFirstCharacteristicsValueId,'')='" + dtBOQ.Rows[i]["FGFirstCharacteristicsValueId"].ToString() + "' AND isnull(FGSecondCharacteristicsValueId,'')='" + dtBOQ.Rows[i]["FGSecondCharacteristicsValueId"].ToString() + "'";

                    bool IncompleteMaterial = false;
                    if (dtBOQ.Rows[i]["FGFirstCharacteristicsValueId"].ToString() == "" || dtBOQ.Rows[i]["FGSecondCharacteristicsValueId"].ToString() == "")
                        IncompleteMaterial = true;
                    if (dsExistingBOQ.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsExistingBOQ.Tables[0].NewRow();
                        CopyRow(dtBOQ.Rows[i], dr);
                        dsExistingBOQ.Tables[0].Rows.Add(dr);

                        dr["Id"] = _id + "-" + ++Index;
                        dr["CostingBOQMasterId"] = CostingBOQMasterId;
                        dr["IncompleteMaterial"] = IncompleteMaterial;
                        dr["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());
                    }
                    else
                    {
                        EditRow(dtBOQ.Rows[i], dsExistingBOQ.Tables[0].DefaultView[0].Row);
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["IncompleteMaterial"] = IncompleteMaterial;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["CostingBOQMasterId"] = CostingBOQMasterId;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());
                    }
                }
            }
            #endregion SKU1SKU2

            #region Destination
            dtSourceBOQ.DefaultView.RowFilter = "BOQCriteria='" + BOQCriteria.Destination + "'";
            dtTemp = dtSourceBOQ.DefaultView.ToTable();
            if (dtTemp.Rows.Count > 0)
            {
                dtBOQ = dtTemp.AsEnumerable().GroupBy(x => new
                {
                    SalesOrderId = DBNull.Value,
                    MasterOrderItemId = x["MasterOrderItemId"],
                    DestinationId = x["DestinationId"],
                    FGFirstCharacteristicsValueId = DBNull.Value,
                    FGSecondCharacteristicsValueId = DBNull.Value,
                    FGThirdCharacteristicsValueId = DBNull.Value,
                    CostingItemId = x["CostingItemId"],
                    MaterialMasterId = x["MaterialMasterId"],
                    ArticleId = x["ArticleId"],
                    UoMId = x["UoMId"],
                    CurrencyId = x["CurrencyId"],
                    VendorId = x["VendorId"],
                    BOQCriteria = x["BOQCriteria"],
                    POCriteria = x["POCriteria"],
                    Sequence = x["Sequence"]
                })
                                  .Select(x =>
                                  {
                                      DataRow row = dtTemp.NewRow();
                                      row["SalesOrderId"] = DBNull.Value; row["DestinationId"] = x.Key.DestinationId; row["FGFirstCharacteristicsValueId"] = DBNull.Value; row["FGSecondCharacteristicsValueId"] = DBNull.Value; row["FGThirdCharacteristicsValueId"] = DBNull.Value;
                                      row["MasterOrderItemId"] = x.Key.MasterOrderItemId;
                                      row["CostingItemId"] = x.Key.CostingItemId; row["Sequence"] = x.Key.Sequence; row["MaterialMasterId"] = x.Key.MaterialMasterId; row["ArticleId"] = x.Key.ArticleId; row["UoMId"] = x.Key.UoMId; row["CurrencyId"] = x.Key.CurrencyId; row["VendorId"] = x.Key.VendorId; row["BOQCriteria"] = x.Key.BOQCriteria; row["POCriteria"] = x.Key.POCriteria;
                                      row["OrderQty"] = x.Sum(r => (decimal)r["OrderQty"]); row["BOMQty"] = x.Sum(r => (decimal)r["BOMQty"]); row["RequiredQty"] = x.Sum(r => (decimal)r["RequiredQty"]); row["BOMAmount"] = x.Sum(r => (decimal)r["BOMAmount"]);
                                      return row;
                                  }
                                  ).CopyToDataTable();

                for (int i = 0; i < dtBOQ.Rows.Count; i++)
                {
                    dsExistingBOQ.Tables[0].DefaultView.RowFilter = "CostingItemId='" + dtBOQ.Rows[i]["CostingItemId"].ToString() + "' AND isnull(DestinationId,'')='" + dtBOQ.Rows[i]["DestinationId"].ToString() + "'";
                    if (dsExistingBOQ.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsExistingBOQ.Tables[0].NewRow();
                        CopyRow(dtBOQ.Rows[i], dr);
                        dsExistingBOQ.Tables[0].Rows.Add(dr);
                        dr["Id"] = _id + "-" + ++Index;
                        dr["IncompleteMaterial"] = false;
                        dr["CostingBOQMasterId"] = CostingBOQMasterId;
                        dr["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());
                    }
                    else
                    {
                        EditRow(dtBOQ.Rows[i], dsExistingBOQ.Tables[0].DefaultView[0].Row);
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["IncompleteMaterial"] = false;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["CostingBOQMasterId"] = CostingBOQMasterId;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());
                    }
                }
            }
            #endregion Destination

            #region General
            dtSourceBOQ.DefaultView.RowFilter = "BOQCriteria='" + BOQCriteria.General + "' OR isnull(BOQCriteria,'')=''";
            dtTemp = dtSourceBOQ.DefaultView.ToTable();
            if (dtTemp.Rows.Count > 0)
            {
                dtBOQ = dtTemp.AsEnumerable().GroupBy(x => new
                {
                    MasterOrderItemId = x["MasterOrderItemId"],
                    SalesOrderId = DBNull.Value,
                    DestinationId = DBNull.Value,
                    FGFirstCharacteristicsValueId = DBNull.Value,
                    FGSecondCharacteristicsValueId = DBNull.Value,
                    FGThirdCharacteristicsValueId = DBNull.Value,
                    CostingItemId = x["CostingItemId"],
                    MaterialMasterId = x["MaterialMasterId"],
                    ArticleId = x["ArticleId"],
                    UoMId = x["UoMId"],
                    CurrencyId = x["CurrencyId"],
                    VendorId = x["VendorId"],
                    BOQCriteria = x["BOQCriteria"],
                    POCriteria = x["POCriteria"],
                    Sequence = x["Sequence"]
                })
                                  .Select(x =>
                                  {
                                      DataRow row = dtTemp.NewRow();
                                      row["SalesOrderId"] = DBNull.Value; row["DestinationId"] = DBNull.Value; row["FGFirstCharacteristicsValueId"] = DBNull.Value; row["FGSecondCharacteristicsValueId"] = DBNull.Value; row["FGThirdCharacteristicsValueId"] = DBNull.Value;
                                      row["MasterOrderItemId"] = x.Key.MasterOrderItemId;
                                      row["CostingItemId"] = x.Key.CostingItemId; row["Sequence"] = x.Key.Sequence; row["MaterialMasterId"] = x.Key.MaterialMasterId; row["ArticleId"] = x.Key.ArticleId; row["UoMId"] = x.Key.UoMId; row["CurrencyId"] = x.Key.CurrencyId; row["VendorId"] = x.Key.VendorId; row["BOQCriteria"] = x.Key.BOQCriteria; row["POCriteria"] = x.Key.POCriteria;
                                      row["OrderQty"] = x.Sum(r => (decimal)r["OrderQty"]); row["BOMQty"] = x.Sum(r => (decimal)r["BOMQty"]); row["RequiredQty"] = x.Sum(r => (decimal)r["RequiredQty"]); row["BOMAmount"] = x.Sum(r => (decimal)r["BOMAmount"]);
                                      return row;
                                  }
                                  ).CopyToDataTable();

                for (int i = 0; i < dtBOQ.Rows.Count; i++)
                {
                    dsExistingBOQ.Tables[0].DefaultView.RowFilter = "CostingItemId='" + dtBOQ.Rows[i]["CostingItemId"].ToString() + "'";
                    if (dsExistingBOQ.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsExistingBOQ.Tables[0].NewRow();
                        CopyRow(dtBOQ.Rows[i], dr);
                        dsExistingBOQ.Tables[0].Rows.Add(dr);
                        dr["Id"] = _id + "-" + ++Index;
                        dr["CostingBOQMasterId"] = false;
                        dr["CostingBOQMasterId"] = CostingBOQMasterId;
                        dr["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());
                    }
                    else
                    {
                        EditRow(dtBOQ.Rows[i], dsExistingBOQ.Tables[0].DefaultView[0].Row);
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["IncompleteMaterial"] = false;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["CostingBOQMasterId"] = CostingBOQMasterId;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());
                    }
                }
            }
            #endregion General

            #region SO
            dtSourceBOQ.DefaultView.RowFilter = "BOQCriteria='" + BOQCriteria.SO + "'";
            dtTemp = dtSourceBOQ.DefaultView.ToTable();
            if (dtTemp.Rows.Count > 0)
            {
                dtBOQ = dtTemp.AsEnumerable().GroupBy(x => new
                {
                    MasterOrderItemId = x["MasterOrderItemId"],
                    SalesOrderId = x["SalesOrderId"],
                    DestinationId = DBNull.Value,
                    FGFirstCharacteristicsValueId = DBNull.Value,
                    FGSecondCharacteristicsValueId = DBNull.Value,
                    FGThirdCharacteristicsValueId = DBNull.Value,
                    CostingItemId = x["CostingItemId"],
                    MaterialMasterId = x["MaterialMasterId"],
                    ArticleId = x["ArticleId"],
                    UoMId = x["UoMId"],
                    CurrencyId = x["CurrencyId"],
                    VendorId = x["VendorId"],
                    BOQCriteria = x["BOQCriteria"],
                    POCriteria = x["POCriteria"],
                    Sequence = x["Sequence"]
                })
                                  .Select(x =>
                                  {
                                      DataRow row = dtTemp.NewRow();
                                      row["SalesOrderId"] = x.Key.SalesOrderId; row["DestinationId"] = DBNull.Value; row["FGFirstCharacteristicsValueId"] = DBNull.Value; row["FGSecondCharacteristicsValueId"] = DBNull.Value; row["FGThirdCharacteristicsValueId"] = DBNull.Value; row["MasterOrderItemId"] = x.Key.MasterOrderItemId;
                                      row["CostingItemId"] = x.Key.CostingItemId; row["Sequence"] = x.Key.Sequence; row["MaterialMasterId"] = x.Key.MaterialMasterId; row["ArticleId"] = x.Key.ArticleId; row["UoMId"] = x.Key.UoMId; row["CurrencyId"] = x.Key.CurrencyId; row["VendorId"] = x.Key.VendorId; row["BOQCriteria"] = x.Key.BOQCriteria; row["POCriteria"] = x.Key.POCriteria;
                                      row["OrderQty"] = x.Sum(r => (decimal)r["OrderQty"]); row["BOMQty"] = x.Sum(r => (decimal)r["BOMQty"]); row["RequiredQty"] = x.Sum(r => (decimal)r["RequiredQty"]); row["BOMAmount"] = x.Sum(r => (decimal)r["BOMAmount"]);
                                      return row;
                                  }
                                  ).CopyToDataTable();


                for (int i = 0; i < dtBOQ.Rows.Count; i++)
                {
                    dsExistingBOQ.Tables[0].DefaultView.RowFilter = "CostingItemId='" + dtBOQ.Rows[i]["CostingItemId"].ToString() + "' AND isnull(SalesOrderId,'')='" + dtBOQ.Rows[i]["SalesOrderId"].ToString() + "'";
                    if (dsExistingBOQ.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsExistingBOQ.Tables[0].NewRow();
                        CopyRow(dtBOQ.Rows[i], dr);
                        dsExistingBOQ.Tables[0].Rows.Add(dr);
                        dr["Id"] = _id + "-" + ++Index;
                        dr["IncompleteMaterial"] = false;
                        dr["CostingBOQMasterId"] = CostingBOQMasterId;
                        dr["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());

                    }
                    else
                    {
                        EditRow(dtBOQ.Rows[i], dsExistingBOQ.Tables[0].DefaultView[0].Row);
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["IncompleteMaterial"] = false;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["CostingBOQMasterId"] = CostingBOQMasterId;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());
                    }
                }
            }
            #endregion SO

            #region SKU1
            dtSourceBOQ.DefaultView.RowFilter = "BOQCriteria='" + BOQCriteria.SKU1 + "'";
            dtTemp = dtSourceBOQ.DefaultView.ToTable();
            if (dtTemp.Rows.Count > 0)
            {
                dtBOQ = dtTemp.AsEnumerable().GroupBy(x => new
                {
                    MasterOrderItemId = x["MasterOrderItemId"],
                    SalesOrderId = DBNull.Value,
                    DestinationId = DBNull.Value,
                    FGFirstCharacteristicsValueId = x["FGFirstCharacteristicsValueId"],
                    FGSecondCharacteristicsValueId = DBNull.Value,
                    FGThirdCharacteristicsValueId = DBNull.Value,
                    CostingItemId = x["CostingItemId"],
                    MaterialMasterId = x["MaterialMasterId"],
                    ArticleId = x["ArticleId"],
                    UoMId = x["UoMId"],
                    CurrencyId = x["CurrencyId"],
                    VendorId = x["VendorId"],
                    BOQCriteria = x["BOQCriteria"],
                    POCriteria = x["POCriteria"],
                    Sequence = x["Sequence"]
                })
                                  .Select(x =>
                                  {
                                      DataRow row = dtTemp.NewRow();
                                      row["SalesOrderId"] = DBNull.Value; row["DestinationId"] = DBNull.Value; row["FGFirstCharacteristicsValueId"] = x.Key.FGFirstCharacteristicsValueId; row["FGSecondCharacteristicsValueId"] = DBNull.Value; row["FGThirdCharacteristicsValueId"] = DBNull.Value; row["MasterOrderItemId"] = x.Key.MasterOrderItemId;
                                      row["CostingItemId"] = x.Key.CostingItemId; row["Sequence"] = x.Key.Sequence; row["MaterialMasterId"] = x.Key.MaterialMasterId; row["ArticleId"] = x.Key.ArticleId; row["UoMId"] = x.Key.UoMId; row["CurrencyId"] = x.Key.CurrencyId; row["VendorId"] = x.Key.VendorId; row["BOQCriteria"] = x.Key.BOQCriteria; row["POCriteria"] = x.Key.POCriteria;
                                      row["OrderQty"] = x.Sum(r => (decimal)r["OrderQty"]); row["BOMQty"] = x.Sum(r => (decimal)r["BOMQty"]); row["RequiredQty"] = x.Sum(r => (decimal)r["RequiredQty"]); row["BOMAmount"] = x.Sum(r => (decimal)r["BOMAmount"]);
                                      return row;
                                  }
                                  ).CopyToDataTable();


                for (int i = 0; i < dtBOQ.Rows.Count; i++)
                {
                    dsExistingBOQ.Tables[0].DefaultView.RowFilter = "CostingItemId='" + dtBOQ.Rows[i]["CostingItemId"].ToString() + "' AND isnull(FGFirstCharacteristicsValueId,'')='" + dtBOQ.Rows[i]["FGFirstCharacteristicsValueId"].ToString() + "'";
                    bool IncompleteMaterial = false;
                    if (dtBOQ.Rows[i]["FGFirstCharacteristicsValueId"].ToString() == "")
                        IncompleteMaterial = true;
                    if (dsExistingBOQ.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsExistingBOQ.Tables[0].NewRow();
                        CopyRow(dtBOQ.Rows[i], dr);
                        dsExistingBOQ.Tables[0].Rows.Add(dr);
                        dr["Id"] = _id + "-" + ++Index;
                        dr["CostingBOQMasterId"] = CostingBOQMasterId;
                        dr["IncompleteMaterial"] = IncompleteMaterial;
                        dr["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());
                    }
                    else
                    {
                        EditRow(dtBOQ.Rows[i], dsExistingBOQ.Tables[0].DefaultView[0].Row);
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["IncompleteMaterial"] = IncompleteMaterial;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["CostingBOQMasterId"] = CostingBOQMasterId;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());
                    }
                }
            }
            #endregion SKU1

            #region SKU2
            dtSourceBOQ.DefaultView.RowFilter = "BOQCriteria='" + BOQCriteria.SKU2 + "'";
            dtTemp = dtSourceBOQ.DefaultView.ToTable();
            if (dtTemp.Rows.Count > 0)
            {
                dtBOQ = dtTemp.AsEnumerable().GroupBy(x => new
                {
                    MasterOrderItemId = x["MasterOrderItemId"],
                    SalesOrderId = DBNull.Value,
                    DestinationId = DBNull.Value,
                    FGFirstCharacteristicsValueId = DBNull.Value,
                    FGSecondCharacteristicsValueId = x["FGSecondCharacteristicsValueId"],
                    FGThirdCharacteristicsValueId = DBNull.Value,
                    CostingItemId = x["CostingItemId"],
                    MaterialMasterId = x["MaterialMasterId"],
                    ArticleId = x["ArticleId"],
                    UoMId = x["UoMId"],
                    CurrencyId = x["CurrencyId"],
                    VendorId = x["VendorId"],
                    BOQCriteria = x["BOQCriteria"],
                    POCriteria = x["POCriteria"],
                    Sequence = x["Sequence"]
                })
                                  .Select(x =>
                                  {
                                      DataRow row = dtTemp.NewRow();
                                      row["SalesOrderId"] = DBNull.Value; row["DestinationId"] = DBNull.Value; row["FGFirstCharacteristicsValueId"] = DBNull.Value; row["FGSecondCharacteristicsValueId"] = x.Key.FGSecondCharacteristicsValueId; row["FGThirdCharacteristicsValueId"] = DBNull.Value;
                                      row["MasterOrderItemId"] = x.Key.MasterOrderItemId;
                                      row["CostingItemId"] = x.Key.CostingItemId; row["Sequence"] = x.Key.Sequence; row["MaterialMasterId"] = x.Key.MaterialMasterId; row["ArticleId"] = x.Key.ArticleId; row["UoMId"] = x.Key.UoMId; row["CurrencyId"] = x.Key.CurrencyId; row["VendorId"] = x.Key.VendorId; row["BOQCriteria"] = x.Key.BOQCriteria; row["POCriteria"] = x.Key.POCriteria;
                                      row["OrderQty"] = x.Sum(r => (decimal)r["OrderQty"]); row["BOMQty"] = x.Sum(r => (decimal)r["BOMQty"]); row["RequiredQty"] = x.Sum(r => (decimal)r["RequiredQty"]); row["BOMAmount"] = x.Sum(r => (decimal)r["BOMAmount"]);
                                      return row;
                                  }
                                  ).CopyToDataTable();

                for (int i = 0; i < dtBOQ.Rows.Count; i++)
                {
                    dsExistingBOQ.Tables[0].DefaultView.RowFilter = "CostingItemId='" + dtBOQ.Rows[i]["CostingItemId"].ToString() + "' AND isnull(FGSecondCharacteristicsValueId,'')='" + dtBOQ.Rows[i]["FGSecondCharacteristicsValueId"].ToString() + "'";
                    bool IncompleteMaterial = false;
                    if (dtBOQ.Rows[i]["FGSecondCharacteristicsValueId"].ToString() == "")
                        IncompleteMaterial = true;
                    if (dsExistingBOQ.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsExistingBOQ.Tables[0].NewRow();
                        CopyRow(dtBOQ.Rows[i], dr);
                        dsExistingBOQ.Tables[0].Rows.Add(dr);
                        dr["Id"] = _id + "-" + ++Index;
                        dr["CostingBOQMasterId"] = CostingBOQMasterId;
                        dr["IncompleteMaterial"] = IncompleteMaterial;
                        dr["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());
                    }
                    else
                    {
                        EditRow(dtBOQ.Rows[i], dsExistingBOQ.Tables[0].DefaultView[0].Row);
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["IncompleteMaterial"] = IncompleteMaterial;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["CostingBOQMasterId"] = CostingBOQMasterId;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());

                    }
                }
            }
            #endregion SKU2

            #region SODestination
            dtSourceBOQ.DefaultView.RowFilter = "BOQCriteria='" + BOQCriteria.SODestination + "'";
            dtTemp = dtSourceBOQ.DefaultView.ToTable();
            if (dtTemp.Rows.Count > 0)
            {
                dtBOQ = dtTemp.AsEnumerable().GroupBy(x => new
                {
                    MasterOrderItemId = x["MasterOrderItemId"],
                    SalesOrderId = x["SalesOrderId"],
                    DestinationId = x["DestinationId"],
                    FGFirstCharacteristicsValueId = DBNull.Value,
                    FGSecondCharacteristicsValueId = DBNull.Value,
                    FGThirdCharacteristicsValueId = DBNull.Value,
                    CostingItemId = x["CostingItemId"],
                    MaterialMasterId = x["MaterialMasterId"],
                    ArticleId = x["ArticleId"],
                    UoMId = x["UoMId"],
                    CurrencyId = x["CurrencyId"],
                    VendorId = x["VendorId"],
                    BOQCriteria = x["BOQCriteria"],
                    POCriteria = x["POCriteria"],
                    Sequence = x["Sequence"]
                })
                                  .Select(x =>
                                  {
                                      DataRow row = dtTemp.NewRow();
                                      row["SalesOrderId"] = x.Key.SalesOrderId; row["DestinationId"] = x.Key.DestinationId; row["FGFirstCharacteristicsValueId"] = DBNull.Value; row["FGSecondCharacteristicsValueId"] = DBNull.Value; row["FGThirdCharacteristicsValueId"] = DBNull.Value; row["MasterOrderItemId"] = x.Key.MasterOrderItemId;
                                      row["CostingItemId"] = x.Key.CostingItemId; row["Sequence"] = x.Key.Sequence; row["MaterialMasterId"] = x.Key.MaterialMasterId; row["ArticleId"] = x.Key.ArticleId; row["UoMId"] = x.Key.UoMId; row["CurrencyId"] = x.Key.CurrencyId; row["VendorId"] = x.Key.VendorId; row["BOQCriteria"] = x.Key.BOQCriteria; row["POCriteria"] = x.Key.POCriteria;
                                      row["OrderQty"] = x.Sum(r => (decimal)r["OrderQty"]); row["BOMQty"] = x.Sum(r => (decimal)r["BOMQty"]); row["RequiredQty"] = x.Sum(r => (decimal)r["RequiredQty"]); row["BOMAmount"] = x.Sum(r => (decimal)r["BOMAmount"]);
                                      return row;
                                  }
                                  ).CopyToDataTable();


                for (int i = 0; i < dtBOQ.Rows.Count; i++)
                {
                    dsExistingBOQ.Tables[0].DefaultView.RowFilter = "CostingItemId='" + dtBOQ.Rows[i]["CostingItemId"].ToString() + "' AND isnull(SalesOrderId,'')='" + dtBOQ.Rows[i]["SalesOrderId"].ToString() + "' AND isnull(DestinationId,'')='" + dtBOQ.Rows[i]["DestinationId"].ToString() + "'";
                    if (dsExistingBOQ.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsExistingBOQ.Tables[0].NewRow();
                        CopyRow(dtBOQ.Rows[i], dr);
                        dsExistingBOQ.Tables[0].Rows.Add(dr);
                        dr["Id"] = _id + "-" + ++Index;
                        dr["IncompleteMaterial"] = false;
                        dr["CostingBOQMasterId"] = CostingBOQMasterId;
                        dr["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());

                    }
                    else
                    {
                        EditRow(dtBOQ.Rows[i], dsExistingBOQ.Tables[0].DefaultView[0].Row);
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["IncompleteMaterial"] = false;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["CostingBOQMasterId"] = CostingBOQMasterId;
                        dsExistingBOQ.Tables[0].DefaultView[0].Row["Rate"] = GetCostingRate(dtRateFromCosting, dtBOQ.Rows[i]["CostingItemId"].ToString());

                    }
                }
            }
            #endregion SODestination


            return dsExistingBOQ;
        }

        public double GetCostingRate(DataSet dsRate, string CostingItemId)
        {

            dsRate.Tables[0].DefaultView.RowFilter = "CostingItemId='" + CostingItemId + @"'";
            if (dsRate.Tables[0].DefaultView.Count > 0)
                return clsStaticInfo.dbl(dsRate.Tables[0].DefaultView[0]["GrossAmount"].ToString());

            return 0;
        }

        private DataTable BOQReportQuery(string CostingBOQMasterId)
        {


            string MainQtuery = @"SELECT distinct boq.Id,boq.CostingBOQMasterId,boq.CostingItemId,so.Id SalesOrderId,d.UserName AS Destination,cv1.UserName AS SKU1,cv2.UserName AS SKU2
                                    ,BOQ.IncompleteMaterial,cb.AddedBy AS PreparedBy,FORMAT(cb.AddedDate,'dd-MMM-yyyy') AS CostingDate,
                                     ci.Sequence,ci.UserName AS ItemDesc,mm.UserName AS Material,mma.StandardName AS Article,BOQ.ItemRefNo,p.UserName AS Vendor,
                                    mm.Code AS MaterialCode,mma.Code AS ArticleCode,emp.EmployeeName AS ResponsiblePerson
									--,so.Qty SOQty
									,isnull(boq.OrderQty,0) SOQty
                                   ,isnull(boq.BOMQty,0) BOMQty,isnull(so.Qty*OPCD.GrossConsumption,0) RequiredQty,isnull(OPCD.Rate,0) UnitRate,uom.UserName AS UOM
								   --,boq.Rate*BOQ.RequiredQty AS PlanAmount,boq.Rate*BOQ.BOMQty AS BOMAmount
								   ,isnull(boq.BOMQty*OPCD.Rate,0) PlanAmount,isnull((so.Qty*OPCD.GrossConsumption)*OPCD.Rate,0) AS RequiredAmount
								   ,BOQ.BOQCriteria,c.Code AS Currency
                                    ,boq.POCriteria,isnull(OPCD.GrossConsumption,0) GrossConsumption
                                    FROM BOQ boq
                                    LEFT JOIN CostingBOQMaster AS cb ON cb.Id=boq.CostingBOQMasterId
                                    --LEFT JOIN CostingBOQ AS CBOQ ON CBOQ.CostingBOQMasterId=cb.Id
                                    LEFT JOIN hkp.Party AS p ON p.Id=boq.VendorId
                                    LEFT JOIN employeeinformation emp ON emp.SystemId=cb.EmployeeSystemId
									LEFT JOIN (Select DISTINCT SalesOrderId,CostingBOQMasterId,CostingItemId,OrderProcurementCostingDirectMaterialId from CostingBOQItems )CBI on CBI.CostingBOQMasterId=boq.CostingBOQMasterId AND CBI.CostingItemId=boq.CostingItemId --AND so.Id=CBI.SalesOrderId
									LEFT JOIN OrderProcurementCostingDirectMaterial OPCD on OPCD.Id=CBI.OrderProcurementCostingDirectMaterialId AND CBI.CostingItemId=OPCD.CostingItemId AND boq.CostingItemId=OPCD.CostingItemId
                                    LEFT JOIN trn.SalesOrder AS so ON so.Id=CBI.SalesOrderId
                                    LEFT JOIN mst.Destination AS d ON d.Id=so.DestinationId
                                    LEFT JOIN hkp.CostingItem AS ci ON ci.Id=boq.CostingItemId
                                    LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=boq.MaterialMasterId
                                    LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=boq.ArticleId
                                    LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=boq.UoMId
                                    LEFT JOIN scs.Currency AS c ON c.Id=boq.CurrencyId
                                    LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=boq.FGFirstCharacteristicsValueId
                                    LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=boq.FGSecondCharacteristicsValueId

                                    WHERE BOQ.CostingBOQMasterId='" + CostingBOQMasterId + @"' --AND ISNULL(BOQ.BOMQty,0)>0
                                    
                                    ORDER BY ci.Sequence";

            string sql = @"SELECT distinct BOQ.Id, boq.CostingBOQMasterId, convert(bit,isnull(mm.WithSKU,0)) AS WithSKU
                            ,cv1.UserName AS SKU1,cv2.UserName AS SKU2,BOQ.IncompleteMaterial,cb.AddedBy AS PreparedBy,FORMAT(cb.AddedDate,'dd-MMM-yyyy') AS CostingDate,
                                    ci.Sequence,ci.UserName AS ItemDesc,mm.UserName AS Material,mma.StandardName AS Article,BOQ.ItemRefNo,p.UserName AS Vendor,
                                    mm.Code AS MaterialCode,mma.Code AS ArticleCode,emp.EmployeeName AS ResponsiblePerson
                                    ,isnull(boq.OrderQty,0) SOQty,boq.BOMQty
									,isnull(SCBI.SQty*OPCD.GrossConsumption,0) RequiredQty
									,isnull(OPCD.Rate,0) UnitRate,boq.BOMQty-boq.RequiredQty AS BalanceToPurchase,uom.UserName AS UOM,boq.rate*boq.RequiredQty AS BOMAmount,boq.Rate,BOQ.BOQCriteria,c.Code AS Currency
									,OPCD.GrossConsumption,isnull(boq.BOMQty*OPCD.Rate,0) PlanAmount,isnull((SCBI.SQty*OPCD.GrossConsumption)*OPCD.Rate,0) AS RequiredAmount
                                    ,BOQ.RMDescription,BOQ.RMCustomerSpec,BOQ.RMVendorSpec,BOQ.SKUDesc,ci.Id CostingItemId
  --boq.Rate*BOQ.RequiredQty AS PlanAmount,boq.Rate*BOQ.BOMQty AS BOMAmount 
  ,BOQ.OwnReferenceNo,BOQ.Remark,
  SKUDescConcat= ISNULL(BOQ.SKUDesc,CONCAT(boq.SalesOrderId,' ',d.UserName,' ',cv1.UserName,' ',cv2.UserName)),
 CriteriaDetail= ISNULL(BOQ.SKUDesc,CONCAT(boq.SalesOrderId,' ',d.UserName,' ',cv1.UserName,' ',cv2.UserName)),
 BOQ.FileName,BOQ.FileOriginalName,BOQ.Extension,BOQ.POCriteria
,SalesOrderId=STUFF((select distinct ','+XSO.Id 
                                         from trn.SalesOrder XSO
										 LEFT JOIN dbo.CostingBOQMaster CBM ON CBM.Id=XSO.CostingBOQMasterId
							             where CBM.Id=boq.CostingBOQMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
,Destination=STUFF((select distinct ','+d.UserName 
                                         from mst.Destination D
										 LEFT JOIN trn.SalesOrder XSO ON XSO.DestinationId=D.Id
										 LEFT JOIN dbo.CostingBOQMaster CBM ON CBM.Id=XSO.CostingBOQMasterId
							             where CBM.Id=boq.CostingBOQMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                    FROM BOQ
                                    LEFT JOIN CostingBOQMaster AS cb ON cb.Id=boq.CostingBOQMasterId
                                    LEFT JOIN hkp.Party AS p ON p.Id=boq.VendorId
                                    LEFT JOIN employeeinformation emp ON emp.SystemId=cb.EmployeeSystemId
									LEFT JOIN (Select DISTINCT SalesOrderId,CostingBOQMasterId,CostingItemId,OrderProcurementCostingDirectMaterialId from CostingBOQItems )CBI on CBI.CostingBOQMasterId=boq.CostingBOQMasterId AND CBI.CostingItemId=boq.CostingItemId 
									LEFT JOIN OrderProcurementCostingDirectMaterial OPCD on OPCD.Id=CBI.OrderProcurementCostingDirectMaterialId AND CBI.CostingItemId=OPCD.CostingItemId AND boq.CostingItemId=OPCD.CostingItemId
									LEFT JOIN (Select SUM(S.Qty)SQty, C.CostingBOQMasterId,C.CostingItemId from CostingBOQItems C
									JOIN TRN.SalesOrder S ON S.Id=C.SalesOrderId GROUP BY C.CostingBOQMasterId,C.CostingItemId
									)SCBI on SCBI.CostingBOQMasterId=boq.CostingBOQMasterId AND SCBI.CostingItemId=boq.CostingItemId 
                                    LEFT JOIN trn.SalesOrder AS so ON so.Id=boq.SalesOrderId
                                    LEFT JOIN mst.Destination AS d ON d.Id=so.DestinationId
                                    LEFT JOIN hkp.CostingItem AS ci ON ci.Id=boq.CostingItemId
                                    LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=boq.MaterialMasterId
                                    LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=boq.ArticleId
                                    LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=boq.UoMId
                                    LEFT JOIN scs.Currency AS c ON c.Id=boq.CurrencyId

                                    LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=boq.FGFirstCharacteristicsValueId
                                    LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=boq.FGSecondCharacteristicsValueId
                                    WHERE BOQ.CostingBOQMasterId='" + CostingBOQMasterId + @"'                                     
                                    ORDER BY ci.Sequence";

            DataTable dtData = _sqlRepository.GetDataTable(sql);

            return dtData;

        }

        public void ReportXls(string CostingBOQMasterId)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                var report = new ReportUtility();
                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "BOQ Report";

                var headerData = getBOQStatusReportHeaderSql(CostingBOQMasterId);
                DataTable dtEmployeeData = BOQReportQuery(CostingBOQMasterId);

                int ROW = 6;
                int COL = 1;


                #region Header

                report.SetMasterHeaderText(ref sheet, ROW, 1, "BOM Id");
                //sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, headerData["BOMId"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                //sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "BOM Creation Date");
                sheet[ROW, 4].ColumnWidth = 20;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, headerData["BOMCreationDate"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                //sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "Customer");
                //sheet[ROW, 1].ColumnWidth = 25;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, headerData["Customer"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                //sheet[ROW, 2].ColumnWidth = 30;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "Buyer");
                //sheet[ROW, 4].ColumnWidth = 25;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, headerData["BuyerName"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                sheet[ROW, 5].ColumnWidth = 30;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "Master Order Id");
                sheet[ROW, 1].ColumnWidth = 16;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, headerData["MasterOrderId"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "Buyer Ref No");
                //sheet[ROW, 4].ColumnWidth = 20;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, headerData["BuyerItemNo"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                sheet[ROW, 5].ColumnWidth = 20;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "Product");
                //sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, headerData["ArticleName"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "Contract No");
                //sheet[ROW, 4].ColumnWidth = 25;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, headerData["OwnOrderNo"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                //sheet[ROW, 5].ColumnWidth = 25;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "SO Id");
                //sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, headerData["SalesOrderId"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "PO No");
                //sheet[ROW, 4].ColumnWidth = 25;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, headerData["PONumber"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                //sheet[ROW, 5].ColumnWidth = 25;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "SO Quantity");
                //sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, headerData["SOQty"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                ROW++;
                ROW++;
                #endregion

                #region Middle Data

                sheet[ROW, COL].Text = "Responsible Person:" + dtEmployeeData.Rows[0]["ResponsiblePerson"].ToString();
                sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
                sheet.Range[ROW, 1, ROW, 19].Merge();
                ROW++;

                sheet[ROW, COL].Text = "Sl.No";
                //sheet[ROW, COL].ColumnWidth = 13;
                int colSlNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Item Ref#";
                sheet[ROW, COL].ColumnWidth = 15;
                int colItemRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Item";
                sheet[ROW, COL].ColumnWidth = 20;
                int colItemDesc = COL;
                COL++;
                sheet[ROW, COL].Text = "Criteria";
                sheet[ROW, COL].ColumnWidth = 20;
                int colBOQCriteria = COL;
                COL++;
                
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 20;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 20;
                int colArticle = COL;
                COL++;
                
                sheet[ROW, COL].Text = "Vendor";
                sheet[ROW, COL].ColumnWidth = 25;
                int colVendor = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Id";
                sheet[ROW, COL].ColumnWidth = 15;
                int colSalesOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Destination";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDestination = COL;
                COL++;
                sheet[ROW, COL].Text = "SKU1";
                sheet[ROW, COL].ColumnWidth = 12;
                int colSKU1 = COL;
                COL++;
                sheet[ROW, COL].Text = "SKU2";
                sheet[ROW, COL].ColumnWidth = 12;
                int colSKU2 = COL;
                COL++;
                sheet[ROW, COL].Text = "PO Criteria";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPOCriteria = COL;
                COL++;

                sheet[ROW, COL].Text = "Gross Consumption";
                sheet[ROW, COL].ColumnWidth = 18;
                int colGrossConsumption = COL;
                COL++;

                sheet[ROW, COL].Text = "UOM";
                //sheet[ROW, COL].ColumnWidth = 8;
                int colUOM = COL;
                COL++;

                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].ColumnWidth = 18; 
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSOQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Plan Qty";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colBOMQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Required Qty";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 12;
                int colRequiredQty = COL;

                COL++;
                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 15;
                int colCurrency = COL;
                COL++;

                sheet[ROW, COL].Text = "Unit Rate";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 18;
                int colUnitRate = COL;
                COL++;
                
                sheet[ROW, COL].Text = "Plan Amount";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colPlanAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Reqired Amount";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colBOMAmount = COL;
                

                #endregion



                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                ROW++;
                int StartRow = ROW; //row 20
                for (int i = 0; i < dtEmployeeData.Rows.Count; i++)
                {


                    sheet[ROW, colSlNo].Number = (i + 1);

                    sheet[ROW, colSalesOrderId].Text = dtEmployeeData.Rows[i]["SalesOrderId"].ToString();
                    sheet[ROW, colDestination].Text = dtEmployeeData.Rows[i]["Destination"].ToString();
                    sheet[ROW, colSKU1].Text = dtEmployeeData.Rows[i]["SKU1"].ToString();
                    sheet[ROW, colSKU2].Text = dtEmployeeData.Rows[i]["SKU2"].ToString();
                    sheet[ROW, colItemDesc].Text = dtEmployeeData.Rows[i]["ItemDesc"].ToString();
                    sheet[ROW, colMaterial].Text = dtEmployeeData.Rows[i]["Material"].ToString();
                    sheet[ROW, colArticle].Text = dtEmployeeData.Rows[i]["Article"].ToString();
                    sheet[ROW, colVendor].Text = dtEmployeeData.Rows[i]["Vendor"].ToString();
                    sheet[ROW, colUOM].Text = dtEmployeeData.Rows[i]["UOM"].ToString();
                    sheet[ROW, colSOQty].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["SOQty"].ToString());
                    sheet[ROW, colBOQCriteria].Text = dtEmployeeData.Rows[i]["BOQCriteria"].ToString();
                    sheet[ROW, colItemRefNo].Text = dtEmployeeData.Rows[i]["ItemRefNo"].ToString();
                    sheet[ROW, colPOCriteria].Text = dtEmployeeData.Rows[i]["POCriteria"].ToString();

                    sheet[ROW, colGrossConsumption].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["GrossConsumption"].ToString());
                    sheet[ROW, colBOMQty].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["BOMQty"].ToString());
                    sheet[ROW, colRequiredQty].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["RequiredQty"].ToString());
                    sheet[ROW, colCurrency].Text = dtEmployeeData.Rows[i]["Currency"].ToString();
                    sheet[ROW, colUnitRate].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["UnitRate"].ToString());
                    sheet[ROW, colBOMAmount].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["RequiredAmount"].ToString());
                    sheet[ROW, colPlanAmount].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["PlanAmount"].ToString());


                    if (bplib.clsWebLib.GetBoolData(dtEmployeeData.Rows[i]["IncompleteMaterial"].ToString()))
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Red;

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                    ROW++;

                }

                sheet.IsGridLinesVisible = false;

                //sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 11f;

                //Total Start
                var endRow = ROW++;

                sheet.Range[endRow, colSlNo].Text = "Total";
                sheet.Range[endRow, colSlNo, endRow, colCurrency].Merge();
                sheet.Range[endRow, colSlNo].CellStyle.Font.Bold = true;
                //sheet.Range[endRow, colSlNo].CellStyle.Font.Size = 11;
                
                //sheet.Range[endRow, colBOMAmount].Number = clsStaticInfo.dbl(dtEmployeeData.Compute("avg(RequiredAmount)", null));
                //sheet.Range[endRow, colBOMAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[endRow, colBOMAmount].CellStyle.Font.Bold = true;
                //sheet.Range[endRow, colBOMAmount, endRow, colBOMAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet.Range[endRow, colBOMAmount, endRow, colBOMAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[endRow, colPlanAmount].Number = clsStaticInfo.dbl(dtEmployeeData.Compute("SUM(PlanAmount)", null));
                sheet.Range[endRow, colPlanAmount].NumberFormat = clsStaticInfo.NumberFormat(2); 
                sheet.Range[endRow, colPlanAmount].CellStyle.Font.Bold = true;
                sheet.Range[endRow, colPlanAmount, endRow, colPlanAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, colPlanAmount, endRow, colPlanAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[endRow, colPlanAmount].CellStyle.Font.Bold = true;

                sheet.Range[endRow, 1, endRow, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[endRow, 1, endRow, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                endRow++;
                endRow++;

                //Total End
                sheet["A" + StartRow.ToString()].FreezePanes();

                sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colSlNo, ROW, colSlNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;


                sheet.Range[StartRow, colBOMAmount, ROW, colBOMAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colBOMQty, ROW, colBOMQty].NumberFormat = clsStaticInfo.NumberFormat(2);

                //string HeaderCaption = string.Format("BOM Report (#{0}),Prepared By:{1}, BOM Creation Date:{2}",
                //    CostingBOQMasterId
                //     , dtEmployeeData.Rows[0]["PreparedBy"].ToString()
                //     , dtEmployeeData.Rows[0]["CostingDate"].ToString());3

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol,null, identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;


                ROW += 3;


                sheet[ROW, 1].Text = "BOM SUMMARY:";
                sheet.Range[ROW, 1].CellStyle.Font.Size = 15;
                sheet.Range[ROW, 1, ROW, 19].Merge();
                ROW++;


                COL = 1;
                sheet[ROW, COL].Text = "Item";
                //int colItem = COL;
                COL++;

                sheet[ROW, COL].Text = "Article";
                //int colArticle = COL;
                COL++;

                sheet[ROW, COL].Text = "UOM";
                //int colUOMs = COL;
                COL++;

                sheet[ROW, COL].Text = "Gross Consumption";
                //int colGrossConsumption = COL;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                COL++;

                sheet[ROW, COL].Text = "Required Quantity";
                //int colRequiredQuantity = COL;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                COL++;

                sheet[ROW, COL].Text = "Plan Quantity";
                //int colPlanQuantity = COL;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                //int colCurrencys = COL;
                COL++;


                sheet[ROW, COL].Text = "Unit Rate";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int colUnitRate = COL;
                COL++;

                sheet[ROW, COL].Text = "Required Amount";
                int colRequiredAmount = COL;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                COL++;

                sheet[ROW, COL].Text = "Plan Amount";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPlanAmounts = COL;


                endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                //ROW++;
                //StartRow = ROW;
                DataTable dtSO = _sqlRepository.GetDataTable(new Library.OrderManagement.Production.ProductionOrder().GetExistingSalesOrderListForReport(CostingBOQMasterId));

                DataTable dtM = new DataView(dtEmployeeData).ToTable(true, "CostingBOQMasterId", "ItemDesc", "UOM", "Currency", "Article");
                DataView dvM = new DataView(dtM);
                dvM.RowFilter = "CostingBOQMasterId='" + dtEmployeeData.Rows[0]["CostingBOQMasterId"].ToString() + "'";

                var edRow = ROW;
                var col = endCol;

                edRow++;
                int msr = edRow;
                //int sc = 4;
                int ec = 0;
                for (int i = 0; i < dtM.Rows.Count; i++)
                {

                    col = 1;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["ItemDesc"].ToString(); col++;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["Article"].ToString(); col++;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["UOM"].ToString(); col++;


                    double GrossConsumption = clsStaticInfo.dbl(dtEmployeeData.Compute("avg(GrossConsumption)", "ItemDesc='" + dtM.Rows[i]["ItemDesc"].ToString() + "'"));
                    sheet.Range[edRow, col].Number = GrossConsumption;
                    sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    col++;

                    //double MTotalSPT = clsStaticInfo.dbl(dtEmployeeData.Compute("SUM(TotalSPT)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                    double BOMQuantity = clsStaticInfo.dbl(dtEmployeeData.Compute("avg(RequiredQty)", "ItemDesc='" + dtM.Rows[i]["ItemDesc"].ToString() + "'"));
                    sheet.Range[edRow, col].Number = BOMQuantity;
                    sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    col++;

                    double PlanQuantity = clsStaticInfo.dbl(dtEmployeeData.Compute("SUM(BOMQty)", "ItemDesc='" + dtM.Rows[i]["ItemDesc"].ToString() + "'"));
                    sheet.Range[edRow, col].Number = PlanQuantity;
                    sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    col++;

                    sheet.Range[edRow, col].Text = dtM.Rows[i]["Currency"].ToString(); col++;

                    double UnitRate = clsStaticInfo.dbl(dtEmployeeData.Compute("avg(UnitRate)", "ItemDesc='" + dtM.Rows[i]["ItemDesc"].ToString() + "'"));
                    sheet.Range[edRow, col].Number = UnitRate;
                    sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    col++;

                    double RequiredAmount = clsStaticInfo.dbl(dtEmployeeData.Compute("avg(RequiredAmount)", "ItemDesc='" + dtM.Rows[i]["ItemDesc"].ToString() + "'"));
                    sheet.Range[edRow, col].Number = RequiredAmount;
                    sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    col++;

                    double PlanAmount = clsStaticInfo.dbl(dtEmployeeData.Compute("SUM(PlanAmount)", "ItemDesc='" + dtM.Rows[i]["ItemDesc"].ToString() + "'"));
                    sheet.Range[edRow, col].Number = PlanAmount;
                    sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    

                    int endCols = col;
                    sheet.Range[edRow, 1, edRow, endCols].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[edRow, 1, edRow, endCols].BorderInside(ExcelLineStyle.Hair);
                    sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    edRow++;
                }

                ec = col;
                int mer = edRow;

                var cols = 1;
                sheet.Range[edRow, cols].Text = "TOTAL";
                sheet.Range[edRow, cols].CellStyle.Font.Bold = true;
                sheet.Range[edRow, cols, edRow, 8].Merge();

                //sheet.Range[edRow, 9].Number = clsStaticInfo.dbl(dtEmployeeData.Compute("SUM(RequiredAmount)", null));
                //sheet.Range[edRow, 9].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[edRow, 9].CellStyle.Font.Bold = true;
                //sheet.Range[edRow, 9, edRow, 9].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet.Range[edRow, 9, edRow, 9].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[edRow, 10].Number = clsStaticInfo.dbl(dtEmployeeData.Compute("SUM(PlanAmount)", null));
                sheet.Range[edRow, 10].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 10, edRow, 10].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 10, edRow, 10].HorizontalAlignment = ExcelHAlign.HAlignRight;

                int endColl = 10;
                sheet.Range[edRow, 1, edRow, endColl].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[edRow, 1, edRow, endColl].BorderInside(ExcelLineStyle.Hair);

                string strFileName = "BOM Report " + CostingBOQMasterId + ".xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {

                throw;
            }

        }

        private DataTable GetNonProcessBOQReportQuery(string CostingBOQMasterId)
        {

            string sql = @"SELECT convert(bit,CASE WHEN isnull(cb.SOCount,0)=0 THEN 0 ELSE 1 END,0) AS Saved, convert(bit,CASE WHEN isnull(cb.SOCount,0)=0 THEN 0 ELSE 1 END,0) AS Selected,cm.OrderCostingMasterTemplateId	
                             ,cv1.UserName AS SKU1,cv2.UserName AS SKU2
							 ,p.UserName AS Vendor,''BOQCriteria,''ItemRefNo,''POCriteria,''Currency,0 RequiredAmount,0 PlanAmount,0 IncompleteMaterial,SO.CostingBOQMasterId
							  ,mm.Code AS MaterialCode,mma.Code AS ArticleCode
							,ci.Sequence,isnull(tr.SOCount,0) SOCount,
                             cm.Id, cm.CostingItemId, cm.Consumption,uom.UserName AS UOM
							 ,isnull(CM.Rate,0) UnitRate
							 ,CM.GrossConsumption,CM.GrossAmount,
                            ci.UserName AS ItemDesc,
							 CASE WHEN isnull(tc.Id,'')<>'' THEN tc.Qty ELSE 
                                CASE WHEN ISNULL(sc.Id,'')<>'' THEN sc.Qty ELSE
                                CASE WHEN ISNULL(fc.Id,'')<>'' THEN fc.Qty ELSE so.Qty END END END AS SOQty,

								 CASE WHEN isnull(tc.Id,'')<>'' THEN tc.Qty ELSE 
                                CASE WHEN ISNULL(sc.Id,'')<>'' THEN sc.Qty ELSE
                                CASE WHEN ISNULL(fc.Id,'')<>'' THEN fc.Qty ELSE so.Qty END END END AS BOMQty,

								RequiredQty=(CASE WHEN isnull(tc.Id,'')<>'' THEN tc.Qty ELSE 
                                CASE WHEN ISNULL(sc.Id,'')<>'' THEN sc.Qty ELSE
                                CASE WHEN ISNULL(fc.Id,'')<>'' THEN fc.Qty ELSE so.Qty END END END)*CM.GrossConsumption,

								BOMAmount=(CASE WHEN isnull(tc.Id,'')<>'' THEN tc.Qty ELSE 
                                CASE WHEN ISNULL(sc.Id,'')<>'' THEN sc.Qty ELSE
                                CASE WHEN ISNULL(fc.Id,'')<>'' THEN fc.Qty ELSE so.Qty END END END)*CM.GrossAmount,

                                   CM.[Description], CM.SourcingType, CM.Remarks,ei.EmployeeName AS ResponsiblePerson,
                                   mm.UserName AS Material,mma.StandardName AS Article
								   ,cbm.AddedBy AS PreparedBy,FORMAT(cbm.AddedDate,'dd-MMM-yyyy') AS CostingDate

								   ,so.Id SalesOrderId,d.UserName Destination

                              FROM OrderProcurementCostingDirectMaterial AS CM
                         INNER JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=cm.OrderCostingMasterTemplateId
                            INNER JOIN trn.SalesOrder AS so ON  so.Id IN (select Id from [TRN].[SalesOrder] where CostingBOQMasterid='" + CostingBOQMasterId+@"') and so.MasterOrderItemId=moi.id
							LEFT JOIN mst.Destination D ON D.Id=SO.DestinationId
							 LEFT JOIN CostingBOQMaster AS cbm ON cbm.Id=SO.CostingBOQMasterId
							 LEFT JOIN CostingBOQ CBQ ON CBQ.CostingBOQMasterId=cbm.Id
							 LEFT JOIN hkp.Party AS p ON p.Id=CBQ.VendorId
                               INNER JOIN hkp.CostingItem AS ci ON ci.Id=cm.CostingItemId
                            LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=cm.MaterialMasterId
                            LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=cm.ArticleId
                            LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=cm.ResponsiblePersonId
                            LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=ci.UnitOfMeasurementId
                            LEFT JOIN ( SELECT cb.OrderProcurementCostingDirectMaterialId,COUNT(DISTINCT cb.SalesOrderId) AS SOCount
									   FROM CostingBOQItems AS cb
									 WHERE cb.SalesOrderId IN (select Id from [TRN].[SalesOrder] where CostingBOQMasterid='"+CostingBOQMasterId+@"')
									 GROUP BY cb.OrderProcurementCostingDirectMaterialId) AS TR ON tr.OrderProcurementCostingDirectMaterialId=cm.Id
									 
							LEFT JOIN ( SELECT cb.OrderProcurementCostingDirectMaterialId,COUNT(DISTINCT cb.SalesOrderId) AS SOCount
									   FROM CostingBOQItems AS cb
									 WHERE cb.SalesOrderId IN (select Id from [TRN].[SalesOrder] where CostingBOQMasterid='"+CostingBOQMasterId+@"') AND cb.CostingBOQMasterId='"+CostingBOQMasterId+@"'
									 GROUP BY cb.OrderProcurementCostingDirectMaterialId) AS CB ON CB.OrderProcurementCostingDirectMaterialId=cm.Id
                           
						   LEFT JOIN trn.FirstCharacteristics AS fc ON fc.SalesOrderId=so.Id
                           LEFT JOIN trn.SecondCharacteristics AS sc ON sc.FirstCharacteristicsId=fc.Id AND sc.SalesOrderId=so.Id
                           LEFT JOIN trn.ThirdCharacteristics AS tc ON tc.SecondCharacteristicsId=sc.Id AND tc.SalesOrderId=so.Id

                       
							                                LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=fc.CharacteristicsValueId
							                                LEFT JOIN hkp.Characteristics AS c1 ON c1.Id=cv1.CharacteristicsId
							
							                                LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=sc.CharacteristicsValueId
							                                LEFT JOIN hkp.Characteristics AS c2 ON c2.Id=cv2.CharacteristicsId
							
							                                LEFT JOIN hkp.CharacteristicsValue AS cv3 ON cv3.Id=tc.CharacteristicsValueId
							                                LEFT JOIN hkp.Characteristics AS c3 ON c3.Id=cv3.CharacteristicsId
                      Order BY  CASE WHEN convert(bit,CASE WHEN isnull(cb.SOCount,0)=0 THEN 0 ELSE 1 END,0)=0 AND isnull(tr.SOCount,0)=1 THEN 1 ELSE 0 END,ci.Sequence ";

            DataTable dtData = _sqlRepository.GetDataTable(sql);

            return dtData;

        }

        public void GetNonProcessReportXls(string CostingBOQMasterId)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                var report = new ReportUtility();
                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "BOQ Report";

                var headerData = getBOQStatusReportHeaderSql(CostingBOQMasterId);
                DataTable dtEmployeeData = GetNonProcessBOQReportQuery(CostingBOQMasterId);

                int ROW = 6;
                int COL = 1;


                #region Header

                report.SetMasterHeaderText(ref sheet, ROW, 1, "BOM Id");
                //sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, headerData["BOMId"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                //sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "BOM Creation Date");
                sheet[ROW, 4].ColumnWidth = 20;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, headerData["BOMCreationDate"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                //sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "Customer");
                //sheet[ROW, 1].ColumnWidth = 25;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, headerData["Customer"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                //sheet[ROW, 2].ColumnWidth = 30;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "Buyer");
                //sheet[ROW, 4].ColumnWidth = 25;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, headerData["BuyerName"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                sheet[ROW, 5].ColumnWidth = 30;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "Master Order Id");
                sheet[ROW, 1].ColumnWidth = 16;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, headerData["MasterOrderId"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "Buyer Ref No");
                //sheet[ROW, 4].ColumnWidth = 20;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, headerData["BuyerItemNo"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                sheet[ROW, 5].ColumnWidth = 20;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "Product");
                //sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, headerData["ArticleName"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "Contract No");
                //sheet[ROW, 4].ColumnWidth = 25;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, headerData["OwnOrderNo"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                //sheet[ROW, 5].ColumnWidth = 25;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "SO Id");
                //sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, headerData["SalesOrderId"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 4, "PO No");
                //sheet[ROW, 4].ColumnWidth = 25;
                sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 5, headerData["PONumber"].ToString());
                sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
                //sheet[ROW, 5].ColumnWidth = 25;
                sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "SO Quantity");
                //sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, headerData["SOQty"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                ROW++;
                ROW++;
                #endregion

                #region Middle Data

                sheet[ROW, COL].Text = "Responsible Person:" + dtEmployeeData.Rows[0]["ResponsiblePerson"].ToString();
                sheet.Range[ROW, COL].CellStyle.Font.Size = 15;
                sheet.Range[ROW, 1, ROW, 19].Merge();
                ROW++;

                sheet[ROW, COL].Text = "Sl.No";
                //sheet[ROW, COL].ColumnWidth = 13;
                int colSlNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Item Ref#";
                sheet[ROW, COL].ColumnWidth = 15;
                int colItemRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Item";
                sheet[ROW, COL].ColumnWidth = 20;
                int colItemDesc = COL;
                COL++;
                sheet[ROW, COL].Text = "Criteria";
                sheet[ROW, COL].ColumnWidth = 20;
                int colBOQCriteria = COL;
                COL++;

                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 20;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 20;
                int colArticle = COL;
                COL++;

                sheet[ROW, COL].Text = "Vendor";
                sheet[ROW, COL].ColumnWidth = 25;
                int colVendor = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Id";
                sheet[ROW, COL].ColumnWidth = 15;
                int colSalesOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Destination";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDestination = COL;
                COL++;
                sheet[ROW, COL].Text = "SKU1";
                sheet[ROW, COL].ColumnWidth = 12;
                int colSKU1 = COL;
                COL++;
                sheet[ROW, COL].Text = "SKU2";
                sheet[ROW, COL].ColumnWidth = 12;
                int colSKU2 = COL;
                COL++;
                sheet[ROW, COL].Text = "PO Criteria";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPOCriteria = COL;
                COL++;

                sheet[ROW, COL].Text = "Gross Consumption";
                sheet[ROW, COL].ColumnWidth = 18;
                int colGrossConsumption = COL;
                COL++;

                sheet[ROW, COL].Text = "UOM";
                //sheet[ROW, COL].ColumnWidth = 8;
                int colUOM = COL;
                COL++;

                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].ColumnWidth = 18;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSOQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Plan Qty";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colBOMQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Required Qty";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 12;
                int colRequiredQty = COL;

                COL++;
                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 15;
                int colCurrency = COL;
                COL++;

                sheet[ROW, COL].Text = "Unit Rate";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 18;
                int colUnitRate = COL;
                COL++;

                sheet[ROW, COL].Text = "Plan Amount";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colPlanAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Reqired Amount";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colBOMAmount = COL;


                #endregion



                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                ROW++;
                int StartRow = ROW; //row 20
                for (int i = 0; i < dtEmployeeData.Rows.Count; i++)
                {


                    sheet[ROW, colSlNo].Number = (i + 1);

                    sheet[ROW, colSalesOrderId].Text = dtEmployeeData.Rows[i]["SalesOrderId"].ToString();
                    sheet[ROW, colDestination].Text = dtEmployeeData.Rows[i]["Destination"].ToString();
                    sheet[ROW, colSKU1].Text = dtEmployeeData.Rows[i]["SKU1"].ToString();
                    sheet[ROW, colSKU2].Text = dtEmployeeData.Rows[i]["SKU2"].ToString();
                    sheet[ROW, colItemDesc].Text = dtEmployeeData.Rows[i]["ItemDesc"].ToString();
                    sheet[ROW, colMaterial].Text = dtEmployeeData.Rows[i]["Material"].ToString();
                    sheet[ROW, colArticle].Text = dtEmployeeData.Rows[i]["Article"].ToString();
                    sheet[ROW, colVendor].Text = dtEmployeeData.Rows[i]["Vendor"].ToString();
                    sheet[ROW, colUOM].Text = dtEmployeeData.Rows[i]["UOM"].ToString();
                    sheet[ROW, colSOQty].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["SOQty"].ToString());
                    sheet[ROW, colBOQCriteria].Text = dtEmployeeData.Rows[i]["BOQCriteria"].ToString();
                    sheet[ROW, colItemRefNo].Text = dtEmployeeData.Rows[i]["ItemRefNo"].ToString();
                    sheet[ROW, colPOCriteria].Text = dtEmployeeData.Rows[i]["POCriteria"].ToString();

                    sheet[ROW, colGrossConsumption].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["GrossConsumption"].ToString());
                    sheet[ROW, colBOMQty].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["BOMQty"].ToString());
                    sheet[ROW, colRequiredQty].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["RequiredQty"].ToString());
                    sheet[ROW, colCurrency].Text = dtEmployeeData.Rows[i]["Currency"].ToString();
                    sheet[ROW, colUnitRate].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["UnitRate"].ToString());
                    sheet[ROW, colBOMAmount].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["RequiredAmount"].ToString());
                    sheet[ROW, colPlanAmount].Number = clsStaticInfo.dbl(dtEmployeeData.Rows[i]["PlanAmount"].ToString());


                    if (bplib.clsWebLib.GetBoolData(dtEmployeeData.Rows[i]["IncompleteMaterial"].ToString()))
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Red;

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                    ROW++;

                }

                sheet.IsGridLinesVisible = false;

                //sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 11f;

                //Total Start
                var endRow = ROW++;

                sheet.Range[endRow, colSlNo].Text = "Total";
                sheet.Range[endRow, colSlNo, endRow, colCurrency].Merge();
                sheet.Range[endRow, colSlNo].CellStyle.Font.Bold = true;
                //sheet.Range[endRow, colSlNo].CellStyle.Font.Size = 11;

                //sheet.Range[endRow, colBOMAmount].Number = clsStaticInfo.dbl(dtEmployeeData.Compute("avg(RequiredAmount)", null));
                //sheet.Range[endRow, colBOMAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[endRow, colBOMAmount].CellStyle.Font.Bold = true;
                //sheet.Range[endRow, colBOMAmount, endRow, colBOMAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet.Range[endRow, colBOMAmount, endRow, colBOMAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[endRow, colPlanAmount].Number = clsStaticInfo.dbl(dtEmployeeData.Compute("SUM(PlanAmount)", null));
                sheet.Range[endRow, colPlanAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[endRow, colPlanAmount].CellStyle.Font.Bold = true;
                sheet.Range[endRow, colPlanAmount, endRow, colPlanAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, colPlanAmount, endRow, colPlanAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[endRow, colPlanAmount].CellStyle.Font.Bold = true;

                sheet.Range[endRow, 1, endRow, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[endRow, 1, endRow, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                endRow++;
                endRow++;

                //Total End
                sheet["A" + StartRow.ToString()].FreezePanes();

                sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colSlNo, ROW, colSlNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;


                sheet.Range[StartRow, colBOMAmount, ROW, colBOMAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colBOMQty, ROW, colBOMQty].NumberFormat = clsStaticInfo.NumberFormat(2);

                //string HeaderCaption = string.Format("BOM Report (#{0}),Prepared By:{1}, BOM Creation Date:{2}",
                //    CostingBOQMasterId
                //     , dtEmployeeData.Rows[0]["PreparedBy"].ToString()
                //     , dtEmployeeData.Rows[0]["CostingDate"].ToString());3

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, null, identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;


                ROW += 3;


                sheet[ROW, 1].Text = "BOM SUMMARY:";
                sheet.Range[ROW, 1].CellStyle.Font.Size = 15;
                sheet.Range[ROW, 1, ROW, 19].Merge();
                ROW++;


                COL = 1;
                sheet[ROW, COL].Text = "Item";
                //int colItem = COL;
                COL++;

                sheet[ROW, COL].Text = "Article";
                //int colArticle = COL;
                COL++;

                sheet[ROW, COL].Text = "UOM";
                //int colUOMs = COL;
                COL++;

                sheet[ROW, COL].Text = "Gross Consumption";
                //int colGrossConsumption = COL;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                COL++;

                sheet[ROW, COL].Text = "Required Quantity";
                //int colRequiredQuantity = COL;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                COL++;

                sheet[ROW, COL].Text = "Plan Quantity";
                //int colPlanQuantity = COL;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                //int colCurrencys = COL;
                COL++;


                sheet[ROW, COL].Text = "Unit Rate";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int colUnitRate = COL;
                COL++;

                sheet[ROW, COL].Text = "Required Amount";
                int colRequiredAmount = COL;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                COL++;

                sheet[ROW, COL].Text = "Plan Amount";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPlanAmounts = COL;


                endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                //ROW++;
                //StartRow = ROW;
                DataTable dtSO = _sqlRepository.GetDataTable(new Library.OrderManagement.Production.ProductionOrder().GetExistingSalesOrderListForReport(CostingBOQMasterId));

                DataTable dtM = new DataView(dtEmployeeData).ToTable(true, "CostingBOQMasterId", "ItemDesc", "UOM", "Currency", "Article");
                DataView dvM = new DataView(dtM);
                dvM.RowFilter = "CostingBOQMasterId='" + dtEmployeeData.Rows[0]["CostingBOQMasterId"].ToString() + "'";

                var edRow = ROW;
                var col = endCol;

                edRow++;
                int msr = edRow;
                //int sc = 4;
                int ec = 0;
                for (int i = 0; i < dtM.Rows.Count; i++)
                {

                    col = 1;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["ItemDesc"].ToString(); col++;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["Article"].ToString(); col++;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["UOM"].ToString(); col++;


                    double GrossConsumption = clsStaticInfo.dbl(dtEmployeeData.Compute("avg(GrossConsumption)", "ItemDesc='" + dtM.Rows[i]["ItemDesc"].ToString() + "'"));
                    sheet.Range[edRow, col].Number = GrossConsumption;
                    sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    col++;

                    //double MTotalSPT = clsStaticInfo.dbl(dtEmployeeData.Compute("SUM(TotalSPT)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                    double BOMQuantity = clsStaticInfo.dbl(dtEmployeeData.Compute("avg(RequiredQty)", "ItemDesc='" + dtM.Rows[i]["ItemDesc"].ToString() + "'"));
                    sheet.Range[edRow, col].Number = BOMQuantity;
                    sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    col++;

                    double PlanQuantity = clsStaticInfo.dbl(dtEmployeeData.Compute("SUM(BOMQty)", "ItemDesc='" + dtM.Rows[i]["ItemDesc"].ToString() + "'"));
                    sheet.Range[edRow, col].Number = PlanQuantity;
                    sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    col++;

                    sheet.Range[edRow, col].Text = dtM.Rows[i]["Currency"].ToString(); col++;

                    double UnitRate = clsStaticInfo.dbl(dtEmployeeData.Compute("avg(UnitRate)", "ItemDesc='" + dtM.Rows[i]["ItemDesc"].ToString() + "'"));
                    sheet.Range[edRow, col].Number = UnitRate;
                    sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    col++;

                    double RequiredAmount = clsStaticInfo.dbl(dtEmployeeData.Compute("avg(RequiredAmount)", "ItemDesc='" + dtM.Rows[i]["ItemDesc"].ToString() + "'"));
                    sheet.Range[edRow, col].Number = RequiredAmount;
                    sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    col++;

                    double PlanAmount = clsStaticInfo.dbl(dtEmployeeData.Compute("SUM(PlanAmount)", "ItemDesc='" + dtM.Rows[i]["ItemDesc"].ToString() + "'"));
                    sheet.Range[edRow, col].Number = PlanAmount;
                    sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignRight;


                    int endCols = col;
                    sheet.Range[edRow, 1, edRow, endCols].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[edRow, 1, edRow, endCols].BorderInside(ExcelLineStyle.Hair);
                    sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    edRow++;
                }

                ec = col;
                int mer = edRow;

                var cols = 1;
                sheet.Range[edRow, cols].Text = "TOTAL";
                sheet.Range[edRow, cols].CellStyle.Font.Bold = true;
                sheet.Range[edRow, cols, edRow, 8].Merge();

                //sheet.Range[edRow, 9].Number = clsStaticInfo.dbl(dtEmployeeData.Compute("SUM(RequiredAmount)", null));
                //sheet.Range[edRow, 9].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[edRow, 9].CellStyle.Font.Bold = true;
                //sheet.Range[edRow, 9, edRow, 9].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet.Range[edRow, 9, edRow, 9].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[edRow, 10].Number = clsStaticInfo.dbl(dtEmployeeData.Compute("SUM(PlanAmount)", null));
                sheet.Range[edRow, 10].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 10, edRow, 10].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 10, edRow, 10].HorizontalAlignment = ExcelHAlign.HAlignRight;

                int endColl = 10;
                sheet.Range[edRow, 1, edRow, endColl].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[edRow, 1, edRow, endColl].BorderInside(ExcelLineStyle.Hair);

                string strFileName = "BOM Report " + CostingBOQMasterId + ".xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public Dictionary<string, object> getBOQStatusReportHeaderSql(string CostingBOQMasterId)
        {
            try
            {
                var sql = @"select cost.Id BOMId,FORMAT(cost.AddedDate,'dd-MMM-yyyy') BOMCreationDate
									,P.UserName AS Customer
									
									,BuyerName=STUFF((SELECT distinct ','+  XITM.UserName
                                    from HKP.Buyer  AS XITM
									JOIN [TRN].[MasterOrder] AS MO ON XITM.Id = MO.BuyerId
									JOIN [TRN].[MasterOrderItem] AS MOI ON MO.Id=MOI.MasterOrderId
									left join [TRN].[SalesOrder] AS SO on SO.MasterOrderItemId=MOI.Id
                                    where SO.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									,BuyerItemNo=STUFF((SELECT distinct ','+  MOI.BuyerReferenceNo
                                    from [TRN].[MasterOrderItem] AS MOI 
									left join [TRN].[SalesOrder] AS SO on SO.MasterOrderItemId=MOI.Id
                                    where SO.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									,ArticleName=STUFF((SELECT distinct ','+  MMA.StandardName
                                    from [MST].[MaterialMasterArticle] MMA
									left join [TRN].[MasterOrderItem] AS MOI on MOI.ArticleId=MMA.Id
									left join [TRN].[SalesOrder] AS SO on SO.MasterOrderItemId=MOI.Id
                                    where SO.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                    

									,MasterOrderId=STUFF((SELECT distinct ','+  MO.Id
                                    from [TRN].[MasterOrder] AS MO
									left join [TRN].[MasterOrderItem] AS MOI ON MO.Id=MOI.MasterOrderId
									left join [TRN].[SalesOrder] AS SO on SO.MasterOrderItemId=MOI.Id
                                    where SO.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									,OwnOrderNo=STUFF((SELECT distinct ','+  MO.OwnReferenceNo
                                    from [TRN].[MasterOrder] AS MO
									left join [TRN].[MasterOrderItem] AS MOI ON MO.Id=MOI.MasterOrderId
									left join [TRN].[SalesOrder] AS SO on SO.MasterOrderItemId=MOI.Id
                                    where SO.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									,PONumber=STUFF((SELECT distinct ','+  CPO.PONumber
                                    from [TRN].[CustomerPO] CPO
									left join [TRN].[SalesOrder] AS SO on SO.CustomerPOId=CPO.Id
                                    where SO.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									,SalesOrderId=STUFF((SELECT distinct ','+  XITM.Id
                                    from trn.SalesOrder AS XITM
                                    where XITM.CostingBOQMasterId=cost.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                    
                                     ,so.SOQty 

									from CostingBOQMaster AS cost
									LEFT JOIN [HKP].[Party] AS P ON cost.CustomerId = P.Id
									left join (SELECT sum(Qty) SOQty,CostingBOQMasterId from trn.SalesOrder group by CostingBOQMasterId) SO on SO.CostingBOQMasterId=cost.Id

									where cost.Id = '" + CostingBOQMasterId + "'";
                return _sqlRepository.GetData(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string UpdateString(object FieldValue)
        {
            if (FieldValue == null)
                return "Null";

            if (string.IsNullOrEmpty(FieldValue.ToString()))
                return "Null";

            return "'" + FieldValue + "'";
        }

        public void UpdateBOQGeneration(string Id, List<Dictionary<string, object>> MaterialAttachmentData, List<Dictionary<string, object>> QuantityData)
        {
            try
            {

                Library.General.Conversions.UOMConversion conversion = new Library.General.Conversions.UOMConversion();

                ConnectionManager.clsConnectionManager ConManager = new ConnectionManager.clsConnectionManager();
                ConManager.getDataSet("select * from BOQ where CostingBOQMasterId='" + Id + "'", out DataSet dsMaster);
                for (int i = 0; i < MaterialAttachmentData.Count; i++)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + MaterialAttachmentData[i]["CostingItemId"].ToString() + "'";
                    for (int k = 0; k < dsMaster.Tables[0].DefaultView.Count; k++)
                    {
                        dsMaster.Tables[0].DefaultView[k].Row.BeginEdit();
                        dsMaster.Tables[0].DefaultView[k]["MaterialMasterId"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(MaterialAttachmentData[i]["MaterialMasterId"]));
                        dsMaster.Tables[0].DefaultView[k]["ArticleId"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(MaterialAttachmentData[i]["ArticleId"]));
                        dsMaster.Tables[0].DefaultView[k]["VendorId"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(MaterialAttachmentData[i]["VendorId"]));
                        dsMaster.Tables[0].DefaultView[k].Row.EndEdit();

                    }
                }



                for (int i = 0; i < QuantityData.Count; i++)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "Id='" + QuantityData[i]["Id"].ToString() + "'";
                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                    {

                        dsMaster.Tables[0].DefaultView[0].Row.BeginEdit();
                        dsMaster.Tables[0].DefaultView[0]["RequiredQty"] = QuantityData[i]["RequiredQty"];
                        dsMaster.Tables[0].DefaultView[0]["RMDescription"] = clsStaticInfo.nullrecorder(QuantityData[i]["RMDescription"]).Trim();
                        dsMaster.Tables[0].DefaultView[0]["RMCustomerSpec"] = clsStaticInfo.nullrecorder(QuantityData[i]["RMCustomerSpec"]).Trim();
                        dsMaster.Tables[0].DefaultView[0]["RMVendorSpec"] = clsStaticInfo.nullrecorder(QuantityData[i]["RMVendorSpec"]).Trim();
                        dsMaster.Tables[0].DefaultView[0]["SKUDesc"] = clsStaticInfo.nullrecorder(QuantityData[i]["SKUDescConcat"]).Trim();
                        dsMaster.Tables[0].DefaultView[0]["OwnReferenceNo"] = clsStaticInfo.nullrecorder(QuantityData[i]["OwnReferenceNo"]).Trim();
                        dsMaster.Tables[0].DefaultView[0]["Remark"] = QuantityData[i]["Remark"];
                        dsMaster.Tables[0].DefaultView[0]["UpDownCharge"] = QuantityData[i]["UpDownCharge"];


                        string BaseUOM = conversion.GetMaterialUOMByCategory(dsMaster.Tables[0].DefaultView[0]["MaterialMasterId"].ToString(), General.Conversions.UOMConversion.UOMCategory.BaseUOMId);
                        string POUoMId = conversion.GetMaterialUOMByCategory(dsMaster.Tables[0].DefaultView[0]["MaterialMasterId"].ToString(), General.Conversions.UOMConversion.UOMCategory.PurchaseOrderUOMId);

                        dsMaster.Tables[0].DefaultView[0]["BaseUoMId"] = bplib.clsWebLib.RetValidLen(BaseUOM);
                        dsMaster.Tables[0].DefaultView[0]["POUoMId"] = bplib.clsWebLib.RetValidLen(POUoMId);

                        //in case someone changes material master, might also change the base uom
                        dsMaster.Tables[0].DefaultView[0]["BOMQtyBase"] = conversion.Convert(
                            dsMaster.Tables[0].DefaultView[0]["MaterialMasterId"].ToString(),
                            dsMaster.Tables[0].DefaultView[0]["UoMId"].ToString(),
                            BaseUOM,
                            clsStaticInfo.dbl(dsMaster.Tables[0].DefaultView[0]["BOMQty"].ToString()));

                        dsMaster.Tables[0].DefaultView[0]["RequiredQtyBase"] = conversion.Convert(
                           dsMaster.Tables[0].DefaultView[0]["MaterialMasterId"].ToString(),
                           dsMaster.Tables[0].DefaultView[0]["UoMId"].ToString(),
                           BaseUOM,
                           clsStaticInfo.dbl(dsMaster.Tables[0].DefaultView[0]["RequiredQty"].ToString()));

                        dsMaster.Tables[0].DefaultView[0]["RequiredQtyPO"] = conversion.Convert(
                            dsMaster.Tables[0].DefaultView[0]["MaterialMasterId"].ToString(),
                            dsMaster.Tables[0].DefaultView[0]["UoMId"].ToString(),
                            POUoMId,
                            clsStaticInfo.dbl(dsMaster.Tables[0].DefaultView[0]["RequiredQty"].ToString()));


                        double POUOMFactor = conversion.Convert(
                                                  dsMaster.Tables[0].DefaultView[0]["MaterialMasterId"].ToString(),
                                                  dsMaster.Tables[0].DefaultView[0]["POUoMId"].ToString(),
                                                  POUoMId,
                                                1);

                        if (POUOMFactor > 0)
                            dsMaster.Tables[0].DefaultView[0]["Rate"] = clsStaticInfo.dbl(dsMaster.Tables[0].DefaultView[0]["Rate"]) / POUOMFactor;


                        dsMaster.Tables[0].DefaultView[0].Row.EndEdit();
                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void UpdateBOQ(List<Dictionary<string, object>> QuantityData)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string id = string.Empty;
            try
            {
                foreach (var item in QuantityData)
                {
                    if (id == "")
                        id = "'" + item["Id"] + "'";
                    else
                        id = id + ",'" + item["Id"] + "'";
                }
                string mosql = "SELECT * FROM dbo.BOQ WHERE Id IN (" + id + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMaster, false, "1");

                string cId = string.Empty;
                foreach (var item in QuantityData)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();

                        drmo["Status"] = item["Status"];
                        drmo["Reason"] = item["Reason"];
                        drmo["RequiredQtyApproved"] = item["RequiredQtyApproved"];
                        drmo["UpdatedBy"] = identity.Name;
                        drmo["UpdatedDate"] = DateTime.Now.ToString();
                        drmo["UpdatedFromIP"] = identity.IPAddress;

                        drmo.EndEdit();

                    }

                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
