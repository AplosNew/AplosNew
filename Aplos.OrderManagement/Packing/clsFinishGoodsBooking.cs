using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Library.OrderManagement.Packing
{
    public class clsFinishGoodsBooking
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        #region Constructor
        public clsFinishGoodsBooking()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        #endregion Constructor

        public IEnumerable<object> GetList()
        {
            try
            {
                string sql = @"Select E.UserName ProductionEntity, P.UserName Process,FORMAT(FGB.FromDate,'dd-MMM-yyyy') FDate,FORMAT(FGB.ToDate,'dd-MMM-yyyy') TDate,FGB.* from [dbo].[FinishGoodsBooking] FGB
                                LEFT JOIN ORG.Entity E ON E.Id=FGB.ProductionEntityId
                                LEFT JOIN HKP.Process P ON P.Id=FGB.ProcessId";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetDetailList(string masterId, string entityId, string processId, string productionOrderId)
        {
            try
            {
                string sql = @"SELECT DISTINCT mo.MasterOrderNo,FGBD.Id
									,ISNULL(so.Id,'') SalesOrderId
									,PO.Id ProductionOrderId
									,mm.Id MaterialMasterId
									,mm.UserName MaterialMaster
									,ISNULL(mma.Id, '') ArticleId
									,ISNULL(mma.StandardName, '') Article
									,moi.ProductLibraryId
									,ISNULL(PL.Code,'') ProductCode
									,ISNULL(POD.ProductionOrderId, '') POId
									,BU.UserName Buyer
									,mo.TotalQty OrderQty
									,CEILING(SO.PlannedQty) PlannedQty,FB.Qty OtherBookedQty,FGBD.Qty,ISNULL(B.Rate,0) Rate,B.OrderCostingMasterTemplateId,B.CostingItem
									,ISNULL(MO.BuyerReferenceNo,'') BuyerOrder,ISNULL(MO.OwnReferenceNo,'') OwnOrder,ISNULL(moi.BuyerReferenceNo,'') BuyerItem,ISNULL(moi.OwnReferenceNo,'') OwnItem
									FROM TRN.ProductionOrderDetail POD
									LEFT JOIN (
									SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
									,s.Id,s.MasterOrderItemId
									FROM trn.SalesOrder AS s
									INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
									GROUP BY S.Id,s.MasterOrderItemId
									) so ON POD.SalesOrderId = SO.Id
									LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
									LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
									LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
									LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
									LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
									LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
									INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
									) OS ON OS.ProductionOrderId = POD.ProductionOrderId
									LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
									LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
									LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
									LEFT JOIN dbo.ProductLibrary PL ON PL.Id=moi.ProductLibraryId
									LEFT JOIN (
									select MOI.Id,A.OrderCostingMasterTemplateId, SUM(A.Rate) Rate,CI.UserName CostingItem
									from TRN.MasterOrderItem MOI
									LEFT JOIN [dbo].[OrderCostingMasterTemplate] OCMT ON OCMT.Id=MOI.OrderCostingMasterTemplateId
									LEFT JOIN
									( 
									Select DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossAmount Rate from [dbo].[OrderProcurementCostingDirectMaterial] DM
									UNION
									Select DP.CostingItemId,DP.OrderCostingMasterTemplateId,DP.Amount Rate from [dbo].[OrderProcurementCostingDirectProcess] DP
									UNION
									Select OP.CostingItemId,OP.OrderCostingMasterTemplateId,OP.[Value] Rate from [dbo].[OrderProcurementCostingOperation] OP
									UNION
									Select P.CostingItemId,P.OrderCostingMasterTemplateId,P.[Value] Rate from [dbo].[OrderProcurementCostingProfit] P
									UNION
									Select SE.CostingItemId,SE.OrderCostingMasterTemplateId,SE.[Value] Rate from [dbo].[OrderProcurementCostingSalesExpense] SE
									UNION
									Select VL.CostingItemId,VL.OrderCostingMasterTemplateId,VL.[Value] Rate from [dbo].[OrderProcurementCostingValueLoss] VL
									) 
									A ON A.OrderCostingMasterTemplateId=OCMT.OrderCostingMasterTemplateId
									JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
									JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
									WHERE ISNULL(ConsiderForFGValuation,0)=1 
									GROUP BY MOI.Id,A.OrderCostingMasterTemplateId,CI.UserName
									) B ON B.Id=moi.Id AND B.OrderCostingMasterTemplateId=moi.OrderCostingMasterTemplateId
									LEFT JOIN (
									SELECT SUM(B.Qty) Qty,A.ProductionOrderId,B.SalesOrderId
									FROM dbo.[FinishGoodsBooking] A 
									JOIN dbo.[FinishGoodsBookingDetail] B ON A.Id=B.FinishGoodsBookingId
									WHERE ISNULL(B.ProductLibraryId,'')<>'' AND ISNULL(A.Id,'') <>'" + masterId + @"'
									GROUP BY A.ProductionOrderId,B.SalesOrderId
									) FB ON FB.ProductionOrderId=PO.Id AND FB.SalesOrderId=SO.Id
									LEFT JOIN [dbo].[FinishGoodsBookingDetail] FGBD ON FGBD.ProductionOrderId=PO.Id AND FGBD.SalesOrderId=SO.Id 
									WHERE  POSP.ProcessId = '" + processId+"' AND PO.EntityId='"+entityId+"'  AND PO.Id='"+productionOrderId+"' AND FGBD.FinishGoodsBookingId='"+masterId+"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveData(Dictionary<string, object> data, List<Dictionary<string, object>> FinishGoodsBookingDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
				string pOId = null;
				string productCode = null;


				foreach (var item in FinishGoodsBookingDetailList)
                {
                    if (pOId==null)
                    {
						pOId = "'" + item["ProductionOrderId"].ToString() + "'";

					}
                    else
                    {
						pOId+= ",'"+ item["ProductionOrderId"].ToString() + "'"; 
					}

					if (productCode == null)
					{
						productCode = "'" + item["ProductCode"].ToString() + "'";

					}
					else
					{
						productCode += ",'" + item["ProductCode"].ToString() + "'"; 
					}

				}

                DataSet dsMaster, dsFinishGoodsBookingDetail, dsItemScanChild;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[FinishGoodsBooking] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.FinishGoodsBookingDetail WHERE FinishGoodsBookingId ='" + data["Id"] + "'", out dsFinishGoodsBookingDetail, false, "1");
                con.OpenDataSetThroughAdapter(@"SELECT * FROM dbo.ItemScanChild WHERE MasterId IN (Select Id from dbo.ItemScan ISN WHERE ISN.WorkDate between '" + data["FromDate"] + "' AND '" + data["ToDate"] + "') AND POId IN (" + pOId + @") AND ProductCode IN (" + productCode + @")", out dsItemScanChild, false, "1");
                //con.OpenDataSetThroughAdapter(@"SELECT * FROM dbo.ItemScanChild WHERE MasterId IN (Select Id from dbo.ItemScan ISN WHERE ISN.WorkDate between '" + data["FromDate"] + "' AND '" + DateTime.Now.ToString("dd-MMM-yyyy") + "') AND POId IN (" + pOId + @") AND ProductCode IN (" + productCode + @")", out dsItemScanChild, false, "1");

                string _Id = "";
                string masterId = "";
                string detailId = "";


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "FinishGoodsBooking", out _Id);

                    data["Id"] = "FB" + _Id;
                    
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();


                foreach (var item in FinishGoodsBookingDetailList)
                {
                    DataView dv = new DataView(dsFinishGoodsBookingDetail.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
						detailId = GetFinishGoodsBookingDetailPK();

						item["Id"] = detailId;
                        item["FinishGoodsBookingId"] = masterId;

                        AddNewRow(dsFinishGoodsBookingDetail.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
						detailId = dsFinishGoodsBookingDetail.Tables[0].Rows[0]["Id"].ToString();
					}
					
					if (dsItemScanChild.Tables[0].Rows.Count > 0)
					{
						for (int i = 0; i < dsItemScanChild.Tables[0].Rows.Count; i++)
						{
							dsItemScanChild.Tables[0].DefaultView.RowFilter = "Id='" + dsItemScanChild.Tables[0].Rows[i]["Id"].ToString() + "' AND POId = '" + item["ProductionOrderId"] + "' AND ProductCode = '" + item["ProductCode"] + "'";
							//dv.RowFilter = "Id='" + item["Id"] + "' AND POId = '" + item["ProductionOrderId"] + "' AND ProductCode = '" + item["ProductCode"] + "'";
							if (dsItemScanChild.Tables[0].DefaultView.Count > 0)
							{
								//edit
								DataRow dr = dsItemScanChild.Tables[0].DefaultView[0].Row;
								dr.BeginEdit();
								dr["FinishGoodsBookingId"] = masterId;
								dr["FinishGoodsBookingDetailId"] = detailId;

								dr["UpdatedBy"] = identity.Name;
								dr["UpdatedDate"] = System.DateTime.Now.ToString();
								dr.EndEdit();
							}
						}
					}

				}

				

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsFinishGoodsBookingDetail, dsItemScanChild);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private string GetFinishGoodsBookingDetailPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "FinishGoodsBookingDetail", out sID);
            return sID;
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

        public IEnumerable<object> GetProcessCbo(string entityId)
        {
            try
            {
                var sql = @"SELECT DISTINCT P.Id AS Value,P.UserName AS Text FROM dbo.EntityConfig EC
							JOIN HKP.Process P ON P.Id=EC.ConsumptionProcessId
							WHERE EC.EntityId='" + entityId + "' AND P.Active=1";
                return _sqlRepository.GetCombo(sql, "Value", "Text");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetProductionOrderDataList(string entityId, string processId)
        {

            try
            {
                string sql = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
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
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory
								   
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
								   WHERE  E.Id='" + entityId + "' AND POSP.ProcessId = '" + processId + "' ";

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
                var sql = @"SELECT DISTINCT mo.MasterOrderNo
                                    ,'' Id
                                    ,''ProductCodeList
									,ISNULL(so.Id,'') SalesOrderId
									,PO.Id ProductionOrderId
									,mm.Id MaterialMasterId
									,mm.UserName MaterialMaster
									,ISNULL(mma.Id, '') ArticleId
									,ISNULL(mma.StandardName, '') Article
									,moi.ProductLibraryId
									,ISNULL(PL.Code,'') ProductCode
									,ISNULL(POD.ProductionOrderId, '') POId
									,BU.UserName Buyer
									,mo.TotalQty OrderQty
									,CEILING(SO.PlannedQty) PlannedQty,FB.Qty OtherBookedQty,0 Qty,ISNULL(B.Rate,0) Rate,B.OrderCostingMasterTemplateId,B.CostingItem
									,ISNULL(MO.BuyerReferenceNo,'') BuyerOrder,ISNULL(MO.OwnReferenceNo,'') OwnOrder,ISNULL(moi.BuyerReferenceNo,'') BuyerItem,ISNULL(moi.OwnReferenceNo,'') OwnItem
									FROM TRN.ProductionOrderDetail POD
									LEFT JOIN (
									SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
									,s.Id,s.MasterOrderItemId
									FROM trn.SalesOrder AS s
									INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
									GROUP BY S.Id,s.MasterOrderItemId
									) so ON POD.SalesOrderId = SO.Id
									LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
									LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
									LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
									LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
									LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
									LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
									INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
									) OS ON OS.ProductionOrderId = POD.ProductionOrderId
									LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
									LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
									LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
									LEFT JOIN dbo.ProductLibrary PL ON PL.Id=moi.ProductLibraryId
									LEFT JOIN (
									select MOI.Id,A.OrderCostingMasterTemplateId, SUM(A.Rate) Rate,CI.UserName CostingItem
									from TRN.MasterOrderItem MOI
									LEFT JOIN [dbo].[OrderCostingMasterTemplate] OCMT ON OCMT.Id=MOI.OrderCostingMasterTemplateId
									LEFT JOIN
									( 
									Select DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossAmount Rate from [dbo].[OrderProcurementCostingDirectMaterial] DM
									UNION
									Select DP.CostingItemId,DP.OrderCostingMasterTemplateId,DP.Amount Rate from [dbo].[OrderProcurementCostingDirectProcess] DP
									UNION
									Select OP.CostingItemId,OP.OrderCostingMasterTemplateId,OP.[Value] Rate from [dbo].[OrderProcurementCostingOperation] OP
									UNION
									Select P.CostingItemId,P.OrderCostingMasterTemplateId,P.[Value] Rate from [dbo].[OrderProcurementCostingProfit] P
									UNION
									Select SE.CostingItemId,SE.OrderCostingMasterTemplateId,SE.[Value] Rate from [dbo].[OrderProcurementCostingSalesExpense] SE
									UNION
									Select VL.CostingItemId,VL.OrderCostingMasterTemplateId,VL.[Value] Rate from [dbo].[OrderProcurementCostingValueLoss] VL
									) 
									A ON A.OrderCostingMasterTemplateId=OCMT.OrderCostingMasterTemplateId
									JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
									JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
									WHERE ISNULL(ConsiderForFGValuation,0)=1 
									GROUP BY MOI.Id,A.OrderCostingMasterTemplateId,CI.UserName
									) B ON B.Id=moi.Id AND B.OrderCostingMasterTemplateId=moi.OrderCostingMasterTemplateId
									LEFT JOIN (
									SELECT SUM(B.Qty) Qty,A.ProductionOrderId,B.SalesOrderId
									FROM dbo.[FinishGoodsBooking] A 
									JOIN dbo.[FinishGoodsBookingDetail] B ON A.Id=B.FinishGoodsBookingId
									WHERE ISNULL(B.ProductLibraryId,'')<>'' AND ISNULL(A.Id,'') <>'"+ masterId + @"'
									GROUP BY A.ProductionOrderId,B.SalesOrderId
									) FB ON FB.ProductionOrderId=PO.Id AND FB.SalesOrderId=SO.Id
									WHERE  POSP.ProcessId = '" + processId + "' AND PO.EntityId='" + entityId + "'  AND PO.Id='" + productionOrderId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

		public IEnumerable<object> GetScanPackingData(string fromDate, string toDate)
		{
			try
			{
				string sql = @"SELECT CONVERT (bit,0) Active,NULL AS COSTList, '' Qty,sc.ProductCode, PL.Id ProductLibraryId , PO.Id ProductionOrderId,FGQty=SUM(CASE WHEN SC.IsDespatch=0 THEN sc.NetWeight ELSE 0 END)
								,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,MasterOrderItemNo=STUFF((select distinct ','+MOI.Id from 
								                                 TRN.MasterOrderItem MOI
                                                                 JOIN trn.SalesOrder XSO ON XSO.MasterOrderItemId=MOI.Id
                                                                 JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                ,MaterialMaster=STUFF((select distinct ','+MM.UserName from 
									                            MST.MaterialMaster MM
								                                JOIN TRN.MasterOrderItem MI ON MI.MaterialMasterId=MM.Id                                                                 
                                                                 JOIN trn.SalesOrder XSO ON XSO.MasterOrderItemId=MI.Id
                                                                 JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                        
							    ,Article=STUFF((select distinct ','+MMA.StandardName from 
									                            MST.MaterialMasterArticle MMA
								                                JOIN TRN.MasterOrderItem MI ON MI.ArticleId=MMA.Id                                                                 
                                                                 JOIN trn.SalesOrder XSO ON XSO.MasterOrderItemId=MI.Id
                                                                 JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')     
	                        ,ISNULL(B.Rate,0) Rate,B.OrderCostingMasterTemplateId
							FROM dbo.ItemScanChild sc
	                        LEFT JOIN dbo.ProductLibrary PL ON PL.Code=SC.ProductCode
							LEFT JOIN dbo.ItemScan ISN ON ISN.Id=SC.MasterId
	    
							LEFT JOIN TRN.ProductionOrder PO ON PO.Id=SC.POId
							LEFT JOIN TRN.ProductionOrderDetail POD ON POD.ProductionOrderId=PO.Id													
							
							LEFT JOIN 
							(
								SELECT DISTINCT COST.ProductionOrderId, COST.OrderCostingMasterTemplateId,COST.Rate FROM (select po.ProductionOrderId, moi.Id,a.OrderCostingMasterTemplateId,sum(a.rate) AS Rate from trn.MasterOrderItem moi 
								 JOIN
									( 
									Select DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossAmount Rate from [dbo].[OrderProcurementCostingDirectMaterial] DM
									UNION
									Select DP.CostingItemId,DP.OrderCostingMasterTemplateId,DP.Amount Rate from [dbo].[OrderProcurementCostingDirectProcess] DP
									UNION
									Select OP.CostingItemId,OP.OrderCostingMasterTemplateId,OP.[Value] Rate from [dbo].[OrderProcurementCostingOperation] OP
									UNION
									Select P.CostingItemId,P.OrderCostingMasterTemplateId,P.[Value] Rate from [dbo].[OrderProcurementCostingProfit] P
									UNION
									Select SE.CostingItemId,SE.OrderCostingMasterTemplateId,SE.[Value] Rate from [dbo].[OrderProcurementCostingSalesExpense] SE
									UNION
									Select VL.CostingItemId,VL.OrderCostingMasterTemplateId,VL.[Value] Rate from [dbo].[OrderProcurementCostingValueLoss] VL
									)  AS 	A ON A.OrderCostingMasterTemplateId=moi.OrderCostingMasterTemplateId

									left JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
									left JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
									join (select POD.ProductionOrderId,SO.MasterOrderItemId from TRN.ProductionOrderDetail POD
									join trn.salesOrder SO ON SO.id=pod.SalesOrderId group by POD.ProductionOrderId,SO.MasterOrderItemId) AS PO on PO.MasterOrderItemId=MOI.Id
									WHERE ISNULL(ConsiderForFGValuation,0)=1 AND moi.OrderCostingMasterTemplateId<>''
								group by  po.ProductionOrderId,moi.Id,a.OrderCostingMasterTemplateId) AS COST
							) B ON B.ProductionOrderId=PO.Id

	                        WHERE ISN.WorkDate between '" + fromDate+@"' AND '"+toDate+ @"' AND ISNULL(SC.FinishGoodsBookingId,'')=''
							GROUP BY SC.ProductCode, PO.Id, PL.Id,B.Rate,B.OrderCostingMasterTemplateId";

				return _sqlRepository.GetDataCollection(sql, null);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public IEnumerable<object> GetCostingItemDetailData(string costingId)
        {
            try
            {
				string sql = @"select CT.Id, SUM(A.Rate) Rate,CI.UserName CostingItem, CC.UserName CostingComponent
								from dbo.CostingMasterTemplate CT
								left JOIN
									( 
									Select DM.CostingItemId,DM.CostingMasterTemplateId,DM.GrossAmount Rate from [dbo].PreCostingDirectMaterial DM
									UNION
									Select DP.CostingItemId,DP.CostingMasterTemplateId,DP.Amount Rate from [dbo].PreCostingDirectProcess DP
									UNION
									Select OP.CostingItemId,OP.CostingMasterTemplateId,OP.[Value] Rate from [dbo].PreCostingOperation OP
									UNION
									Select P.CostingItemId,P.CostingMasterTemplateId,P.[Value] Rate from [dbo].PreCostingProfit P
									UNION
									Select SE.CostingItemId,SE.CostingMasterTemplateId,SE.[Value] Rate from [dbo].PreCostingSalesExpense SE
									UNION
									Select VL.CostingItemId,VL.CostingMasterTemplateId,VL.[Value] Rate from [dbo].PreCostingValueLoss VL
									) 
								A ON A.CostingMasterTemplateId=CT.Id
								left JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
								left JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
								WHERE CT.Id='"+ costingId + @"'
								GROUP BY CT.Id,CI.UserName, CC.UserName";
				return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
		public IEnumerable<object> GetCostingItemData(string productionOrderId)
		{
			try
			{
				string sql = @"select distinct COST.ProductionOrderId, COST.OrderCostingMasterTemplateId,COST.Rate,COST.UserName FROM (select po.ProductionOrderId, moi.Id,a.OrderCostingMasterTemplateId,sum(a.rate) AS Rate,OCMT.UserName 
from trn.MasterOrderItem moi 
 JOIN
	( 
	Select DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossAmount Rate from [dbo].[OrderProcurementCostingDirectMaterial] DM
	UNION
	Select DP.CostingItemId,DP.OrderCostingMasterTemplateId,DP.Amount Rate from [dbo].[OrderProcurementCostingDirectProcess] DP
	UNION
	Select OP.CostingItemId,OP.OrderCostingMasterTemplateId,OP.[Value] Rate from [dbo].[OrderProcurementCostingOperation] OP
	UNION
	Select P.CostingItemId,P.OrderCostingMasterTemplateId,P.[Value] Rate from [dbo].[OrderProcurementCostingProfit] P
	UNION
	Select SE.CostingItemId,SE.OrderCostingMasterTemplateId,SE.[Value] Rate from [dbo].[OrderProcurementCostingSalesExpense] SE
	UNION
	Select VL.CostingItemId,VL.OrderCostingMasterTemplateId,VL.[Value] Rate from [dbo].[OrderProcurementCostingValueLoss] VL
	)  AS 	A ON A.OrderCostingMasterTemplateId=moi.OrderCostingMasterTemplateId

	left JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
	left JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
	left JOIN [dbo].[OrderCostingMasterTemplate] OCMT ON OCMT.Id=A.OrderCostingMasterTemplateId
	join (select POD.ProductionOrderId,SO.MasterOrderItemId from TRN.ProductionOrderDetail POD
	join trn.salesOrder SO ON SO.id=pod.SalesOrderId group by POD.ProductionOrderId,SO.MasterOrderItemId) AS PO on PO.MasterOrderItemId=MOI.Id
	WHERE ISNULL(ConsiderForFGValuation,0)=1 AND PO.ProductionOrderId='"+ productionOrderId + @"'
AND moi.OrderCostingMasterTemplateId<>''
group by  po.ProductionOrderId,moi.Id,a.OrderCostingMasterTemplateId,OCMT.UserName) AS COST";
				return _sqlRepository.GetDataCollection(sql, null);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetBookedAndBalancedData(string productionOrderId)
        {
            try
            {
                string sql = @"SELECT sc.ProductCode, PL.Id ProductLibraryId , sc.POId,FGQty=SUM(CASE WHEN SC.IsDespatch=0 THEN sc.NetWeight ELSE 0 END),ISNULL(FB.Qty,0) BookedQty
                            , Balance=(SUM(CASE WHEN SC.IsDespatch=0 THEN sc.NetWeight ELSE 0 END)-ISNULL(FB.Qty,0))
	                        FROM dbo.ItemScanChild sc
	                        LEFT JOIN dbo.ProductLibrary PL ON PL.Code=SC.ProductCode
	                        LEFT JOIN (
	                        SELECT SUM(B.Qty) Qty,A.ProductionOrderId ,PL.Code
	                        FROM dbo.[FinishGoodsBooking] A 
	                        JOIN dbo.[FinishGoodsBookingDetail] B ON A.Id=B.FinishGoodsBookingId
	                        JOIN dbo.ProductLibrary PL ON PL.Id=B.ProductLibraryId
	                        WHERE ISNULL(B.ProductLibraryId,'')<>''
	                        GROUP BY A.ProductionOrderId,PL.Code
	                        ) FB ON FB.ProductionOrderId=SC.POId AND FB.Code=SC.ProductCode
	                        WHERE sc.POId='" + productionOrderId + @"'
							GROUP BY SC.ProductCode, SC.POId, PL.Id,FB.Qty
							--Having (SUM(CASE WHEN SC.IsDespatch=0 THEN sc.NetWeight ELSE 0 END)-ISNULL(FB.Qty,0))>0
							";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSavedBookedAndBalancedData(string productionOrderId)
        {
            try
            {
                string sql = @"SELECT sc.ProductCode, PL.Id ProductLibraryId , sc.POId,FGQty=SUM(CASE WHEN SC.IsDespatch=0 THEN sc.NetWeight ELSE 0 END),ISNULL(FB.Qty,0) BookedQty, Balance=(SUM(CASE WHEN SC.IsDespatch=0 THEN sc.NetWeight ELSE 0 END)-ISNULL(FB.Qty,0))
	                        FROM dbo.ItemScanChild sc
	                        LEFT JOIN dbo.ProductLibrary PL ON PL.Code=SC.ProductCode
	                        LEFT JOIN (
	                        SELECT SUM(B.Qty) Qty,A.Id,A.ProductionOrderId ,PL.Code
	                        FROM dbo.[FinishGoodsBooking] A 
	                        JOIN dbo.[FinishGoodsBookingDetail] B ON A.Id=B.FinishGoodsBookingId
	                        JOIN dbo.ProductLibrary PL ON PL.Id=B.ProductLibraryId
	                        WHERE ISNULL(B.ProductLibraryId,'')<>''
	                        GROUP BY A.Id,A.ProductionOrderId,PL.Code
	                        ) FB ON FB.ProductionOrderId=SC.POId AND FB.Code=SC.ProductCode
	                        WHERE sc.POId='" + productionOrderId + "' GROUP BY SC.ProductCode, SC.POId, PL.Id,FB.Qty";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

		public IEnumerable<object> GetItemScanChildData(string fromDate, string toDate)
		{
			try
			{
				string sql = @"SELECT SC.POId ProductionOrderId,sc.ProductCode, PL.Id ProductLibraryId, PL.CostingMasterTemplateId, CT.UserName CostingMasterTemplate,MM.UserName MaterialMaster,MMA.StandardName Article
							 ,Qty=SUM(CASE WHEN SC.IsDespatch=0 THEN SC.NetWeight ELSE 0 END),ISNULL(B.Rate,0)Rate, Amount=SUM(CASE WHEN SC.IsDespatch=0 THEN SC.NetWeight ELSE 0 END) * ISNULL(B.Rate,0)
							FROM dbo.ItemScanChild SC 
							LEFT JOIN dbo.ProductLibrary PL ON PL.Code=SC.ProductCode
							LEFT JOIN dbo.ItemScan ISN ON ISN.Id=SC.MasterId
							LEFT JOIN dbo.CostingMasterTemplate AS CT ON CT.Id=PL.CostingMasterTemplateId
							LEFT JOIN 
							(
							SELECT DISTINCT COST.CostingMasterTemplateId,COST.Rate 
							FROM (select A.CostingMasterTemplateId,sum(A.rate) AS Rate from CostingMasterTemplate CMT 
								JOIN
								( 
								SELECT DM.CostingItemId,DM.CostingMasterTemplateId,DM.GrossAmount Rate from [dbo].PreCostingDirectMaterial DM
								UNION
								SELECT DP.CostingItemId,DP.CostingMasterTemplateId,DP.Amount Rate from [dbo].PreCostingDirectProcess DP
								UNION
								SELECT OP.CostingItemId,OP.CostingMasterTemplateId,OP.[Value] Rate from [dbo].PreCostingOperation OP
								UNION
								SELECT P.CostingItemId,P.CostingMasterTemplateId,P.[Value] Rate from [dbo].PreCostingProfit P
								UNION
								SELECT SE.CostingItemId,SE.CostingMasterTemplateId,SE.[Value] Rate from [dbo].PreCostingSalesExpense SE
								UNION
								SELECT VL.CostingItemId,VL.CostingMasterTemplateId,VL.[Value] Rate from [dbo].PreCostingValueLoss VL
								)  AS 	A ON A.CostingMasterTemplateId=CMT.Id
								LEFT JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
								LEFT JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
							GROUP BY a.CostingMasterTemplateId) AS COST
							) B ON B.CostingMasterTemplateId=CT.Id
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=PL.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=PL.ArticleId
							WHERE ISN.WorkDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(SC.FinishGoodsBookingId,'')=''
							GROUP BY SC.POId,SC.ProductCode,PL.Id,PL.CostingMasterTemplateId,B.Rate,CT.UserName,MM.UserName,MMA.StandardName";
				return _sqlRepository.GetDataCollection(sql, null);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetItemDetailData(string masterId)
		{
			try
			{
				string sql = @"SELECT FGD.Id,SC.POId ProductionOrderId,sc.ProductCode, PL.Id ProductLibraryId, PL.CostingMasterTemplateId, CT.UserName CostingMasterTemplate,MM.UserName MaterialMaster,MMA.StandardName Article
							 ,Qty=SUM(CASE WHEN SC.IsDespatch=0 THEN SC.NetWeight ELSE 0 END),ISNULL(B.Rate,0)Rate, Amount=SUM(CASE WHEN SC.IsDespatch=0 THEN SC.NetWeight ELSE 0 END) * ISNULL(B.Rate,0),Active=Convert(bit,1)
							FROM dbo.ItemScanChild SC 
							LEFT JOIN dbo.ProductLibrary PL ON PL.Code=SC.ProductCode
							LEFT JOIN dbo.ItemScan ISN ON ISN.Id=SC.MasterId
							LEFT JOIN dbo.CostingMasterTemplate AS CT ON CT.Id=PL.CostingMasterTemplateId
							LEFT JOIN 
							(
							SELECT DISTINCT COST.CostingMasterTemplateId,COST.Rate 
							FROM (select A.CostingMasterTemplateId,sum(A.rate) AS Rate from CostingMasterTemplate CMT 
								JOIN
								( 
								SELECT DM.CostingItemId,DM.CostingMasterTemplateId,DM.GrossAmount Rate from [dbo].PreCostingDirectMaterial DM
								UNION
								SELECT DP.CostingItemId,DP.CostingMasterTemplateId,DP.Amount Rate from [dbo].PreCostingDirectProcess DP
								UNION
								SELECT OP.CostingItemId,OP.CostingMasterTemplateId,OP.[Value] Rate from [dbo].PreCostingOperation OP
								UNION
								SELECT P.CostingItemId,P.CostingMasterTemplateId,P.[Value] Rate from [dbo].PreCostingProfit P
								UNION
								SELECT SE.CostingItemId,SE.CostingMasterTemplateId,SE.[Value] Rate from [dbo].PreCostingSalesExpense SE
								UNION
								SELECT VL.CostingItemId,VL.CostingMasterTemplateId,VL.[Value] Rate from [dbo].PreCostingValueLoss VL
								)  AS 	A ON A.CostingMasterTemplateId=CMT.Id
								LEFT JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
								LEFT JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
							GROUP BY a.CostingMasterTemplateId) AS COST
							) B ON B.CostingMasterTemplateId=CT.Id
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=PL.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=PL.ArticleId
							LEFT JOIN FinishGoodsBooking FG ON FG.Id=SC.FinishGoodsBookingId
							LEFT JOIN FinishGoodsBookingDetail FGD ON FGD.FinishGoodsBookingId=FG.Id AND SC.FinishGoodsBookingDetailId=FGD.Id
	                        WHERE FG.Id='" + masterId + @"'
							GROUP BY FGD.Id,SC.POId,SC.ProductCode,PL.Id,PL.CostingMasterTemplateId,B.Rate,CT.UserName,MM.UserName,MMA.StandardName";

				return _sqlRepository.GetDataCollection(sql, null);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public object GetFromDate()
        {
			string sql = "";
            try
            {
				sql = @"Select FORMAT(MIN(A.WorkDate),'dd-MMM-yyyy') FromDate,FORMAT(MAX(A.WorkDate),'dd-MMM-yyyy') ToDate
						from dbo.ItemScan A
						JOIN dbo.ItemScanChild B oN A.Id=B.MasterId
						Where B.IsDespatch=0";
				return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
	}

}
