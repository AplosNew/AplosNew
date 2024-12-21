using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
#region Using
using Syncfusion.DocIO.DLS;
using Library.Service.Enums;
using Library.Service.Logs;
using System.Collections.Specialized;
using System.Linq;
using Syncfusion.XlsIO;
using Library.Service.Helpers;

#endregion Using

namespace Library.MaterialManagement.InventoryManagements
{
    public class PurchaseOrderQueryService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public PurchaseOrderQueryService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Constructor

        public IEnumerable<object> GetPOBOQItems(string ContractId, string VendorId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var tempsql = "";
            if (!string.IsNullOrEmpty(ContractId) && string.IsNullOrEmpty(VendorId))
            {
                tempsql = @"b.Id in ( SELECT B.ID FROM boq B JOIN trn.SalesOrder SO ON SO.CostingBOQMasterId=b.CostingBOQMasterId
                                    --JOIN trn.MasterOrderItem MOI ON MOI.Id = SO.MasterOrderItemId
                                    WHERE(isnull(SO.ContractId, '') = '' OR isnull(SO.ContractId, null) = '"+ ContractId + @"') ) ";
            }
            if (string.IsNullOrEmpty(ContractId) && !string.IsNullOrEmpty(VendorId))
            {
                tempsql = @"b.Id in ( SELECT B.ID FROM boq B JOIN trn.SalesOrder SO ON SO.CostingBOQMasterId=b.CostingBOQMasterId
                                    JOIN trn.MasterOrderItem MOI ON MOI.Id = SO.MasterOrderItemId 
                                    WHERE (isnull(b.VendorId,'')='" + VendorId + @"'))";
            }
            else
            {
                tempsql = @"b.Id in ( SELECT B.ID FROM boq B JOIN trn.SalesOrder SO ON SO.CostingBOQMasterId=b.CostingBOQMasterId
                                    --JOIN trn.MasterOrderItem MOI ON MOI.Id = SO.MasterOrderItemId 
                                    WHERE (isnull(SO.ContractId, null) = '" + ContractId + @"')
                                    AND (isnull(b.VendorId,'')='" + VendorId + @"'))";
            }
            var sql = "";
            sql = @"SELECT DISTINCT NULL AS uoMList, b.Id BOQId,b.CostingItemId,b.POCriteria,b.CostingBOQMasterId BOMId
                        ,GroupId=CASE WHEN isnull(b.POCriteria,'CostingItem')='CostingItem' THEN b.CostingItemId ELSE b.Id END
                        ,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
                        ,IsNULL(MGA.UserName,'') AS MaterialGroupMasterName
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FGFirstCharacteristicsValueId FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.FGSecondCharacteristicsValueId SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.FGThirdCharacteristicsValueId ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty,C.Id
						,null CheckedStatus   ,null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
						--,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
						--,ISNULL(cpo.PONumber,'') PONumber
						,b.RequiredQty
						,uom.UserName BOQUOM
						,b.POUoMId FromPoUomId
					    ,b.POUoMId
						,b.RequiredQtyPO 
						,b.RequiredQtyPO RequiredQtyPOOrginal
						,TransactionUoMId=CASE WHEN b.POUoMId IS NULL THEN b.UoMId ELSE b.POUoMId END
                        ,TransactionUoM=CASE WHEN b.POUoMId IS NULL THEN uom.UserName ELSE Tuom.UserName END 
						,RefferenceNo=ISNULL(moi.BuyerReferenceNo,'')  
						--,ISNULL(DE.UserName,'') Destination
						,mm.BaseUOMId,Isnull(OPC.Rate,0) TransactionRate
						,Isnull(OPC.Rate,0) TransactionRateBOQ
                        ,isnull(b.UpDownCharge,0) UpDownCharge
						,UnitPrice=Isnull(OPC.Rate,0)+isnull(b.UpDownCharge,0)
                        ,ISNULL(POBoqMap.MapQty,0) OtherMapQty
                        , TransactionQty=Round(Round(ISNULL(b.RequiredQtyPO,0),4),4)-ISNULL(POBoqMap.MapQty,0)
                        , BalanceQty=Round(Round(ISNULL(b.RequiredQtyPO,0),4),4)-ISNULL(POBoqMap.MapQty,0)
                        , BalanceTrnUOMQty=Round(Round(ISNULL(b.RequiredQtyPO,0),4),4)-ISNULL(POBoqMap.MapQty,0)
                        ,0 Tolerance,0 TrnAmount
						,MOI.Type,isnull(moi.Consignment,0) AS Consignment,
						 CASE WHEN isnull(moi.Consignment,0)=1 THEN
        					  CONCAT(POWN.UserName,'(',EOWN.UserName,')')	          
						ELSE
							case when isnull(MOI.JobWorkType,'')<>'' THEN 
								CASE WHEN ISNULL(eout.Id,'')<>'' THEN CONCAT(POUT.UserName,'(',EOUT.UserName,')') ELSE TOUT.UserName END
						   ELSE CONCAT(POWN.UserName,'(',EOWN.UserName,')') END
           
							 END AS PurchaseAuthority,
						   case when isnull(MOI.JobWorkType,'')<>'' THEN 
								CASE WHEN ISNULL(eout.Id,'')<>'' THEN CONCAT(POUT.UserName,'(',EOUT.UserName,')') ELSE TOUT.UserName END
						   ELSE CONCAT(POWN.UserName,'(',EOWN.UserName,')') END AS ProductionAuthority,c.Id ContractId,ISNULL(b.ItemRefNo,'') BOQItemRefNo
						   ,ISNULL(CI.UserName,'') CostingItemName,ISNULL(b.SKUDesc,'')SKUDesc,ISNULL(b.RMDescription,'')RMDescription
						   ,ISNULL(b.RMVendorSpec,'')RMVendorSpec,ISNULL(b.RMCustomerSpec,'')RMCustomerSpec
                   ,b.BOQCriteria  , CriteriaDetail= ISNULL(b.SKUDesc,CONCAT(b.SalesOrderId,' ',v1.UserName,' ',v2.UserName)),b.OwnReferenceNo BOQOwnReferenceNo
,b.Rate*b.BOMQty AS BOMAmount ,b.Rate*b.RequiredQty AS PlanAmount ,  mm.Code AS MaterialCode,mma.Code AS ArticleCode,V1.UserName SKU1,v2.UserName SKU2
, SKUDescConcat= ISNULL(b.SKUDesc,CONCAT(b.SalesOrderId,' ',v1.UserName,' ',v2.UserName))
   ,b.RequiredQty,b.BOMQty-b.RequiredQty AS BalanceToPurchase,b.CostingItemId,b.Remark,b.[FileName]
						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                        LEFT OUTER JOIN MST.MaterialGroupMaster AS MGA ON MGA.Id=mm.MaterialGroupMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                        LEFT OUTER JOIN scs.UnitOfMeasurement AS Tuom ON Tuom.Id=b.POUoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT JOIN (Select DISTINCT SalesOrderId,CostingBOQMasterId,CostingItemId,OrderProcurementCostingDirectMaterialId from CostingBOQItems )CBI on CBI.CostingBOQMasterId=b.CostingBOQMasterId AND CBI.CostingItemId=b.CostingItemId --AND so.Id=CBI.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
                        LEFT JOIN TRN.SalesOrder SO ON SO.Id=CBI.SalesOrderId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FGFirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.FGSecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.FGThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId
                        --left outer join mst.Destination DE ON DE.Id=so.DestinationId
						 JOIN [dbo].[Contract] C ON C.Id=SO.ContractId
						LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
						LEFT JOIN org.Plant AS POUT ON POUT.Id=EOUT.PlantId
						LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
						LEFT JOIN org.Plant AS POWN ON POWN.Id=MO.PlantId
						LEFT JOIN org.Entity AS EOWN ON EOWN.Id=MO.EntityId
                        LEFT JOIN HKP.CostingItem CI ON CI.Id=b.CostingItemId
						LEFT JOIN OrderProcurementCostingDirectMaterial OPC on OPC.Id=CBI.OrderProcurementCostingDirectMaterialId AND CBI.CostingItemId=OPC.CostingItemId AND b.CostingItemId=OPC.CostingItemId
                        LEFT JOIN (SELECT SUM(ISNULL(TransactionQty,0)) MapQty,BOQDetailId FROM TRN.POBOQMAP GROUP BY BOQDetailId) 
									AS POBoqMap ON POBoqMap.BOQDetailId=B.Id
						where " + tempsql + @"
                        AND b.MaterialMasterId<>'' AND b.ArticleId<>''
						ORDER BY b.Sequence, b.SalesOrderId";//b.MaterialMasterId,
            var Data = _sqlRepository.GetDataCollection(sql);
            StringCollection strCol = new StringCollection();
            string MaterialMasterList = "''";
            for (int i = 0; i < Data.Count; i++)
            {
                if (strCol.Contains(Data[i]["MaterialMasterId"].ToString()) == true)
                    continue;
                strCol.Add(Data[i]["MaterialMasterId"].ToString());
                MaterialMasterList += ",'" + Data[i]["MaterialMasterId"].ToString() + "'";

            }

            var UOMList = _sqlRepository.GetDataCollection(@"SELECT M.Id AS MaterialMasterId, UOM1.Id AS [Value],UOM1.UserName AS [Text],BaseUOMFactor FROM (
																	SELECT Id,BaseUOMId UOMId,1 BaseUOMFactor  FROM mst.MaterialMaster
																	UNION
																	SELECT MaterialMasterId Id,AlternativeUOMId UOMId,BaseUOMFactor FROM mst.MaterialMasterAlternativeUOM
																	) AS M
																	 JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=m.UOMId
																	 where m.Id in (" + MaterialMasterList + @")");

            for (int i = 0; i < Data.Count; i++)
            {
                var temp = UOMList.Where(ee => ee["MaterialMasterId"].ToString() == Data[i]["MaterialMasterId"].ToString()).ToList();
                Data[i]["uoMList"] = temp;
            }

            return Data;

        }

        public IEnumerable<object> GetBOQItems(string ContractId, string VendorId, string IsOwnVendor, string inveReveiveMasterId, bool istradingPO)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (IsOwnVendor == "OwnVendor")
            {
                try
                {
                    string whereClause = @"WHERE so.ContractId='" + ContractId + @"' --AND (b.VendorId='" + VendorId + @"' OR b.VendorId is null)
                                AND isnull(B.MasterOrderItemId,'') NOT IN (
                                select isnull(MOI.Id,'') from trn.MasterOrderItem MOI
                                join trn.MasterOrder MO ON MO.Id=moi.MasterOrderId 
                                join trn.SalesOrder SO ON SO.MasterOrderItemId=moi.Id 

                                WHERE MOI.Type='OutSource' and isnull(MOI.consignment,0)=0 AND MO.plantId='" + identity.PlantId + @"'
                            )";
                    //string whereClause = @"WHERE (b.VendorId='" + VendorId + @"' OR ISNULL(b.VendorId,'')='')
                    //            AND isnull(B.MasterOrderItemId,'') NOT IN (
                    //            select isnull(MOI.Id,'') from trn.MasterOrderItem MOI
                    //            join trn.MasterOrder MO ON MO.Id=moi.MasterOrderId 
                    //            WHERE MOI.Type='OutSource' and isnull(MOI.consignment,0)=0 AND MO.plantId='" + identity.PlantId + @"'
                    //        )  AND (moi.ContractId='" + ContractId + @"' OR ISNULL( b.CostingBOQMasterId,'')<>'') ";

                    if (istradingPO)
                    {
                        whereClause = @"WHERE so.ContractId='" + ContractId + @"'
                                AND isnull(B.Id,'') IN (
                                select isnull(BOQ.Id,'') from BOQ
                            join trn.MasterOrderItem MOI on moi.id= BOQ.MasterOrderItemId
                            join trn.SalesOrder SO ON SO.MasterOrderItemId=moi.Id 
                            join hkp.PartyPlant P on p.PartyId= boq.VendorId

                            where P.PlantId= '" + identity.PlantId + @"' AND so.ContractId='" + ContractId + @"'
                       
                            )";

                    }


                    var sql = "";
                    sql = @"SELECT NULL AS uoMList, b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty,C.Id
						,null CheckedStatus   ,null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
						,Isnull(POMAP.TransactionQty,0) PORaisedQry, Round(ISNULL(OtherPOData.TransactionQty,0),4) OtherPOQty, Round(ISNULL(OtherPOData.TransactionQty,0),4) OtherPOQtyOrginal
						,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
						,ISNULL(cpo.PONumber,'') PONumber
						--,AUOM.AlternativeUOMId,AUOM.BaseUOMId,AUOM.BaseUOMFactor,AUOM.AlternativeUOMFactor
						--,uom1.UserName AlternateUOM
						,b.RequiredQty
						--,RequiredQty= CASE WHEN AUOM.BaseUOMFactor IS NULL THEN ROUND(isnull(b.RequiredQty,0),2) ELSE ROUND(isnull(b.BOMQty,0)/ISNULL(AUOM.BaseUOMFactor,0),2) END
						,uom.UserName BOQUOM
						--,UOM=CASE WHEN AUOM.AlternativeUOMId IS NULL then uom.UserName else  uom1.UserName END
						,b.POUoMId FromPoUomId
					    ,b.POUoMId
						--,TransactionUoMId=CASE WHEN AUOM.AlternativeUOMId IS NULL THEN b.UoMId ELSE AUOM.AlternativeUOMId END
						,b.RequiredQtyPO 
						,b.RequiredQtyPO RequiredQtyPOOrginal
						,TransactionUoMId=CASE WHEN b.POUoMId IS NULL THEN b.UoMId ELSE b.POUoMId END
						--,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '-' + ISNULL(mo.BuyerReferenceNo,'') +'-'+ ISNULL(moi.OwnReferenceNo,'')+'-'+ISNULL(moi.BuyerReferenceNo,'')
						--,RefferenceNo=ISNULL(moi.OwnReferenceNo,'') 
						,RefferenceNo=ISNULL(moi.BuyerReferenceNo,'')  ,ISNULL(DE.UserName,'') Destination
						,mm.BaseUOMId,Isnull(b.Rate,0) TransactionRate,Isnull(b.Rate,0) TransactionRateBOQ
                        ,ISNULL(uom1.UserName,'') POUoM,Round(Round(ISNULL(b.RequiredQtyPO,0),4)-Round(ISNULL(OtherPOData.TransactionQty,0),4),4) TransactionQty,0 Tolerance
						,MOI.Type,isnull(moi.Consignment,0) AS Consignment,
						 CASE WHEN isnull(moi.Consignment,0)=1 THEN
        					  CONCAT(POWN.UserName,'(',EOWN.UserName,')')	          
						ELSE
							case when isnull(MOI.JobWorkType,'')<>'' THEN 
								CASE WHEN ISNULL(eout.Id,'')<>'' THEN CONCAT(POUT.UserName,'(',EOUT.UserName,')') ELSE TOUT.UserName END
						   ELSE CONCAT(POWN.UserName,'(',EOWN.UserName,')') END
           
							 END AS PurchaseAuthority,
						   case when isnull(MOI.JobWorkType,'')<>'' THEN 
								CASE WHEN ISNULL(eout.Id,'')<>'' THEN CONCAT(POUT.UserName,'(',EOUT.UserName,')') ELSE TOUT.UserName END
						   ELSE CONCAT(POWN.UserName,'(',EOWN.UserName,')') END AS ProductionAuthority,c.Id ContractId,ISNULL(b.ItemRefNo,'') BOQItemRefNo
						   ,ISNULL(CI.UserName,'') CostingItemName,ISNULL(b.SKUDesc,'')SKUDesc,ISNULL(b.RMDescription,'')RMDescription
						   ,ISNULL(b.RMVendorSpec,'')RMVendorSpec,ISNULL(b.RMCustomerSpec,'')RMCustomerSpec
                            ,0 TrnAmount,0 MaterialTranAmount
						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId

						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId
                        left outer join mst.Destination DE ON DE.Id=so.DestinationId
						LEFT JOIN [dbo].[Contract] C ON C.Id=so.ContractId
						--LEFT JOIN(Select  BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty ,POD.TransactionUoMId 	
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id ='" + inveReveiveMasterId + @"'
									GROUP by POBOQMAP1.BOQDetailId,POD.TransactionUoMId 									
									)POMAP ON POMAP.BOQDetailId=b.Id
                         LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=POMAP.TransactionUoMId
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.POBOQQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id !='" + inveReveiveMasterId + @"'
									GROUP by POBOQMAP1.BOQDetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id

								LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
							LEFT JOIN org.Plant AS POUT ON POUT.Id=EOUT.PlantId
							LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
							LEFT JOIN org.Plant AS POWN ON POWN.Id=MO.PlantId
							LEFT JOIN org.Entity AS EOWN ON EOWN.Id=MO.EntityId
                            LEFT JOIN HKP.CostingItem CI ON CI.Id=b.CostingItemId

                        --LEFT JOIN MST.MaterialMasterAlternativeUOM AUOM ON AUOM.MaterialMasterId=mm.Id 
						--LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=AUOM.AlternativeUOMId
						
                                " + whereClause + @"


						AND ISNULL(b.isParent,0)=0 --and isChild=0
						ORDER BY b.Sequence, b.SalesOrderId";//b.MaterialMasterId,
                    var Data = _sqlRepository.GetDataCollection(sql);
                    StringCollection strCol = new StringCollection();
                    string MaterialMasterList = "''";
                    for (int i = 0; i < Data.Count; i++)
                    {
                        if (strCol.Contains(Data[i]["MaterialMasterId"].ToString()) == true)
                            continue;
                        strCol.Add(Data[i]["MaterialMasterId"].ToString());
                        MaterialMasterList += ",'" + Data[i]["MaterialMasterId"].ToString() + "'";

                    }

                    var UOMList = _sqlRepository.GetDataCollection(@"select M.Id AS MaterialMasterId, UOM1.Id AS [Value],UOM1.UserName AS [Text] from (select Id,BaseUOMId UOMId from mst.MaterialMaster
																	union
																	select MaterialMasterId,AlternativeUOMId from mst.MaterialMasterAlternativeUOM
																	) AS M
																	 JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=m.UOMId
																	 where m.Id in (" + MaterialMasterList + @")");

                    for (int i = 0; i < Data.Count; i++)
                    {
                        var temp = UOMList.Where(ee => ee["MaterialMasterId"].ToString() == Data[i]["MaterialMasterId"].ToString()).ToList();
                        Data[i]["uoMList"] = temp;
                    }

                    return Data;
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                }
            }
            else if (IsOwnVendor == "OtherVendor")
            {
                try
                {
                    var sql = "";
                    sql = @"SELECT  b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty,C.Id
						,null CheckedStatus   ,null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
						,Isnull(POMAP.TransactionQty,0) PORaisedQry,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty
						,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
						,ISNULL(cpo.PONumber,'') PONumber
						,AUOM.AlternativeUOMId,AUOM.BaseUOMId,AUOM.BaseUOMFactor,AUOM.AlternativeUOMFactor
						,uom1.UserName AlternateUOM
						,RequiredQty= CASE WHEN AUOM.BaseUOMFactor IS NULL THEN b.RequiredQty ELSE AUOM.BaseUOMFactor END
						,uom.UserName BOQUOM
						,UOM=CASE WHEN AUOM.AlternativeUOMId IS NULL then uom.UserName else  uom1.UserName END
						,TransactionUoMId=CASE WHEN AUOM.AlternativeUOMId IS NULL THEN b.UoMId ELSE AUOM.AlternativeUOMId END
						,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '/' + ISNULL(mo.BuyerReferenceNo,'') +'/'+ ISNULL(moi.OwnReferenceNo,'')+'/'+ISNULL(moi.BuyerReferenceNo,'')

						,MOI.Type,isnull(moi.Consignment,0) AS Consignment,
						 CASE WHEN isnull(moi.Consignment,0)=1 THEN
        					  CONCAT(POWN.UserName,'(',EOWN.UserName,')')	          
						ELSE
							case when isnull(MOI.JobWorkType,'')<>'' THEN 
								CASE WHEN ISNULL(eout.Id,'')<>'' THEN CONCAT(POUT.UserName,'(',EOUT.UserName,')') ELSE TOUT.UserName END
						   ELSE CONCAT(POWN.UserName,'(',EOWN.UserName,')') END
           
							 END AS PurchaseAuthority,
						   case when isnull(MOI.JobWorkType,'')<>'' THEN 
								CASE WHEN ISNULL(eout.Id,'')<>'' THEN CONCAT(POUT.UserName,'(',EOUT.UserName,')') ELSE TOUT.UserName END
						   ELSE CONCAT(POWN.UserName,'(',EOWN.UserName,')') END AS ProductionAuthority,c.Id ContractId

						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId

						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId

						LEFT JOIN [dbo].[Contract] C ON C.Id=SO.ContractId
						--LEFT JOIN(Select  BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id ='" + inveReveiveMasterId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id !='" + inveReveiveMasterId + @"'
									GROUP by POBOQMAP1.BOQDetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
								LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
							LEFT JOIN org.Plant AS POUT ON POUT.Id=EOUT.PlantId
							LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
							LEFT JOIN org.Plant AS POWN ON POWN.Id=MO.PlantId
							LEFT JOIN org.Entity AS EOWN ON EOWN.Id=MO.EntityId

						LEFT JOIN MST.MaterialMasterAlternativeUOM AUOM ON AUOM.MaterialMasterId=mm.Id 
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=AUOM.AlternativeUOMId
						WHERE so.ContractId='" + ContractId + @"' AND b.VendorId<>'" + VendorId + @"' 
						AND b.isParent=0 --and isChild=0
						ORDER BY b.MaterialMasterId,b.SalesOrderId";
                    return _sqlRepository.GetDataCollection(sql);
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                }
            }
            else
            {
                try
                {
                    var sql = "";
                    sql = @"SELECT  b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer
						,b.BOMQty,C.Id
						,null CheckedStatus   ,null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
						,Isnull(POMAP.TransactionQty,0) PORaisedQry,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty
						,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
						,ISNULL(cpo.PONumber,'') PONumber
						,AUOM.AlternativeUOMId,AUOM.BaseUOMId,AUOM.BaseUOMFactor,AUOM.AlternativeUOMFactor
						,uom1.UserName AlternateUOM
						,RequiredQty= CASE WHEN AUOM.BaseUOMFactor IS NULL THEN ROUND(isnull(b.RequiredQty,0),2) ELSE ROUND(isnull(b.BOMQty,0)/ISNULL(AUOM.BaseUOMFactor,0),2) END
						,uom.UserName BOQUOM
						,UOM=CASE WHEN AUOM.AlternativeUOMId IS NULL then uom.UserName else  uom1.UserName END
						,TransactionUoMId=CASE WHEN AUOM.AlternativeUOMId IS NULL THEN b.UoMId ELSE AUOM.AlternativeUOMId END
						,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '/' + ISNULL(mo.BuyerReferenceNo,'') +'/'+ ISNULL(moi.OwnReferenceNo,'')+'/'+ISNULL(moi.BuyerReferenceNo,'')

						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId

						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId

						LEFT JOIN [dbo].[Contract] C ON C.Id=so.ContractId
						--LEFT JOIN(Select  BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id ='" + inveReveiveMasterId + @"'
									GROUP by POBOQMAP1.BOQDetailId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id !='" + inveReveiveMasterId + @"'
									GROUP by POBOQMAP1.BOQDetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
						LEFT JOIN MST.MaterialMasterAlternativeUOM AUOM ON AUOM.MaterialMasterId=mm.Id 
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=AUOM.AlternativeUOMId
						WHERE so.ContractId='" + ContractId + @"' AND (b.VendorId='" + VendorId + @"' OR b.VendorId is null) AND b.isParent=1 
						ORDER BY b.MaterialMasterId,b.SalesOrderId";
                    return _sqlRepository.GetDataCollection(sql);
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                }
            }

        }

        public IEnumerable<object> GetBOQItemsDetailsData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                var sql = "";
                sql = @"SELECT  b.Id BOQId,b.Sequence Sequence1,b.MasterOrderItemId,moi.MasterOrderId
                                    ,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
                                    ,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

                                    ,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
                                    ,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
                                    , b.VendorId
                                    ,b.SalesOrderId
                                    ,mm.Id MaterialMasterId,mma.Id ArticleId
                                    ,IsNULL(mm.UserName,'') AS UserName
                                    ,IsNULL(mma.StandardName,'') AS StandardName
                                    ,IsNULL(p.UserName,'') AS Vendor
                                    ,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
                                    ,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
                                    ,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

                                    ,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
                                    ,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
                                    ,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
                                    ,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
                                    ,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
                                    ,b.RequiredQtyPO,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,Balance=b.RequiredQtyPO-Isnull(POMAP.TransactionQty,0)
                                    ,b.BOMQty,C.Id
                                    ,null CheckedStatus,null TaxList,MM.HSNCodeId,MM.IsOriginApplicable
                                    ,Isnull(POMAP.TransactionQty,0) PORaisedQry,ISNULL(POMAP.TransactionQty,0) OtherPOQty
                                    --,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
                                    ,ISNULL(cpo.PONumber,'') PONumber,uom.UserName BOQUOM
                                    ,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '/' + ISNULL(mo.BuyerReferenceNo,'') +'/'+ ISNULL(moi.OwnReferenceNo,'')+'/'+ISNULL(moi.BuyerReferenceNo,''),C.ContractNo
                                    ,CASE WHEN ISNULL(b.IsMainMaterial,0)=1 THEN BD.MainRawMaterialInhouseDate else BD.OtherRawMaterialInhouseDate END AS DeliveryDate
                                    ,MOI.Type,isnull(moi.Consignment,0) AS Consignment,
									 CASE WHEN isnull(moi.Consignment,0)=1 THEN
        								  CONCAT(POWN.UserName,'(',EOWN.UserName,')')	          
									ELSE
										case when isnull(MOI.JobWorkType,'')<>'' THEN 
											CASE WHEN ISNULL(eout.Id,'')<>'' THEN CONCAT(POUT.UserName,'(',EOUT.UserName,')') ELSE TOUT.UserName END
									   ELSE CONCAT(POWN.UserName,'(',EOWN.UserName,')') END
           
										 END AS PurchaseAuthority,
									   case when isnull(MOI.JobWorkType,'')<>'' THEN 
											CASE WHEN ISNULL(eout.Id,'')<>'' THEN CONCAT(POUT.UserName,'(',EOUT.UserName,')') ELSE TOUT.UserName END
									   ELSE CONCAT(POWN.UserName,'(',EOWN.UserName,')') END AS ProductionAuthority

                                    FROM BOQ AS b
                                    LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                                    LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                                    LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                                    LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                                    LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
                                    LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                                    LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId

                                    LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
									LEFT JOIN org.Plant AS POUT ON POUT.Id=EOUT.PlantId
									LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
									LEFT JOIN org.Plant AS POWN ON POWN.Id=MO.PlantId
									LEFT JOIN org.Entity AS EOWN ON EOWN.Id=MO.EntityId

                                    left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId

                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

                                    LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
                                    LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
                                    LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId

                                    LEFT JOIN [dbo].[Contract] C ON C.Id=so.ContractId
                                    LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty 
			                                     FROM [TRN].[POBOQMAP] POBOQMAP1
			                                    LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
			                                    LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId									
			                                    GROUP by POBOQMAP1.BOQDetailId								
			                                    )POMAP ON POMAP.BOQDetailId=b.Id

                                LEFT JOIN (Select BOQId,FORMAT(MIN(B.MainRawMaterialInhouseDate),'dd-MMM-yyyy') MainRawMaterialInhouseDate
							    ,FORMAT(MIN(B.OtherRawMaterialInhouseDate),'dd-MMM-yyyy') OtherRawMaterialInhouseDate FROM BOQDetail A
							    LEFT OUTER JOIN TRN.SalesOrder B ON B.Id=A.SalesOrderId
							    GROUP BY BOQId
							    ) BD ON BD.BOQId=B.Id

                                    WHERE (b.RequiredQtyPO-Isnull(POMAP.TransactionQty,0))>0
                                    ORDER BY b.MaterialMasterId,b.SalesOrderId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }


        }

        public IEnumerable<object> GetCostingBOQItems(string CostingItemIds, string CostingBOQMasterIds, string ContractId, string VendorId, string IsOwnVendor, string inveReveiveMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (clsStaticInfo.nullrecorder(CostingItemIds) != "")
            {
                CostingItemIds = " AND isnull(B.CostingItemId,'') IN (" + CostingItemIds + @") AND isnull(B.CostingBOQMasterId,'') IN (" + CostingBOQMasterIds + @")";
            }
            try
            {
                var sql = "";
                sql = @"SELECT NULL AS uoMList, b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,'Yes' AS RequiredQtyApproved--=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END 
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty,C.Id
						,null CheckedStatus   ,null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
						,Isnull(POMAP.TransactionQty,0) PORaisedQry, Round(ISNULL(OtherPOData.TransactionQty,0),2) OtherPOQty,ISNULL(OtherPOData.TransactionQty,0) OtherPOQtyOrginal
						,REPLACE(CONVERT(CHAR(11), so.DeliveryDate, 106),' ','-') AS DeliveryDate 
						,ISNULL(cpo.PONumber,'') PONumber
						--,AUOM.AlternativeUOMId,AUOM.BaseUOMId,AUOM.BaseUOMFactor,AUOM.AlternativeUOMFactor
						--,uom1.UserName AlternateUOM
						,b.RequiredQty
						--,RequiredQty= CASE WHEN AUOM.BaseUOMFactor IS NULL THEN ROUND(isnull(b.RequiredQty,0),2) ELSE ROUND(isnull(b.BOMQty,0)/ISNULL(AUOM.BaseUOMFactor,0),2) END
						,uom.UserName BOQUOM
						--,UOM=CASE WHEN AUOM.AlternativeUOMId IS NULL then uom.UserName else  uom1.UserName END
						,b.POUoMId FromPoUomId
					    ,b.POUoMId
						--,TransactionUoMId=CASE WHEN AUOM.AlternativeUOMId IS NULL THEN b.UoMId ELSE AUOM.AlternativeUOMId END
						,b.RequiredQtyPO 
						,b.RequiredQtyPO RequiredQtyPOOrginal
						,TransactionUoMId=CASE WHEN b.POUoMId IS NULL THEN b.UoMId ELSE b.POUoMId END
						--,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '-' + ISNULL(mo.BuyerReferenceNo,'') +'-'+ ISNULL(moi.OwnReferenceNo,'')+'-'+ISNULL(moi.BuyerReferenceNo,'')
						,RefferenceNo=ISNULL(moi.OwnReferenceNo,'') 
						,mm.BaseUOMId,Isnull(b.Rate,0) TransactionRate,ISNULL(uom1.UserName,'') POUoM
						,ci.UserName AS CostingItem,d.UserName AS Destination 
						FROM BOQ AS b
						JOIN hkp.CostingItem AS ci ON ci.Id=b.CostingItemId
						LEFT JOIN mst.Destination AS d ON d.Id=b.DestinationId
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId

						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId

						LEFT JOIN [dbo].[Contract] C ON C.Id=so.ContractId
						--LEFT JOIN(Select  BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN (SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty ,POD.TransactionUoMId 	
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id ='" + inveReveiveMasterId + @"'
									GROUP by POBOQMAP1.BOQDetailId,POD.TransactionUoMId 									
									)POMAP ON POMAP.BOQDetailId=b.Id
                         LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=POMAP.TransactionUoMId
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.POBOQQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id !='" + inveReveiveMasterId + @"'
									GROUP by POBOQMAP1.BOQDetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
                        --LEFT JOIN MST.MaterialMasterAlternativeUOM AUOM ON AUOM.MaterialMasterId=mm.Id 
						--LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=AUOM.AlternativeUOMId
						WHERE --so.ContractId='" + ContractId + @"' AND 
						(b.VendorId='" + VendorId + @"' OR b.VendorId is null) " + CostingItemIds + @"
						--AND b.isParent=0 --and isChild=0
						ORDER BY b.Sequence, b.SalesOrderId";//b.MaterialMasterId,
                var Data = _sqlRepository.GetDataCollection(sql);
                StringCollection strCol = new StringCollection();
                string MaterialMasterList = "''";
                for (int i = 0; i < Data.Count; i++)
                {
                    if (strCol.Contains(Data[i]["MaterialMasterId"].ToString()) == true)
                        continue;
                    strCol.Add(Data[i]["MaterialMasterId"].ToString());
                    MaterialMasterList += ",'" + Data[i]["MaterialMasterId"].ToString() + "'";

                }

                var UOMList = _sqlRepository.GetDataCollection(@"select M.Id AS MaterialMasterId, UOM1.Id AS [Value],UOM1.UserName AS [Text] from (select Id,BaseUOMId UOMId from mst.MaterialMaster
																	union
																	select MaterialMasterId,AlternativeUOMId from mst.MaterialMasterAlternativeUOM
																	) AS M
																	 JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=m.UOMId
																	 where m.Id in (" + MaterialMasterList + @")");

                for (int i = 0; i < Data.Count; i++)
                {
                    var temp = UOMList.Where(ee => ee["MaterialMasterId"].ToString() == Data[i]["MaterialMasterId"].ToString()).ToList();
                    Data[i]["uoMList"] = temp;
                }

                return Data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }


        }
        public IEnumerable<object> GetCostingBOQItemsListForUpdate(string VendorId, string inveReveiveId, string inveReveiveMasterId, string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId)
        {
            Library.General.Conversions.UOMConversion conversion = new Library.General.Conversions.UOMConversion();
            try
            {
                if (FirstCharacteristicsValueId == "null" || string.IsNullOrEmpty(FirstCharacteristicsValueId))
                {
                    FirstCharacteristicsValueId = "";
                }
                if (SecondCharacteristicsValueId == "null" || string.IsNullOrEmpty(FirstCharacteristicsValueId))
                {
                    SecondCharacteristicsValueId = "";
                }
                if (ThirdCharacteristicsValueId == "null" || string.IsNullOrEmpty(FirstCharacteristicsValueId))
                {
                    ThirdCharacteristicsValueId = "";
                }
                var _sql = @"SELECT Distinct map.Id AS SavedPOBOQId,b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END  
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty,b.RequiredQtyPO RequiredQtyPO
						,b.RequiredQtyPO RequiredQtyPOOrginal
						,uom.UserName AS UOM,C.Id
						,uom1.Id TransactionUoMId,CheckedStatus=convert(bit,CASE WHEN POMAP.PODetailId IS NULL then 0 else 1 end),null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
                        ,Isnull(POMAP.TransactionQty,0) PORaisedQry,POMAP.PODetailId InventoryReceiveDetailId,Isnull(POMAP.TransactionQty,0) TransactionQty 
						,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty,ISNULL(OtherPOData.TransactionQty,0) OtherPOQtyOrginal
						--,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '-' + ISNULL(mo.BuyerReferenceNo,'') +'-'+ ISNULL(moi.OwnReferenceNo,'') +'-'+ISNULL(moi.BuyerReferenceNo,'')
						,RefferenceNo=ISNULL(moi.OwnReferenceNo,'') 
						,POMAP.TransactionRate,POMAP.DeliveryDate,b.POUoMId,mm.BaseUOMId,uom1.UserName POUoM,uom.UserName BOQUOM
					    ,b.POUoMId FromPoUomId
					    ,b.POUoMId
						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId
						LEFT JOIN [dbo].[Contract] C ON C.Id=so.ContractId
						--LEFT JOIN(Select  PODetailId,BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId,PODetailId)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN (SELECT  POBOQMAP1.PODetailId,POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty ,sum(POD.TransactionRate) TransactionRate,REPLACE(CONVERT(CHAR(11), POD.DeliveryDate, 106),' ','-') AS DeliveryDate ,POD.TransactionUoMId
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id ='" + inveReveiveMasterId + @"'
									GROUP by POBOQMAP1.BOQDetailId,POBOQMAP1.PODetailId,POD.DeliveryDate,POD.TransactionUoMId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						 LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=POMAP.TransactionUoMId
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.POBOQQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id !='" + inveReveiveMasterId + @"'
									GROUP by POBOQMAP1.BOQDetailId--,POBOQMAP1.PODetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
							left join trn.POBOQMAP MAP ON MAP.BOQDetailId=B.Id and MAP.PODetailId in (select Id from TRN.PurchaseOrderDetail xd where xd.InventoryReceiveId='" + inveReveiveMasterId + @"'  )
						where POMAP.PODetailId='" + inveReveiveId + @"'
						UNION ALL
						SELECT Distinct '' SavedPOBOQId,b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END  
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty,b.RequiredQty RequiredQtyPO,b.RequiredQtyPO RequiredQtyPOOrginal,uom.UserName AS UOM,C.Id
						,b.UoMId TransactionUoMId,null CheckedStatus   ,null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
                        ,0 PORaisedQry,'' InventoryReceiveDetailId, 0 TransactionQty
						,0 OtherPOQty,0 OtherPOQtyOrginal
						,RefferenceNo=ISNULL(moi.OwnReferenceNo,'')
						,b.Rate TransactionRate,'' DeliveryDate,b.POUoMId,mm.BaseUOMId,'' POUoM,uom.UserName BOQUOM
						,b.POUoMId FromPoUomId
					    ,b.POUoMId
						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId
						LEFT JOIN [dbo].[Contract] C ON C.Id=so.ContractId	
                        LEFT JOIN [TRN].[POBOQMAP] a ON a.BOQDetailId=b.Id
						where (b.VendorId='" + VendorId + @"' OR b.VendorId is null) AND mm.Id='" + MaterialMasterId + @"' AND mma.Id='" + ArticleId + @"' AND ISNULL(b.FirstCharacteristicsValueId,'')='" + FirstCharacteristicsValueId + @"' AND ISNULL(b.SecondCharacteristicsValueId,'')='" + SecondCharacteristicsValueId + @"' AND ISNULL(b.ThirdCharacteristicsValueId,'')='" + ThirdCharacteristicsValueId + @"' AND b.Id not in(select a.Id FROM [TRN].[POBOQMAP] b join BOQ a on a.Id=b.BOQDetailId where b.PODetailId='" + inveReveiveId + @"') --AND b.Id not in(select BOQDetailId  FROM [TRN].[POBOQMAP]) 
						ORDER BY b.Sequence,map.Id DESC";// b.SalesOrderId
                                                         //WHERE IM.MaterialMasterId='" + MaterialMasterId + "' and ArticleId='" + ArticleId + "' and IM.FirstCharacteristicsValueId='" + FirstCharacteristicsValueId + "' And IM.PORcvQty=0";
                var Data = _sqlRepository.GetDataCollection(_sql);
                //string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId
                StringCollection strCol = new StringCollection();
                string MaterialMasterList = "''";
                for (int i = 0; i < Data.Count; i++)
                {
                    if (strCol.Contains(Data[i]["MaterialMasterId"].ToString()) == true)
                        continue;
                    strCol.Add(Data[i]["MaterialMasterId"].ToString());
                    MaterialMasterList += ",'" + Data[i]["MaterialMasterId"].ToString() + "'";

                }

                var UOMList = _sqlRepository.GetDataCollection(@"select M.Id AS MaterialMasterId, UOM1.Id AS [Value],UOM1.UserName AS [Text] from (select Id,BaseUOMId UOMId from mst.MaterialMaster
																	union
																	select MaterialMasterId,AlternativeUOMId from mst.MaterialMasterAlternativeUOM
																	) AS M
																	 JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=m.UOMId
																	 where m.Id in (" + MaterialMasterList + @")");

                for (int i = 0; i < Data.Count; i++)
                {
                    var temp = UOMList.Where(ee => ee["MaterialMasterId"].ToString() == Data[i]["MaterialMasterId"].ToString()).ToList();
                    Data[i]["uoMList"] = temp;
                    //Data[i]["OtherPOQty"] = conversion.Convert(Data[i]["MaterialMasterId"].ToString(), Data[i]["FromPoUomId"].ToString(), Data[i]["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(Data[i]["OtherPOQtyOrginal"].ToString())).ToString("F2");
                    //Data[i]["OtherPOQty"] = conversion.Convert(Data[i]["MaterialMasterId"].ToString(), Data[i]["POUoMId"].ToString(), Data[i]["ToUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(Data[i]["OtherPOQtyOrginal"].ToString())).ToString("F2");
                    Data[i]["OtherPOQty"] = conversion.Convert(Data[i]["MaterialMasterId"].ToString(), Data[i]["POUoMId"].ToString(), Data[i]["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(Data[i]["OtherPOQtyOrginal"].ToString())).ToString("F2");
                }

                return Data;




            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        //string inveReveiveId, string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId
        //sk3
        public IEnumerable<object> GetBOQItemsListForUpdate(string ContractId, string VendorId, string inveReveiveId, string inveReveiveMasterId, string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId)
        {
            Library.General.Conversions.UOMConversion conversion = new Library.General.Conversions.UOMConversion();
            try
            {
                if (FirstCharacteristicsValueId == "null" || string.IsNullOrEmpty(FirstCharacteristicsValueId))
                {
                    FirstCharacteristicsValueId = "";
                }
                if (SecondCharacteristicsValueId == "null" || string.IsNullOrEmpty(FirstCharacteristicsValueId))
                {
                    SecondCharacteristicsValueId = "";
                }
                if (ThirdCharacteristicsValueId == "null" || string.IsNullOrEmpty(FirstCharacteristicsValueId))
                {
                    ThirdCharacteristicsValueId = "";
                }
                var _sql = @"SELECT Distinct map.Id AS SavedPOBOQId,b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END  
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty,b.RequiredQtyPO RequiredQtyPO
						,b.RequiredQtyPO RequiredQtyPOOrginal
						,uom.UserName AS UOM,C.Id
						,uom1.Id TransactionUoMId,CheckedStatus=convert(bit,CASE WHEN POMAP.PODetailId IS NULL then 0 else 1 end),null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
                        ,Isnull(POMAP.TransactionQty,0) PORaisedQry,POMAP.PODetailId InventoryReceiveDetailId,Isnull(POMAP.TransactionQty,0) TransactionQty 
						,ISNULL(OtherPOData.TransactionQty,0) OtherPOQty,ISNULL(OtherPOData.TransactionQty,0) OtherPOQtyOrginal
						--,RefferenceNo=ISNULL(mo.OwnReferenceNo,'') + '-' + ISNULL(mo.BuyerReferenceNo,'') +'-'+ ISNULL(moi.OwnReferenceNo,'') +'-'+ISNULL(moi.BuyerReferenceNo,'')
						,RefferenceNo=ISNULL(moi.BuyerReferenceNo,'') 
						,POMAP.TransactionRate,POMAP.DeliveryDate,b.POUoMId,mm.BaseUOMId,uom1.UserName POUoM,uom.UserName BOQUOM
					    ,b.POUoMId FromPoUomId
					    ,b.POUoMId

						,MOI.Type,isnull(moi.Consignment,0) AS Consignment,
						 CASE WHEN isnull(moi.Consignment,0)=1 THEN
        					  CONCAT(POWN.UserName,'(',EOWN.UserName,')')	          
						ELSE
							case when isnull(MOI.JobWorkType,'')<>'' THEN 
								CASE WHEN ISNULL(eout.Id,'')<>'' THEN CONCAT(POUT.UserName,'(',EOUT.UserName,')') ELSE TOUT.UserName END
						   ELSE CONCAT(POWN.UserName,'(',EOWN.UserName,')') END
           
							 END AS PurchaseAuthority,
						   case when isnull(MOI.JobWorkType,'')<>'' THEN 
								CASE WHEN ISNULL(eout.Id,'')<>'' THEN CONCAT(POUT.UserName,'(',EOUT.UserName,')') ELSE TOUT.UserName END
						   ELSE CONCAT(POWN.UserName,'(',EOWN.UserName,')') END AS ProductionAuthority,c.Id ContractId

						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId
						LEFT JOIN [dbo].[Contract] C ON C.Id=so.ContractId
						--LEFT JOIN(Select  PODetailId,BOQDetailId,sum(TransactionQty) TransactionQty from [TRN].[POBOQMAP] group by BOQDetailId,PODetailId)POMAP ON POMAP.BOQDetailId=b.Id
						LEFT JOIN (SELECT  POBOQMAP1.PODetailId,POBOQMAP1.BOQDetailId,sum(POBOQMAP1.TransactionQty) TransactionQty ,sum(POD.TransactionRate) TransactionRate,REPLACE(CONVERT(CHAR(11), POD.DeliveryDate, 106),' ','-') AS DeliveryDate ,POD.TransactionUoMId
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id ='" + inveReveiveMasterId + @"'
									GROUP by POBOQMAP1.BOQDetailId,POBOQMAP1.PODetailId,POD.DeliveryDate,POD.TransactionUoMId								
									)POMAP ON POMAP.BOQDetailId=b.Id
						 LEFT OUTER JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=POMAP.TransactionUoMId
						LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.POBOQQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id !='" + inveReveiveMasterId + @"'
									GROUP by POBOQMAP1.BOQDetailId--,POBOQMAP1.PODetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
							left join trn.POBOQMAP MAP ON MAP.BOQDetailId=B.Id and MAP.PODetailId in (select Id from TRN.PurchaseOrderDetail xd where xd.InventoryReceiveId='" + inveReveiveMasterId + @"'  )
						LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
							LEFT JOIN org.Plant AS POUT ON POUT.Id=EOUT.PlantId
							LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
							LEFT JOIN org.Plant AS POWN ON POWN.Id=MO.PlantId
							LEFT JOIN org.Entity AS EOWN ON EOWN.Id=MO.EntityId

						where POMAP.PODetailId='" + inveReveiveId + @"'
						UNION ALL
						SELECT Distinct '' SavedPOBOQId,b.Id BOQId,b.Sequence Sequence1
						,b.MasterOrderItemId
						,moi.MasterOrderId
						,ISNULL(mo.OwnReferenceNo,'') OwnOrderReferenceNo
						,ISNULL(mo.BuyerReferenceNo,'') BuyerOrderReferenceNo

						,ISNULL(moi.OwnReferenceNo,'') OwnItemReferenceNo
						,ISNULL(moi.BuyerReferenceNo,'') BuyerItemReferenceNo
						, b.VendorId
						,b.SalesOrderId
						,mm.Id MaterialMasterId,mma.Id ArticleId
						,IsNULL(mm.UserName,'') AS UserName
						,IsNULL(mma.StandardName,'') AS StandardName
						,IsNULL(p.UserName,'') AS Vendor
						,IsNULL(v1.UserName,'') AS FirstCharacteristicsValue
						,IsNULL(v2.UserName,'') AS SecondCharacteristicsValue
						,IsNULL(v3.UserName,'') AS ThirdCharacteristicsValue

						,b.FirstCharacteristicsValueId,FC.Id FirstCharacteristicsId
						,b.SecondCharacteristicsValueId,SC.Id FirstCharacteristicsId
						,b.ThirdCharacteristicsValueId,TC.Id ThirdCharacteristicsId
						,RequiredQtyApproved=Case When CONVERT(BIT, isnull(b.RequiredQtyApproved,0))=0 Then 'No' ELSE 'Yes' END
						,IncompleteMaterial=CASE WHEN CONVERT(BIT, isnull(b.IncompleteMaterial,0))=1 THEN 'Yes' ELSE 'No' END  
						,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
						b.BOMQty,b.RequiredQty RequiredQtyPO,b.RequiredQtyPO RequiredQtyPOOrginal,uom.UserName AS UOM,C.Id
						,b.UoMId TransactionUoMId,null CheckedStatus   ,null TaxList,MM.HSNCodeId	,MM.IsOriginApplicable
                        ,0 PORaisedQry,'' InventoryReceiveDetailId, 0 TransactionQty
						,OtherPOData.TransactionQty OtherPOQty,OtherPOData.TransactionQty OtherPOQtyOrginal
						,RefferenceNo=ISNULL(moi.BuyerReferenceNo,'')
						,b.Rate TransactionRate,'' DeliveryDate,b.POUoMId,mm.BaseUOMId,'' POUoM,uom.UserName BOQUOM
						,b.POUoMId FromPoUomId
					    ,b.POUoMId

						,MOI.Type,isnull(moi.Consignment,0) AS Consignment,
						 CASE WHEN isnull(moi.Consignment,0)=1 THEN
        					  CONCAT(POWN.UserName,'(',EOWN.UserName,')')	          
						ELSE
							case when isnull(MOI.JobWorkType,'')<>'' THEN 
								CASE WHEN ISNULL(eout.Id,'')<>'' THEN CONCAT(POUT.UserName,'(',EOUT.UserName,')') ELSE TOUT.UserName END
						   ELSE CONCAT(POWN.UserName,'(',EOWN.UserName,')') END
           
							 END AS PurchaseAuthority,
						   case when isnull(MOI.JobWorkType,'')<>'' THEN 
								CASE WHEN ISNULL(eout.Id,'')<>'' THEN CONCAT(POUT.UserName,'(',EOUT.UserName,')') ELSE TOUT.UserName END
						   ELSE CONCAT(POWN.UserName,'(',EOWN.UserName,')') END AS ProductionAuthority,c.Id ContractId

						FROM BOQ AS b
						LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
						LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
						LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
						LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
						LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
						LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
						LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
						left outer join [TRN].[CustomerPO] cpo On cpo.Id=so.CustomerPOId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
						LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

						LEFT JOIN HKP.Characteristics AS FC ON FC.Id=V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id=V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id=V3.CharacteristicsId
						LEFT JOIN [dbo].[Contract] C ON C.Id=so.ContractId	
                        --LEFT JOIN [TRN].[POBOQMAP] a ON a.BOQDetailId=b.Id

						LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
							LEFT JOIN org.Plant AS POUT ON POUT.Id=EOUT.PlantId
							LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
							LEFT JOIN org.Plant AS POWN ON POWN.Id=MO.PlantId
							LEFT JOIN org.Entity AS EOWN ON EOWN.Id=MO.EntityId

                       LEFT JOIN(SELECT  POBOQMAP1.BOQDetailId,sum(POBOQMAP1.POBOQQty) TransactionQty 
									FROM [TRN].[POBOQMAP] POBOQMAP1
									LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=POBOQMAP1.PODetailId
									LEFT JOIN TRN.PurchaseOrder POM ON POM.Id=POD.InventoryReceiveId
									where POM.Id !='" + inveReveiveMasterId + @"'
									GROUP by POBOQMAP1.BOQDetailId--,POBOQMAP1.PODetailId
								) OtherPOData ON OtherPOData.BOQDetailId=b.Id
						where so.ContractId='" + ContractId + @"' AND (b.VendorId='" + VendorId + @"' OR b.VendorId is null) AND mm.Id='" + MaterialMasterId + @"' AND mma.Id='" + ArticleId + @"' AND ISNULL(b.FirstCharacteristicsValueId,'')='" + FirstCharacteristicsValueId + @"' AND ISNULL(b.SecondCharacteristicsValueId,'')='" + SecondCharacteristicsValueId + @"' AND ISNULL(b.ThirdCharacteristicsValueId,'')='" + ThirdCharacteristicsValueId + @"' AND b.Id not in(select a.Id FROM [TRN].[POBOQMAP] b join BOQ a on a.Id=b.BOQDetailId where b.PODetailId='" + inveReveiveId + @"') --AND b.Id not in(select BOQDetailId  FROM [TRN].[POBOQMAP]) 
						ORDER BY b.Sequence,map.Id DESC";// b.SalesOrderId
                                                         //WHERE IM.MaterialMasterId='" + MaterialMasterId + "' and ArticleId='" + ArticleId + "' and IM.FirstCharacteristicsValueId='" + FirstCharacteristicsValueId + "' And IM.PORcvQty=0";
                var Data = _sqlRepository.GetDataCollection(_sql);
                //string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId
                StringCollection strCol = new StringCollection();
                string MaterialMasterList = "''";
                for (int i = 0; i < Data.Count; i++)
                {
                    if (strCol.Contains(Data[i]["MaterialMasterId"].ToString()) == true)
                        continue;
                    strCol.Add(Data[i]["MaterialMasterId"].ToString());
                    MaterialMasterList += ",'" + Data[i]["MaterialMasterId"].ToString() + "'";

                }

                var UOMList = _sqlRepository.GetDataCollection(@"select M.Id AS MaterialMasterId, UOM1.Id AS [Value],UOM1.UserName AS [Text] from (select Id,BaseUOMId UOMId from mst.MaterialMaster
																	union
																	select MaterialMasterId,AlternativeUOMId from mst.MaterialMasterAlternativeUOM
																	) AS M
																	 JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=m.UOMId
																	 where m.Id in (" + MaterialMasterList + @")");

                for (int i = 0; i < Data.Count; i++)
                {
                    var temp = UOMList.Where(ee => ee["MaterialMasterId"].ToString() == Data[i]["MaterialMasterId"].ToString()).ToList();
                    Data[i]["uoMList"] = temp;
                    //Data[i]["OtherPOQty"] = conversion.Convert(Data[i]["MaterialMasterId"].ToString(), Data[i]["FromPoUomId"].ToString(), Data[i]["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(Data[i]["OtherPOQtyOrginal"].ToString())).ToString("F2");
                    //Data[i]["OtherPOQty"] = conversion.Convert(Data[i]["MaterialMasterId"].ToString(), Data[i]["POUoMId"].ToString(), Data[i]["ToUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(Data[i]["OtherPOQtyOrginal"].ToString())).ToString("F2");
                    //if (!string.IsNullOrEmpty(Data[i]["SavedPOBOQId"].ToString()))
                    //{
                    Data[i]["OtherPOQty"] = conversion.Convert(Data[i]["MaterialMasterId"].ToString(), Data[i]["POUoMId"].ToString(), Data[i]["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(Data[i]["OtherPOQtyOrginal"].ToString())).ToString("F2");
                    Data[i]["OtherPOQtyOrginal"] = conversion.Convert(Data[i]["MaterialMasterId"].ToString(), Data[i]["POUoMId"].ToString(), Data[i]["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(Data[i]["OtherPOQtyOrginal"].ToString())).ToString("F2");

                    Data[i]["RequiredQtyPO"] = conversion.Convert(Data[i]["MaterialMasterId"].ToString(), Data[i]["FromPoUomId"].ToString(), Data[i]["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(Data[i]["RequiredQtyPOOrginal"].ToString())).ToString("F2");

                    // }
                }

                return Data;




            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> ContractWiseData(string ContractId)
        {
            try
            {
                var _sql = @"SELECT C.Id ContractId
							,c.CustomerId
							,c.IsLC
							,c.AddedBy
							,c.AddedDate
							,c.AddedFromIP
							,C.UpdatedBy
							,C.UpdatedDate
							,C.UpdatedFromIP
							, P.UserName AS CustomerName
							, MLC.Id MasterLCNo
							,MLC.LCRef
							,C.ContractNo
							,[Buyer]=STUFF((select distinct ','+B.UserName from
									trn.MasterOrder XMOI
									LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
									LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
									LEFT JOIN trn.SalesOrder SO ON SO.MasterOrderItemId=I.Id
									where SO.ContractId=C.Id for xml path('') ), 1, 1, ''
									)
							FROM [dbo].[Contract] C
							JOIN [HKP].[Party] AS P ON C.CustomerId=P.Id
							LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=C.MasterLCId--MLC ON MLC.ContractId=C.Id
							where c.Id='" + ContractId + @"'
							ORDER BY C.CustomerId";
                //WHERE IM.MaterialMasterId='" + MaterialMasterId + "' and ArticleId='" + ArticleId + "' and IM.FirstCharacteristicsValueId='" + FirstCharacteristicsValueId + "' And IM.PORcvQty=0";
                return _sqlRepository.GetDataCollection(_sql);
                //string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }



        public IEnumerable<object> PODocumentMapData(string POID)
        {
            try
            {
                var _sql = @"SELECT
								Id
							  ,CompanyGroupId
							  
							  ,POId
							  ,UserFilename 
							  ,SystemFileName
							  ,Description
							  ,Remarks
							  ,AddedBy
							  ,AddedDate
							  ,AddedFromIP
							  ,UpdatedBy
							  ,UpdatedDate
							  ,UpdatedFromIP
						  FROM [TRN].[PODocumentMap] 
							where POId='" + POID + @"'
							ORDER BY UserFilename";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> ServicePODocumentMap(string POID)
        {
            try
            {
                var _sql = @"SELECT
								Id
							  ,CompanyGroupId
							  
							  ,ServicePOMasterId
							  ,UserFilename 
							  ,SystemFileName
							  ,Description
							  ,Remarks
							  ,AddedBy
							  ,AddedDate
							  ,AddedFromIP
							  ,UpdatedBy
							  ,UpdatedDate
							  ,UpdatedFromIP
						  FROM [TRN].[ServicePODocumentMap] 
							where ServicePOMasterId='" + POID + @"'
							ORDER BY UserFilename";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> ServicePOAckDocumentMap(string POID)
        {
            try
            {
                var _sql = @"SELECT
								Id
							  ,CompanyGroupId
							  
							  ,ServiceAcknowledgementMasterId
							  ,UserFilename 
							  ,SystemFileName
							  ,Description
							  ,Remarks
							  ,AddedBy
							  ,AddedDate
							  ,AddedFromIP
							  ,UpdatedBy
							  ,UpdatedDate
							  ,UpdatedFromIP
						  FROM [TRN].[ServicePOAckDocumentMap] 
							where ServiceAcknowledgementMasterId='" + POID + @"'
							ORDER BY UserFilename";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> PODocumentMapDataAll(string POID)
        {
            try
            {
                var _sql = @"DECLARE @pathval varchar(200)='POPResources/PurchaseOrder'
							SELECT POId,Remarks,'<a href='''  + @pathval+'/'+SystemFileName + ''' target=''_blank''>'+ UserFilename +'</a>' As UserFilename,Description
							--stuff(
							--(
							--  SELECT '<a href=''' + SystemFileName + ''' target=''_blank''>'+ UserFilename +'</a>'
							--  FROM [TRN].[GRNDocumentMap] 	 WHERE GRNId = t.GRNId FOR XML path('')
							--),1,1,' ') UserFilename
							FROM (select Id,CompanyGroupId	,POId,UserFilename ,SystemFileName,Description,Remarks,AddedBy,AddedDate,AddedFromIP,UpdatedBy,UpdatedDate,UpdatedFromIP 
							FROM [TRN].[PODocumentMap] )t
							ORDER BY t.UserFilename";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> ServicePODocumentMapDataAll(string POID)
        {
            try
            {
                var _sql = @"DECLARE @pathval varchar(200)='POPResources/ServicePO'
							SELECT ServicePOMasterId,Remarks,'<a href='''  + @pathval+'/'+SystemFileName + ''' target=''_blank''>'+ UserFilename +'</a>' As UserFilename,Description
							--stuff(
							--(
							--  SELECT '<a href=''' + SystemFileName + ''' target=''_blank''>'+ UserFilename +'</a>'
							--  FROM [TRN].[GRNDocumentMap] 	 WHERE GRNId = t.GRNId FOR XML path('')
							--),1,1,' ') UserFilename
							FROM (select Id,CompanyGroupId	,ServicePOMasterId,UserFilename ,SystemFileName,Description,Remarks,AddedBy,AddedDate,AddedFromIP,UpdatedBy,UpdatedDate,UpdatedFromIP 
							FROM [TRN].[ServicePODocumentMap] )t
							ORDER BY t.UserFilename";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> ServicePOAckDocumentMapDataAll(string POID)
        {
            try
            {
                var _sql = @"DECLARE @pathval varchar(200)='POPResources/ServicePOAck'
							SELECT ServiceAcknowledgementMasterId,Remarks,'<a href='''  + @pathval+'/'+SystemFileName + ''' target=''_blank''>'+ UserFilename +'</a>' As UserFilename,Description
							--stuff(
							--(
							--  SELECT '<a href=''' + SystemFileName + ''' target=''_blank''>'+ UserFilename +'</a>'
							--  FROM [TRN].[GRNDocumentMap] 	 WHERE GRNId = t.GRNId FOR XML path('')
							--),1,1,' ') UserFilename
							FROM (select Id,CompanyGroupId	,ServiceAcknowledgementMasterId,UserFilename ,SystemFileName,Description,Remarks,AddedBy,AddedDate,AddedFromIP,UpdatedBy,UpdatedDate,UpdatedFromIP 
							FROM [TRN].[ServicePOAckDocumentMap] )t
							ORDER BY t.UserFilename";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GRNDocumentMapData(string POID)
        {
            try
            {
                var _sql = @"SELECT
								Id
							  ,CompanyGroupId							  
							  ,GRNId
							  ,UserFilename 
							  ,SystemFileName
							  ,Description
							  ,Remarks
							  ,AddedBy
							  ,AddedDate
							  ,AddedFromIP
							  ,UpdatedBy
							  ,UpdatedDate
							  ,UpdatedFromIP
						  FROM [TRN].[GRNDocumentMap] 
							where GRNId='" + POID + @"'
							ORDER BY UserFilename";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GRNDocumentMapDataBOQ(string POID)
        {
            try
            {
                var _sql = @"SELECT
								Id
							  ,CompanyGroupId							  
							  ,GRNId
							  ,UserFilename 
							  ,SystemFileName
							  ,Description
							  ,Remarks
							  ,AddedBy
							  ,AddedDate
							  ,AddedFromIP
							  ,UpdatedBy
							  ,UpdatedDate
							  ,UpdatedFromIP
						  FROM [TRN].[GRNDocumentMap] 
							where GRNId='" + POID + @"'
							ORDER BY UserFilename";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GRNImageDelete(string Id)
        {
            try
            {
                var _sql = @" Delete from [TRN].[GRNDocumentMap] where Id='" + Id + @"'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> ServicePOImageDelete(string Id)
        {
            try
            {
                var _sql = @"Delete from [TRN].[ServicePODocumentMap] where Id='" + Id + @"'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> ServicePOAckImageDelete(string Id)
        {
            try
            {
                var _sql = @"Delete from [TRN].[ServicePOAckDocumentMap] where Id='" + Id + @"'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> PurchaseOrderRegisterData(string fromDate, string toDate, string Type,bool isClose)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var tempQuery = " ";
            if (isClose)
            {
                tempQuery = " AND IR.IsClosed=1 ";
            }
            try
            {
                var _sql = @"SELECT --ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo,
							IR.Id PONo
                            ,POType= CASE WHEN IR.POType='PO' Then 'Individual PO' ELSE 'Requisition Based PO' END
							, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
													WHEN TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
													WHEN TAxInfo2.HSCode<>'' then TAxInfo2.HSCode ELSE '' END
						,IM.InventoryReceiveId AS PORowId
						,REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate ,ISNULL(IR.DocRefNo,'') DocRefNo
						,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate ,p.UserName AS PartyName
						,IR.PartyId ,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
						,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
						,MT.UserName MaterialType ,MGM.UserName AS MaterialGroupMasterName ,IM.InventoryMaterialId MaterialMasterId
						,MM.UserName MaterialMasterName , ART.StandardName ArticleName , ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue , ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue ,TUoM.UserName AS UOM
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,IM.TransactionQty,Isnull(GRN.GRNQty,0) ReceiptQty,0 RejectionQty
						,(IM.TransactionQty-Isnull(GRN.GRNQty,0)) BalanceQty,IM.Tolerance
						,ROUND(Isnull(IM.TransactionRate,0),2) TransactionRate
						,ROUND(Isnull(IM.TransactionAmount,0),2) TransactionAmount
						,ROUND(Isnull(IM.TotalTaxAmount,0),2) TotalTaxAmount
						,ROUND(Isnull(IM.ChargesAmount,0),2) ServiceCharge
                        ,Isnull(servicetax.TaxAmount,0) ServiceChargeTax
						,ROUND(Isnull(IM.BaseAmount,0),2) BaseAmount ,IR.AddedBy
						,CASE WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
						WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'
						WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null Then 'To be approved'
						WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
						WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
						WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
						WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected' END GRNCheckStatus
						,ISNULL(EI1.EmployeeName,'') CheckedBY ,EI2.EmployeeName AuthorizedBy
						,ROUND(ISNULL(TAxInfo.TaxAmount,0),2) CGST,isnull(TAxInfo.Percentage,0) CGSTTaxPercentage--MaterialTaxPer
						,ROUND(ISNULL(TAxInfo2.TaxAmount,0),2) SGST,isnull(TAxInfo2.Percentage,0) SGSTTaxPercentage
						,ROUND(ISNULL(TAxInfo1.TaxAmount,0),2) IGST,isnull(TAxInfo1.Percentage,0) IGSTTaxPercentage
						,ROUND(ISNULL(TAxInfo3.TaxAmount,0),2) TDS,isnull(TAxInfo3.Percentage,0) TDSTaxPercentage
						,ROUND(ISNULL(TAxInfo6.TaxAmount,0),2) TCS,isnull(TAxInfo6.Percentage,0) TCSTaxPercentage
                        ,isnull(PLC.LCANo,'') LCANo,isnull(PLC.LCRef,'')LCRef,isnull(IR.ContractId,'')ContractId,isnull(IM.RefferenceNo,'')RefferenceNo
                        ,IsClosed=case when IR.IsClosed=0 then 'NO' Else 'YES' END
						FROM TRN.PurchaseOrderDetail AS IM
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						left JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId=MM.Id
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left jOIN [TRN].[PurchaseOrder] AS IR ON IR.Id=IM.InventoryReceiveId
						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
						left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
						LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId
						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
                        LEFT JOIN (SELECT PODetailsId,SUM(TransactionQty) GRNQty FROM TRN.InventoryReceiveDetail IRD  GROUP BY PODetailsId) GRN ON GRN.PODetailsId=IM.Id
						LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
						LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
                        LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
						LEFT JOIN dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='CGST' and A.InventoryServiceId IS NULL

						) TAxInfo ON TAxInfo.InventoryReceiveDetailId=IM.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='IGST' and A.InventoryServiceId IS NULL
						--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code ,A.Percentage
						) TAxInfo1 ON TAxInfo1.InventoryReceiveDetailId=IM.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='SGST' and A.InventoryServiceId IS NULL
						) TAxInfo2 ON TAxInfo2.InventoryReceiveDetailId=IM.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='TDS' and A.InventoryServiceId IS NULL
						) TAxInfo3 ON TAxInfo3.InventoryReceiveDetailId=IM.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='VAT' and A.InventoryServiceId IS NULL
						) TAxInfo4 ON TAxInfo4.InventoryReceiveDetailId=IM.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='AIT' and A.InventoryServiceId IS NULL
						) TAxInfo5 ON TAxInfo5.InventoryReceiveDetailId=IM.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='TCS' and A.InventoryServiceId IS NULL
						) TAxInfo6 ON TAxInfo6.InventoryReceiveDetailId=IM.Id
					LEFT JOIN(select InventoryReceiveId,sum(TaxAmount) TaxAmount from trn.purchaseOrderTax where inventoryReceiveDetailId is null
											group By InventoryReceiveId
											)servicetax ON servicetax.InventoryReceiveId=IM.InventoryReceiveId
					
			WHERE  IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.PODate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' "+ tempQuery + @"
			UNION ALL
			SELECT --ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo
					'ServicePO' POType ,IR.Id POId
					 , HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
					 when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
					 when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode else '' end
					,IM.Id AS PORowId ,REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
					,ISNULL(IR.DocRefNo,'') DocRefNo ,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
					,p.UserName AS PartyName ,IR.PartyId ,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
					,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
					,'' MaterialType ,'' AS MaterialGroupMasterName
					,'' MaterialMasterId ,SM.UserName MaterialMasterName
					,'' ArticleName , '' FirstCharacteristicsValue , '' SecondCharacteristicsValue , '' ThirdCharacteristicsValue ,'' UOM
					,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
					,0 TransactionQty,0 ReceiptQty,0 RejectionQty
					,0 BalanceQty,0 Tolerance ,0 TransactionRate ,IM.Amount TransactionAmount
					,ROUND(Isnull(servicetax.TaxAmount,0),2) TotalTaxAmount
					,0 ServiceCharge ,0 ServiceChargeTax ,0 BaseAmount ,IR.AddedBy
					,CASE WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.ApprovedBy is NOT null AND IR.ApprovedBy = 'Approved' Then 'Approved'
					WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.ApprovedBy is null And IR.ApprovedByStatus is null Then 'To be Checked'
					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.ApprovedBy is NOT null Then 'To be approved'
					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
					WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
					WHEN IR.CheckedBy is not null ANd IR.ApprovedByStatus = 'Hold' Then 'Approving Hold'
					WHEN IR.CheckedBy is not null AND IR.ApprovedByStatus = 'Rejected' Then 'Approving Rejected'
					END GRNCheckStatus
					,ISNULL(EI1.EmployeeName,'') CheckedBY ,EI2.EmployeeName AuthorizedBy
					,ROUND(ISNULL(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer
					,ROUND(ISNULL(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
					,ROUND(ISNULL(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
					,ROUND(ISNULL(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
					,ROUND(ISNULL(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
                    ,'' LCANo,'' LCRef,'' ContractId,'' RefferenceNo,IsClosed=case when IR.IsClosed=0 then 'NO' Else 'YES' END
					from TRN.ServicePODetail AS IM
					left JOIN hkp.ServiceMaster SM ON SM.Id=IM.ServiceMasterId
					left jOIN [TRN].ServicePOMaster AS IR ON IR.Id=IM.ServicePOMasterId
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
					LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy

					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
					WHERE B.Code='CGST'
					) TAxInfo ON TAxInfo.ServicePODetailId=IM.Id
					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
					WHERE B.Code='IGST'
					) TAxInfo1 ON TAxInfo1.ServicePODetailId=IM.Id
					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
					WHERE B.Code='SGST'
					) TAxInfo2 ON TAxInfo2.ServicePODetailId=IM.Id
					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					WHERE B.Code='TDS'
					) TAxInfo3 ON TAxInfo3.ServicePODetailId=IM.Id
					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					WHERE B.Code='VAT' and A.ServicePODetailId IS NULL
					) TAxInfo4 ON TAxInfo4.ServicePODetailId=IM.Id
					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					WHERE B.Code='AIT'
					) TAxInfo5 ON TAxInfo5.ServicePODetailId=IM.Id
					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					WHERE B.Code='TCS'
					) TAxInfo6 ON TAxInfo6.ServicePODetailId=IM.Id
					left join(select ServicePOMasterId,sum(TaxAmount) TaxAmount from trn.[ServicePOTax] where ServicePOMasterId is null
					group By ServicePOMasterId
					)servicetax ON servicetax.ServicePOMasterId=IM.ServicePOMasterId
					WHERE  IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.PODate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'  " + tempQuery + @"";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> ServicePurchaseOrderRegisterData(string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"SELECT SM.UserName ServiceName
                                	,sg.UserName ServiceGroup
                                	,'ServicePO' POType
                                	,IR.Id POId
                                	,HSNCode = CASE 
                                		WHEN TAxInfo.HSCode <> ''
                                			THEN TAxInfo.HSCode
                                		WHEN TAxInfo1.HSCode <> ''
                                			THEN TAxInfo1.HSCode
                                		WHEN TAxInfo2.HSCode <> ''
                                			THEN TAxInfo2.HSCode
                                		ELSE ''
                                		END
                                	,IM.Id AS PORowId
                                	,REPLACE(CONVERT(CHAR(11), IR.PODate, 106), ' ', '-') AS PODate
                                	,IR.DocRefNo
                                	,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
                                	,p.UserName AS PartyName
                                	,IR.PartyId
                                	,IR.InvoicingPartyPlantId
                                	,PP.UserName InvoicingPartyPlant
                                	,IR.DeliveryPartyPlantId
                                	,PPD.UserName DeliveryPartyPlant
                                	,'' MaterialType
                                	,'' AS MaterialGroupMasterName
                                	,'' MaterialMasterId
                                	,SM.UserName MaterialMasterName
                                	,'' ArticleName
                                	,'' FirstCharacteristicsValue
                                	,'' SecondCharacteristicsValue
                                	,'' ThirdCharacteristicsValue
                                	,'' UOM
                                	,CASE 
                                		WHEN IR.IsNonCreditable = 1
                                			THEN 'NonCreditable'
                                		WHEN IR.IsNonCreditable = 0
                                			THEN 'Creditable'
                                		END CredtibleStatus
                                	,0 TransactionQty
                                	,0 ReceiptQty
                                	,0 RejectionQty
                                	,0 ReturnQty
                                	,0 BalanceQty
                                	,0 TransactionRate
                                	,IM.Amount TransactionAmount
                                	,ROUND(Isnull(servicetax.TaxAmount, 0), 2) TotalTaxAmount
                                	,0 ServiceCharge
                                	,0 ServiceChargeTax
                                	,0 BaseAmount
                                	,IR.AddedBy
                                	,CASE 
                                		WHEN IR.CheckedBy IS NOT NULL
                                			AND IR.CheckedByStatus = 'Checked'
                                			AND IR.ApprovedBy IS NOT NULL
                                			AND IR.ApprovedBy = 'Approved'
                                			THEN 'Approved'
                                		WHEN IR.CheckedBy IS NOT NULL
                                			AND IR.CheckedByStatus = 'ForChecked'
                                			AND IR.ApprovedBy IS NULL
                                			AND IR.ApprovedByStatus IS NULL
                                			THEN 'To be Checked'
                                		WHEN IR.CheckedBy IS NOT NULL
                                			AND IR.CheckedByStatus = 'Checked'
                                			AND IR.ApprovedBy IS NOT NULL
                                			THEN 'To be approved'
                                		WHEN IR.CheckedBy IS NOT NULL
                                			AND IR.CheckedByStatus = 'Hold'
                                			THEN 'Checking Hold'
                                		WHEN IR.CheckedBy IS NOT NULL
                                			AND IR.CheckedByStatus = 'Rejected'
                                			THEN 'Checking Rejected'
                                		WHEN IR.CheckedBy IS NOT NULL
                                			AND IR.ApprovedByStatus = 'Hold'
                                			THEN 'Approving Hold'
                                		WHEN IR.CheckedBy IS NOT NULL
                                			AND IR.ApprovedByStatus = 'Rejected'
                                			THEN 'Approving Rejected'
                                		END GRNCheckStatus
                                	,EI1.EmployeeName CheckedBY
                                	,EI2.EmployeeName AuthorizedBy
                                	,round(isnull(TAxInfo.TaxAmount, 0), 2) CGST
                                	,TAxInfo.Percentage CGSTTaxPercentage --MaterialTaxPer
                                	,round(isnull(TAxInfo2.TaxAmount, 0), 2) SGST
                                	,TAxInfo2.Percentage SGSTTaxPercentage
                                	,round(isnull(TAxInfo1.TaxAmount, 0), 2) IGST
                                	,TAxInfo1.Percentage IGSTTaxPercentage
                                	,round(isnull(TAxInfo3.TaxAmount, 0), 2) TDS
                                	,TAxInfo3.Percentage TDSTaxPercentage
                                	,round(isnull(TAxInfo6.TaxAmount, 0), 2) TCS
                                	,TAxInfo6.Percentage TCSTaxPercentage
                                	,'' LCANo
                                	,'' LCRef
                                	,'' ContractId
                                	,'' RefferenceNo
                                	,'' RequisitionNo
                                FROM TRN.ServicePODetail AS IM
                                LEFT JOIN hkp.ServiceMaster SM ON SM.Id = IM.ServiceMasterId
                                LEFT JOIN hkp.ServiceGroup AS sg ON sg.Id = SM.ServiceGroupId
                                LEFT JOIN [TRN].ServicePOMaster AS IR ON IR.Id = IM.ServicePOMasterId
                                LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
                                LEFT JOIN HKP.Party AS P ON P.Id = IR.PartyId
                                LEFT JOIN HKP.PartyPlant AS PP ON PP.Id = IR.InvoicingPartyPlantId
                                LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id = IR.DeliveryPartyPlantId
                                LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId = IR.CheckedBy
                                LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId = IR.ApprovedBy
                                LEFT JOIN (
                                	SELECT A.ServicePODetailId
                                		,B.UserName TaxCategoryName
                                		,B.Code
                                		,A.Percentage Percentage
                                		,A.TaxAmount TaxAmount
                                		,hs.Code HSCode
                                	FROM [TRN].[ServicePOTax] A
                                	LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId = B.Id
                                	LEFT JOIN hkp.HSNCode HS ON HS.Id = A.HSNCodeId
                                	WHERE B.Code = 'CGST'
                                	) TAxInfo ON TAxInfo.ServicePODetailId = IM.Id
                                LEFT JOIN (
                                	SELECT A.ServicePODetailId
                                		,B.UserName TaxCategoryName
                                		,B.Code
                                		,A.Percentage Percentage
                                		,A.TaxAmount TaxAmount
                                		,hs.Code HSCode
                                	FROM [TRN].[ServicePOTax] A
                                	LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId = B.Id
                                	LEFT JOIN hkp.HSNCode HS ON HS.Id = A.HSNCodeId
                                	WHERE B.Code = 'IGST'
                                	) TAxInfo1 ON TAxInfo1.ServicePODetailId = IM.Id
                                LEFT JOIN (
                                	SELECT A.ServicePODetailId
                                		,B.UserName TaxCategoryName
                                		,B.Code
                                		,A.Percentage Percentage
                                		,A.TaxAmount TaxAmount
                                		,hs.Code HSCode
                                	FROM [TRN].[ServicePOTax] A
                                	LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId = B.Id
                                	LEFT JOIN hkp.HSNCode HS ON HS.Id = A.HSNCodeId
                                	WHERE B.Code = 'SGST'
                                	) TAxInfo2 ON TAxInfo2.ServicePODetailId = IM.Id
                                LEFT JOIN (
                                	SELECT A.ServicePODetailId
                                		,B.UserName TaxCategoryName
                                		,B.Code
                                		,A.Percentage Percentage
                                		,A.TaxAmount TaxAmount
                                	FROM [TRN].[ServicePOTax] A
                                	LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId = B.Id
                                	WHERE B.Code = 'TDS'
                                	) TAxInfo3 ON TAxInfo3.ServicePODetailId = IM.Id
                                LEFT JOIN (
                                	SELECT A.ServicePODetailId
                                		,B.UserName TaxCategoryName
                                		,B.Code
                                		,A.Percentage Percentage
                                		,A.TaxAmount TaxAmount
                                	FROM [TRN].[ServicePOTax] A
                                	LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId = B.Id
                                	WHERE B.Code = 'VAT'
                                		AND A.ServicePODetailId IS NULL
                                	) TAxInfo4 ON TAxInfo4.ServicePODetailId = IM.Id
                                LEFT JOIN (
                                	SELECT A.ServicePODetailId
                                		,B.UserName TaxCategoryName
                                		,B.Code
                                		,A.Percentage Percentage
                                		,A.TaxAmount TaxAmount
                                	FROM [TRN].[ServicePOTax] A
                                	LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId = B.Id
                                	WHERE B.Code = 'AIT'
                                	) TAxInfo5 ON TAxInfo5.ServicePODetailId = IM.Id
                                LEFT JOIN (
                                	SELECT A.ServicePODetailId
                                		,B.UserName TaxCategoryName
                                		,B.Code
                                		,A.Percentage Percentage
                                		,A.TaxAmount TaxAmount
                                	FROM [TRN].[ServicePOTax] A
                                	LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId = B.Id
                                	WHERE B.Code = 'TCS'
                                	) TAxInfo6 ON TAxInfo6.ServicePODetailId = IM.Id
                                LEFT JOIN (
                                	SELECT ServicePOMasterId
                                		,sum(TaxAmount) TaxAmount
                                	FROM trn.[ServicePOTax]
                                	WHERE ServicePOMasterId IS NULL
                                	GROUP BY ServicePOMasterId
                                	) servicetax ON servicetax.ServicePOMasterId = IM.ServicePOMasterId
                                WHERE IR.PlantId = '" + identity.PlantId + @"'                                
                                    AND convert(DATE, IR.PODate) BETWEEN '" + fromDate + @"'                                
                                        AND '" + toDate + @"'";

                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IWorkbook CreatePurchaseOrderRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate, string Type, string POId)
        {
            try
            {

                var excelEngine = new ExcelEngine();
                var report = new Service.Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                var Head = "Purchase Order Register";// + " " + fromDate + " " + "To" + " " + toDate ;
                CreatePurchaseOrderRegisterReportSheets(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate, toDate, Type, POId);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IWorkbook CreateServicePurchaseOrderRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            try
            {

                var excelEngine = new ExcelEngine();
                var report = new Service.Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                var Head = "Service PO Register Report";
                CreateServicePORegisterReportSheets(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate, toDate, Type);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CreateServicePORegisterReportSheets(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var cmdText = "";

            cmdText = @" SELECT SM.UserName ServiceName,sg.UserName ServiceGroup,
					'ServicePO' POType
					,IR.Id POId
					 , HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
					 when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
					 when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
					 else '' end
					,IM.Id AS PORowId
					,REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
					,IR.DocRefNo
					,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
					,p.UserName AS PartyName
					,IR.PartyId ,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
					,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
					,'' MaterialType
					,'' AS MaterialGroupMasterName
					,'' MaterialMasterId
				,SM.UserName MaterialMasterName
					,'' ArticleName
					, '' FirstCharacteristicsValue
					, '' SecondCharacteristicsValue
					, '' ThirdCharacteristicsValue
					,'' UOM
					,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
					,0 TransactionQty,0 ReceiptQty, 0 RejectionQty, 0 ReturnQty,0 BalanceQty
					,0 TransactionRate
					,IM.Amount TransactionAmount ,ISNULL(SA.Amount,0) ReceiptAmount,BalanceAmount=IM.Amount-ISNULL(SA.Amount,0)
					,ROUND(Isnull(servicetax.TaxAmount,0),2) TotalTaxAmount
					,0 ServiceCharge
					,0 ServiceChargeTax
					,0 BaseAmount
					,IR.AddedBy
					,CASE
					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.ApprovedBy is NOT null AND IR.ApprovedBy = 'Approved' Then 'Approved'
					WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.ApprovedBy is null And IR.ApprovedByStatus is null Then 'To be Checked'
					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.ApprovedBy is NOT null Then 'To be approved'


					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
					WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
					WHEN IR.CheckedBy is not null ANd IR.ApprovedByStatus = 'Hold' Then 'Approving Hold'
					WHEN IR.CheckedBy is not null AND IR.ApprovedByStatus = 'Rejected' Then 'Approving Rejected'
					END GRNCheckStatus

					,EI1.EmployeeName CheckedBY
					,EI2.EmployeeName AuthorizedBy
					,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer
					,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
					,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
					,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
					,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
                    ,'' LCANo,'' LCRef,'' ContractId,'' RefferenceNo,''RequisitionNo
					from TRN.ServicePODetail AS IM
					left JOIN hkp.ServiceMaster SM ON SM.Id=IM.ServiceMasterId
                    LEFT JOIN hkp.ServiceGroup AS sg ON sg.Id = SM.ServiceGroupId
					left jOIN [TRN].ServicePOMaster AS IR ON IR.Id=IM.ServicePOMasterId
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
					LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy
                    LEFT JOIN (SELECT SAM.ServicePOId,SAD.ServicePODetailId,SUM(SAD.Amount) Amount FROM TRN.ServiceAcknowledgementDetail SAD 
								JOIN TRN.ServiceAcknowledgementMaster SAM ON SAM.Id=SAD.ServiceAcknowledgementMasterId GROUP BY SAM.ServicePOId,SAD.ServicePODetailId) SA ON 
							SA.ServicePODetailId=IM.Id

					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
					WHERE B.Code='CGST'

					) TAxInfo ON TAxInfo.ServicePODetailId=IM.Id
					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
					WHERE B.Code='IGST'

					) TAxInfo1 ON TAxInfo1.ServicePODetailId=IM.Id

					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
					WHERE B.Code='SGST'

					) TAxInfo2 ON TAxInfo2.ServicePODetailId=IM.Id

					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					WHERE B.Code='TDS'

					) TAxInfo3 ON TAxInfo3.ServicePODetailId=IM.Id


					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					WHERE B.Code='VAT' and A.ServicePODetailId IS NULL

					) TAxInfo4 ON TAxInfo4.ServicePODetailId=IM.Id
					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					WHERE B.Code='AIT'
					) TAxInfo5 ON TAxInfo5.ServicePODetailId=IM.Id

					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					WHERE B.Code='TCS'
					) TAxInfo6 ON TAxInfo6.ServicePODetailId=IM.Id

					left join(select ServicePOMasterId,sum(TaxAmount) TaxAmount from trn.[ServicePOTax] where ServicePOMasterId is null
					group By ServicePOMasterId
					)servicetax ON servicetax.ServicePOMasterId=IM.ServicePOMasterId
					where  IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.PODate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";
            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);

            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


            var colTransactionQtyTotal = 0.00;
            var colTransactionAmountTotal = 0.00;
            var colReceiptAmountTotal = 0.00;
            var colBalanceAmountTotal = 0.00;
            var colTotalMaterialTranAmountTotal = 0.00;
            var colTaxAmountTotal = 0.00;
            var colTotalMaterialBooksCurrencyAmountTotal = 0.00;
            var colTrnCurrencyBaseRateTotal = 0.00;
            var colBooksCurrencyBaseRateTotal = 0.00;
            var colShortageQtyTotal = 0.00;
            var colRejectionQtyTotal = 0.00;
            var colApprovedQtyTotal = 0.00;

            var colCGSTTotal = 0.00;
            var colSGSTTotal = 0.00;
            var colIGSTTotal = 0.00;
            var colTDSTotal = 0.00;
            var colTCSTotal = 0.00;

            var colCGSTTotal1 = 0.00;
            var colSGSTTotal1 = 0.00;
            var colIGSTTotal1 = 0.00;
            var colTDSTotal1 = 0.00;
            var colTCSTotal1 = 0.00;
            var colTaxableAmountTotal = 0.00;
            var colBaseAmount = 0.00;
            var colServiceCharge = 0.00;
            var colServiceTax = 0.00;


            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _rowd = 4;

            if (fromDate != "" && toDate != "")
            {


                sheet1[_rowd, 4].Text = fromDate + " " + "To" + " " + toDate;
                sheet1[_rowd, 4].CellStyle.Font.Size = 8;
                sheet1[_rowd, 4].CellStyle.Font.Bold = false;
                sheet1.Range[_rowd, 3, _rowd, 4].Merge();
                //sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            }

            var _rows = 5;
            sheet1[_rows, 5].Text = "Report Ref No: ";
            sheet1[_rows, 5].CellStyle.Font.Size = 8;
            sheet1[_rows, 5].CellStyle.Font.Bold = false;
            sheet1.Range[_rows, 3, _rows, 6].Merge();

            var _row = 6;
            var _rowL = _row;
            var row = _row + 1;


            var sheet1headreColIndex = 1;


            _rowL += 1;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colPONo = sheet1headreColIndex;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Requisition No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colrequisitionNo = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colPODate = sheet1headreColIndex;

            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colPOType = sheet1headreColIndex;

            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Party";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colParty = sheet1headreColIndex;

            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Invoicing Party Plant";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Delivery Party PlantId";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Delivery Party Plant";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Service Group";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Service Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;

            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTransactionAmountTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipt Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colReceiptAmountTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Balance Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colBalanceAmountTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Total Tax Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTaxableAmountTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Service Charge";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colServiceCharge = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Service Tax";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colServiceTax = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colBaseAmount = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGST";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colCGSTTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGST Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colCGSTTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SGST";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colSGSTTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SGST Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colSGSTTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "IGST";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colIGSTTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "IGST Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colIGSTTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TDS";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTDSTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TDS Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTDSTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TCS";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTCSTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TCS Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTCSTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;




            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Prepared By");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Prepared By";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Checking Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Approving Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "LCANo";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "LCRef";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "ContractId";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "RefferenceNo";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;

            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                int col = 1;
                _rowL++;

                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["POId"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["RequisitionNo"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["PODate"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["POType"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["PartyName"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["InvoicingPartyPlant"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["DeliveryPartyPlantId"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["DeliveryPartyPlant"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["DocRefNo"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["DocDate"].ToString()); col++;
                //report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialType"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ServiceGroup"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ServiceName"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionRate"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionAmount"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceiptAmount"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BalanceAmount"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalTaxAmount"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ServiceCharge"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ServiceChargeTax"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BaseAmount"].ToString())); col++;
                //report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGST"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGSTTaxPercentage"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["SGST"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["SGSTTaxPercentage"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IGST"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IGSTTaxPercentage"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TDS"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TDSTaxPercentage"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TCS"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TCSTaxPercentage"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["AddedBy"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["CheckedBY"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["AuthorizedBy"].ToString()); col++;

                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["LCANo"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["LCRef"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ContractId"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["RefferenceNo"].ToString());

            }
            _rowL++;

            if (fromDate != "" && toDate != "")
            {
                object sumObject;
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
                sumObject = inventoryMaterialList.Compute("Sum(CGST)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colCGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
                sumObject = inventoryMaterialList.Compute("Sum(SGST)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colSGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
                sumObject = inventoryMaterialList.Compute("Sum(IGST)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colIGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                sumObject = inventoryMaterialList.Compute("Sum(TDS)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTDSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                sumObject = inventoryMaterialList.Compute("Sum(TCS)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTCSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
            }

            //sheet1.Range[(Row_Total_Start), 22, _rowL, 22].NumberFormat = "#,##0.00;(#,##0.0000)";
            sheet1.Range[(Row_Total_Start), 26, _rowL, 26].NumberFormat = clsStaticInfo.NumberFormat(4);
            sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);
            //_rowL++;

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            //sheet1.UsedRange.CellStyle.Font.Size = 8;
            sheet1.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);


        }

        private void CreatePurchaseOrderRegisterReportSheets(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type, string POId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var cmdText = "";

            cmdText = @"SELECT --ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo
						POType= CASE WHEN IR.POType='PO' Then 'Individual PO' ELSE 'Requisition Based PO' END
						,IR.Id POId
							, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
													when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
													when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
													else '' end
						,IM.InventoryReceiveId AS PORowId
						,REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
						,IR.DocRefNo
						,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						,p.UserName AS PartyName
						,IR.PartyId ,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
						,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
						,MT.UserName MaterialType
						,MGM.UserName AS MaterialGroupMasterName
						,IM.InventoryMaterialId MaterialMasterId
						,MM.UserName MaterialMasterName
						, ART.StandardName ArticleName
						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue
						,TUoM.UserName AS UOM
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,IM.TransactionQty,ISNULL(IRD.GRNQty,0) ReceiptQty, 0 RejectionQty, 0 ReturnQty,IM.TransactionQty-ISNULL(IRD.GRNQty,0) BalanceQty
						,ROUND(Isnull(IM.TransactionRate,0),2) TransactionRate
						,ROUND(Isnull(IM.TransactionAmount,0),2) TransactionAmount
						,ROUND(Isnull(POS.TotalTaxAmount,0),2) TotalTaxAmount
						,ROUND(Isnull(POS.Amount,0),2) ServiceCharge
						,servicetax.TaxAmount ServiceChargeTax
						,ROUND(Isnull(IM.BaseAmount,0),2) BaseAmount
						,IR.AddedBy
						,CASE
						WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
						WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'
						WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null Then 'To be approved'


						WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
						WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
						WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
						WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'
						END GRNCheckStatus

						,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
                        ,PLC.LCANo,PLC.LCRef,IR.ContractId,IM.RefferenceNo,IM.RequisitionId RequisitionNo
						from TRN.PurchaseOrderDetail AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.InventoryMaterialId=MM.Id
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left jOIN [TRN].[PurchaseOrder] AS IR ON IR.Id=IM.InventoryReceiveId
						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
						left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
						LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId
						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId

						LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
						LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
                        LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
						left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
                        left join TRN.POService POS ON POS.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT PODetailsId,SUM(TransactionQty) GRNQty FROM TRN.InventoryReceiveDetail GROUP BY PODetailsId)IRD ON IRD.PODetailsId=IM.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='CGST' and A.InventoryServiceId IS NULL

						) TAxInfo ON TAxInfo.InventoryReceiveDetailId=IM.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='IGST' and A.InventoryServiceId IS NULL
						--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code ,A.Percentage
						) TAxInfo1 ON TAxInfo1.InventoryReceiveDetailId=IM.Id

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='SGST' and A.InventoryServiceId IS NULL

						) TAxInfo2 ON TAxInfo2.InventoryReceiveDetailId=IM.Id

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='TDS' and A.InventoryServiceId IS NULL

						) TAxInfo3 ON TAxInfo3.InventoryReceiveDetailId=IM.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='VAT' and A.InventoryServiceId IS NULL

						) TAxInfo4 ON TAxInfo4.InventoryReceiveDetailId=IM.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='AIT' and A.InventoryServiceId IS NULL

						) TAxInfo5 ON TAxInfo5.InventoryReceiveDetailId=IM.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
						FROM [TRN].[PurchaseOrderTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='TCS' and A.InventoryServiceId IS NULL
						) TAxInfo6 ON TAxInfo6.InventoryReceiveDetailId=IM.Id

					left join(select InventoryReceiveId,sum(TaxAmount) TaxAmount from trn.purchaseOrderTax where inventoryReceiveDetailId is null
											group By InventoryReceiveId
											)servicetax ON servicetax.InventoryReceiveId=IM.InventoryReceiveId
					
			where  IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.PODate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.Id in (" + POId + @")
			UNION ALL
			SELECT --ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo
					'ServicePO' POType
					,IR.Id POId
					 , HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
					 when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
					 when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
					 else '' end
					,IM.Id AS PORowId
					,REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
					,IR.DocRefNo
					,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
					,p.UserName AS PartyName
					,IR.PartyId ,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
					,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
					,'' MaterialType
					,'' AS MaterialGroupMasterName
					,'' MaterialMasterId
				,SM.UserName MaterialMasterName
					,'' ArticleName
					, '' FirstCharacteristicsValue
					, '' SecondCharacteristicsValue
					, '' ThirdCharacteristicsValue
					,'' UOM
					,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
					,0 TransactionQty,0 ReceiptQty, 0 RejectionQty, 0 ReturnQty,0 BalanceQty
					,0 TransactionRate
					,IM.Amount TransactionAmount
					,ROUND(Isnull(servicetax.TaxAmount,0),2) TotalTaxAmount
					,0 ServiceCharge
					,0 ServiceChargeTax
					,0 BaseAmount
					,IR.AddedBy
					,CASE
					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.ApprovedBy is NOT null AND IR.ApprovedBy = 'Approved' Then 'Approved'
					WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.ApprovedBy is null And IR.ApprovedByStatus is null Then 'To be Checked'
					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.ApprovedBy is NOT null Then 'To be approved'


					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
					WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
					WHEN IR.CheckedBy is not null ANd IR.ApprovedByStatus = 'Hold' Then 'Approving Hold'
					WHEN IR.CheckedBy is not null AND IR.ApprovedByStatus = 'Rejected' Then 'Approving Rejected'
					END GRNCheckStatus

					,EI1.EmployeeName CheckedBY
					,EI2.EmployeeName AuthorizedBy
					,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer
					,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
					,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
					,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
					,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
                    ,'' LCANo,'' LCRef,'' ContractId,'' RefferenceNo,''RequisitionNo
					from TRN.ServicePODetail AS IM
					left JOIN hkp.ServiceMaster SM ON SM.Id=IM.ServiceMasterId
					left jOIN [TRN].ServicePOMaster AS IR ON IR.Id=IM.ServicePOMasterId
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
					LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy


					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
					WHERE B.Code='CGST'

					) TAxInfo ON TAxInfo.ServicePODetailId=IM.Id
					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
					WHERE B.Code='IGST'

					) TAxInfo1 ON TAxInfo1.ServicePODetailId=IM.Id

					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
					WHERE B.Code='SGST'

					) TAxInfo2 ON TAxInfo2.ServicePODetailId=IM.Id

					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					WHERE B.Code='TDS'

					) TAxInfo3 ON TAxInfo3.ServicePODetailId=IM.Id


					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					WHERE B.Code='VAT' and A.ServicePODetailId IS NULL

					) TAxInfo4 ON TAxInfo4.ServicePODetailId=IM.Id
					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					WHERE B.Code='AIT'
					) TAxInfo5 ON TAxInfo5.ServicePODetailId=IM.Id

					LEFT JOIN (SELECT A.ServicePODetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
					FROM [TRN].[ServicePOTax] A
					LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
					WHERE B.Code='TCS'
					) TAxInfo6 ON TAxInfo6.ServicePODetailId=IM.Id

					left join(select ServicePOMasterId,sum(TaxAmount) TaxAmount from trn.[ServicePOTax] where ServicePOMasterId is null
					group By ServicePOMasterId
					)servicetax ON servicetax.ServicePOMasterId=IM.ServicePOMasterId
					where  IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.PODate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.Id in (" + POId + @")";
            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);

            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


            var colTransactionQtyTotal = 0.00;
            var colTransactionAmountTotal = 0.00;
            var colTotalMaterialTranAmountTotal = 0.00;
            var colTaxAmountTotal = 0.00;
            var colTotalMaterialBooksCurrencyAmountTotal = 0.00;
            var colTrnCurrencyBaseRateTotal = 0.00;
            var colBooksCurrencyBaseRateTotal = 0.00;
            var colShortageQtyTotal = 0.00;
            var colRejectionQtyTotal = 0.00;
            var colApprovedQtyTotal = 0.00;

            var colCGSTTotal = 0.00;
            var colSGSTTotal = 0.00;
            var colIGSTTotal = 0.00;
            var colTDSTotal = 0.00;
            var colTCSTotal = 0.00;

            var colCGSTTotal1 = 0.00;
            var colSGSTTotal1 = 0.00;
            var colIGSTTotal1 = 0.00;
            var colTDSTotal1 = 0.00;
            var colTCSTotal1 = 0.00;
            var colTaxableAmountTotal = 0.00;
            var colBaseAmount = 0.00;
            var colServiceCharge = 0.00;
            var colServiceTax = 0.00;


            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _rowd = 4;

            if (fromDate != "" && toDate != "")
            {


                sheet1[_rowd, 4].Text = fromDate + " " + "To" + " " + toDate;
                sheet1[_rowd, 4].CellStyle.Font.Size = 8;
                sheet1[_rowd, 4].CellStyle.Font.Bold = false;
                sheet1.Range[_rowd, 3, _rowd, 4].Merge();
                //sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            }

            var _rows = 5;
            sheet1[_rows, 5].Text = "Report Ref No: ";
            sheet1[_rows, 5].CellStyle.Font.Size = 8;
            sheet1[_rows, 5].CellStyle.Font.Bold = false;
            sheet1.Range[_rows, 3, _rows, 6].Merge();

            var _row = 6;
            var _rowL = _row;
            var row = _row + 1;


            var sheet1headreColIndex = 1;


            _rowL += 1;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colPONo = sheet1headreColIndex;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Requisition No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colrequisitionNo = sheet1headreColIndex;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN Date");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colPODate = sheet1headreColIndex;

            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Type");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colPOType = sheet1headreColIndex;

            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Party");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Party";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colParty = sheet1headreColIndex;

            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PartyId";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colPartyId = sheet1headreColIndex;

            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "InvoicingPartyPlantId";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colInvoicingPartyPlantId = sheet1headreColIndex;

            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "InvoicingPartyPlant";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "DeliveryPartyPlantId";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "DeliveryPartyPlant";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;



            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Doc Ref No");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Doc Ref Date");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Grn Doc Date Difference");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Grn Doc Date Difference";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Type");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            ////report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
            ////sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            ////report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
            ////sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Qty");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTransactionQtyTotal = sheet1headreColIndex;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UoM");
            //sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "UoM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 8;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipt Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colReceiptQty = sheet1headreColIndex;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Rejection Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colRejectionQty = sheet1headreColIndex;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Return Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colReturnQty = sheet1headreColIndex;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Balance Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colBalanceQty = sheet1headreColIndex;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Rate");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;

            sheet1headreColIndex++;



            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Lot No";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;



            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Quality Status";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;




            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Gross Amount";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;



            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Discount Amount";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTransactionAmountTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Amount");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Total Tax Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTaxableAmountTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Amount";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTransactionAmountTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Service Charge";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colServiceCharge = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Service Tax";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colServiceTax = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colBaseAmount = sheet1headreColIndex;
            sheet1headreColIndex++;



            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Credtible Status";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            ////report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Tax Amount");
            ////sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "RCM";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTaxAmountTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;



            sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGST";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colCGSTTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGST Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colCGSTTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SGST";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colSGSTTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SGST Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colSGSTTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "IGST";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colIGSTTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "IGST Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colIGSTTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TDS";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTDSTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TDS Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTDSTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TCS";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTCSTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TCS Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTCSTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;




            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Prepared By");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Prepared By";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Status");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Status";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;



            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Checking Name");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Checking Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Approving Name");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Approving Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "LCANo";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "LCRef";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "ContractId";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "RefferenceNo";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;





            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;
            //sheet1headreColIndex++;




            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                int col = 1;
                _rowL++;

                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["POId"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["RequisitionNo"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["PODate"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["POType"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["PartyName"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["PartyId"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["InvoicingPartyPlantId"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["InvoicingPartyPlant"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["DeliveryPartyPlantId"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["DeliveryPartyPlant"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["DocRefNo"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["DocDate"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialType"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleName"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionQty"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["UOM"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceiptQty"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RejectionQty"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReturnQty"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BalanceQty"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionRate"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionAmount"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalTaxAmount"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ServiceCharge"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ServiceChargeTax"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BaseAmount"].ToString())); col++;
                //report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGST"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGSTTaxPercentage"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["SGST"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["SGSTTaxPercentage"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IGST"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IGSTTaxPercentage"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TDS"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TDSTaxPercentage"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TCS"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TCSTaxPercentage"].ToString())); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["AddedBy"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["CheckedBY"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["AuthorizedBy"].ToString()); col++;

                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["LCANo"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["LCRef"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ContractId"].ToString()); col++;
                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["RefferenceNo"].ToString());

            }
            _rowL++;

            if (fromDate != "" && toDate != "")
            {


                report.SetText(ref sheet1, _rowL, (Convert.ToInt32(colTransactionQtyTotal) - 1), "Total");
                sheet1.Range[_rowL, (Convert.ToInt32(colTransactionQtyTotal) - 1)].CellStyle.Font.Bold = true;
                //sheet1.Range[1, _rowL, Convert.ToInt32(colTransactionQtyTotal) - 1, _rowL].Merge();
                object sumObject;
                //sumObject = inventoryMaterialList.Compute("Sum(MaterialTranAmount)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionAmountTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
                sumObject = inventoryMaterialList.Compute("Sum(CGST)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colCGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
                sumObject = inventoryMaterialList.Compute("Sum(SGST)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colSGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
                sumObject = inventoryMaterialList.Compute("Sum(IGST)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colIGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                sumObject = inventoryMaterialList.Compute("Sum(TDS)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTDSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                sumObject = inventoryMaterialList.Compute("Sum(TCS)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTCSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
            }

            //sheet1.Range[(Row_Total_Start), 22, _rowL, 22].NumberFormat = "#,##0.00;(#,##0.0000)";
            sheet1.Range[(Row_Total_Start), 26, _rowL, 26].NumberFormat = clsStaticInfo.NumberFormat(4);
            sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);
            //_rowL++;

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            //sheet1.UsedRange.CellStyle.Font.Size = 8;
            sheet1.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);


        }

        public IEnumerable<object> getServicePOTaxForAckSave(string ServicePOId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string paramter = "";
            if (ServicePOId != "")
            {
                if (paramter == "")
                    paramter += "A.ServicePOMasterId in(" + ServicePOId + ")";
                else
                    paramter += " AND A.ServicePOMasterId in(" + ServicePOId + ")";
            }
            try
            {
                var _sql = @"select a.ServicePOMasterId,a.ServicePoDetailId,b.UserName,a.HSNCodeId,a.TaxAmount,a.Percentage,a.TaxCategoryId from trn.servicePOtax a
				left join[MST].[TaxCategory] b On b.Id=a.TaxCategoryId
			   where " + paramter + @"";

                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> getServicePOAckTax(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"select SAT.Id, SAT.ServiceAcknowledgementMasterId, SAT.ServiceAcknowledgementDetailId,TC.UserName TaxCategory, SAT.TaxCategoryId, SAT.HSNCodeId, SAT.Percentage, SAT.TaxAmount from trn.ServicePOAckTax SAT
							 Left JOIN MST.TaxCategory TC ON TC.Id= SAT.TaxCategoryId
			   where SAT.ServiceAcknowledgementMasterId='" + Id + "'";

                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void UpdateServicePOAckTax(string MasterId, List<Dictionary<string, object>> UserSendData)
        {
            try
            {
                string sql = "select * from trn.ServicePOAckTax where ServiceAcknowledgementMasterId='" + MasterId + "'";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

                for (int i = 0; i < UserSendData.Count; i++)
                {
                    dsDetail.Tables[0].DefaultView.RowFilter = "ServiceAcknowledgementDetailId='" + UserSendData[i]["ServiceAcknowledgementDetailId"].ToString() + "'";
                    if (dsDetail.Tables[0].DefaultView.Count == 0)
                    {
                        //genId.GenID("TNA MASTER", out TNAMasterSystemID);
                        //TNAMasterSystemID = "TM" + TNAMasterSystemID;
                        //DataRow dr = dsMaster.Tables[0].NewRow();
                        //dr["Id"] = TNAMasterSystemID;
                        //dr[columnname] = TransactionId;
                        //dr["TNAAppliedOn"] = ScheduleFor.ToString();
                        //dr["AddedBy"] = "Scheduler";
                        //dr["AddedDate"] = System.DateTime.Now.ToString();
                        //dr["AddedFromIP"] = "";
                        //dr["UpdatedBy"] = "Scheduler";
                        //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        //dr["UpdatedFromIP"] = "";

                        //dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["Percentage"] = UserSendData[i]["Percentage"];
                        dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
                        dr.EndEdit();
                    }
                }

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private string ServiceAcknowledgementAdditionalTaxId()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ServiceAcknowledgementAdditionalTax", out sID);
            return sID;
        }

        public void ServiceAcknowledgementAdditionalTax(string MasterId, List<Dictionary<string, object>> UserSendData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = "select * from TRN.ServiceAcknowledgementAdditionalTax where ServicePOAckMasterId='" + MasterId + "'";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

                for (int i = 0; i < UserSendData.Count; i++)
                {
                    dsDetail.Tables[0].DefaultView.RowFilter = "TaxCodeId='" + UserSendData[i]["TaxCodeId"].ToString() + "'";
                    if (dsDetail.Tables[0].DefaultView.Count == 0)
                    {

                        DataRow dr = dsDetail.Tables[0].NewRow();
                        dr["Id"] = ServiceAcknowledgementAdditionalTaxId();
                        dr["TaxCodeId"] = UserSendData[i]["TaxCodeId"];
                        dr["Percentage"] = UserSendData[i]["ValueOfFixed"];
                        dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        //dr["UpdatedBy"] = "";
                        //dr["UpdatedDate"] = "";
                        //dr["UpdatedFromIP"] = "";
                        dr["ServicePOAckMasterId"] = MasterId.ToString();
                        dsDetail.Tables[0].Rows.Add(dr);
                    }
                    //else
                    //{
                    //	DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
                    //	dr.BeginEdit();
                    //	dr["ShortageRatePercent"] = UserSendData[i]["ShortageRate"];
                    //	dr["ShortageValue"] = UserSendData[i]["ShortageValue"];
                    //	dr["RejectRatePercent"] = UserSendData[i]["RejectionRate"];
                    //	dr["RejectValue"] = UserSendData[i]["RejectionValue"];
                    //	dr["RejectClamPercent"] = UserSendData[i]["RejectionClamRate"];
                    //	dr.EndEdit();
                    //}
                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public IEnumerable<object> GetServiceAcknowledgementAdditionalTaxInfo(string ServicePOAckMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                sql = @"Select a.Id,a.TaxCodeId,a.Percentage ValueOfFixed,a.TaxAmount,a.AddedBy,a.AddedDate,a.AddedFromIP,b.UserName TaxName,ServicePOAckMasterId
						from [TRN].ServiceAcknowledgementAdditionalTax a
						left join [mst].[TAXCode] b ON b.Id=a.TaxCodeId where a.ServicePOAckMasterId='" + ServicePOAckMasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public IEnumerable<object> ServiceAcknowledgementAdditionalTaxDelete(string Id)
        {
            try
            {
                var _sql = @" Delete from [TRN].ServiceAcknowledgementAdditionalTax where Id='" + Id + @"'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #region ServiceAcknowledgement Register Report

        public IEnumerable<object> ServiceAcknowledgementRegisterGridData(string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"SELECT --ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo
						--'ServicePO' POType
						IR.Id POId
						,CU.Code Currency
						, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
						when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
						when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
						else '' end
						,IM.Id AS PORowId
						,REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS PODate
						,IR.DocRefNo
						,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						,p.UserName AS PartyName
						,IR.PartyId ,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
						,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
						,'' MaterialType
						,'' AS MaterialGroupMasterName
						,'' MaterialMasterId
						,SM.UserName MaterialMasterName
						,'' ArticleName
						, '' FirstCharacteristicsValue
						, '' SecondCharacteristicsValue
						, '' ThirdCharacteristicsValue
						,'' UOM
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,0 TransactionQty
						,0 TransactionRate
						,IM.Amount TransactionAmount
						,ROUND(Isnull(servicetax.TaxAmount,0),2) TotalTaxAmount
						,0 ServiceCharge
						,0 ServiceChargeTax
						,0 BaseAmount
						,IR.AddedBy
						,CASE
						WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.ApprovedBy is NOT null AND IR.ApprovedBy = 'Approved' Then 'Approved'
						WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.ApprovedBy is null And IR.ApprovedByStatus is null Then 'To be Checked'
						WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.ApprovedBy is NOT null Then 'To be approved'


						WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
						WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
						WHEN IR.CheckedBy is not null ANd IR.ApprovedByStatus = 'Hold' Then 'Approving Hold'
						WHEN IR.CheckedBy is not null AND IR.ApprovedByStatus = 'Rejected' Then 'Approving Rejected'
						END GRNCheckStatus

						,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
						from TRN.ServiceAcknowledgementDetail AS IM
						left JOIN hkp.ServiceMaster SM ON SM.Id=IM.ServiceMasterId
						left jOIN [TRN].ServiceAcknowledgementMaster AS IR ON IR.Id=IM.ServiceAcknowledgementMasterId
						left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId
						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
						LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
						LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy


						LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
						FROM [TRN].ServicePOAckTax A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='CGST'

						) TAxInfo ON TAxInfo.ServiceAcknowledgementDetailId=IM.Id
						LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
						FROM [TRN].ServicePOAckTax A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='IGST'

						) TAxInfo1 ON TAxInfo1.ServiceAcknowledgementDetailId=IM.Id

						LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
						FROM [TRN].ServicePOAckTax A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='SGST'

						) TAxInfo2 ON TAxInfo2.ServiceAcknowledgementDetailId=IM.Id

						LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
						FROM [TRN].ServicePOAckTax A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='TDS'

						) TAxInfo3 ON TAxInfo3.ServiceAcknowledgementDetailId=IM.Id

						LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
						FROM [TRN].ServicePOAckTax A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='VAT' and A.ServiceAcknowledgementDetailId IS NULL

						) TAxInfo4 ON TAxInfo4.ServiceAcknowledgementDetailId=IM.Id
						LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
						FROM [TRN].ServicePOAckTax A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='AIT'
						) TAxInfo5 ON TAxInfo5.ServiceAcknowledgementDetailId=IM.Id

						LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
						FROM [TRN].ServicePOAckTax A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='TCS'
						) TAxInfo6 ON TAxInfo6.ServiceAcknowledgementDetailId=IM.Id

						left join(select ServicePOMasterId,sum(TaxAmount) TaxAmount from trn.[ServicePOTax] where ServicePOMasterId is null
						group By ServicePOMasterId
						)servicetax ON servicetax.ServicePOMasterId=IM.ServicePOMasterId
						where IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.AcknowledgementDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";


                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public IWorkbook CreateServiceAcknowledgementRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            try
            {

                var excelEngine = new ExcelEngine();
                var report = new Service.Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                var Head = "Service Acknowledgement Register";// + " " + fromDate + " " + "To" + " " + toDate ;
                CreateServiceAcknowledgementRegisterReportSheets(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate, toDate, Type);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void CreateServiceAcknowledgementRegisterReportSheets(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var cmdText = "";

            cmdText = @"SELECT --ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo
							--'ServicePO' POType
							IR.Id POId
							,CU.Code Currency
							, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
							when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
							when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
							else '' end
							,IM.Id AS PORowId
							,REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS PODate
							,IR.DocRefNo
							,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
							,p.UserName AS PartyName
							,IR.PartyId ,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
							,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
							,'' MaterialType
							,'' AS MaterialGroupMasterName
							,'' MaterialMasterId
							,SM.UserName MaterialMasterName
							,'' ArticleName
							, '' FirstCharacteristicsValue
							, '' SecondCharacteristicsValue
							, '' ThirdCharacteristicsValue
							,'' UOM
							,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
							,0 TransactionQty
							,0 TransactionRate
							,IM.Amount TransactionAmount
							,ROUND(Isnull(servicetax.TaxAmount,0),2) TotalTaxAmount
							,0 ServiceCharge
							,0 ServiceChargeTax
							,0 BaseAmount
							,IR.AddedBy
							,CASE
							WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.ApprovedBy is NOT null AND IR.ApprovedBy = 'Approved' Then 'Approved'
							WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.ApprovedBy is null And IR.ApprovedByStatus is null Then 'To be Checked'
							WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.ApprovedBy is NOT null Then 'To be approved'


							WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
							WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
							WHEN IR.CheckedBy is not null ANd IR.ApprovedByStatus = 'Hold' Then 'Approving Hold'
							WHEN IR.CheckedBy is not null AND IR.ApprovedByStatus = 'Rejected' Then 'Approving Rejected'
							END GRNCheckStatus

							,EI1.EmployeeName CheckedBY
							,EI2.EmployeeName AuthorizedBy
							,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer
							,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
							,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
							,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
							,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
							from TRN.ServiceAcknowledgementDetail AS IM
							left JOIN hkp.ServiceMaster SM ON SM.Id=IM.ServiceMasterId
							left jOIN [TRN].ServiceAcknowledgementMaster AS IR ON IR.Id=IM.ServiceAcknowledgementMasterId
							left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
							LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
							LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId
							LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
							LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy


							LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
							FROM [TRN].ServicePOAckTax A
							LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
							left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
							WHERE B.Code='CGST'

							) TAxInfo ON TAxInfo.ServiceAcknowledgementDetailId=IM.Id
							LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
							FROM [TRN].ServicePOAckTax A
							LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
							left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
							WHERE B.Code='IGST'

							) TAxInfo1 ON TAxInfo1.ServiceAcknowledgementDetailId=IM.Id

							LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode
							FROM [TRN].ServicePOAckTax A
							LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
							left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
							WHERE B.Code='SGST'

							) TAxInfo2 ON TAxInfo2.ServiceAcknowledgementDetailId=IM.Id

							LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
							FROM [TRN].ServicePOAckTax A
							LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
							WHERE B.Code='TDS'

							) TAxInfo3 ON TAxInfo3.ServiceAcknowledgementDetailId=IM.Id

							LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
							FROM [TRN].ServicePOAckTax A
							LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
							WHERE B.Code='VAT' and A.ServiceAcknowledgementDetailId IS NULL

							) TAxInfo4 ON TAxInfo4.ServiceAcknowledgementDetailId=IM.Id
							LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
							FROM [TRN].ServicePOAckTax A
							LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
							WHERE B.Code='AIT'
							) TAxInfo5 ON TAxInfo5.ServiceAcknowledgementDetailId=IM.Id

							LEFT JOIN (SELECT A.ServiceAcknowledgementDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount
							FROM [TRN].ServicePOAckTax A
							LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
							WHERE B.Code='TCS'
							) TAxInfo6 ON TAxInfo6.ServiceAcknowledgementDetailId=IM.Id

							left join(select ServicePOMasterId,sum(TaxAmount) TaxAmount from trn.[ServicePOTax] where ServicePOMasterId is null
							group By ServicePOMasterId
							)servicetax ON servicetax.ServicePOMasterId=IM.ServicePOMasterId
							where IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.AcknowledgementDate) BETWEEN'" + fromDate + @"' AND '" + toDate + @"'";

            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);

            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


            var colTransactionQtyTotal = 0.00;
            var colTransactionAmountTotal = 0.00;
            var colTotalMaterialTranAmountTotal = 0.00;
            var colTaxAmountTotal = 0.00;
            var colTotalMaterialBooksCurrencyAmountTotal = 0.00;
            var colTrnCurrencyBaseRateTotal = 0.00;
            var colBooksCurrencyBaseRateTotal = 0.00;
            var colShortageQtyTotal = 0.00;
            var colRejectionQtyTotal = 0.00;
            var colApprovedQtyTotal = 0.00;

            var colCGSTTotal = 0.00;
            var colSGSTTotal = 0.00;
            var colIGSTTotal = 0.00;
            var colTDSTotal = 0.00;
            var colTCSTotal = 0.00;

            var colCGSTTotal1 = 0.00;
            var colSGSTTotal1 = 0.00;
            var colIGSTTotal1 = 0.00;
            var colTDSTotal1 = 0.00;
            var colTCSTotal1 = 0.00;
            var colTaxableAmountTotal = 0.00;
            var colBaseAmount = 0.00;
            var colServiceCharge = 0.00;
            var colServiceTax = 0.00;
            var colCurrency = 0.00;


            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _rowd = 4;

            if (fromDate != "" && toDate != "")
            {


                sheet1[_rowd, 4].Text = fromDate + " " + "To" + " " + toDate;
                sheet1[_rowd, 4].CellStyle.Font.Size = 8;
                sheet1[_rowd, 4].CellStyle.Font.Bold = false;
                sheet1.Range[_rowd, 3, _rowd, 4].Merge();
                //sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            }

            var _rows = 5;
            sheet1[_rows, 5].Text = "Report Ref No: ";
            sheet1[_rows, 5].CellStyle.Font.Size = 8;
            sheet1[_rows, 5].CellStyle.Font.Bold = false;
            sheet1.Range[_rows, 3, _rows, 6].Merge();

            var _row = 6;
            var _rowL = _row;
            var row = _row + 1;


            var sheet1headreColIndex = 1;


            _rowL += 1;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Ack No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN Date");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Ack Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Type");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO Type";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Party");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Party";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PartyId";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "InvoicingPartyPlantId";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "InvoicingPartyPlant";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "DeliveryPartyPlantId";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "DeliveryPartyPlant";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;



            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Doc Ref No");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Doc Ref Date");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Grn Doc Date Difference");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Grn Doc Date Difference";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Type");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            ////report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group");
            ////sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Service Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;


            ////report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
            ////sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            ////report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
            ////sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            ////report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
            ////sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;







            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTransactionAmountTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Amount");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Currency";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colCurrency = sheet1headreColIndex;
            sheet1headreColIndex++;






            sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGST";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colCGSTTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGST Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colCGSTTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SGST";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colSGSTTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SGST Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colSGSTTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "IGST";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colIGSTTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "IGST Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colIGSTTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TDS";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTDSTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TDS Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTDSTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TCS";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTCSTotal = sheet1headreColIndex;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TCS Tax (%)";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTCSTotal1 = sheet1headreColIndex;
            sheet1headreColIndex++;




            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Prepared By");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Prepared By";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Status");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Status";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;



            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Checking Name");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Checking Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Approving Name");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Approving Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;



            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;
            //sheet1headreColIndex++;




            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;

                report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["POId"].ToString());
                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["PODate"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["PartyName"].ToString());
                report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["PartyId"].ToString());
                report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["InvoicingPartyPlantId"].ToString());
                report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["InvoicingPartyPlant"].ToString());
                report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["DeliveryPartyPlantId"].ToString());
                report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["DeliveryPartyPlant"].ToString());
                report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["DocRefNo"].ToString());
                report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["DocDate"].ToString());
                report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
                report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["Currency"].ToString());
                report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGST"].ToString()));
                report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGSTTaxPercentage"].ToString()));
                report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["SGST"].ToString()));
                report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["SGSTTaxPercentage"].ToString()));
                report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IGST"].ToString()));
                report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IGSTTaxPercentage"].ToString()));
                report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TDS"].ToString()));
                report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TDSTaxPercentage"].ToString()));
                report.SetText(ref sheet1, _rowL, 23, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TCS"].ToString()));
                report.SetText(ref sheet1, _rowL, 24, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TCSTaxPercentage"].ToString()));
                report.SetText(ref sheet1, _rowL, 25, inventoryMaterialList.Rows[n]["AddedBy"].ToString());
                report.SetText(ref sheet1, _rowL, 26, inventoryMaterialList.Rows[n]["CheckedBY"].ToString());
                report.SetText(ref sheet1, _rowL, 27, inventoryMaterialList.Rows[n]["AuthorizedBy"].ToString());

            }
            _rowL++;

            if (fromDate != "" && toDate != "")
            {


                report.SetText(ref sheet1, _rowL, (Convert.ToInt32(colTransactionAmountTotal) - 2), "Total");
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal) - 2].CellStyle.Font.Bold = true;
                //sheet1.Range[1, _rowL, Convert.ToInt32(colTransactionQtyTotal) - 1, _rowL].Merge();
                object sumObject;
                sumObject = inventoryMaterialList.Compute("Sum(TransactionAmount)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionAmountTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
                sumObject = inventoryMaterialList.Compute("Sum(CGST)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colCGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
                sumObject = inventoryMaterialList.Compute("Sum(SGST)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colSGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
                sumObject = inventoryMaterialList.Compute("Sum(IGST)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colIGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                sumObject = inventoryMaterialList.Compute("Sum(TDS)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTDSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                sumObject = inventoryMaterialList.Compute("Sum(TCS)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTCSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;


            }

            sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);
            //_rowL++;

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            //sheet1.UsedRange.CellStyle.Font.Size = 8;
            sheet1.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);


        }

        #endregion ServiceAcknowledgement Register Report


        #region 

        public IEnumerable<object> getPOCheckedListData(string plantId)

        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10) = '201816';
				SELECT ROW_NUMBER()  OVER(ORDER BY  IR.Id) AS SiNo, IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106), ' ', '-') AS PODate

									--,IR.PODate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate

									--, IR.GateEntryNo
									--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106), ' ', '-') AS EntryDate
									  , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
										, IR.FixedAssetOrInventory, IR.PODepended
										--, IR.AlongwithInvoice
										--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
										  , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,pgl.CtnId
                                    ,IR.AddedBy
		                            ,PLC.LCANo PurchaseLC
									, Ctc.ContractNo ContructNumber
									 , Par.UserName Customer
									  , IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,isnull(PO.RequisitionId, '') RequisitionId
						 FROM[TRN].[PurchaseOrder] AS IR left JOIN[HKP].[Party] AS P ON IR.PartyId = P.Id

						LEFT JOIN(SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG

									ON PAG.Id= C.PartyAccountGroupId WHERE C.PartyType= 'Vendor') AS CP ON CP.PartyId = IR.PartyId AND CP.PlantId = IR.PlantId


						LEFT JOIN[dbo].[PurchaseLC] PLC ON PLC.Id = IR.PurchaseLCId
						LEFT JOIN[dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN[HKP].[Party] Par ON Par.Id = Ctc.CustomerId

						LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId = IR.CheckedBy

						LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId = IR.AuthorizedBy


						left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id

						left JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId = PT.Id

						LEFT JOIN[HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId = IPP.Id

						LEFT JOIN[MST].[AddressMaster] AS AM ON IPP.AddressMasterId = AM.Id

						LEFT JOIN[SCS].[State] AS S1 ON AM.StateId = S1.Id

						LEFT JOIN[HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId = DPP.Id

						LEFT JOIN[MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId = AM2.Id

						LEFT JOIN[SCS].[State] AS S2 ON AM2.StateId = S2.Id

						LEFT JOIN[ORG].Plant PL ON PL.Id = IR.PlantId

						LEFT JOIN[MST].[AddressMaster] AS AMP ON AMP.Id = PL.AddressMasterId
						LEFT JOIN[SCS].[State] AS SP ON SP.Id = AMP.StateId

						LEFT JOIN(SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A

									JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id WHERE B.PlantId= '20171' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId = IR.Id

						LEFT JOIN(SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id

									WHERE B.PlantId= '" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId = IR.Id

						LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId = UoM.Id

						LEFT JOIN(Select count(Id) as CtnId
						,POID from TRN.PurchaseOrderApprovalLog where Status = 'Approved' group by POID) as pgl  on pgl.POID = IR.Id

						LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											, RequisitionId= STUFF((select distinct ',' + xpo.Id from
												trn.MaterialRequsitionMaster xpo
	
												INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id = xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId = PDAMAP.InventoryReceiveId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from TRN.PurchaseOrderDetail PDAMAP
											LEFT JOIN[TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId

				WHERE   CheckedbyStatus = 'Checked' AND AuthorizedByStatus='For Approval' Order by IR.PODate ASC  ";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> getPOApprovedListData(string plantId)

        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IR.AddedBy
			                        ,PLC.LCANo PurchaseLC
									,Ctc.ContractNo ContructNumber
									,Par.UserName Customer
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI1.EmployeeName AuthorizedBy
	                                ,IR.CheckedHoldRejectReason CheckedRejectReason

                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                           LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE    AuthorizedByStatus='Approved' Order by IR.POdate ASC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        private string GRNApprovalLogTblId()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GRNApprovalLogTbl", out sID);
            return sID;
        }
        private string PurchaseOrderApprovalLogId()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PurchaseOrderApprovalLog", out sID);
            return sID;
        }
        public void POUncheckUpdate(string MasterId, Dictionary<string, object> UserSendData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = "select * from TRN.InventoryReceive where Id='" + MasterId + "'";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

                string sqllog = "select * from TRN.GRNApprovalLogTbl where 1=2";
                con.OpenDataSetThroughAdapter(sqllog, out DataSet dsDetailLog, false, "1");

                //for (int i = 0; i < UserSendData.Count; i++)
                //{
                dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData["Id"].ToString() + "'";
                if (dsDetail.Tables[0].DefaultView.Count == 0)
                {
                    //DataRow dr = dsDetail.Tables[0].NewRow();
                    //dr["Id"] = GRNDAddiTaxId();
                    //dr["TaxCodeId"] = UserSendData[i]["TaxCodeId"];
                    //dr["Percentage"] = UserSendData[i]["ValueOfFixed"];
                    //dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
                    //dr["AddedBy"] = identity.Name;
                    //dr["AddedDate"] = System.DateTime.Now.ToString();
                    //dr["AddedFromIP"] = identity.IPAddress;
                    ////dr["UpdatedBy"] = "";
                    ////dr["UpdatedDate"] = "";
                    ////dr["UpdatedFromIP"] = "";
                    //dr["InventoryReceiveId"] = MasterId.ToString();
                    //dsDetail.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["CheckedByStatus"] = "ForChecked";
                    dr["AuthorizedBy"] = null;
                    dr["AuthorizedByStatus"] = null;
                    dr["IsApproved"] = 0;
                    dr.EndEdit();
                    DataRow drlog = dsDetailLog.Tables[0].NewRow();
                    drlog["Id"] = MasterId.ToString() + '-' + GRNApprovalLogTblId();
                    drlog["CompanyGroupId"] = identity.CompanyGroupId;
                    drlog["CompanyId"] = identity.CompanyId;
                    drlog["PlantId"] = identity.PlantId;
                    drlog["ApprovedBy"] = identity.EmployeeId;
                    drlog["Date"] = System.DateTime.Now.ToString();
                    drlog["POValue"] = UserSendData["TransactionQty"];
                    drlog["Status"] = "UnChecked";
                    drlog["AddedBy"] = identity.Name;
                    drlog["AddedDate"] = System.DateTime.Now.ToString();
                    drlog["AddedFromIP"] = identity.IPAddress;
                    //dr["UpdatedBy"] = "";
                    //dr["UpdatedDate"] = "";
                    //dr["UpdatedFromIP"] = "";
                    drlog["GRNID"] = MasterId.ToString();
                    dsDetailLog.Tables[0].Rows.Add(drlog);
                }
                //}


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsDetail, dsDetailLog);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void POUnapprovedUpdate(string MasterId, Dictionary<string, object> UserSendData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = "select * from TRN.InventoryReceive where Id='" + MasterId + "'";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

                string sqllog = "select * from TRN.GRNApprovalLogTbl where 1=2";
                con.OpenDataSetThroughAdapter(sqllog, out DataSet dsDetailLog, false, "1");

                //for (int i = 0; i < UserSendData.Count; i++)
                //{
                dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData["Id"].ToString() + "'";
                if (dsDetail.Tables[0].DefaultView.Count == 0)
                {

                    //DataRow dr = dsDetail.Tables[0].NewRow();
                    //dr["Id"] = GRNDAddiTaxId();
                    //dr["TaxCodeId"] = UserSendData[i]["TaxCodeId"];
                    //dr["Percentage"] = UserSendData[i]["ValueOfFixed"];
                    //dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
                    //dr["AddedBy"] = identity.Name;
                    //dr["AddedDate"] = System.DateTime.Now.ToString();
                    //dr["AddedFromIP"] = identity.IPAddress;
                    ////dr["UpdatedBy"] = "";
                    ////dr["UpdatedDate"] = "";
                    ////dr["UpdatedFromIP"] = "";
                    //dr["InventoryReceiveId"] = MasterId.ToString();
                    //dsDetail.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["CheckedByStatus"] = "ForChecked";
                    dr["AuthorizedBy"] = null;
                    dr["AuthorizedByStatus"] = null;
                    dr["IsApproved"] = 0;
                    dr.EndEdit();
                    //Id,CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,POValue,Status,AddedBy,AddedDate,AddedFromIP,UpdatedBy,UpdatedDate,UpdatedFromIP,GRNID
                    DataRow drlog = dsDetailLog.Tables[0].NewRow();
                    drlog["Id"] = MasterId.ToString() + '-' + GRNApprovalLogTblId();
                    drlog["CompanyGroupId"] = identity.CompanyGroupId;
                    drlog["CompanyId"] = identity.CompanyId;
                    drlog["PlantId"] = identity.PlantId;
                    drlog["ApprovedBy"] = identity.EmployeeId;
                    drlog["Date"] = System.DateTime.Now.ToString();
                    drlog["POValue"] = UserSendData["TransactionQty"];
                    drlog["Status"] = "UnApproved";
                    drlog["AddedBy"] = identity.Name;
                    drlog["AddedDate"] = System.DateTime.Now.ToString();
                    drlog["AddedFromIP"] = identity.IPAddress;
                    //dr["UpdatedBy"] = "";
                    //dr["UpdatedDate"] = "";
                    //dr["UpdatedFromIP"] = "";
                    drlog["GRNID"] = MasterId.ToString();
                    dsDetailLog.Tables[0].Rows.Add(drlog);
                }
                //}

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsDetail, dsDetailLog);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        public IEnumerable<object> getCheckedList(string plantId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IR.AddedBy
		                            ,PLC.LCANo PurchaseLC
									,Ctc.ContractNo ContructNumber
									,Par.UserName Customer
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI1.EmployeeName AuthorizedBy,isnull(PO.RequisitionId,'')  RequisitionId

                        FROM [TRN].[PurchaseOrder] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                  LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                             GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
                        WHERE IR.PlantId='" + plantId + @"' and IR.CheckedBy='" + identity.EmployeeId + @"' AND CheckedbyStatus = 'Checked'  Order by IR.ID DESC";//AND IR.IsApproved=0
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> getPendingList(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='201816';
                           SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,pgl.CtnId
                                    ,IR.AddedBy
		                            ,PLC.LCANo PurchaseLC
									,Ctc.ContractNo ContructNumber
									,Par.UserName Customer
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,isnull(PO.RequisitionId,'') RequisitionId
                        FROM [TRN].[PurchaseOrder] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                        LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                              GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId
						,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
                WHERE IR.CheckedBy='" + identity.EmployeeId + "' AND CheckedbyStatus ='pending' Order by IR.PODate ASC  "; //IR.PlantId = '" + plantId + "' and
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetTaxCategoryListForSalesMaterial(string companyGroupId, string plantId, string partyPlantId, string hsnCodeId, string InventorySalesDate)
        {
            try
            {
                var sql = @"DECLARE
                            @partyState VARCHAR(30)
                            , @partyCountry VARCHAR(10)
                            , @plantState VARCHAR(30)
                            , @plantCountry VARCHAR(10)
                            , @plantId VARCHAR(30)='" + plantId + @"'
                            , @hsnCodeId VARCHAR(30)='" + hsnCodeId + @"'
                            SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP
					                            LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id WHERE PP.Id='" + partyPlantId + @"')
                            SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP
					                            LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id WHERE PP.Id='" + partyPlantId + @"')
                            SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)
                            SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)
                            SELECT NULL Id, TVD.TaxCategoryId, HN.Id HSNCodeId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage], 0) AS [Percentage], 0 TotalAmount
                            FROM [MST].[TaxVariantDetail] AS TVD
                            JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                            JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                            LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
	                            FROM [MST].[HSNTaxPercentage] WHERE CountryId=@partyCountry AND HSNCodeId=@hsnCodeId) AS TBL WHERE RN=1 AND EffectiveDate<='" + InventorySalesDate + @"'
                            ) AS HP ON HP.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON @hsnCodeId=HN.Id
                            WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry
                            AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN 'DomesticSales'
			                            WHEN @partyCountry<>@plantCountry THEN 'OverseasSales' END
                            AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
			                            WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
                                OR TV.Different IS NULL)
                            ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetFiscalYear(string POID)
        {
            DateTime date = DateTime.Now;
            string formattedDate = date.ToString("dd-MM-yyyy");
            try
            {
                var _sql = @"SELECT StartDate,EndDate FROM scs.FiscalYear WHERE StartDate <='" + formattedDate + "' AND EndDate >='" + formattedDate + "'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetListForServiceRequisition(string Id)
        {
            try
            {
                var sql = @"SELECT
                           SRD.Id ServiceRequsitionDetailId 
                          ,SRD.ServiceRequisitionMasterID ServiceReqMasterId
                          ,SRD.CurrencyId
                          ,SRD.Rate
                          --,SM.Description
                          ,SRD.ServiceMasterId
                          ,SRD.TotalServiceTranAmount
                          ,SRD.TotalServiceBooksCurrencyAmount Amount 
                          ,SRD.AddedBy
                          ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate 
                          ,SRD.AddedDate
                          ,SRD.AddedFromIP
                          ,SRD.UpdatedBy
                          ,SRD.UpdatedDate
                          ,SRD.UpdatedFromIP
                          ,SRD.Remarks,SRD.RefferenceNo
                          ,SM.StandardName ServiceMasterName
						  ,SM.ID ServiceMasterId
                          ,CR.Code CurrencyName
                          ,0  Active 
                          ,SRD.Id ServiceRequsitionDetailId
	                      ,SRD.Description,SM.HSNCodeId
                          ,ISNULL(SRD.Qty,0) Qty
						  ,ISNULL(SRD.TransactionRate,0) TransactionRate
                          ,UOM.UserName UoM
                          ,SRD.TransactionUoMId,Isnull(PODetail.Qty,0) OtherPOReceivedQty,0 TransactionQty
                  FROM TRN.ServiceRequsitionDetail SRD
                  left JOIN[TRN].[ServiceRequsitionMaster] AS SRM ON SRM.Id=SRD.ServiceRequisitionMasterID
                  left JOIN[HKP].[ServiceMaster]   AS SM ON SM.Id= SRD.ServiceMasterId
                  left JOIN [SCS].[Currency] AS CR ON CR .Id= SRD.CurrencyId
                  left JOIN SCS.UnitOfMeasurement UOM ON UOM.Id=SRD.TransactionUoMId
                   LEFT JOIN (SELECT SPD.ServiceRequsitionDetailId,SUM(Qty) Qty 
				             from trn.ServicePODetail SPD
							 --LEFT JOIN trn.servicePOMaster SPM ON SPD.ServicePOMasterId=SPM.Id
							 where SPD.ServicePOMasterId!='" + Id + @"'
							 GROUP BY SPD.ServiceRequsitionDetailId
							 )PODetail ON PODetail.ServiceRequsitionDetailId=SRD.Id				 
                  WHERE SRM.AuthorizedByStatus='Approved'               
                --SRD.Id not in(select ServiceRequsitionDetailId from trn.ServicePODetail where ServiceRequsitionDetailId is not null)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        public void PORollBackChecked(string MasterId, Dictionary<string, object> UserSendData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = "select * from TRN.PurchaseOrder where Id='" + MasterId + "'";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

                string sqllog = "select * from [TRN].[PurchaseOrderApprovalLog] where 1=2";
                con.OpenDataSetThroughAdapter(sqllog, out DataSet dsDetailLog, false, "1");

                //for (int i = 0; i < UserSendData.Count; i++)
                //{
                dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData["Id"].ToString() + "'";
                if (dsDetail.Tables[0].DefaultView.Count == 0)
                {
                    //DataRow dr = dsDetail.Tables[0].NewRow();
                    //dr["Id"] = GRNDAddiTaxId();
                    //dr["TaxCodeId"] = UserSendData[i]["TaxCodeId"];
                    //dr["Percentage"] = UserSendData[i]["ValueOfFixed"];
                    //dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
                    //dr["AddedBy"] = identity.Name;
                    //dr["AddedDate"] = System.DateTime.Now.ToString();
                    //dr["AddedFromIP"] = identity.IPAddress;
                    ////dr["UpdatedBy"] = "";
                    ////dr["UpdatedDate"] = "";
                    ////dr["UpdatedFromIP"] = "";
                    //dr["InventoryReceiveId"] = MasterId.ToString();
                    //dsDetail.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["CheckedByStatus"] = "Pending";
                    dr["AuthorizedBy"] = null;
                    dr["AuthorizedByStatus"] = null;
                    dr["IsApproved"] = 0;
                    dr.EndEdit();
                    DataRow drlog = dsDetailLog.Tables[0].NewRow();
                    drlog["Id"] = MasterId.ToString() + '-' + PurchaseOrderApprovalLogId();
                    drlog["CompanyGroupId"] = identity.CompanyGroupId;
                    drlog["CompanyId"] = identity.CompanyId;
                    drlog["PlantId"] = identity.PlantId;
                    drlog["ApprovedBy"] = identity.EmployeeId;
                    drlog["Date"] = System.DateTime.Now.ToString();
                    drlog["POValue"] = UserSendData["TransactionQty"];
                    drlog["Status"] = "UnChecked";
                    drlog["AddedBy"] = identity.Name;
                    drlog["AddedDate"] = System.DateTime.Now.ToString();
                    drlog["AddedFromIP"] = identity.IPAddress;
                    drlog["UpdatedBy"] = identity.Name;
                    drlog["UpdatedDate"] = DateTime.Now;
                    drlog["UpdatedFromIP"] = identity.IPAddress;
                    drlog["POID"] = MasterId.ToString();
                    dsDetailLog.Tables[0].Rows.Add(drlog);
                }
                //}


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsDetail, dsDetailLog);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void PORollBackUnApproved(string MasterId, Dictionary<string, object> UserSendData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var isgrn =  _sqlRepository.GetDataCollection(@" SELECT POId AS CheckingColumn FROM TRN.InventoryReceiveDetail    WHERE POId = '" + MasterId + @"'");
                if (isgrn.Count==0)
                {
                    string sql = "select * from TRN.PurchaseOrder where Id='" + MasterId + "'";
                    
                    con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

                    string sqllog = "select * from [TRN].[PurchaseOrderApprovalLog] where 1=2";
                    con.OpenDataSetThroughAdapter(sqllog, out DataSet dsDetailLog, false, "1");

                    //for (int i = 0; i < UserSendData.Count; i++)
                    //{
                    dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData["Id"].ToString() + "'";
                    if (dsDetail.Tables[0].DefaultView.Count == 0)
                    {
                        //DataRow dr = dsDetail.Tables[0].NewRow();
                        //dr["Id"] = GRNDAddiTaxId();
                        //dr["TaxCodeId"] = UserSendData[i]["TaxCodeId"];
                        //dr["Percentage"] = UserSendData[i]["ValueOfFixed"];
                        //dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
                        //dr["AddedBy"] = identity.Name;
                        //dr["AddedDate"] = System.DateTime.Now.ToString();
                        //dr["AddedFromIP"] = identity.IPAddress;
                        ////dr["UpdatedBy"] = "";
                        ////dr["UpdatedDate"] = "";
                        ////dr["UpdatedFromIP"] = "";
                        //dr["InventoryReceiveId"] = MasterId.ToString();
                        //dsDetail.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["CheckedByStatus"] = "Pending";
                        dr["AuthorizedBy"] = null;
                        dr["AuthorizedByStatus"] = null;
                        dr["IsApproved"] = 0;
                        dr["IsClosed"] = 0;
                        dr.EndEdit();
                        DataRow drlog = dsDetailLog.Tables[0].NewRow();
                        drlog["Id"] = MasterId.ToString() + '-' + PurchaseOrderApprovalLogId();
                        drlog["CompanyGroupId"] = identity.CompanyGroupId;
                        drlog["CompanyId"] = identity.CompanyId;
                        drlog["PlantId"] = identity.PlantId;
                        drlog["ApprovedBy"] = identity.EmployeeId;
                        drlog["Date"] = System.DateTime.Now.ToString();
                        drlog["POValue"] = UserSendData["TransactionQty"];
                        drlog["Status"] = "UnApproved";
                        drlog["AddedBy"] = identity.Name;
                        drlog["AddedDate"] = System.DateTime.Now.ToString();
                        drlog["AddedFromIP"] = identity.IPAddress;
                        drlog["UpdatedBy"] = identity.Name; ;
                        drlog["UpdatedDate"] = DateTime.Now;
                        drlog["UpdatedFromIP"] = identity.IPAddress;
                        drlog["POID"] = MasterId.ToString();
                        dsDetailLog.Tables[0].Rows.Add(drlog);
                    }
                    //}


                    clsStaticInfo info = new clsStaticInfo();
                    info.SaveDataSets(dsDetail, dsDetailLog);
                }
                else
                {
                    throw new CustomException("PO no "+ MasterId + " already have used in GRN. Rollback should not allow in this case!");
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> GetApprovedListForPOBYReq(string plantId, string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var Sql = @"
				Select top(100) * from (
				SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
						, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
						--,IR.PODate
						, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
						, CP.UserName AS PartyAccountGroupName
						, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						--, IR.GateEntryNo
						--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
						, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
						, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
						, IR.FixedAssetOrInventory, IR.PODepended
						--, IR.AlongwithInvoice
						--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
						, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
						, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
						, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
						,pgl.CtnId
						--,IR.AddedBy
						,IR.CheckedByStatus AS CheckedByStatus,PT.PaymentMode
						,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,eI2.EmployeeName As Addedby,PO.RequisitionId
				FROM [TRN].[PurchaseOrder] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
				LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
						ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId   
				LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
				LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
				LEFT JOIN dbo.EmployeeInformation eI2 ON eI2.SystemId=IR.AddedBy
				LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
				LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
				LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
				LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
				LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
				LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
				LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
				LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
				LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
				LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
				LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
				LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
						JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
				LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
						WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
				LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
				LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
				WHERE  IR.POType='POByReq' 
				AND IR.PlantId='" + plantId + @"' 
				AND IR.CheckedBy IS NOT NULL 
				AND IR.CheckedByStatus='Checked' 
				AND IR.AuthorizedBy IS NOT NULL 
				AND IR.AuthorizedByStatus='Approved' 
				AND isnull(IR.IsClosed,0)=0 

				UNION ALL

				SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
						, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
						--,IR.PODate
						, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
						, CP.UserName AS PartyAccountGroupName
						, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						--, IR.GateEntryNo
						--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
						, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
						, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
						, IR.FixedAssetOrInventory, IR.PODepended
						--, IR.AlongwithInvoice
						--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
						, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
						, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
						, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
						,pgl.CtnId
						--,IR.AddedBy
						,IR.CheckedByStatus AS CheckedByStatus,PT.PaymentMode
						,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,eI2.EmployeeName As Addedby,PO.RequisitionId
				FROM [TRN].[PurchaseOrder] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
				LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
						ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId   
				LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
				LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
				LEFT JOIN dbo.EmployeeInformation eI2 ON eI2.SystemId=IR.AddedBy
				LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
				LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
				LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
				LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
				LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
				LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
				LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
				LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
				LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
				LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
				LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
				LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
						JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
				LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
						WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
				LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
				LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
				WHERE  IR.POType='POByReq' 
				AND IR.PlantId='" + plantId + @"'
				AND IR.CheckedByStatus  Is null
				AND IR.AuthorizedByStatus='Approved'
				AND isnull(IR.IsClosed,0)=0 

				UNION ALL

				SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
						, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
						--,IR.PODate
						, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
						, CP.UserName AS PartyAccountGroupName
						, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						--, IR.GateEntryNo
						--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
						, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
						, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
						, IR.FixedAssetOrInventory, IR.PODepended
						--, IR.AlongwithInvoice
						--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
						, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
						, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
						, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
						,pgl.CtnId
						--,IR.AddedBy
						,IR.CheckedByStatus AS CheckedByStatus,PT.PaymentMode
						,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,eI2.EmployeeName As Addedby,PO.RequisitionId
				FROM [TRN].[PurchaseOrder] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
				LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
						ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId   
				LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
				LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
				LEFT JOIN dbo.EmployeeInformation eI2 ON eI2.SystemId=IR.AddedBy
				LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
				LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
				LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
				LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
				LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
				LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
				LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
				LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
				LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
				LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
				LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
				LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
						JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
				LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
						WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
				LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
				LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
				WHERE  IR.POType='POByReq' 
				AND IR.PlantId='" + plantId + @"'
				AND IR.Id in(Select distinct POId from trn.InventoryReceive where POId is not null)--and RequisitionId='110232'
				AND IR.CheckedByStatus IS NULL
				AND IR.AuthorizedByStatus IS NULL
				AND isnull(IR.IsClosed,0)=0 
				) AS TEMP WHERE " + strkey + " Order by  CONVERT(datetime,TEMP.PODate) desc";

            return _sqlRepository.GetDataCollection(Sql);
        }

        public IEnumerable<object> InWardMaterialSql(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"select IRD.Id,format(IRD.AddedDate,'dd-MMM-yyyy') Date,IR.DocRefNo,IRD.POId PONo,P.UserName VendorName,MM.UserName Material,MMA.StandardName Article
									,NULL SKU1,IRD.lotNo,IRD.TransactionQty Qty,uom.UserName UOM,IRD.MaterialTranRate Rate
									,ISNULL(GE.PackageQty,0) RollBag,GE.PersonName Transporter
                                    ,GE.Remarks GRNo,IR.Id GRNNo,'' Remarks
									
									,BuyerReferenceNo=STUFF((select distinct ','+MO.BuyerReferenceNo from
                                    trn.PurchaseOrderDetail POD
                                    LEFT JOIN TRN.PurchaseOrder xpo on xpo.Id=POD.InventoryReceiveId
                                     LEFT JOIN (select PODetailsId,InventoryReceiveId from TRN.InventoryReceiveDetail) IRD on IRD.PODetailsId=POD.Id and IRD.InventoryReceiveId=IR.Id
                                    LEFT JOIN DBO.[Contract] C on C.Id=xpo.ContractId
                                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
                                    LEFT JOIN trn.MasterOrderItem MO on MO.Id=SO.MasterOrderItemId
                                    where POD.Id=IRD.PODetailsId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									from trn.InventoryReceiveDetail IRD
									left join trn.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId
									left join trn.InventoryMaterial IM on IM.Id=IRD.InventoryMaterialId
									left join MST.MaterialMasterArticle MMA on MMA.Id=IM.ArticleId
									left join MST.MaterialMaster MM on MM.Id=MMA.MaterialMasterId
									left join HKP.Party P on P.Id=IR.PartyId
									left join [SCS].[UnitOfMeasurement] uom on uom.Id=IRD.TransactionUoMId	
									left join TRN.[GateEntry] GE on GE.Id=IR.GateEntryNo	
                                     									 
						where Convert(date,IR.GRNDate) between '" + fromDate + @"' AND '" + toDate + @"'";

                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

    }
}
