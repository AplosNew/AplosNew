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
        public IEnumerable<object> GetOtherSkuDetailList(string OtherSkuId, string SOId, string Sequence,string CharacteristicsValueId)
        {
            try
            {
                var _sql = "";
                var _sql1 = "";

                if (Sequence == "1")
                {
                    _sql = @"select c.UserName Characteristicsvalue ,c.Id CharacteristicsId,sum(fc.Qty)Qty
                                , '' MinimumPlyActualValue, '' MinimumPlyOptionValue,'' CutPlanChildId
								From TRN.FirstCharacteristics fc
								left join hkp.Characteristicsvalue c on c.Id = fc.CharacteristicsValueId
								left join hkp.Characteristics ch on ch.Id=c.CharacteristicsId and ch.Id=fc.CharacteristicsId
								where  ch.Id='" + OtherSkuId + "' and fc.SalesOrderId in (" + SOId + @") group by  c.UserName  ,c.Id ";
                }
                else
                {
                    _sql = @"select c.UserName Characteristicsvalue ,c.Id CharacteristicsId,sum(sc.Qty)Qty
                                , '' MinimumPlyActualValue, '' MinimumPlyOptionValue,'' CutPlanChildId
								From TRN.SecondCharacteristics sc
								left join hkp.Characteristicsvalue c on c.Id = sc.CharacteristicsValueId
								left join hkp.Characteristics ch on ch.Id=c.CharacteristicsId and ch.Id=sc.CharacteristicsId
								where  ch.Id='" + OtherSkuId + "' and sc.SalesOrderId in (" + SOId + @") group by  c.UserName  ,c.Id ";
                }

                _sql1 = @"select cv.Id ColorId,cv.UserName Colorvalue, c.UserName Characteristicsvalue ,c.Id CharacteristicsId,sum(sc.Qty)Qty
                                ,m.Ratio,NULL AS CalculatedPlyQty,NULL AS AvailableQty
								From TRN.FirstCharacteristics fc
								left JOIN TRN.SecondCharacteristics sc ON SC.FirstCharacteristicsId=fc.Id AND SC.SalesOrderId=fc.SalesOrderId
								left join hkp.Characteristicsvalue c on c.Id = sc.CharacteristicsValueId
								left join hkp.Characteristicsvalue cv on cv.Id = fc.CharacteristicsValueId
                                LEFT JOIN MarkerDetails M ON M.CharacteristicsValueId = c.Id
								where    fc.SalesOrderId in (" + SOId + ") AND c.Id IN ("+ CharacteristicsValueId + @")
								 group by  c.UserName  ,c.Id, cv.UserName,cv.Id,m.Ratio";

               var ColorList= _sqlRepository.GetDataCollection(_sql, null);
               var SizeList= _sqlRepository.GetDataCollection(_sql1, null);
                for (int i = 0; i < ColorList.Count; i++)
                {
                    var TempData = SizeList.Where(x => x["ColorId"].ToString() == ColorList[i]["CharacteristicsId"].ToString()).ToList();
                    ColorList[i]["Qty"] = TempData;
                }
                return(ColorList);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Save(List<CutPlantCalculate> CalculatedValueList, List<Dictionary<string, object>> FGCharacteristicsValueList, CutPlanMaster MasterData, CutPlanMarkerDetails CPMarkerDetails, List<Dictionary<string, object>> SkuValueList)
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
                int count = 0;

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
                    MasterData.Id = dr["Id"].ToString();
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
                    MasterData.Id = dr["Id"].ToString();
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = MasterData.ProductionOrderId;
                    dr.EndEdit();
                }

                #endregion

                #region Cut Plan M A R K E R Details

                string cutplantMarkerDetails = string.Empty;

                string sql1 = "SELECT * FROM CutPlanMarkerDetails WHERE CutPlanMasterId='" + CutPlanMasterId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsCutPlanMarkerDetails, false, "1");

                if (dsCutPlanMarkerDetails.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsCutPlanMarkerDetails.Tables[0].NewRow();


                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[CutPlanMarkerDetails]", out string CPM);

                    dr["Id"] = "D" + CPM;
                    cutplantMarkerDetails = dr["Id"].ToString();
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
                    cutplantMarkerDetails = dr["Id"].ToString();
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


                #region Cut Plan C H I L D
                string CutPlanChildId = string.Empty;
                string sql3 = "SELECT * FROM CutPlanChild WHERE CutPlanMarkerDetailsId='" + cutplantMarkerDetails + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql3, out dsCutPlanChild, false, "1");

                while (dsCutPlanChild.Tables[0].DefaultView.Count > 0)
                {
                    dsCutPlanChild.Tables[0].DefaultView[0].Delete();
                }

                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[CutPlanChild]", out string ChildId);
                count = 0;
                for (int j = 0; j < CalculatedValueList.Count; j++)
                {
                    count++;
                    DataRow dr = dsCutPlanChild.Tables[0].NewRow();
                    dr["Id"] = "F" + ChildId + count;
                    CutPlanChildId = dr["Id"].ToString();
                    CalculatedValueList[j].CutPlanChildId = CutPlanChildId;
                    dr["CutPlanMarkerDetailsId"] = cutplantMarkerDetails;
                    dr["CharacteristicsValueId"] = CalculatedValueList[j].CharacteristicsId;
                    dr["RoundingPlyValue"] = CalculatedValueList[j].MinimumPlyOptionValue;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsCutPlanChild.Tables[0].Rows.Add(dr);

                }

                #endregion

                #region Cut Plan F O R M A T I O N

                string CutPlanFormation = string.Empty;
                string sql2 = "SELECT * FROM CutPlanFormation WHERE CutPlanChildId='" + CutPlanChildId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql2, out dsCutPlanFormation, false, "1");

                while (dsCutPlanFormation.Tables[0].DefaultView.Count > 0)
                {
                    dsCutPlanFormation.Tables[0].DefaultView[0].Delete();
                }

                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[CutPlanFormation]", out string TempId);

                //for (int i = 0; i < FGCharacteristicsValueList.Count; i++)
                //{
                //    for (int j = 0; j < SkuValueList.Count; j++)
                //    {
                //        count++;
                //        DataRow dr = dsCutPlanFormation.Tables[0].NewRow();
                //        dr["Id"] = "F" + TempId + count;

                //        dr["CutPlanChildId"] = SkuValueList[j]["CutPlanChildId"];
                //        dr["MarkerCharacteristicsValueId"] = FGCharacteristicsValueList[i]["CharacteristicsValueId"].ToString();
                //        dr["MarkerRatio"] = FGCharacteristicsValueList[i]["Ratio"];
                //        dr["CalculatedQty"] = clsStaticInfo.dbl(SkuValueList[j]["MinimumPlyOptionValue"]) * clsStaticInfo.dbl(FGCharacteristicsValueList[i]["Ratio"]) ;
                //        dr["QtyForCalculation"] = SkuValueList[j]["Qty"];

                //        dr["AddedBy"] = identity.Name;
                //        dr["AddedDate"] = DateTime.Now;
                //        dr["AddedFromIP"] = identity.IPAddress;

                //        dr["UpdatedBy"] = identity.Name;
                //        dr["UpdatedDate"] = DateTime.Now;
                //        dr["UpdatedFromIP"] = identity.IPAddress;

                //        dsCutPlanFormation.Tables[0].Rows.Add(dr);
                //    }
                //}

                foreach (var item in CalculatedValueList)
                {
                    foreach (var y in item.Qty)
                    {
                        count++;
                        DataRow dr = dsCutPlanFormation.Tables[0].NewRow();
                        dr["Id"] = "F" + TempId + count;

                        dr["CutPlanChildId"] = item.CutPlanChildId;
                        dr["MarkerCharacteristicsValueId"] = y.CharacteristicsId;
                        dr["MarkerRatio"] = y.Ratio;
                        dr["CalculatedQty"] = y.CalculatedPlyQty;
                        dr["QtyForCalculation"] = y.Qty;

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




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsCutPlanMaster, dsCutPlanMarkerDetails, dsCutPlanChild, dsCutPlanFormation);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Marker
        public IEnumerable<object> GetMarkerCheckByCbo()
        {
            var sql = @"select distinct E.SystemId As Value,(E.EmployeeCode+'-'+ E.EmployeeName) Text,A.ActionStatus  
                          from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where E.EmployeeStatus='Active' AND A.ActionStatus= 'MarkerCheckedBy'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetMarkerApproveByCbo()
        {
            var sql = @"select distinct E.SystemId As Value,(E.EmployeeCode+'-'+ E.EmployeeName) Text,A.ActionStatus  
                          from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where E.EmployeeStatus='Active' AND A.ActionStatus='MarkerApproveBy'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetMarkerData()
        {
            string sql = @"select m.*,MP.PlanName MasterPlan From MarkerMaster m
LEFT JOIN [MST].[MasterPlan] MP ON MP.Id=M.MasterPlanId
Where CheckByStatus IN('To Be Check','Pending','Reject')
order by m.Sequence ";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetSOList(string MasterPlanId)
        {
            string sql = @"select distinct SO.Id SONo,CAST(0 as bit) Flag,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy')DeliveryDate,MOI.OwnReferenceNo,MOI.BuyerReferenceNo,SO.Qty
,isnull((select SOPlanQty from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id),SO.Qty + (MO.ExtraOrderPercentage*SO.Qty / 100)) as SOPlanQty,SO.Reason Remarks

from TRN.ProductionOrder PO
left join TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderId=PO.Id
left join TRN.ProductionOrderDetail POD ON POD.ProductionOrderId=PO.Id
left join TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
left join [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
left join [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
LEFT JOIN [MST].[MasterPlanSODetails] CPD on CPD.SalesOrderId=SO.Id and CPD.MasterPlanId='" + MasterPlanId + @"'
where CPD.MasterPlanId = '" + MasterPlanId + @"'
and PO.ProductionStatusId in (select Id from HKP.ProductionStatus where MasterPlanApplicable=1)
and SO.OrderStatusId in (select Id from HKP.OrderStatus OS where OS. MasterPlanApplicable=1)
and SO.Id in (select SalesOrderId from MST.MasterPlanSODetails where MasterPlanId='" + MasterPlanId + @"' and Status=1) ";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetMarkerSOData(string markerId)
        {
            try
            {
                string sql = @"select MSO.*,SO.Id SONo,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy')DeliveryDate,MOI.OwnReferenceNo,MOI.BuyerReferenceNo,SO.Qty
,isnull((select SOPlanQty from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id),SO.Qty + (MO.ExtraOrderPercentage*SO.Qty / 100)) as SOPlanQty,SO.Reason Remarks from [dbo].[MarkerSalesOrder] MSO
LEFT JOIN TRN.SalesOrder SO ON SO.Id=MSo.SalesOrderId
left join [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
left join [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
Where MSO.MarkerId='" + markerId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> GetFabricGRNRowList(string soId)
        {
            string sql = @"SELECT CAST(0 as bit) FlagG, MP.Id,MP.InventoryReceiveDetailId,MP.FirstCharacteristicsValueId,IRD.InventoryReceiveId,IRD.TransactionQty,UOM.UserName UOM,BUoM.UserName BaseUoM,IR.Id GRNNo,IR.GRNDate
,P.UserName PartyName,MM.UserName MaterialMasterName,MMA.StandardName ArticleName,CV.UserName SKUValue,MP.SalesOrderId
FROM dbo.GRNSOMap MP
LEFT JOIN  [TRN].[InventoryReceiveDetail] IRD ON MP.InventoryReceiveDetailId=IRD.Id
LEFT JOIN TRN.InventoryReceive IR ON IRD.InventoryReceiveId=IR.Id
LEFT JOIN HKP.Party P ON IR.PartyId=P.Id
LEFT JOIN TRN.InventoryMaterial IM ON IRD.InventoryMaterialId=IM.Id
LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
LEFT JOIN SCS.UnitOfMeasurement UOM ON IRD.TransactionUoMId=UOM.Id
LEFT JOIN SCS.UnitOfMeasurement BUoM ON IRD.BaseUOMId=BUoM.Id
LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id
LEFT JOIN MST.MaterialMasterArticle MMA ON IM.ArticleId=MMA.Id
LEFT JOIN [HKP].[CharacteristicsValue] CV ON MP.FirstCharacteristicsValueId=CV.Id
Where MP.SalesOrderId  " + soId + "";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetMarkerFabricGRNRowList(string soId, string markerId)
        {
            string sql = @"SELECT CAST(0 as bit) FlagG, MP.Id,MP.InventoryReceiveDetailId,MP.FirstCharacteristicsValueId,IRD.InventoryReceiveId,IRD.TransactionQty,UOM.UserName UOM,BUoM.UserName BaseUoM,IR.Id GRNNo,IR.GRNDate
,P.UserName PartyName,MM.UserName MaterialMasterName,MMA.StandardName ArticleName,CV.UserName SKUValue,MP.SalesOrderId
FROM dbo.GRNSOMap MP
LEFT JOIN  [TRN].[InventoryReceiveDetail] IRD ON MP.InventoryReceiveDetailId=IRD.Id
LEFT JOIN TRN.InventoryReceive IR ON IRD.InventoryReceiveId=IR.Id
LEFT JOIN HKP.Party P ON IR.PartyId=P.Id
LEFT JOIN TRN.InventoryMaterial IM ON IRD.InventoryMaterialId=IM.Id
LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
LEFT JOIN SCS.UnitOfMeasurement UOM ON IRD.TransactionUoMId=UOM.Id
LEFT JOIN SCS.UnitOfMeasurement BUoM ON IRD.BaseUOMId=BUoM.Id
LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id
LEFT JOIN MST.MaterialMasterArticle MMA ON IM.ArticleId=MMA.Id
LEFT JOIN [HKP].[CharacteristicsValue] CV ON MP.FirstCharacteristicsValueId=CV.Id
Where MP.SalesOrderId  " + soId + " AND MP.MarkerId='"+ markerId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCheckByList(string EmployeeId)
        {
            string sql = @"select m.*,MP.PlanName MasterPlan From MarkerMaster m
LEFT JOIN [MST].[MasterPlan] MP ON MP.Id=M.MasterPlanId
                                Where m.CheckByStatus='To Be Check' AND m.CheckById='" + EmployeeId + @"'
                                order by m.Sequence ";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetApproveByList(string EmployeeId)
        {
           
            string sql = @"select m.*,MP.PlanName MasterPlan From MarkerMaster m
LEFT JOIN [MST].[MasterPlan] MP ON MP.Id=M.MasterPlanId
                                Where CheckByStatus='Checked' AND ApprovedStatus<> 'Approved ' AND m.ApproveById='" + EmployeeId + @"'
                                order by m.Sequence ";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetDetailsList(string masterid)
        {
            string sql = @"SELECT CV.Id AS CharacteristicsValueId,CV.Code, CV.UserName AS [Text] 
                                ,Ratio = case when M.Id is null then '' else M.Ratio end,M.Id
                                FROM MarkerDetails M
                            LEFT JOIN hkp.CharacteristicsValue CV ON CV.Id=M.CharacteristicsValueId
                            Where M.MarkerMasterId='" + masterid + "'  Order by CV.Sequence";
            return _sqlRepository.GetDataCollection(sql, null);
        }


        #endregion Marker
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

public class CutPlantCalculate
{
    public string Id { get; set; }
    public string Characteristicsvalue { get; set; }
    public string CharacteristicsId { get; set; }
    public string CutPlanChildId { get; set; }
    public string MinimumPlyActualValue { get; set; }
    public string MinimumPlyOptionValue { get; set; }
    public ICollection<CutPlantCalculateDetails> Qty { get; set; }
}
public class CutPlantCalculateDetails
{
    public string CharacteristicsId { get; set; }
    public string ColorId { get; set; }
    public string Qty { get; set; }
    public string Ratio { get; set; }
    public string Characteristicsvalue { get; set; }
    public string Colorvalue { get; set; }
    public string CalculatedPlyQty { get; set; }
}