using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.MaterialManagement.CutPlan
{
    public class clsCutPlan
    {
        ISqlRepository _sqlRepository;
        public clsCutPlan()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetProductionOrderData(string entityId)
        {

            try
            {
                string sql = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, PD.Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
									,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory,SO.Qty
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
									LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = PD.ProductionOrderId
								   WHERE  E.Id='" + entityId + "'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetLineItemData(string entityId, string processId, string productionOrderId, string masterId)
        {
            try
            {
                var _sql = @"SELECT POD.Id,0 AS Checked, POD.ProductionOrderId, POD.SalesOrderId
	                            --, RM.Id AS RecipeMaterialId
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
                                , LSD = REPLACE(CONVERT(CHAR(11), LSD, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , DEST.UserName AS DestinationName, SHP.UserName AS ShipmentModeName
	                            , PO.PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.CM, SO.Rate,SO.Description
	                            , Flag = CAST(0 AS BIT),SO.DestinationDescription
                            FROM [TRN].[ProductionOrderDetail] AS POD
                            LEFT JOIN [TRN].[SalesOrder] AS SO ON POD.SalesOrderId=SO.Id
                            LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
                            --LEFT JOIN [TRN].[RecipeMaterial] AS RM ON RM.MaterialMasterId = MOI.MaterialMasterId AND RM.ArticleId = MOI.ArticleId
                            JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id --RM.MaterialMasterId = MM.Id AND 
                            JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id --RM.ArticleId = ART.Id AND 
                            LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                            LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                            LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                            LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                            LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                            WHERE POD.ProductionOrderId = '" + productionOrderId + "'";

                _sql = @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN
	                            ,POD.Id, POD.ProductionOrderId, MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName,PM.UserName AS ProductName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,MO.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo AS OwnOrderNo
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
                                , LSD = REPLACE(CONVERT(CHAR(11), SO.LSD, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , isnull(PO.PONumber,'') AS PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.CM, SO.Rate,SO.Description
	                            , Flag = CAST(0 AS BIT),SO.DestinationDescription
                       FROM 
                       [TRN].[ProductionOrderDetail] AS POD
                       JOIN [TRN].[SalesOrder] AS SO ON pod.SalesOrderId=so.Id
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                            WHERE POD.ProductionOrderId = '" + productionOrderId + "'" +
                            "ORDER BY MOI.MATERIALMASTERID,MOI.ArticleID";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> getMarkerList(string MaterialId)
        {
            try
            {
                var _sql = @"select M.Id [Value],M.UserName [Text],c.Id SKUId ,c.UserName SKU From MarkerMaster M
								left join HKP.Characteristics c on M.CharacteristicsId=c.Id
								where M.FGMaterialMasterId='" + MaterialId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> GetMarkerDetailList(string MarkerId)
        {
            try
            {
                var _sql = @"select M.Id,M.Ratio,CharacteristicsValueId ,C.UserName Characteristicsvalue 
								From MarkerDetails M
								Left Join hkp.Characteristicsvalue c on c.Id=M.CharacteristicsValueId 
								where MarkerMasterId='" + MarkerId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> GetOtherSkuDetailList(string OtherSkuId, string SOId, string Sequence)
        {
            try
            {
                var _sql = "";

                if (Sequence == "1")
                {
                    _sql = @"select IsSelect=Convert(bit, 'False'), c.UserName Characteristicsvalue ,c.Id CharacteristicsId,sum(fc.Qty)Qty
                                , '' MinimumPlyActualValue, '' MinimumPlyOptionValue
								From TRN.FirstCharacteristics fc
								left join hkp.Characteristicsvalue c on c.Id = fc.CharacteristicsValueId
								left join hkp.Characteristics ch on ch.Id=c.CharacteristicsId and ch.Id=fc.CharacteristicsId
								where  ch.Id='" + OtherSkuId + "' and fc.SalesOrderId in (" + SOId + @") group by  c.UserName  ,c.Id ";
                }
                else
                {
                    _sql = @"select IsSelect=Convert(bit, 'False'), c.UserName Characteristicsvalue ,c.Id CharacteristicsId,sum(sc.Qty)Qty
                                , '' MinimumPlyActualValue, '' MinimumPlyOptionValue
								From TRN.SecondCharacteristics sc
								left join hkp.Characteristicsvalue c on c.Id = sc.CharacteristicsValueId
								left join hkp.Characteristics ch on ch.Id=c.CharacteristicsId and ch.Id=sc.CharacteristicsId
								where  ch.Id='" + OtherSkuId + "' and sc.SalesOrderId in (" + SOId + @") group by  c.UserName  ,c.Id ";
                }

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public void Save(List<Dictionary<string, object>> CalculatedValueList, List<Dictionary<string, object>> FGCharacteristicsValueList, CutPlanMaster MasterData, CutPlanMarkerDetails CPMarkerDetails, List<Dictionary<string, object>> SkuValueList)
        {
            try
            {
                DataSet dsCutPlanMaster;
                DataSet dsCutPlanMarkerDetails;
                DataSet dsCutPlanChild;
                DataSet dsCutPlanFormation;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                string CutPlanMasterId = string.Empty;
                bplib.clsGenID objGenID = new bplib.clsGenID();

                #region Cut Plan M A S T E R save

                string sql = "SELECT * FROM CutPlanMaster WHERE Id='" + MasterData.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsCutPlanMaster, false, "1");

                if (dsCutPlanMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsCutPlanMaster.Tables[0].NewRow();

                    string sID = string.Empty;
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[CutPlanMaster]", out sID);
                    CutPlanMasterId = "M" + sID;
                    dr["Id"] = CutPlanMasterId;
                    dr["ProductionOrderId"] = MasterData.ProductionOrderId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsCutPlanMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsCutPlanMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    CutPlanMasterId = dr["Id"].ToString();
                    dr["ProductionOrderId"] = MasterData.ProductionOrderId;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = MasterData.ProductionOrderId;
                    dr.EndEdit();
                }

                #endregion

                #region Cut Plan M A R K E R Details

                string sql1 = "SELECT * FROM CutPlanMarkerDetails WHERE CutPlanMasterId='" + CutPlanMasterId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsCutPlanMarkerDetails, false, "1");

                if (dsCutPlanMarkerDetails.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsCutPlanMarkerDetails.Tables[0].NewRow();


                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[CutPlanMarkerDetails]", out string CPM);

                    dr["Id"] = "D" + CPM;
                    dr["CutPlanMasterId"] = CutPlanMasterId;
                    dr["MarkerId"] = CPMarkerDetails.MarkerId;
                    dr["MarkerCharacteristicsId"] = CPMarkerDetails.MarkerCharacteristicsId;
                    dr["RoundingType"] = CPMarkerDetails.RoundingType;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsCutPlanMarkerDetails.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsCutPlanMarkerDetails.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();

                    dr["CutPlanMasterId"] = CutPlanMasterId;
                    dr["MarkerId"] = CPMarkerDetails.MarkerId;
                    dr["MarkerCharacteristicsId"] = CPMarkerDetails.MarkerCharacteristicsId;
                    dr["RoundingType"] = CPMarkerDetails.RoundingType;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = MasterData.ProductionOrderId;
                    dr.EndEdit();
                }

                #endregion

                #region Cut Plan F O R M A T I O N

                string sql2 = "SELECT * FROM CutPlanFormation WHERE CutPlanMasterId='" + CutPlanMasterId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql2, out dsCutPlanFormation, false, "1");

                while (dsCutPlanFormation.Tables[0].DefaultView.Count > 0)
                {
                    dsCutPlanFormation.Tables[0].DefaultView[0].Delete();
                }

                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[CutPlanFormation]", out string TempId);
                int count = 0;
                for (int i = 0; i < FGCharacteristicsValueList.Count; i++)
                {
                    for (int j = 0; j < SkuValueList.Count; j++)
                    {
                        count++;
                        DataRow dr = dsCutPlanFormation.Tables[0].NewRow();
                        dr["Id"] = "F" + TempId + count;
                        dr["CutPlanMasterId"] = CutPlanMasterId;
                        dr["MarkerCharacteristicsValueId"] = FGCharacteristicsValueList[i]["CharacteristicsValueId"].ToString();
                        dr["MarkerRatio"] = FGCharacteristicsValueList[i]["Ratio"];
                        dr["CalculatedQty"] = SkuValueList[j]["MinimumPlyActualValue"];
                        dr["QtyForCalculation"] = SkuValueList[j]["Qty"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsCutPlanFormation.Tables[0].Rows.Add(dr);
                    }
                }

                #endregion

                #region Cut Plan C H I L D

                string sql3 = "SELECT * FROM CutPlanChild WHERE CutPlanMasterId='" + CutPlanMasterId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql3, out dsCutPlanChild, false, "1");

                while (dsCutPlanChild.Tables[0].DefaultView.Count > 0)
                {
                    dsCutPlanChild.Tables[0].DefaultView[0].Delete();
                }

                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[CutPlanChild]", out string ChildId);
                count = 0;
                for (int j = 0; j < SkuValueList.Count; j++)
                {
                    count++;
                    DataRow dr = dsCutPlanChild.Tables[0].NewRow();
                    dr["Id"] = "F" + ChildId + count;
                    dr["CutPlanMasterId"] = CutPlanMasterId;
                    dr["CharacteristicsValueId"] = SkuValueList[j]["CharacteristicsId"].ToString();
                    dr["RoundingPlyValue"] = SkuValueList[j]["MinimumPlyOptionValue"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsCutPlanChild.Tables[0].Rows.Add(dr);

                }

                #endregion



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsCutPlanMaster, dsCutPlanMarkerDetails, dsCutPlanFormation, dsCutPlanChild);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
public class CutPlanMaster
{
    public string Id { get; set; }
    public string ProductionEntityId { get; set; }
    public string ProductionOrderId { get; set; }
}
public class CutPlanMarkerDetails
{
    public string Id { get; set; }
    public string CutPlanMasterId { get; set; }
    public string MarkerId { get; set; }
    public string MarkerCharacteristicsId { get; set; }
    public string RoundingType { get; set; }
}